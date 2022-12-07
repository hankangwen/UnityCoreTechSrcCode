using UnityEngine;
using System.Collections.Generic;
using Game;
using MO.MOBA.GameData;
using Game.GameData;
using Game.Network;
using LSToGC;
using System.IO;
using System.Linq;

using Game.Model;

namespace Game.Ctrl
{
    public class RuneCombineCtrl : Singleton<RuneCombineCtrl>
    {
        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneCombineWindowEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneCombineWindowExit);
        }

        public RuneCombineCtrl()
        {
        }
    }
}
