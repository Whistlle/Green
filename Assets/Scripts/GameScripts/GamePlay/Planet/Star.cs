﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Tuples;
using Utilities;

namespace Green
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Planet))]
    public class Star : MonoBehaviour
    {
        //Star的cor
        /*
		public Star(Star star)
		{
			_state = star.State;
			_schedule = star.Schedule;
			_DEF = star.DEF;
			_vigour = star.Vigour;
			_capacity = star.Capacity;
			_location = star.Location;
			_troops = star.Troops;
		}
        */

        void SetProperty(
            e_State state,
            int def,
            int vigour,
            int capacity,
            Vector2 location,
            float enemyTroops,
            float playerTroops,
            float schedule)
        {
            SetInitState(state);
            _DEF = def;
            _vigour = vigour;
            _capacity = capacity;
            _location = location;
            _enemyTroops = enemyTroops;
            _playerTroops = playerTroops;
            _schedule = schedule;
        }

        //星球所处状态
        public enum e_State
        {
            Player = 0,             //属于玩家的和平星球
            AI = 1,                 //属于电脑的和平星球
            NeutralityPeace = 2,    //中立
            NeutralityToPlayer = 3, //玩家正在占领的中立星球
            NeutralityToAI = 4,     //电脑正在占领的中立星球
        }
        private FSM _fsm;

        private FSMState _fsmState;

        [SerializeField, SetProperty("SelectedState")]
        private e_State _selectedState = e_State.Player;

        public FSM FiniteStateMachine
        {
            get
            {
                return _fsm;
            }
        }

        public e_State SelectedState
        {
            get
            {
                return _selectedState;
            }
            set
            {
                _selectedState = value;
            }
        }

        public e_State State
        {
            get
            {
                return _fsmState.EnumState;
            }
        }
        
        public FSMState FsmState
        {
            get
            {
                return _fsmState;
            }
        }

        [SerializeField, SetProperty("DEF")]
        [Range(0f, 10f)]
        private int _DEF;//防御力

        [SerializeField, SetProperty("Vigour")]
        [Range(0f, 10f)]
        private int _vigour;//活力，增长量

        [SerializeField, SetProperty("Capacity")]
        [Range(0f, 20f)]
        private int _capacity;//容量

        [SerializeField, SetProperty("Location")]
        private Vector2 _location;
        //private Tuple<int, int> _location;//坐标


        //private Tuple<double, double> _troops;//双方兵力，前者是玩家，后者是AI

        [SerializeField, SetProperty("Troops")]
        private float _playerTroops;

        [SerializeField, SetProperty("Troops")]
        private float _enemyTroops;

        [SerializeField, SetProperty("Schedule")]
        private float _schedule;//中立星球占领进度（中立星球的那四个状态有效）

        public float PlayerTroops
        {
            get
            {
                return _playerTroops;
            }
            set
            {
                _playerTroops = value;
            }
        }

        public float EnemyTroops
        {
            get
            {
                return _enemyTroops;
            }
            set
            {
                _enemyTroops = value;
            }
        }
        public int DEF
        {
            get
            {
                return _DEF;
            }
            set
            {
                _DEF = value;
            }
        }
        public int Vigour
        {
            get
            {
                return _vigour;
            }
            set
            {
                _vigour = value;
            }
        }
        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                _capacity = value;
            }
        }
        public Vector2 Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
            }
        }

        public float Schedule
        {
            get
            {
                return _schedule;
            }
            set
            {
                _schedule = value;
            }
        }

        public Star(): base()
        {
            _fsm = new FSM(this);
        }

        void Start()
        {
            SetProperty(_selectedState, DEF, Vigour, Capacity, Location, EnemyTroops, PlayerTroops, Schedule);
        }

        void SetInitState(e_State state)
        {
            switch (state)
            {
                case e_State.Player:
                    _fsmState = new StatePlayer(_fsm, this);
                    break;
                case e_State.AI:
                    _fsmState = new StateAI(_fsm, this);
                    break;
                case e_State.NeutralityPeace:
                    _fsmState = new StateNeutralityPeace(_fsm, this);
                    break;
                case e_State.NeutralityToPlayer:
                    _fsmState = new StateNeutralityToPlayer(_fsm, this);
                    break;
                case e_State.NeutralityToAI:
                    _fsmState = new StateNeutralityToAI(_fsm, this);
                    break;
                default:
                    break;
            }
        }

        public bool IsBattleInPlanet()
        {
            //被占领 和平
            if (State == e_State.AI || State == e_State.Player || State == e_State.NeutralityPeace)
            {
                if (_enemyTroops > 0 && _playerTroops > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void BattlePerTime(float perTime)
        {
            if (!IsBattleInPlanet()) return;

            if (State == e_State.AI)
            {
                _enemyTroops  += Formula.CalculateDamageForDefOnePerTime(_enemyTroops, _playerTroops, DEF, perTime);
                _playerTroops += Formula.CalculateDamageForAttackOnePerTime(_enemyTroops, _playerTroops, DEF, perTime);
            }
            else if (State == e_State.Player)
            {
                _enemyTroops  += Formula.CalculateDamageForAttackOnePerTime(_playerTroops, _enemyTroops, DEF, perTime);
                _playerTroops += Formula.CalculateDamageForDefOnePerTime(_playerTroops, _enemyTroops, DEF, perTime);
            }
            else
            {
                _enemyTroops  += Formula.CalculateDamageForNeutralOnePerTime(_enemyTroops, _playerTroops, perTime);
                _playerTroops += Formula.CalculateDamageForNeutralOnePerTime(_enemyTroops, _playerTroops, perTime);
            }
        }

        bool _isDuringCapture = false;
        
        public void StartCapture()
        {
            _schedule = 0;
            _isDuringCapture = true;
        }

        public void StopCapture()
        {
            _isDuringCapture = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUpdateSituation()
        {
            //战斗清算
            BattlePerTime(Formula.CalculatePerTime);

            //本状态执行更新
            _fsmState.OnUpdate();
            //判断是否有到条件进入下一个状态
            _fsmState = _fsmState.NextState();
        }

        Planet m_planet = null;
        public Planet _planet
        {
            get
            {
                if (m_planet == null)
                {
                    m_planet = GetComponent<Planet>();
                    if (m_planet == null)
                        Debug.LogError("Need Script: Planet");
                }
                return m_planet;
            }
        }

        /// <summary>
        /// 更新上层士兵动画：增加/删除
        /// </summary>
        public void OnUpdateSoldierAnimation()
        {
            int enemyCount = Mathf.FloorToInt(_enemyTroops);
            int playerCount = Mathf.FloorToInt(_playerTroops);
            _planet.UpdateSoldiersToCount(enemyCount, SoldierType.Enemy);
            _planet.UpdateSoldiersToCount(playerCount, SoldierType.Player);
        }

        float Truncate(float num, float min, float max)
        {
            if (num >= max) return max;
            if (num <= min) return min;
            return num;
        }

        float Truncate(float num, float min)
        {
            if (num <= min) return min;
            return num;
        }
    }
}