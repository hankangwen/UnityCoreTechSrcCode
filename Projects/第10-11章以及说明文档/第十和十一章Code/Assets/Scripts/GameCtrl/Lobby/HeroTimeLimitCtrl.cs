﻿using UnityEngine;
using System.Collections;

namespace Game.Ctrl
{
    public class HeroTimeLimitCtrl : Singleton<HeroTimeLimitCtrl>
    {

        public void Enter()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_HeroTimeLimitEnter);
        }

        public void Exit()
        {
            EventCenter.Broadcast(EGameEvent.eGameEvent_HeroTimeLimitExit);
        }
    }
}