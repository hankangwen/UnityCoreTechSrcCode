using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the TrigonometricIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class TrigonometricIK : IKAction {

		[System.Serializable]
		public class Solver {

			[Tooltip("The target Transform (optional, you can use just the position and rotation instead).")]
			public SharedGameObject target;

			[Tooltip("Position weight for smooth blending.")]
			public SharedFloat positionWeight;
			
			[Tooltip("The target position.")]
			public SharedVector3 position;
			
			[Tooltip("Rotation weight for smooth blending.")]
			public SharedFloat rotationWeight;
			
			[Tooltip("The target rotation.")]
			public SharedQuaternion rotation;
			
			[Tooltip("The bend plane normal.")]
			public SharedVector3 bendNormal;
			
			public void Reset() {
				target = null;
				positionWeight = 0;
				position = Vector3.zero;
				rotationWeight = 0;
				rotation = Quaternion.identity;
				bendNormal = Vector3.zero;

				bendNormal = Vector3.right;
				rotation = Quaternion.identity;
			}
			
			public void Update(IKSolverTrigonometric solver) {
				positionWeight.Value = Mathf.Clamp(positionWeight.Value, 0f, 1f);
				rotationWeight.Value = Mathf.Clamp(rotationWeight.Value, 0f, 1f);

				solver.target = target.Value.transform;
				solver.IKPositionWeight = positionWeight.Value;
				solver.IKPosition = position.Value;
				solver.IKRotationWeight = rotationWeight.Value;
				solver.IKRotation = rotation.Value;
				solver.bendNormal = bendNormal.Value.normalized;
			}
		}
		
		public Solver solver = new Solver();
		
		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
            solver.Update((component as RootMotion.FinalIK.TrigonometricIK).solver);
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.TrigonometricIK);
		}
		
	}
}

