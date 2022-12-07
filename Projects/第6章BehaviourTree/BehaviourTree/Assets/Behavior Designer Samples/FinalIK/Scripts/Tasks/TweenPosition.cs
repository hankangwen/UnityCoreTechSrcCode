using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Samples.FinalIK
{
    public class TweenPosition : Action
    {
        public float maxDeltaDistance;
        public SharedVector3 currentPosition;
        public SharedVector3 endPosition;
        
        public override TaskStatus OnUpdate()
        {
            if ((currentPosition.Value - endPosition.Value).sqrMagnitude < 0.01f) {
                return TaskStatus.Success;
            }

            currentPosition.Value = Vector3.MoveTowards(currentPosition.Value, endPosition.Value, maxDeltaDistance * Time.deltaTime);
            return TaskStatus.Running;
        }
    }
}