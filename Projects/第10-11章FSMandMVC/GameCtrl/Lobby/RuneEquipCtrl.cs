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
    public class RuneEquipCtrl : Singleton<RuneEquipCtrl>
    {
        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneEquipWindowEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_RuneEquipWindowExit);
        }

        public RuneEquipCtrl()
        {
        }

        public void UnloadRune(int page, int pos)
        {
            RuneEquipModel.Instance.RemoveRune(page, pos);
        }
    }
}
