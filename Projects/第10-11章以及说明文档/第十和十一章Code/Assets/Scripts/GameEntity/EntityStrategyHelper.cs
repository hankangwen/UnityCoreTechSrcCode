using GameDefine;
using UnityEngine;
using MO.MOBA.Tools;

using Game.GameEntity;
using System.Collections;

namespace Game.GameEntity{
	public static class EntityStrategyHelper
	{
		public const float MoveCheckBlockTick = 1.0f;
        public const float IdleTimeTick = 10.0f;

		public static bool IsTick(Ientity self , float tick){
			if (Time.time - self.StrategyTick >= tick) {
				self.StrategyTick = Time.time;
				return true;	
			}
			return false;
		}
		
	}
}
