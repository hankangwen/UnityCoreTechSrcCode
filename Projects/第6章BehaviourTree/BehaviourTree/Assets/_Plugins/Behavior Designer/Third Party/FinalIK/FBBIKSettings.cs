using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the general settings of a FullBodyBipedIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FBBIKSettings : IKAction {
		
		[Tooltip("Solver weight for smooth blending.")]
		public SharedFloat weight;
		
		[Tooltip("Solver iteration count.")]
		public SharedInt iterations;
		
		protected override void ResetAction() {
			weight = 0;
			iterations = 0;

			iterations = 4;
		}
		
		protected override TaskStatus UpdateAction() {
			var solver = (component as RootMotion.FinalIK.FullBodyBipedIK).solver;
			
			weight.Value = Mathf.Clamp(weight.Value, 0f, 1f);
			iterations.Value = Mathf.Clamp(iterations.Value, 0, 10);

			solver.IKPositionWeight = weight.Value;
            solver.iterations = iterations.Value;
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FullBodyBipedIK);
		}
	}
}

