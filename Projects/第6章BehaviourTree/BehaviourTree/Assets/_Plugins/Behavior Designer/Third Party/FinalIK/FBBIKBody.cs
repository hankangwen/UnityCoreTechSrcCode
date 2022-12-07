using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages a FullBodyBipedIK effector")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FBBIKBody : IKAction {

		[Tooltip("The target Transform (optional, you can use just the position instead).")]
		public SharedGameObject target;

		[Tooltip("Set the effector position to a point in world space. This has no effect if the effector's positionWeight is 0.")]
		public SharedVector3 position;
		
		[Tooltip("The effector position weight.")]
		public SharedFloat positionWeight;
		
		[Tooltip("Offsets the hand from it's animated position. If effector positionWeight is 1, this has no effect. " +
		         "Note that the effectors will reset their positionOffset to Vector3.zero after each update, so you can (and should) use them additively. " +
		         "This enables you to easily edit the value by more than one script.")]
		public SharedVector3 positionOffset;

		[Tooltip("If false, child nodes will be ignored by this effector.")]
		public bool useThighs;

		[Tooltip("The bend resistance of the spine.")]
		public SharedFloat spineStiffness;
		
		[Tooltip("Weight of hand effectors pulling the body vertically.")]
		public SharedFloat pullBodyVertical;
		
		[Tooltip("Weight of hand effectors pulling the body horizontally.")]
		public SharedFloat pullBodyHorizontal;

		[Tooltip("Spine mapping FABRIK iteration count.")]
		public SharedInt spineMappingIterations;

		[Tooltip("Weight of twisting the spine bones to the chest triangle.")]
		public SharedFloat spineTwistWeight;

		[Tooltip("The weight of maintaining the bone's animated rotation in world space.")]
		public SharedFloat maintainHeadRotation;

		protected override void ResetAction() {
			position = Vector3.zero;
			positionWeight = 0;
			positionOffset = Vector3.zero;
			spineMappingIterations = 0;
			spineStiffness = 0;
			pullBodyVertical = 0;
			pullBodyHorizontal = 0;
			maintainHeadRotation = 0;
			spineTwistWeight = 0;
			target = null;

			useThighs = true;
			spineMappingIterations = 3;
			spineStiffness = 0.5f;
			pullBodyHorizontal = 0f;
			pullBodyVertical = 0.5f;
			spineTwistWeight = 1f;
		}
		
		protected override TaskStatus UpdateAction() {
			var solver = (component as RootMotion.FinalIK.FullBodyBipedIK).solver;
			
			positionWeight.Value = Mathf.Clamp(positionWeight.Value, 0f, 1f);
			spineMappingIterations.Value = Mathf.Clamp(spineMappingIterations.Value, 1, int.MaxValue);
			spineStiffness.Value = Mathf.Clamp(spineStiffness.Value, 0f, 1f);
			pullBodyVertical.Value = Mathf.Clamp(pullBodyVertical.Value, 0f, 1f);
			pullBodyHorizontal.Value = Mathf.Clamp(pullBodyHorizontal.Value, 0f, 1f);
			maintainHeadRotation.Value = Mathf.Clamp(maintainHeadRotation.Value, 0f, 1f);
			spineTwistWeight.Value = Mathf.Clamp(spineTwistWeight.Value, 0f, 1f);

			solver.bodyEffector.target = target.Value.transform;
			solver.bodyEffector.position = position.Value;
			solver.bodyEffector.positionWeight = positionWeight.Value;
			solver.bodyEffector.positionOffset = positionOffset.Value;
			solver.bodyEffector.effectChildNodes = useThighs;
			solver.spineMapping.iterations = spineMappingIterations.Value;
			solver.spineStiffness = spineStiffness.Value;
			solver.pullBodyVertical = pullBodyVertical.Value;
			solver.pullBodyHorizontal = pullBodyHorizontal.Value;
			solver.spineMapping.twistWeight = spineTwistWeight.Value;

            solver.boneMappings[0].maintainRotationWeight = maintainHeadRotation.Value;
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FullBodyBipedIK);
		}
	}
}
