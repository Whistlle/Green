﻿using UnityEngine;
using System.Collections;

namespace Green
{
    public class StateNeutralityPeace : FSMState
    {
        public Star.e_State State;

        private Star _star;
        public StateNeutralityPeace(Star _star)
        {
            FSMTransition.CheckCondition neutralToAI = () =>
            {
                if (_star.EnemyTroops > 0f && _star.PlayerTroops <= 0f)
                    return true;
                else
                    return false;
            };

            FSMTransition.CheckCondition neutralToPlayer = () =>
            {
                if (_star.EnemyTroops <= 0f && _star.PlayerTroops > 0f)
                    return true;
                else
                    return false;
            };
            _transitions.Add(new FSMTransition(new StateNeutralityToAI(_star), neutralToAI));
            _transitions.Add(new FSMTransition(new StateNeutralityToPlayer(_star), neutralToPlayer));
        }

        public override void OnEnter()
        {
            //改变星球动画
            return;
        }

        protected override void OnExit()
        {
            return;
        }
    }
}