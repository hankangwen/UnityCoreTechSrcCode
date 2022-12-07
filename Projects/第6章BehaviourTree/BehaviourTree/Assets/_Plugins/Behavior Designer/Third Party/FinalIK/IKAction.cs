using UnityEngine;
using RootMotion.FinalIK;

namespace BehaviorDesigner.Runtime.Tasks.FinalIK
{
	// The base abstract class for IK PlayMaker actions
	public abstract class IKAction : Action {

		[RequiredField]
		[Tooltip("The IK gameobject.")]
		public SharedGameObject targetGameObject;

		protected Component component {
			get {
                go = GetDefaultGameObject(targetGameObject.Value);
				if (go == null) return null; // This should not happen, but just in case

				// If gameobject has been switched out, need to find new component
				if (go != lastGo) _component = null;
				lastGo = go;

				if (_component == null) _component = go.GetComponent(GetComponentType());
				if (_component == null) {
					var componentType = GetComponentType().ToString();
					Debug.LogWarning("Component of type " + componentType + " was not found on " + go.name + ". Can't apply Behavior Designer action.");
					return null;
				}
				return _component;
			}
		}
		private Component _component;
		private GameObject go;
		private GameObject lastGo;

		protected virtual void ResetAction() {} // Component might be missing
		protected abstract TaskStatus UpdateAction(); // Component guaranteed
		protected abstract System.Type GetComponentType();
		
		public override void OnReset() {
			gameObject = null;

			ResetAction();
		}
		
		public override TaskStatus OnUpdate() {
			return UpdateActionSafe();
		}

		private TaskStatus UpdateActionSafe() {
			if (component == null) return TaskStatus.Failure;
			
			return UpdateAction();
		}
	}
}
