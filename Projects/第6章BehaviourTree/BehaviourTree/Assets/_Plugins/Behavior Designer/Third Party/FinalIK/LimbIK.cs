using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the LimbIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class LimbIK : IKAction {

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
			
			[Tooltip("Weight of maintaining the rotation of the third bone as it was in the animation")]
			public SharedFloat maintainRotationWeight;
			
			[Tooltip("The bend plane normal.")]
			public SharedVector3 bendNormal;
			
			[Tooltip("Weight of bend normal modifier.")]
			public SharedFloat bendModifierWeight;

			public void Reset() {
				target = null;
				positionWeight = 0;
				position = Vector3.zero;
				rotationWeight = 0;
				rotation = Quaternion.identity;
				maintainRotationWeight = 0;
				bendNormal = Vector3.zero;
				bendModifierWeight = 0;
				
				bendNormal = Vector3.right;
				rotation = Quaternion.identity;
			}
			
			public void Update(IKSolverLimb solver) {
				positionWeight.Value = Mathf.Clamp(positionWeight.Value, 0f, 1f);
				rotationWeight.Value = Mathf.Clamp(rotationWeight.Value, 0f, 1f);
				maintainRotationWeight.Value = Mathf.Clamp(maintainRotationWeight.Value, 0f, 1f);
				bendModifierWeight.Value = Mathf.Clamp(bendModifierWeight.Value, 0f, 1f);

				solver.target = target.Value.transform;
				solver.IKPositionWeight = positionWeight.Value;
				solver.IKPosition = position.Value;
				solver.IKRotationWeight = rotationWeight.Value;
				solver.IKRotation = rotation.Value;
				solver.maintainRotationWeight = maintainRotationWeight.Value;
				solver.bendNormal = bendNormal.Value.normalized;
				solver.bendModifierWeight = bendModifierWeight.Value;
			}
		}

		public Solver solver = new Solver();
		
		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
            solver.Update((component as RootMotion.FinalIK.LimbIK).solver);
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.LimbIK);
		}
	}
}
