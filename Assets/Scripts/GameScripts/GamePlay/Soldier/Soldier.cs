﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Green
{
    public class Soldier : MonoBehaviour
    {
        //在那个星球上
        public Planet InPlanet;

        public int FromPlanetID
        {
            get { return GameWorld.Instance.GetPlanetID(_fromPlanet.GetStar()); }
        }

        public int ToPlanetID
        {
            get { return GameWorld.Instance.GetPlanetID(_toPlanet.GetStar()); }
        }

        Planet _fromPlanet;
        Planet _toPlanet;

        public SoldierType Bloc = SoldierType.Player;

        public enum StateType
        {
            Patrol, //Wander + 墙
            Move,   //Arrive + Wander
            Attack, //Wander + 墙 + 战斗动画
        }

        public StateType CurrentType = StateType.Patrol;

        MovingEntity _movingEntity;

        public SoldierStyle Style
        {
            get; set;
        }

        public void UpdateState(StateType state)
        {
            CurrentType = state;
            _movingEntity.ClearBehavior();
            switch (state)
            {
                case StateType.Patrol:
                    PatrolOn();
                    break;
                case StateType.Move:
                    MoveOn();
                    break;
                case StateType.Attack:
                    break;
                default:
                    break;
            }
        }

        void PatrolOn()
        {
            _movingEntity.BehaviorOn(SteeringBehavior.Type_.wander);
            _movingEntity.BehaviorOn(SteeringBehavior.Type_.wall_avoidance);
            //set Patrol Param
            //_behaviors.WanderScale = 12.34f;
            //_behaviors.SeekScale = 1f;
            //_movingEntity.MaxForce = 14.97f;    
        }

        void MoveOn()
        {
            _movingEntity.BehaviorOn(SteeringBehavior.Type_.seek);
            _movingEntity.BehaviorOn(SteeringBehavior.Type_.wander);
            _fromPlanet = InPlanet;
            InPlanet = null;
            //_behaviors.SeekScale = 25f;
            //_behaviors.WanderScale = 2f;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="onSeekEnded">
        /// 上一层Seek结束后发生的动作
        /// </param>
        public void SetSeekDestination(Planet p, Action onSeekEnded)
        {
            _toPlanet = p;
            var seek = _movingEntity.GetBehavior(SteeringBehavior.Type_.seek) as Seek;
            seek.SetDistination(p,
                () =>
                {
                    SetPlanet(p);
                    OnSeekEnded(onSeekEnded);
                });
        }

        public void SetPlanet(Planet p)
        {
            InPlanet = p;
            
        }

        public void OnSeekEnded(Action onSeekEnded)
        {
            onSeekEnded();
            UpdateState(StateType.Patrol);
        }
        // Use this for initialization
        void Start()
        {
            _movingEntity = GetComponent<MovingEntity>();
            if (_movingEntity == null)
            {
                Debug.LogError("Need Script: MovingEntity");
            }
            UpdateState(StateType.Patrol);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Destory()
        {
            GameObject.DestroyObject(gameObject);
        }

        public float TimeToDestination()
        {
            if (CurrentType == StateType.Move)
            {
                return _movingEntity.TimeToDestination();
            }
            return 0f;
        }
    }
}
