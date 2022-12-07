using UnityEngine;
using System.Collections;

using MO.MOBA.Tools;
using System;

namespace Game.GameEntity
{
    public class INpc : Ientity
    {

        public INpc(UInt64 sGUID, EntityCampType campType)
            : base(sGUID, campType)
        { 

        }

    }
}
