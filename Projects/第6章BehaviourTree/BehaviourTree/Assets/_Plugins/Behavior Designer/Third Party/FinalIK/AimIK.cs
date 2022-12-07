using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the AimIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class AimIK : IKAction {
		
		[System.Serializable]
		public class Solver {

			[Tooltip("The target Transform (optional, you can use just the position instead).")]
			public SharedGameObject target;

			[Tooltip("The pole target Transform (optional) - the position in world space to keep the pole axis of the Aim Transform directed at..")]
			public SharedGameObject poleTarget;

			[Tooltip("The transform that we want to aim at IKPosition.")]
			public SharedGameObject aimTransform;

			[Tooltip("The local axis of the Transform that you want to be aimed at IKPosition.")]
			public SharedVector3 axis;

			[Tooltip("Keeps that axis of the Aim Transform directed at the polePosition.")]
			public SharedVector3 poleAxis;
			
			[Tooltip("Solver weight for smooth blending.")]
			public SharedFloat weight;

			[Tooltip("The weight of the Pole.")]
			public SharedFloat poleWeight;
			
			[Tooltip("Set the position to a point in world space that you want AimIK to aim the AimTransform at. This has no effect if the weight is 0.")]
			public SharedVector3 position;
			
			[Tooltip("Clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to transform axis.")]
			public SharedFloat clampWeight;

			public void Reset() {
				target = null;
                poleTarget = null;
                aimTransform = null;
				axis = Vector3.zero;
                poleAxis = Vector3.zero;
                weight = 0;
                poleWeight = 0;
                position = Vector3.zero;
                clampWeight = 0;

				axis = Vector3.forward;
				poleAxis = Vector3.up;
				weight = 1f;
				poleWeight = 0f;
			}
			
			public void Update(IKSolverAim solver) {
				weight.Value = Mathf.Clamp(weight.Value, 0f, 1f);
				clampWeight.Value = Mathf.Clamp(clampWeight.Value, 0f, 1f);
				poleWeight.Value = Mathf.Clamp(poleWeight.Value, 0f, 1f);

				solver.target = target.Value.transform;
				solver.poleTarget = poleTarget.Value.transform;
				solver.transform = aimTransform.Value.transform;
				solver.axis = axis.Value;
				solver.poleAxis = poleAxis.Value;
				solver.IKPositionWeight = weight.Value;
				solver.poleWeight = poleWeight.Value;
				solver.IKPosition = position.Value;
				solver.clampWeight = clampWeight.Value;
			}
		}

		public Solver solver = new Solver();

		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
			solver.Update((component as RootMotion.FinalIK.AimIK).solver);
            return TaskStatus.Success;
		}

		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.AimIK);
		}
	}
}
