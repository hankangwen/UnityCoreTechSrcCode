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
    public class MarketRuneInfoCtrl : Singleton<MarketRuneInfoCtrl>
    {
        public void Enter(GameObject go)
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneBuyWindowEnter, go);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneBuyWindowExit);
        }

        public void BuyRune(int runeid, GameDefine.ConsumeType type, int num)
        {
            CGLCtrl_GameLogic.Instance.EMsgToGSToCSFromGC_AskBuyGoods(runeid, (int)type, num);
        }
    }
}
