using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the LookAtIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class LookAtIK : IKAction {

		[System.Serializable]
		public class Solver {

			[Tooltip("The target Transform (optional, you can use just the position instead).")]
			public SharedGameObject target;

			[Tooltip("Solver weight for smooth blending.")]
			public SharedFloat weight;
			
			[Tooltip("The target position.")]
			public SharedVector3 position;
			
			[Tooltip("The weight multiplier for the spine bones.")]
			public SharedFloat bodyWeight;
			
			[Tooltip("The weight multiplier for the head bone.")]
			public SharedFloat headWeight;
			
			[Tooltip("The weight multiplier for the eyes")]
			public SharedFloat eyesWeight;
			
			[Tooltip("Clamping rotation of the spine bones. 0 is free rotation, 1 is completely clamped.")]
			public SharedFloat clampWeight = 0.1f;
			
			[Tooltip("Clamping rotation of the head bone. 0 is free rotation, 1 is completely clamped.")]
			public SharedFloat clampWeightHead = 0.1f;
			
			[Tooltip("Clamping rotation of the eyes. 0 is free rotation, 1 is completely clamped.")]
			public SharedFloat clampWeightEyes = 0.1f;
			
			public void Reset() {
				target = null;
				weight = 0;
				position = Vector3.zero;
				bodyWeight = 0;
				headWeight = 0;
				eyesWeight = 0;
				clampWeight = 0;
				clampWeightHead = 0;
				clampWeightEyes = 0;
				
				bodyWeight = 0.5f;
				headWeight = 0.5f;
				eyesWeight = 1f;
				clampWeight = 0.5f;
				clampWeightHead = 0.5f;
				clampWeightEyes = 0.5f;
			}
			
			public void Update(IKSolverLookAt solver) {
				weight.Value = Mathf.Clamp(weight.Value, 0f, 1f);
				bodyWeight.Value = Mathf.Clamp(bodyWeight.Value, 0f, 1f);
				headWeight.Value = Mathf.Clamp(headWeight.Value, 0f, 1f);
				eyesWeight.Value = Mathf.Clamp(eyesWeight.Value, 0f, 1f);
				clampWeight.Value = Mathf.Clamp(clampWeight.Value, 0f, 1f);
				clampWeightHead.Value = Mathf.Clamp(clampWeightHead.Value, 0f, 1f);
				clampWeightEyes.Value = Mathf.Clamp(clampWeightEyes.Value, 0f, 1f);

				solver.target = target.Value.transform;
				solver.IKPositionWeight = weight.Value;
				solver.IKPosition = position.Value;
				solver.bodyWeight = bodyWeight.Value;
				solver.headWeight = headWeight.Value;
				solver.eyesWeight = eyesWeight.Value;
				solver.clampWeight = clampWeight.Value;
				solver.clampWeightHead = clampWeightHead.Value;
				solver.clampWeightEyes = clampWeightEyes.Value;
			}
		}

		public Solver solver = new Solver();
		
		protected override void ResetAction() {
			solver.Reset();
		}
		
		protected override TaskStatus UpdateAction() {
            solver.Update((component as RootMotion.FinalIK.LookAtIK).solver);
            return TaskStatus.Success;
		}

		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.LookAtIK);
		}
	}
}
