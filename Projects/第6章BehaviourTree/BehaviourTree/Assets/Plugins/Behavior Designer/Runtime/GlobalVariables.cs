using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public class GlobalVariables : ScriptableObject, IVariableSource
    {
        private static GlobalVariables instance = null;
        public static GlobalVariables Instance
        {
            get
            {
                if (instance == null) {
                    instance = Resources.Load("BehaviorDesignerGlobalVariables", typeof(GlobalVariables)) as GlobalVariables;
                }
                return instance;
            }
        }

        [SerializeField]
        private List<SharedVariable> mVariables = null;
        public List<SharedVariable> Variables { get { return mVariables; } set { mVariables = value; UpdateVariablesIndex(); } }
        private Dictionary<string, int> mSharedVariableIndex;

        [SerializeField]
        private string mSerialization;
        public string Serialization { get { return mSerialization; } set { mSerialization = value; } }

        [SerializeField]
        private VariableSerializationData mVariableData;
        public VariableSerializationData VariableData { get { return mVariableData; } set { mVariableData = value; } }

        [SerializeField]
        private List<Object> mUnityObjects;
        public List<Object> UnityObjects { get { return mUnityObjects; } set { mUnityObjects = value; } }

        public void CheckForSerialization(bool force)
        {
            // Only deserialize if there isn't any behavior tree elements, as in there is not a entry task or any variables
            // When Unity serializes mVariables to a prefab it is going to serialze the element count but not the actual objects because those object derive from ScriptableObject.
            // If there are elements within the array check to make sure the first element isn't null. The first element will be null when converting to a prefab.
            if (force || mVariables == null || (mVariables.Count > 0 && mVariables[0] == null)) {
                if (!string.IsNullOrEmpty(mSerialization)) {
                    DeserializeJSON.Load(mSerialization, this);
                } else if (VariableData != null && !string.IsNullOrEmpty(VariableData.JSONSerialization)) {
                    DeserializeJSON.Load(VariableData.JSONSerialization, this);
                } else { // binary serialization
                    BinaryDeserialization.Load(this);
                }
            }
        }

        public SharedVariable GetVariable(string name)
        {
            if (name == null) {
                return null;
            }
            CheckForSerialization(false);
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
            CheckForSerialization(false);
            return mVariables;
        }

        public void SetVariable(string name, SharedVariable sharedVariable)
        {
            CheckForSerialization(false);
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
    }
}