using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GameDefine;

using Game.Ctrl;

namespace Game.GuideDate
{
    public class GuideSendNpcTask : GuideTaskBase
    {
        public GuideSendNpcTask(int task, GuideTaskType type, GameObject mParent)
            : base(task, type, mParent)
        {

        }

        public override void EnterTask()
        {
            GamePlayGuideCtrl.Instance.AskSSGuideStepComp(GCToSS.AskSSGuideStepComp.edotype.ebornsolder , mTaskId);
        }

        public override void ExcuseTask()
        {

        }

        public override void ClearTask()
        {
            base.ClearTask();
        }

    }


}
