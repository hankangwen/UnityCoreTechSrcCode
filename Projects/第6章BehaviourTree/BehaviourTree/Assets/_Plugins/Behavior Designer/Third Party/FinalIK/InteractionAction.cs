using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
    // The base abstract class for all Interaction System related actions
    [RequiredComponent(typeof(InteractionSystem))]
    [RequiredComponent(typeof(FullBodyBipedIK))]
	public abstract class InteractionAction : Action {

		protected abstract void Action(InteractionSystem sys);

		[RequiredField]
		[Tooltip("The character with the InteractionSystem component")]
		public SharedGameObject interactionSystem;
		
		[Tooltip("The effector(s) to use for the interaction")]
		public FullBodyBipedEffector[] effectorTypes;
		
		public override void OnReset() {
			interactionSystem = null;
			effectorTypes = new FullBodyBipedEffector[0];
		}
		
		public override void OnAwake() {
            var go = GetDefaultGameObject(interactionSystem.Value);
			if (go == null) return;
			
			var sys = go.GetComponent<InteractionSystem>();
			if (sys == null) {
				Debug.LogWarning("No InteractionSystem component found on " + go.name);
				return;
			}

			Action(sys);
		}
	}
}

