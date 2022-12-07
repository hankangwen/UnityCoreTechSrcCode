using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages the BipedIK component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class BipedIK : IKAction {

        public BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver leftFoot = new BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver rightFoot = new BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver leftHand = new BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver rightHand = new BehaviorDesigner.Runtime.Tasks.FinalIK.LimbIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.FABRIK.Solver spine = new BehaviorDesigner.Runtime.Tasks.FinalIK.FABRIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.AimIK.Solver aim = new BehaviorDesigner.Runtime.Tasks.FinalIK.AimIK.Solver();
        public BehaviorDesigner.Runtime.Tasks.FinalIK.LookAtIK.Solver lookAt = new BehaviorDesigner.Runtime.Tasks.FinalIK.LookAtIK.Solver();

		[Tooltip("Pelvis offset from animation. If there is no animation playing and Fix Transforms is unchecked, it will make the character fly away.")]
		public SharedVector3 pelvisPositionOffset;
		
		[Tooltip("Weight of lerping the pelvis to pelvisPosition")]
		public SharedFloat pelvisPositionWeight;
		
		[Tooltip("The position to lerp the pelvis to by pelvisPositionWeight.")]
		public SharedVector3 pelvisPosition;

		[Tooltip("Pelvis rotation offset from animation. If there is no animation playing and Fix Transforms is unchecked, it will make the character spin.")]
		public SharedVector3 pelvisRotationOffset;
		
		[Tooltip("Weight of slerping the pelvis to pelvisRotation")]
		public SharedFloat pelvisRotationWeight;
		
		[Tooltip("The rotation to lerp the pelvis to by pelvisRotationWeight")]
		public SharedVector3 pelvisRotation;
		
		protected override void ResetAction() {
			leftFoot.Reset();
			rightFoot.Reset();
			leftHand.Reset();
			rightHand.Reset();
			spine.Reset();
			aim.Reset();
			lookAt.Reset();

			pelvisPositionOffset = Vector3.zero;
			pelvisPositionWeight = 0;
			pelvisPosition = Vector3.zero;
			pelvisRotationOffset = Vector3.zero;
			pelvisRotationWeight = 0;
			pelvisRotation = Vector3.zero;

			if (component != null) {
				var solvers = (component as RootMotion.FinalIK.BipedIK).solvers;
				pelvisPosition = solvers.pelvis.position;
				pelvisRotation = solvers.pelvis.rotation;
			}
		}
		
		protected override TaskStatus UpdateAction() {
			var solvers = (component as RootMotion.FinalIK.BipedIK).solvers;

			leftFoot.Update(solvers.leftFoot);
			rightFoot.Update(solvers.rightFoot);
			leftHand.Update(solvers.leftHand);
			rightHand.Update(solvers.rightHand);
			spine.Update(solvers.spine);
			aim.Update(solvers.aim);
			lookAt.Update(solvers.lookAt);

			pelvisPositionWeight.Value = Mathf.Clamp(pelvisPositionWeight.Value, 0f, 1f);
			pelvisRotationWeight.Value = Mathf.Clamp(pelvisRotationWeight.Value, 0f, 1f);

			solvers.pelvis.positionOffset = pelvisPositionOffset.Value;
			solvers.pelvis.positionWeight = pelvisPositionWeight.Value;
			solvers.pelvis.position = pelvisPosition.Value;
			solvers.pelvis.rotationOffset = pelvisRotationOffset.Value;
			solvers.pelvis.rotationWeight = pelvisRotationWeight.Value;
            solvers.pelvis.rotation = pelvisRotation.Value;
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.BipedIK);
		}
	}
}
