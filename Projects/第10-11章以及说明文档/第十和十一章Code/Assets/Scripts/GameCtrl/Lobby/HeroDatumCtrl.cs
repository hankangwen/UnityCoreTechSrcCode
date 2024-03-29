﻿using UnityEngine;
using System.Collections;

namespace Game.Ctrl
{
    public class HeroDatumCtrl : Singleton<HeroDatumCtrl>
    {
        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_HeroDatumEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_HeroDatumExit);
        }
    }
}
