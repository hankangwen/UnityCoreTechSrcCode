using UnityEngine;
using System.Collections;
using Game;
using MO.MOBA.GameData;
using Game.GameData;
using Game.Network;
using LSToGC;
using System.IO;
using System.Linq;

namespace Game.Ctrl
{
    public class MarketCtrl : Singleton<MarketCtrl>
    {
        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_MarketEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_MarketExit);
        }

    }
}
