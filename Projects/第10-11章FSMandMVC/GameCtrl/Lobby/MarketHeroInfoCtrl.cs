using UnityEngine;
using System.Collections;
using Game;
using MO.MOBA.GameData;
using Game.GameData;
using Game.Network;
using LSToGC;
using System.IO;
using System.Linq;
using System;
using Game.Model;

namespace Game.Ctrl
{
    public class MarketHeroInfoCtrl : Singleton<MarketHeroInfoCtrl>
    {
        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_MarketHeroInfoEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_MarketHeroInfoExit);
        }

        public class HeroGoodsInfo
        {
            public enum CostType
            {
                GoldType = 0,
                DiamondType,
            }
           public int mGoodType;
           public int mGoodId;
           public CostType mCostType;
           public int mCost;
           public bool mIsDiscount;
        }
    }
}
