using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Manages a chain of a FABRIKRoot component.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
	public class FABRIKRootChain : IKAction {

		[Tooltip("The index of the chain in FABRIKRoot chains")]
		public int chainIndex;

		[Tooltip("Parent pulling weight.")]
		public SharedFloat pull;
			
		[Tooltip("Resistance to being pulled by child chains.")]
		public SharedFloat pin;
	
		protected override void ResetAction() {
			pull = 0;
			pin = 0;
		}
			
		protected override TaskStatus UpdateAction() {
			pull.Value = Mathf.Clamp(pull.Value, 0f, 1f);
			pin.Value = Mathf.Clamp(pin.Value, 0f, 1f);

			var solver = (component as RootMotion.FinalIK.FABRIKRoot).solver;

			if (chainIndex < 0 || chainIndex >= solver.chains.Length) {
				Debug.LogWarning("Invalid chainindex.");
				return TaskStatus.Failure;
			}

			var chain = solver.chains[chainIndex];

			chain.pull = pull.Value;
            chain.pin = pin.Value;
            return TaskStatus.Success;
		}
		
		protected override System.Type GetComponentType() {
			return typeof(RootMotion.FinalIK.FABRIKRoot);
		}
	}
}

