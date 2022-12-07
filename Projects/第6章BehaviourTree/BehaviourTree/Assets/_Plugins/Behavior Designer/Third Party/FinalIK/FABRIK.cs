using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the FABRIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FABRIK : IKAction {

		[System.Serializable]
		public class Solver {

			[Tooltip("The target Transform (optional, you can use just the position instead).")]
			public SharedGameObject target;

			[Tooltip("Solver weight for smooth blending.")]
			public SharedFloat weight;
			
			[Tooltip("The target position.")]
			public SharedVector3 position;
			
			public void Reset() {
				target = null;
				weight = 0;
				position = Vector3.zero;
			}
			
			public void Update(IKSolverFABRIK solver) {
				weight.Value = Mathf.Clamp(weight.Value, 0f, 1f);

				solver.target = target.Value.transform;
				solver.IKPositionWeight = weight.Value;
				solver.IKPosition = position.Value;
			}
		}
		
		public Solver solver = new Solver();
		
		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
            solver.Update((component as RootMotion.FinalIK.FABRIK).solver);
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FABRIK);
		}
	}
}
