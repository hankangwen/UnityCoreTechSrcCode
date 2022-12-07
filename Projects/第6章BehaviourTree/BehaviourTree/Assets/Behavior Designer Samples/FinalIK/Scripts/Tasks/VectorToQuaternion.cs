using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Samples.FinalIK
{
    public class VectorToQuaternion : Action
    {
        public SharedVector3 vector;
        public SharedQuaternion quaternion;
        
        public override TaskStatus OnUpdate()
        {
            quaternion.Value = Quaternion.Euler(vector.Value);
            return TaskStatus.Success;
        }
    }
}