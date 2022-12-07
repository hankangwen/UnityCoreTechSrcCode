using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Pauses an interaction with the InteractionSystem")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class PauseInteraction : InteractionAction {

		protected override void Action(InteractionSystem sys) {
			foreach (FullBodyBipedEffector effectorType in effectorTypes) {
				sys.PauseInteraction(effectorType);
			}
		}
	}
}

