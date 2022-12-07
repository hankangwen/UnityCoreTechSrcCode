using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Starts an interaction with the InteractionSystem")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
    [RequiredComponent(typeof(InteractionObject))]
	public class StartInteraction : InteractionAction {

		[RequiredField]
		[Tooltip("The character with the InteractionSystem component")]
		public SharedGameObject interactionObject;

		[Tooltip("Can this interaction interrupt an ongoing interaction?")]
		public SharedBool interrupt;

		public override void OnReset() {
			base.OnReset();

			interactionObject = null;
			interrupt = false;
		}

		protected override void Action(InteractionSystem sys) {
			var interactionGo = GetDefaultGameObject(interactionObject.Value);
			if (interactionGo == null) return;

			var obj = interactionGo.GetComponent<InteractionObject>();
			if (obj == null) {
				Debug.LogWarning("No InteractionObject component found on " + interactionGo.name);
				return;
			}

			foreach (FullBodyBipedEffector effectorType in effectorTypes) {
				sys.StartInteraction(effectorType, obj, interrupt.Value);
			}
		}
	}
}
