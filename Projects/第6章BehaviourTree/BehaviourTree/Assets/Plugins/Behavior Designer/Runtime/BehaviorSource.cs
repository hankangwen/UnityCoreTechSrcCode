using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class BehaviorSource : IVariableSource
    {
        public string behaviorName = "Behavior";
        public string behaviorDescription = "";

        private int behaviorID = -1;
        public int BehaviorID { get { return behaviorID; } set { behaviorID = value; } }

        [SerializeField]
        private Task mEntryTask = null;
        public Task EntryTask { get { return mEntryTask; } set { mEntryTask = value; } }
        [SerializeField]
        private Task mRootTask = null;
        public Task RootTask { get { return mRootTask; } set { mRootTask = value; } }
        [SerializeField]
        private List<Task> mDetachedTasks = null;
        public List<Task> DetachedTasks { get { return mDetachedTasks; } set { mDetachedTasks = value; } }
        [SerializeField]
        private List<SharedVariable> mVariables = null;
        public List<SharedVariable> Variables { get { return mVariables; } set { mVariables = value; UpdateVariablesIndex(); } }
        private Dictionary<string, int> mSharedVariableIndex;
        [System.NonSerialized]
        private bool mHasSerialized = false;
        public bool HasSerialized { get { return mHasSerialized; } set { mHasSerialized = value; } }

        [SerializeField]
        private TaskSerializationData mTaskData;
        public TaskSerializationData TaskData { get { return mTaskData; } set { mTaskData = value; } }

        [SerializeField]
        private IBehavior mOwner;
        public IBehavior Owner { get { return mOwner; } set { mOwner = value; } }

        public BehaviorSource(IBehavior owner)
        {
            mOwner = owner;
        }

        public void Save(Task entryTask, Task rootTask, List<Task> detachedTasks)
        {
            mEntryTask = entryTask;
            mRootTask = rootTask;
            mDetachedTasks = detachedTasks;
        }

        public void Load(out Task entryTask, out Task rootTask, out List<Task> detachedTasks)
        {
            entryTask = mEntryTask;
            rootTask = mRootTask;
            detachedTasks = mDetachedTasks;
        }

        public bool CheckForSerialization(bool force, BehaviorSource behaviorSource = null)
        {
            // mark the non-external behavior tree version as serialized since that property will change. 
            bool hasSerialized = (behaviorSource != null ? behaviorSource.HasSerialized : HasSerialized);
            if (!hasSerialized || force) {
                if (behaviorSource != null) {
                    behaviorSource.HasSerialized = true;
                } else {
                    HasSerialized = true;
                }
                if (mTaskData!= null && !string.IsNullOrEmpty(mTaskData.JSONSerialization)) {
                    DeserializeJSON.Load(mTaskData, behaviorSource == null ? this : behaviorSource);
                } else { // binary serialization
                    BinaryDeserialization.Load(mTaskData, behaviorSource == null ? this : behaviorSource);
                }
                return true;
            }
            return false;
        }

        public SharedVariable GetVariable(string name)
        {
            if (name == null) {
                return null;
            }

            if (mVariables != null) {
                if (mSharedVariableIndex == null || (mSharedVariableIndex.Count != mVariables.Count)) {
                    UpdateVariablesIndex();
                }
                if (mSharedVariableIndex.ContainsKey(name)) {
                    return mVariables[mSharedVariableIndex[name]];
                }
            }
            return null;
        }

        public List<SharedVariable> GetAllVariables()
        {
            return mVariables;
        }

        public void SetVariable(string name, SharedVariable sharedVariable)
        {
            if (mVariables == null) {
                mVariables = new List<SharedVariable>();
                // When the game starts the index is null because dictionaries are not serialized by Unity
            } else if (mSharedVariableIndex == null) {
                UpdateVariablesIndex();
            }

            sharedVariable.Name = name;
            if (mSharedVariableIndex != null && mSharedVariableIndex.ContainsKey(name)) {
                mVariables[mSharedVariableIndex[name]] = sharedVariable;
            } else {
                mVariables.Add(sharedVariable);
                UpdateVariablesIndex();
            }
        }

        public void UpdateVariableName(SharedVariable sharedVariable, string name)
        {
            CheckForSerialization(false);
            sharedVariable.Name = name;
            UpdateVariablesIndex();
        }

        public void SetAllVariables(List<SharedVariable> variables)
        {
            mVariables = variables;
        }

        private void UpdateVariablesIndex()
        {
            if (mVariables == null) {
                if (mSharedVariableIndex != null) {
                    mSharedVariableIndex = null;
                }
                return;
            }
            if (mSharedVariableIndex == null) {
                mSharedVariableIndex = new Dictionary<string, int>(mVariables.Count);
            } else {
                mSharedVariableIndex.Clear();
            }
            for (int i = 0; i < mVariables.Count; ++i) {
                if (mVariables[i] == null)
                    continue;
                mSharedVariableIndex.Add(mVariables[i].Name, i);
            }
        }

        public override string ToString()
        {
            if (mOwner == null) {
                return behaviorName;
            } else {
                return string.Format("{0} - {1}", Owner.GetOwnerName(), behaviorName);
            }
        }
    }
}