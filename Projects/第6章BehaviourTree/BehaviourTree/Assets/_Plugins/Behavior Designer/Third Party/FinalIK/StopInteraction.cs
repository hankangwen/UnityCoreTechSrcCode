using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Stops an interaction with the InteractionSystem")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class StopInteraction : InteractionAction {

		protected override void Action(InteractionSystem sys) {
			foreach (FullBodyBipedEffector effectorType in effectorTypes) {
				sys.StopInteraction(effectorType);
			}
		}
	}
}
