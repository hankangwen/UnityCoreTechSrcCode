﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.17929
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.GameData;
using MO.MOBA.Tools;
namespace Game.FSM
{
    using Game.GameEntity;
    public class EntityReliveFSM : EntityFSM
    {
        public static readonly EntityFSM Instance = new EntityReliveFSM();
        public FsmState State
        {
            get
            {
                return FsmState.FSM_STATE_RELIVE;
            }
        }
        public bool CanNotStateChange
        {
            set;
            get;
        }

        public bool StateChange(Ientity entity, EntityFSM fsm)
        {
            return CanNotStateChange;
        }

        public void Enter(Ientity entity, float last)
        {
            entity.OnEnterRelive();
        }

        public void Execute(Ientity entity)
        {
        }

        public void Exit(Ientity entity)
        {
            entity.OnExitRelive();
        }
    }
}

