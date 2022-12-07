using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3)
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
#endif

namespace BehaviorDesigner.Samples
{
    [TaskCategory("RTS")]
    [TaskDescription("Conditional task which determines if the limited resouce is occupied")]
    public class IsResourceAvailable : Conditional
    {
        [Tooltip("The resource that we are interested in")]
        public LimitedResource resource;

        // return success if the resource is empty and failure if it is not
        public override TaskStatus OnUpdate()
        {
            if (resource.OccupiedBy == null) {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}