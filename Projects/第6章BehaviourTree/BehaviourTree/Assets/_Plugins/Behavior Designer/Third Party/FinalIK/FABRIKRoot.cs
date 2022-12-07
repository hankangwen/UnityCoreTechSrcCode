using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the FABRIKRoot component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FABRIKRoot : IKAction {
		
		[System.Serializable]
		public class Solver {
			
			[Tooltip("Solver weight for smooth blending.")]
			public SharedFloat weight;

			[Tooltip("Clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to transform axis.")]
			public SharedFloat rootPin;
			
			public void Reset() {
				weight = 0;
				rootPin = 0;
			}
			
			public void Update(IKSolverFABRIKRoot solver) {
				weight.Value = Mathf.Clamp(weight.Value, 0f, 1f);
				rootPin.Value = Mathf.Clamp(rootPin.Value, 0f, 1f);
				
				solver.IKPositionWeight = weight.Value;
				solver.rootPin = rootPin.Value;
			}
		}
		
		public Solver solver = new Solver();
		
		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
            solver.Update((component as RootMotion.FinalIK.FABRIKRoot).solver);
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FABRIKRoot);
		}
	}
}

