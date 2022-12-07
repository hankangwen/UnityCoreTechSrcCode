using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages a FullBodyBipedIK limb. You can alternately use FBBIKEffector and FBBIKChain and FBBIKMapping.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FBBIKLimb : IKAction {

		[Tooltip("The FBBIK chain type.")]
		public FullBodyBipedChain limb;
		
		[Tooltip("When all chains have pull equal to 1, pull weight is distributed equally between the limbs. " +
		         "That means reaching all effectors is not quaranteed if they are very far from each other. " +
		         "However, when for instance the left arm chain has pull weight equal to 1 and all others have 0, you can pull the character from it's left hand to Infinity without losing contact.")]
		public SharedFloat pull;
		
		[Tooltip("Increasing this value will make the limb pull the body closer to the target.")]
		public SharedFloat reach;

		[Tooltip("The weight of the end-effector pushing the first node.")]
		public SharedFloat push;
		
		[Tooltip("The amount of push force transferred to the parent (from hand or foot to the body).")]
		public SharedFloat pushParent;
		
		[Tooltip("Smoothing the effect of the reach with the expense of some accuracy.")]
		public FBIKChain.Smoothing reachSmoothing;

		[Tooltip("Smoothing the effect of the Push.")]
		public FBIKChain.Smoothing pushSmoothing;

		[Tooltip("The bend goal GameObject. The limb will be bent in the direction towards this GameObject.")]
		public SharedGameObject bendGoal;

		[Tooltip("The weight of bending the limb towards the Bend Goal.")]
		public SharedFloat bendGoalWeight;

		[Tooltip("The target Transform (optional, you can use just the position and rotation instead).")]
		public SharedGameObject target;

		[Tooltip("Set the effector position to a point in world space. This has no effect if the effector's positionWeight is 0.")]
		public SharedVector3 position;
		
		[Tooltip("The effector position weight.")]
		public SharedFloat positionWeight;
		
		[Tooltip("The effector rotation, this only an effect with limb end-effectors (hands and feet).")]
		public SharedQuaternion rotation;
		
		[Tooltip("Weighing in the effector rotation, this only an effect with limb end-effectors (hands and feet).")]
		public SharedFloat rotationWeight;
		
		[Tooltip("Offsets the hand from it's animated position. If effector positionWeight is 1, this has no effect. " +
		         "Note that the effectors will reset their positionOffset to Vector3.zero after each update, so you can (and should) use them additively. " +
		         "This enables you to easily edit the value by more than one script.")]
		public SharedVector3 positionOffset;
		
		[Tooltip("Keeps the node position relative to the triangle defined by the plane bones (applies only to end-effectors).")]
		public SharedFloat maintainRelativePositionWeight;

		[Tooltip("The target Transform (optional, you can use just the position and rotation instead).")]
		public SharedGameObject startEffectorTarget;

		[Tooltip("Set the effector position to a point in world space. This has no effect if the effector's positionWeight is 0.")]
		public SharedVector3 startEffectorPosition;
		
		[Tooltip("The effector position weight.")]
		public SharedFloat startEffectorPositionWeight;
		
		[Tooltip("Offsets the hand from it's animated position. If effector positionWeight is 1, this has no effect. " +
		         "Note that the effectors will reset their positionOffset to Vector3.zero after each update, so you can (and should) use them additively. " +
		         "This enables you to easily edit the value by more than one script.")]
		public SharedVector3 startEffectorPositionOffset;

		[Tooltip("The slerp weight of rotating the limb to it's IK pose. This can be useful if you want to disable the effect of IK for the limb or move the hand to the target in a sperical trajectory instead of linear.")]
		public SharedFloat mappingWeight;

		[Tooltip("The weight of maintaining the bone's animated rotation in world space.")]
		public SharedFloat maintainRotationWeight;

		protected override void ResetAction() {
			limb = FullBodyBipedChain.LeftArm;
			pull = 0;
			reach = 0;
			push = 0;
			pushParent = 0;
			bendGoal = null;
			bendGoalWeight = 0;

			target = null;
			position = Vector3.zero;
			positionWeight = 0;
			rotation = Quaternion.identity;
			rotationWeight = 0;
			positionOffset = Vector3.zero;
			maintainRelativePositionWeight = 0;

			startEffectorTarget = null;
			startEffectorPosition = Vector3.zero;
			startEffectorPositionWeight = 0;
			startEffectorPositionOffset = Vector3.zero;

			maintainRotationWeight = 0;
			mappingWeight = 0;
			reachSmoothing = FBIKChain.Smoothing.Exponential;
			pushSmoothing = FBIKChain.Smoothing.Exponential;

			rotation = Quaternion.identity;
			mappingWeight = 1f;
			pull = 1f;
			reach = 0.05f;
			push = 0f;
			pushParent = 0f;
			bendGoalWeight = 0f;
		}

		protected override TaskStatus UpdateAction() {
			var solver = (component as RootMotion.FinalIK.FullBodyBipedIK).solver;
			
			var effector = solver.GetEndEffector(limb);
			var chain = solver.GetChain(limb);
			var mapping = solver.GetLimbMapping(limb);
			var startEffector = solver.GetEffector(GetStartEffector(limb));

			pull.Value = Mathf.Clamp(pull.Value, 0f, 1f);
			reach.Value = Mathf.Clamp(reach.Value, 0f, 1f);
			push.Value = Mathf.Clamp(push.Value, 0f, 1f);
			pushParent.Value = Mathf.Clamp(pushParent.Value, -1f, 1f);

			positionWeight.Value = Mathf.Clamp(positionWeight.Value, 0f, 1f);
			rotationWeight.Value = Mathf.Clamp(rotationWeight.Value, 0f, 1f);
			maintainRelativePositionWeight.Value = Mathf.Clamp(maintainRelativePositionWeight.Value, 0f, 1f);

			startEffectorPositionWeight.Value = Mathf.Clamp(startEffectorPositionWeight.Value, 0f, 1f);

			maintainRotationWeight.Value = Mathf.Clamp(maintainRotationWeight.Value, 0f, 1f);
			mappingWeight.Value = Mathf.Clamp(mappingWeight.Value, 0f, 1f);

			chain.pull = pull.Value;
			chain.reach = reach.Value;
			chain.push = push.Value;
			chain.pushParent = pushParent.Value;
			chain.reachSmoothing = reachSmoothing;
			chain.pushSmoothing = pushSmoothing;
            if (bendGoal.Value != null) {
                chain.bendConstraint.bendGoal = bendGoal.Value.transform;
                chain.bendConstraint.weight = bendGoalWeight.Value;
            }

            if (target.Value != null) {
                effector.target = target.Value.transform;
            }
			effector.position = position.Value;
			effector.positionWeight = positionWeight.Value;
			effector.rotation = rotation.Value;
			effector.rotationWeight = rotationWeight.Value;
			effector.positionOffset = positionOffset.Value;
			effector.maintainRelativePositionWeight = maintainRelativePositionWeight.Value;

            if (startEffectorTarget.Value != null) {
                startEffector.target = startEffectorTarget.Value.transform;
                startEffector.position = startEffectorPosition.Value;
                startEffector.positionWeight = startEffectorPositionWeight.Value;
                startEffector.positionOffset = startEffectorPositionOffset.Value;
            }

			mapping.maintainRotationWeight = maintainRotationWeight.Value;
            mapping.weight = mappingWeight.Value;
            return TaskStatus.Success;
		}

		private static FullBodyBipedEffector GetStartEffector(FullBodyBipedChain chain) {
			switch(chain) {
			case FullBodyBipedChain.LeftArm: return FullBodyBipedEffector.LeftShoulder;
			case FullBodyBipedChain.RightArm: return FullBodyBipedEffector.RightShoulder;
			case FullBodyBipedChain.LeftLeg: return FullBodyBipedEffector.LeftThigh;
			default: return FullBodyBipedEffector.RightThigh;
			}
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FullBodyBipedIK);
		}
	}
}