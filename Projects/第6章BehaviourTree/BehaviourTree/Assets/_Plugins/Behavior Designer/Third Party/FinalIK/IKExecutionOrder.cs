using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	[TaskCategory("Final IK")]
    [TaskDescription("Controls the updating order of IK components")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=153")]
    [TaskIcon("FinalIKIcon.png")]
    public class IKExecutionOrder : Action
    {
		[Tooltip("Update order of the IK components")]
		public IK[] IKComponents = new IK[0];
		
		public override void OnReset() {
			IKComponents = new IK[0];
		}
		
		public override void OnStart() {
			foreach (IK ik in IKComponents) ik.Disable();
		}
		
		public override TaskStatus OnUpdate() {
			Action();
            return TaskStatus.Success;
		}
		
		private void Action() {
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}
	}
}

