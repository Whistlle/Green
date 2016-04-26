﻿using System;
using System.Collections.Generic;
using UnityEngine;



/**
 * 
 * AI类 实现AI的相关功能
 * 
 */

namespace Green
{
	public class AI
	{
		//单例类
		private AI()
		{
			//统计situation里面士兵的分布情况
			StatistDistribution();
			//计算双方的属性和
			caculateTotalProperty ();
		}
		//实例
		private AI _instance;

		//获取实例
		public static AI GetInstance()
		{
			if (_instance == null)
			{
				_instance = new AI ();
			}

			//统计situation里面士兵的分布情况
			StatistDistribution();
			//计算双方的属性和
			caculateTotalProperty ();
			return _instance;
		}

		/**
		 * 接口设定
		 * 在GameWorld里Planets获取所有星球
		 * 在Planet类里GetProperty返回star star里面有双方兵力以及各属性
		 * star里面PlayerTroops和EnemyTroops表示双方兵力
		 * 每个Solidier里面TimeToDestination是他到目的地所需时间，如果他的状态是在路上的话
		 */
		public void runAI()
		{
			//获取目标星球下标
			int desIndex = getDestination ();
			if (desIndex == -1)
				return;//若没有符合条件的星球 什么也不做
			//获取起点星球和派兵数量 从最近的有己方兵力的星球 过剩兵力为正数的星球 过剩兵力全派过来 
			int minDistance = int.MaxValue;
			int sourceIndex = desIndex;//起点星球
			int troops = 0;//派遣兵力
			for (int index = 0; index < _starCopys.Count; ++index) 
			{
				float excess = CaculateExcessForceCoefficient (index);//过剩兵力
				if (getDistance (_starCopys [desIndex].Location, _starCopys [index].Location) < minDistance//距离最小
				   && _starCopys [index].EnemyTroops > 0//有AI兵力
					&& excess > 0) 
				{
					sourceIndex = index;
					troops = Math.Floor (excess);
				}
			}

			if (sourceIndex != desIndex) 
			{
				GameWorld.Instance.Planets [sourceIndex].SendAISoldiers (GameWorld.Instance.Planets [desIndex], troops);
			}

		}

		struct SoldierOnTheWay
		{
			public int m_origin;//起点ID
			public int m_terminal;//终点ID
			public float m_needTime;//还剩几秒到达目的地;
		}

		//玩家在路上的兵力及还有多少秒到达目的地
		private List<SoldierOnTheWay> _playerOnTheWay;

		//AI在路上的兵力及还有多少秒到达目的地
		private List<SoldierOnTheWay> _aiOnTheWay;

		//所有星球的情况 复制过来 因为会模拟导致会改动
		private List<Star> _starCopys;

		//玩家总活力
		private int _playerTotalVigour;

		//AI总活力
		private int _aiTotalVigour;

		//玩家总容量
		private int _playerTotalCapacity;

		//AI总容量
		private int _aiTotalCapacity;

		//获取situation里面的双方兵力分布情况
		private void StatistDistribution()
		{
			_starCopys = new List<Star> ();
			//获取GameWorld里面Star的list
			foreach(Star star in GameWorld.Instance.Planets)
			{
				_starCopys.Add (star);
			}
			//获取在路上的士兵信息
			_playerOnTheWay = new List<SoldierOnTheWay>();
			_aiOnTheWay = new List<SoldierOnTheWay> ();
			foreach(Soldier soldier in GameWorld.Instance.Soldiers)
			{
				if (soldier.CurrentType == Soldier.StateType.Move) 
				{//若士兵的状态是正在移动的状态
					SoldierOnTheWay soldierOnTheWay = new SoldierOnTheWay();
					soldierOnTheWay.m_needTime = soldier.TimeToDestination;//还有多少秒到达目的地
					soldierOnTheWay.m_origin = soldier.FromPlanetID;
					soldierOnTheWay.m_terminal = soldier.ToPlanetID;

					//判断这个士兵是哪一方的就加到对应的list
					if (soldier.Bloc == SoldierType.Player) 
					{
						_playerOnTheWay.Add (soldierOnTheWay);
					} else if(soldier.Bloc == SoldierType.Enemy)
					{
						_aiOnTheWay.Add (soldierOnTheWay);
					}
				}
			}
		}

		//获取双方属性和（容量和and活力和）
		private void caculateTotalProperty()
		{
			//双方容量和
			_aiTotalCapacity = GameWorld.Instance.EnemyMaxPopulation;
			_playerTotalCapacity = GameWorld.Instance.PlayerMaxPopulation;
			//双方活力和
			_aiTotalVigour = 0;
			_playerTotalVigour = 0;
			foreach(Star star in _starCopys) 
			{
				//加到对应里面
				if (star.State == Star.e_State.AI) 
				{
					_aiTotalVigour += star.Vigour;
				} else if (star.State == Star.e_State.Player) 
				{
					_playerTotalVigour += star.Vigour;
				}
			}

		}

		/*
		 * 模拟双方都不派兵的情况下某个星球t秒后状态 t=-1时为无穷时间
		 * 公式都放在Formula里面
		 * 
		 */
		private Star CalculateFuture(int planetId,int t)
		{
			
			Star star = new Star(_starCopys [planetId]);//复制一份 因为会变
			int nowTime = 0;

			//复制一份 因为会变
			List<SoldierOnTheWay> playerOnTheWay = new List<SoldierOnTheWay>(_playerOnTheWay);
			List<SoldierOnTheWay> aiOnTheWay = new List<SoldierOnTheWay> (_aiOnTheWay);
			//统计现在双方各有多少兵力是派往这个星球的
			//int playerTroopCountToPlanet = TroopCountToPlanet(planetId,playerOnTheWay);
			//int aiTroopCountToPlanet = TroopCountToPlanet(planetId,aiOnTheWay);

			while (true) 
			{
				
				//函数出口
				if(t != -1 && nowTime >= t) break;
				//当这个星球上只有AI的兵力且路上木有到这里来的玩家兵力 或 只有玩家兵力且路上木有到这里来的AI兵力 时结束 =。=
				if (t == -1 && ((Math.Floor (star.PlayerTroops) == 0 && playerOnTheWay.Count == 0)
					|| (Math.Floor (star.EnemyTroops) == 0 && aiOnTheWay.Count == 0)))
					break;


				//若双方在上面都有兵力 战斗过程
				if((Math.Floor (star.PlayerTroops) > 0) && (Math.Floor (star.EnemyTroops) > 0))
				{
					if (star.State == Star.e_State.AI) 
					{//这个星球是AI的
						float damageForAttackOnePerTime = Formula.CalculateDamageForAttackOnePerTime(star.EnemyTroops,star.PlayerTroops,star.DEF,1.0);
						float damageForDefOnePerTime = Formula.CalculateDamageForDefOnePerTime (star.EnemyTroops, star.PlayerTroops, star.DEF, 1.0);
						star.PlayerTroops += damageForAttackOnePerTime;
						star.EnemyTroops += damageForDefOnePerTime;
					} else if (star.State == Star.e_State.Player) 
					{//这个星球是玩家的
						float damageForAttackOnePerTime = Formula.CalculateDamageForAttackOnePerTime(star.PlayerTroops,star.EnemyTroops,star.DEF,1.0);
						float damageForDefOnePerTime = Formula.CalculateDamageForDefOnePerTime (star.PlayerTroops, star.EnemyTroops, star.DEF, 1.0);
						star.PlayerTroops += damageForDefOnePerTime;
						star.EnemyTroops += damageForAttackOnePerTime;
					}else
					{//这个星球是中立的
						float damageForPlayerOnePerTime = Formula.CalculateDamageForNeutralOnePerTime(star.PlayerTroops,star.EnemyTroops,1);
						float damageForEnemyOnePerTime = Formula.CalculateDamageForNeutralOnePerTime(star.EnemyTroops,star.PlayerTroops,1);
						star.PlayerTroops += damageForPlayerOnePerTime;
						star.EnemyTroops += damageForEnemyOnePerTime;
					}
				}


				//更新时间
				++nowTime;
				//更新各自的onTheWay
				playerOnTheWay.ForEach(async delegate(SoldierOnTheWay soldier) {
					--soldier.m_needTime;
					if (soldier.m_needTime < 0) 
					{
						playerOnTheWay.Remove (soldier);
						++star.PlayerTroops;
					}
				});
				aiOnTheWay.ForEach(async delegate(SoldierOnTheWay soldier) {
					--soldier.m_needTime;
					if (soldier.m_needTime < 0) 
					{
						playerOnTheWay.Remove (soldier);
						++star.EnemyTroops;
					}
				});
			}
		}
			
		//统计有多少赶往此星球的玩家或AI士兵 传入_playerOnTheWay和_aiOnTheWay
		int TroopCountToPlanet(int planetIndex,List<SoldierOnTheWay> soldiersOnTheWay)
		{
			int count = 0;
			foreach (SoldierOnTheWay soldierOnTheWay in soldiersOnTheWay) 
			{
				if (soldierOnTheWay.m_terminal == planetIndex) 
				{
					++count;
				}
			}
			return count;
		}

		/**
		 * 
		 * 选取目的地
		 * 获取攻击指数为正且最小的星球
		 * 若无目的地则为-1
		 */
		private int getDestination()
		{
			int minIndex = -1;
			float minValue = float.MaxValue;
			for (int index = 0; index < _starCopys.Count; ++index) 
			{
				float starAttackValue = getAttackValue (index);//星球重要程度
				if (starAttackValue > 0 && starAttackValue < minValue) 
				{
					minIndex = index;
					minValue = starAttackValue;
				}
			}

			return minIndex;
		}




		/**
		 * 
		 * AI相关计算公式
		 * 
		 * 
		 */


		/**
		 * 2 to 0.5递减的缓和公式
		 * 输入是AI和玩家某属性的比值
		 * 输出是属性的权值（也就是属性的重要程度）
		 * 此函数是为了使AI越在某个属性上占劣势，这个属性越重要
		 */
		float AttributeImportanceFormula(float attributeRate)
		{
			return (attributeRate + 2) / (2 * attributeRate + 1);
		}

		/**
		 * 0.5 to 1递增的缓和公式
		 * 输入是星球三个属性的加权求和
		 * 输出是星球的最终重要程度（没有算上地理位置和实际情况我很抱歉）
		 * 此函数是为了使最重要的星球也不会比最不重要的差别太大
		 */
		float StarImportanceFormula(float attributeSum)
		{
			return (2 * attributeSum + 1) / (2 * attributeSum + 2);
		}

		/**
		 * 过剩兵力系数公式
		 * 1 to 0.5递减的缓和公式
		 * 输入是星球重要程度
		 * 输出是过剩兵力系数
		 * 此函数是为了重要的星球留较多的人 不怎么重要的星球留较少的人
		 */
		float ExcessForceCoefficientFormula(float starImportance)
		{
			return 1.5 - starImportance;
		}

		//计算某星球过剩兵力
		private int CaculateExcessForceCoefficient(int starIndex)
		{
			//获取星球最后情况
			Star finalStar = CalculateFuture(starIndex,-1);
			return Math.Floor(finalStar.EnemyTroops - finalStar.PlayerTroops) //剩下的兵力
				* ExcessForceCoefficientFormula (StarImportance (starIndex)); //过剩兵力系数
		}

		//计算某星球的重要程度
		private float StarImportance(Star star)
		{
			//f(g(双方总容量比)*星球容量+g(双方总活力比)*星球活力+星球防御力)
			return StarImportanceFormula (AttributeImportanceFormula ((float)_aiTotalCapacity / _playerTotalCapacity) * star.Capacity
			+ AttributeImportanceFormula ((float)_aiTotalVigour / _playerTotalVigour) * star.Vigour
			+ star.DEF);
		}

		private float  StarImportance(int starIndex)
		{
			return StarImportance (_starCopys[starIndex]);
		}

		//获取某星球攻占难易程度
		private float getAttackDifficulty(int planetIndex)
		{
			return getAttackDifficulty (_starCopys[planetIndex]);
		}

		private float getAttackDifficulty(Star star)
		{
			//复制一份
			Star theStar = new Star (star);

			//获取离这个星球最远的上有AI士兵的星球
			int maxDistance = 0;
			int starIndex = _starCopys.FindIndex (star);
			for (int index = 0; index < _starCopys.Count; ++index) 
			{
				if (getDistance (theStar.Location, _starCopys [index].Location) > maxDistance && 
					_starCopys[index].EnemyTroops >= 1) 
				{
					maxDistance = getDistance (theStar.Location, _starCopys [index].Location);
				}
			}

			Star finalStar = CalculateFuture (starIndex, Math.Floor (maxDistance / v));

			return finalStar.PlayerTroops - finalStar.EnemyTroops; 

		}

		//获取星球攻击目标指数（决定进不进攻或是派不派兵支援）
		private float getAttackValue(int planetIndex)
		{
			return getAttackDifficulty (planetIndex) / StarImportance (planetIndex);
		}
		float getAttackValue(Star star)
		{
			return getAttackDifficulty (star) / StarImportance (star);
		}



		//两点之间的距离
		private float getDistance(Vector2 aLocation,Vector2 otherLocation)
		{
			return (aLocation - otherLocation).sqrMagnitude;
		}
	
	}




}

