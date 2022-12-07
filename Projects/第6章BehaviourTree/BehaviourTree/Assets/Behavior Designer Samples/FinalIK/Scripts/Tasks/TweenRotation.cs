using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Samples.FinalIK
{
    public class TweenRotation : Action
    {
        public float maxDeltaDegrees;
        public SharedVector3 currentRotation;
        public SharedVector3 endRotation;

        private Quaternion endQuaternion;

        public override void OnStart()
        {
            endQuaternion = Quaternion.Euler(endRotation.Value);
        }
        
        public override TaskStatus OnUpdate()
        {
            var currentQuaternion = Quaternion.Euler(currentRotation.Value);
            if (Quaternion.Angle(currentQuaternion, endQuaternion) < 0.1f) {
                return TaskStatus.Success;
            }

            currentRotation.Value = Quaternion.RotateTowards(Quaternion.Euler(currentRotation.Value), endQuaternion, maxDeltaDegrees).eulerAngles;
            return TaskStatus.Running;
        }
    }
}