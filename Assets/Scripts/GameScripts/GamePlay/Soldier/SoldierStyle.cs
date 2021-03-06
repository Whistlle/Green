﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Green
{
    public enum Soldier_Style
    {
        None,
        Player_1,
        Enemy_1
    }

    public class SoldierStyle
    {
        Soldier _soldier;
        public Soldier GetSoldier()
        {
            return _soldier;
        }

        GameObject _go;

        //TODO
        public static string GetPrefabPath(Soldier_Style style, SoldierType type)
        {
            string path = Settings.SOLDIER_PREFAB_PATH;
            if (style == Soldier_Style.None)
                path += "plane";
            else if (style == Soldier_Style.Player_1)
            {
                path += "player_1";
            }
            else if (style == Soldier_Style.Enemy_1)
            {
                path += "enemy_1";
            }
            return path;
        }

        public SoldierStyle(Planet inPlanet, Soldier_Style style, SoldierType type)
        {

            _go = Resources.Load<GameObject>(GetPrefabPath(style, type));
            var go = _go;
            go = GameObject.Instantiate(go);

            _soldier = go.GetComponent<Soldier>();
            if (_soldier == null)
                Debug.LogError("Need Script: Soldier");

            _soldier.Style = this;
            _soldier.Bloc = type;
            _soldier.InPlanet = inPlanet;
            //set tag
            go.tag = Settings.SOLDIER_TAG;

            //set name
            go.name = GenerateSoldierName(inPlanet, type);

            var transform = go.transform;

            var z = transform.position.z;

            var pos = inPlanet.GetRandomPositionInPlanet();
            transform.position = new Vector3(pos.x, pos.y, z);
            //set parent
            transform.SetParent(GameObject.Find(GetParentName(type)).transform, true);

            go.SetActive(true);     
        }

        string GenerateSoldierName(Planet planet, SoldierType type)
        {
            return "Soldier" + "_in_" + planet.name + "_" + type.ToString();
        }

        string GetParentName(SoldierType type)
        {
            var root = GameplayManager.SoldierRoot;
            switch (type)
            {
                case SoldierType.Player:
                    root += "Players";
                    break;
                case SoldierType.Enemy:

                    root += "Enemies";
                    break;
                default:
                    break;
            }
            return root;
        }
    }
}
