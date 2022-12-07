using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public abstract class ExternalBehavior : ScriptableObject, IBehavior
    {
        [SerializeField]
        private BehaviorSource mBehaviorSource;
        public BehaviorSource BehaviorSource { get { return mBehaviorSource; } set { mBehaviorSource = value; } }
        public BehaviorSource GetBehaviorSource() { return mBehaviorSource; }
        public void SetBehaviorSource(BehaviorSource behaviorSource) { mBehaviorSource = behaviorSource; }
        public Object GetObject() { return this; }
        public string GetOwnerName() { return "External Behavior"; }
        
        // Support blackboard variables:
        public SharedVariable GetVariable(string name)
        {
            mBehaviorSource.CheckForSerialization(false);
            return mBehaviorSource.GetVariable(name);
        }

        public void SetVariable(string name, SharedVariable item)
        {
            mBehaviorSource.CheckForSerialization(false);
            mBehaviorSource.SetVariable(name, item);
        }
    }
}