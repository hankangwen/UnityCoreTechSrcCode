using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public enum UpdateIntervalType { EveryFrame, SpecifySeconds, Manual }

    public class BehaviorManager : MonoBehaviour
    {
        static public BehaviorManager instance;

        [Obsolete("BehaviorManager.UpdateInterval has been deprecated. Use UpdateInterval instead.")]
        public enum UpdateIntervalType { EveryFrame, SpecifySeconds, Manual }
        [SerializeField]
        private BehaviorDesigner.Runtime.UpdateIntervalType updateInterval = BehaviorDesigner.Runtime.UpdateIntervalType.EveryFrame;
        public BehaviorDesigner.Runtime.UpdateIntervalType UpdateInterval { get { return updateInterval; } set { updateInterval = value; UpdateIntervalChanged(); } }
        [SerializeField]
        private float updateIntervalSeconds = 0;
        public float UpdateIntervalSeconds { get { return updateIntervalSeconds; } set { updateIntervalSeconds = value; UpdateIntervalChanged(); } }
        private WaitForSeconds updateWait = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public delegate void TaskBreakpointHandler();
        public event TaskBreakpointHandler onTaskBreakpoint;
#endif

        // one behavior tree for each client
        public class BehaviorTree
        {
            public class ConditionalReevaluate
            {
                public int index;
                public TaskStatus taskStatus;
                public int compositeIndex = -1; // -1 means inactive
                public int stackIndex = -1;

                public ConditionalReevaluate() { }

                public void Initialize(int i, TaskStatus status, int stack, int composite)
                {
                    index = i;
                    taskStatus = status;
                    stackIndex = stack;
                    compositeIndex = composite;
                }
            }

            public List<Task> taskList;
            public List<int> parentIndex;
            public List<List<int>> childrenIndex;
            // the relative child index is the index relative to the parent. For example, the first child has a relative child index of 0
            public List<int> relativeChildIndex;
            public List<Stack<int>> activeStack;
            public List<TaskStatus> nonInstantTaskStatus;
            public List<int> interruptionIndex;
            public List<ConditionalReevaluate> conditionalReevaluate;
            public Dictionary<int, BehaviorTree.ConditionalReevaluate> conditionalReevaluateMap;
            public List<int> decoratorReevaluate;
            public List<int> parentCompositeIndex;
            public List<List<int>> childConditionalIndex;
            public Behavior behavior;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            public List<Task> originalTaskList;
            public List<int> originalIndex;
#endif
        }
        private List<BehaviorTree> behaviorTrees = new List<BehaviorTree>();
        public List<BehaviorTree> BehaviorTrees { get { return behaviorTrees; } }
        private Dictionary<Behavior, BehaviorTree> pausedBehaviorTrees = new Dictionary<Behavior, BehaviorTree>();
        private Dictionary<Behavior, BehaviorTree> behaviorTreeMap = new Dictionary<Behavior, BehaviorTree>();
        private List<int> conditionalParentIndexes = new List<int>();

        // Third party support
        public enum ThirdPartyObjectType { PlayMaker, uScript, DialogueSystem, uSequencer, AIForMecanim, SimpleWaypointSystem }
        public class ThirdPartyTask
        {
            private Task task;
            public Task Task { get { return task; } set { task = value; } }
            private ThirdPartyObjectType thirdPartyObjectType;
            public ThirdPartyObjectType ThirdPartyObjectType { get { return thirdPartyObjectType; } }
            public ThirdPartyTask(Task t, ThirdPartyObjectType objectType)
            {
                task = t;
                thirdPartyObjectType = objectType;
            }
        }
        public class ThirdPartyTaskComparer : IEqualityComparer<ThirdPartyTask>
        {
            public bool Equals(ThirdPartyTask a, ThirdPartyTask b)
            {
                if (ReferenceEquals(a, null)) return false;
                if (ReferenceEquals(b, null)) return false;
                return a.Task.Equals(b.Task);
            }

            public int GetHashCode(ThirdPartyTask obj)
            {
                return obj != null ? obj.Task.GetHashCode() : 0;
            }
        }

        private Dictionary<object, ThirdPartyTask> objectTaskMap = new Dictionary<object, ThirdPartyTask>();
        private Dictionary<ThirdPartyTask, object> taskObjectMap = new Dictionary<ThirdPartyTask, object>(new ThirdPartyTaskComparer());
        private ThirdPartyTask thirdPartyTaskCompare = new ThirdPartyTask(null, ThirdPartyObjectType.PlayMaker);

        private static MethodInfo playMakerStopMethod = null;
        private static MethodInfo PlayMakerStopMethod
        {
            get { if (playMakerStopMethod == null) { playMakerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker").GetMethod("StopPlayMaker"); } return playMakerStopMethod; }
        }
        private static MethodInfo uScriptStopMethod = null;
        private static MethodInfo UScriptStopMethod
        {
            get { if (uScriptStopMethod == null) { uScriptStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uScript").GetMethod("StopuScript"); } return uScriptStopMethod; }
        }
        private static MethodInfo dialogueSystemStopMethod = null;
        private static MethodInfo DialogueSystemStopMethod
        {
            get { if (dialogueSystemStopMethod == null) { dialogueSystemStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_DialogueSystem").GetMethod("StopDialogueSystem"); } return dialogueSystemStopMethod; }
        }
        private static MethodInfo uSequencerStopMethod = null;
        private static MethodInfo USequencerStopMethod
        {
            get { if (uSequencerStopMethod == null) { uSequencerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uSequencer").GetMethod("StopuSequencer"); } return uSequencerStopMethod; }
        }
        private static MethodInfo aiForMecanimStopMethod = null;
        private static MethodInfo AIForMecanimStopMethod
        {
            get { if (aiForMecanimStopMethod == null) { aiForMecanimStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_AIForMecanim").GetMethod("StopAIForMecanim"); } return aiForMecanimStopMethod; }
        }
        private static MethodInfo simpleWaypointSystemStopMethod = null;
        private static MethodInfo SimpleWaypointSystemStopMethod
        {
            get { if (simpleWaypointSystemStopMethod == null) { simpleWaypointSystemStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_SimpleWaypointSystem").GetMethod("StopSimpleWaypointSystem"); } return simpleWaypointSystemStopMethod; }
        }
        private static object[] invokeParameters = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        private bool atBreakpoint = false;
        public bool AtBreakpoint { get { return atBreakpoint; } set { atBreakpoint = value; } }
        private bool showExternalTrees = false;
        private bool dirty = false;
        public bool Dirty { get { return dirty; } set { dirty = value; } }
#endif
        
        // convenience class used for adding new tasks
        public class TaskAddData
        {
            public class InheritedFieldValue
            {
                private object value;
                public object Value { get { return value; } }
                private int depth;
                public int Depth { get { return depth; } }
                public InheritedFieldValue(object v, int d)
                {
                    value = v;
                    depth = d;
                }
            }

            public bool fromExternalTask = false;
            public ParentTask parentTask = null;
            public int parentIndex = -1;
            public int depth = 0;
            public int compositeParentIndex = -1;
            public Dictionary<string, object> sharedVariables = null;
            public Dictionary<string, InheritedFieldValue> inheritedFields = null;
            public int errorTask = -1;
            public string errorTaskName = "";
        }

        public void Awake()
        {
            instance = this;

            UpdateIntervalChanged();
        }

        private void UpdateIntervalChanged()
        {
            StopCoroutine("CoroutineUpdate");
            if (updateInterval == BehaviorDesigner.Runtime.UpdateIntervalType.EveryFrame) {
                enabled = true;
            } else if (updateInterval == BehaviorDesigner.Runtime.UpdateIntervalType.SpecifySeconds) {
                if (Application.isPlaying) {
                    updateWait = new WaitForSeconds(updateIntervalSeconds);
                    StartCoroutine("CoroutineUpdate");
                }
                enabled = false;
            } else { // manual
                enabled = false;
            }
        }

        public void OnDestroy()
        {
            for (int i = behaviorTrees.Count - 1; i > -1; --i) {
                DisableBehavior(behaviorTrees[i].behavior);
            }
        }

        public void OnApplicationQuit()
        {
            for (int i = behaviorTrees.Count - 1; i > -1; --i) {
                DisableBehavior(behaviorTrees[i].behavior);
            }
        }

        public void EnableBehavior(Behavior behavior)
        {
            BehaviorTree behaviorTree;
            if (IsBehaviorEnabled(behavior)) {
                // unpause
                if (pausedBehaviorTrees.ContainsKey(behavior)) {
                    behaviorTree = pausedBehaviorTrees[behavior];
                    behaviorTrees.Add(behaviorTree);
                    pausedBehaviorTrees.Remove(behavior);

                    for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                        behaviorTree.taskList[i].OnPause(false);
                    }
                }
                return;
            }

            // ensure the tree is deserialized
            if (behavior.ExternalBehavior != null) {
                behavior.ExternalBehavior.BehaviorSource.Owner = behavior.ExternalBehavior;
                behavior.ExternalBehavior.BehaviorSource.CheckForSerialization(!behavior.IsSceneObject, behavior.GetBehaviorSource());
            } else {
                behavior.GetBehaviorSource().CheckForSerialization(!behavior.IsSceneObject);
            }
            behavior.IsSceneObject = true;

            var rootTask = behavior.GetBehaviorSource().RootTask;
            if (rootTask == null) {
                Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains no root task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                return;
            }

            behaviorTree = new BehaviorTree();
            behaviorTree.taskList = new List<Task>();
            behaviorTree.behavior = behavior;
            behaviorTree.parentIndex = new List<int>();
            behaviorTree.childrenIndex = new List<List<int>>();
            behaviorTree.relativeChildIndex = new List<int>();
            behaviorTree.parentCompositeIndex = new List<int>();
            behaviorTree.childConditionalIndex = new List<List<int>>();
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            behaviorTree.originalTaskList = new List<Task>();
            behaviorTree.originalIndex = new List<int>();
#endif
            behaviorTree.parentIndex.Add(-1); // add the first entry for the root task
            behaviorTree.relativeChildIndex.Add(-1);
            behaviorTree.parentCompositeIndex.Add(-1);

            bool hasExternalBehavior = behavior.ExternalBehavior != null;
            var taskAddData = new TaskAddData();
            int status = AddToTaskList(behaviorTree, rootTask, ref hasExternalBehavior, taskAddData);
            if (status < 0) {
                // something is wrong with the tree. Don't go any further
                behaviorTree = null;
                switch (status) {
                    case -1:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a parent task ({2} (index {3})) with no children This behavior will be disabled.",
                            behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name, taskAddData.errorTaskName, taskAddData.errorTask));
                        break;
                    case -2:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" cannot find the referenced external task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -3:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a null task (referenced from parent task {2} (index {3})). This behavior will be disabled.",
                                behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name, taskAddData.errorTaskName, taskAddData.errorTask));
                        break;
                    case -4:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains multiple external behavior trees at the root task or as a child of a parent task which cannot contain so many children (such as a decorator task). This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -5:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a Behavior Tree Reference task ({2} (index {3})) that which has an element with a null value in the externalBehaviors array. This behavior will be disabled.",
                                behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name, taskAddData.errorTaskName, taskAddData.errorTask));
                        break;
                }
                return;
            }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // a behavior tree is dirty when a new behavior tree is loaded. This will update the editor
            dirty = true;
            if (behavior.ExternalBehavior != null) {
                behavior.GetBehaviorSource().EntryTask = behavior.ExternalBehavior.BehaviorSource.EntryTask;
            }
            // update the root task with the newly instantiated task
            behavior.GetBehaviorSource().RootTask = behaviorTree.taskList[0];
#endif

            behaviorTree.activeStack = new List<Stack<int>>();
            behaviorTree.interruptionIndex = new List<int>();
            behaviorTree.conditionalReevaluate = new List<BehaviorTree.ConditionalReevaluate>();
            behaviorTree.conditionalReevaluateMap = new Dictionary<int, BehaviorTree.ConditionalReevaluate>();
            behaviorTree.decoratorReevaluate = new List<int>();
            behaviorTree.nonInstantTaskStatus = new List<TaskStatus>();
            // add the first entry
            behaviorTree.activeStack.Add(new Stack<int>());
            behaviorTree.interruptionIndex.Add(-1);
            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behaviorTree.behavior.LogTaskChanges) {
                for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                    Debug.Log(string.Format("{0}: Task {1} ({2}, index {3}) {4}", RoundedTime(), behaviorTree.taskList[i].FriendlyName, behaviorTree.taskList[i].GetType(), i, behaviorTree.taskList[i].GetHashCode()));
                }
            }
#endif

            // set the MonoBehavior components
            Animation clientAnimation = behavior.GetComponent<Animation>();
            AudioSource clientAudio = behavior.GetComponent<AudioSource>();
            Camera clientCamera = behavior.GetComponent<Camera>();
            Collider clientCollider = behavior.GetComponent<Collider>();
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
            Collider2D clientCollider2D = behavior.GetComponent<Collider2D>();
#endif
            ConstantForce clientConstantForce = behavior.GetComponent<ConstantForce>();
            GameObject clientGameObject = behavior.gameObject;
            GUIText clientGUIText = behavior.GetComponent<GUIText>();
            GUITexture clientGUITexture = behavior.GetComponent<GUITexture>();
            HingeJoint clientHingeJoint = behavior.GetComponent<HingeJoint>();
            Light clientLight = behavior.GetComponent<Light>();
#if !UNITY_WINRT && !DLL_DEBUG && !DLL_RELEASE
            NetworkView clientNetworkView = behavior.GetComponent<NetworkView>();
#endif
            ParticleEmitter clientParticleEmitter = behavior.GetComponent<ParticleEmitter>();
            ParticleSystem clientParticleSystem = behavior.GetComponent<ParticleSystem>();
            Renderer clientRenderer = behavior.GetComponent<Renderer>();
            Rigidbody clientRigidbody = behavior.GetComponent<Rigidbody>();
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
            Rigidbody2D clientRigidbody2D = behavior.GetComponent<Rigidbody2D>();
#endif
            Transform clientTransform = behavior.transform;

            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                // Assign the mono behavior components
                behaviorTree.taskList[i].Animation = clientAnimation;
                behaviorTree.taskList[i].Audio = clientAudio;
                behaviorTree.taskList[i].Camera = clientCamera;
                behaviorTree.taskList[i].Collider = clientCollider;
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
                behaviorTree.taskList[i].Collider2D = clientCollider2D;
#endif
                behaviorTree.taskList[i].ConstantForce = clientConstantForce;
                behaviorTree.taskList[i].GameObject = clientGameObject;
                behaviorTree.taskList[i].GUIText = clientGUIText;
                behaviorTree.taskList[i].GUITexture = clientGUITexture;
                behaviorTree.taskList[i].HingeJoint = clientHingeJoint;
                behaviorTree.taskList[i].Light = clientLight;
#if !UNITY_WINRT && !DLL_DEBUG && !DLL_RELEASE
                behaviorTree.taskList[i].NetworkView = clientNetworkView;
#endif
                behaviorTree.taskList[i].ParticleEmitter = clientParticleEmitter;
                behaviorTree.taskList[i].ParticleSystem = clientParticleSystem;
                behaviorTree.taskList[i].Renderer = clientRenderer;
                behaviorTree.taskList[i].Rigidbody = clientRigidbody;
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
                behaviorTree.taskList[i].Rigidbody2D = clientRigidbody2D;
#endif
                behaviorTree.taskList[i].Transform = clientTransform;
                behaviorTree.taskList[i].Owner = behaviorTree.behavior;

                behaviorTree.taskList[i].OnAwake();
            }

            // the behavior tree is ready to go
            behaviorTrees.Add(behaviorTree);
            behaviorTreeMap.Add(behavior, behaviorTree);

            // start with the first index if it isn't disabled
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (!behaviorTree.taskList[0].NodeData.Disabled) {
#endif
            PushTask(behaviorTree, 0, 0);
            behavior.ExecutionStatus = TaskStatus.Running;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            }
#endif
        }

        // returns 0 for success
        // returns -1 if a parent doesn't have any children
        // returns -2 if the external task cannot be found
        // returns -3 if the task is null
        // returns -4 if there are multiple external behavior trees and the parent task is null or cannot handle as many behavior trees specified
        // returns -5 if a behavior tree reference task contains a null external tree
        private int AddToTaskList(BehaviorTree behaviorTree, Task task, ref bool hasExternalBehavior, TaskAddData data)
        {
            if (task == null) {
                return -3;
            }

            if (task is BehaviorReference) {
                BehaviorSource[] behaviorSource = null;
                var behaviorReference = task as BehaviorReference;
                if (behaviorReference != null) {
                    ExternalBehavior[] externalBehaviors = null;
                    if ((externalBehaviors = behaviorReference.GetExternalBehaviors()) != null) {
                        behaviorSource = new BehaviorSource[externalBehaviors.Length];
                        for (int i = 0; i < externalBehaviors.Length; ++i) {
                            if (externalBehaviors[i] == null) {
                                data.errorTask = behaviorTree.taskList.Count;
                                data.errorTaskName = !string.IsNullOrEmpty(task.FriendlyName) ? task.FriendlyName : task.GetType().ToString();
                                return -5;
                            }
                            behaviorSource[i] = externalBehaviors[i].BehaviorSource;
                            behaviorSource[i].Owner = externalBehaviors[i];
                        }
                    } else {
                        return -2;
                    }
                } else {
                    return -2;
                }
                if (behaviorSource != null) {
                    var parentTask = data.parentTask;
                    int parentIndex = data.parentIndex;
                    int compositeParentIndex = data.compositeParentIndex;
                    data.depth++;
                    for (int i = 0; i < behaviorSource.Length; ++i) {
                        // deserialize the external tasks into a new behavior source which will then be added into the original tree
                        var loadedBehaviorSource = new BehaviorSource(behaviorSource[i].Owner);
                        behaviorSource[i].CheckForSerialization(true, loadedBehaviorSource);
                        var externalRootTask = loadedBehaviorSource.RootTask;
                        if (externalRootTask != null) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            if (!data.fromExternalTask && i == 0) {
                                behaviorTree.originalTaskList.Add(task);
                            }
#endif
                            // bring the external behavior trees variables into the parent behavior tree
                            if (loadedBehaviorSource.Variables != null) {
                                for (int j = 0; j < loadedBehaviorSource.Variables.Count; ++j) {
                                    SharedVariable sharedVariable = null;
                                    // only set the behavior tree variable if it doesn't already exist in the parent behavior tree
                                    if ((sharedVariable = behaviorTree.behavior.GetVariable(loadedBehaviorSource.Variables[j].Name)) == null) {
                                        sharedVariable = loadedBehaviorSource.Variables[j];
                                        behaviorTree.behavior.SetVariable(sharedVariable.Name, sharedVariable);
                                        // add it to the shared variables dictionary
                                        if (data.sharedVariables == null) {
                                            data.sharedVariables = new Dictionary<string, object>();
                                        }
                                        if (!data.sharedVariables.ContainsKey(sharedVariable.Name)) {
                                            data.sharedVariables.Add(sharedVariable.Name, sharedVariable);
                                        }
                                    }

                                    // automatically import the shared variables from the parent tree
                                    if (behaviorReference.autoInheritVariables) {
                                        if (data.inheritedFields == null) {
                                            data.inheritedFields = new Dictionary<string, TaskAddData.InheritedFieldValue>();
                                        }
                                        if (!data.inheritedFields.ContainsKey(sharedVariable.Name)) {
                                            data.inheritedFields.Add(sharedVariable.Name, new TaskAddData.InheritedFieldValue(sharedVariable, data.depth));
                                        }
                                    }
                                }
                            }

                            // find all of the inherited fields
                            var fields = TaskUtility.GetAllFields(task.GetType());
                            for (int j = 0; j < fields.Length; ++j) {
                                if (TaskUtility.HasAttribute(fields[j], typeof(InheritedFieldAttribute))) {
                                    if (data.inheritedFields == null) {
                                        data.inheritedFields = new Dictionary<string, TaskAddData.InheritedFieldValue>();
                                    }
                                    if (!data.inheritedFields.ContainsKey(fields[j].Name)) {
                                        if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable))) {
                                            var sharedVariable = fields[j].GetValue(task) as SharedVariable;
                                            if (sharedVariable.IsShared) {
                                                var newSharedVariable = behaviorTree.behavior.GetVariable(sharedVariable.Name);
                                                if (newSharedVariable == null && data.sharedVariables != null && data.sharedVariables.ContainsKey(sharedVariable.Name)) {
                                                    newSharedVariable = data.sharedVariables[sharedVariable.Name] as SharedVariable;
                                                }
                                                data.inheritedFields.Add(fields[j].Name, new TaskAddData.InheritedFieldValue(newSharedVariable, data.depth));
                                            } else {
                                                data.inheritedFields.Add(fields[j].Name, new TaskAddData.InheritedFieldValue(sharedVariable, data.depth));
                                            }
                                        } else {
                                            data.inheritedFields.Add(fields[j].Name, new TaskAddData.InheritedFieldValue(fields[j].GetValue(task), data.depth));
                                        }
                                    }
                                }
                            }
                            if (i > 0) {
                                // If there are multiple external behavior trees then the TaskAddData was probably changed
                                data.parentTask = parentTask;
                                data.parentIndex = parentIndex;
                                data.compositeParentIndex = compositeParentIndex;
                                // Return an error if the parent task is null (root task) or if there are too many external behavior trees
                                if (data.parentTask == null || i >= data.parentTask.MaxChildren()) {
                                    return -4;
                                } else {
                                    // add the external tree
                                    behaviorTree.parentIndex.Add(data.parentIndex);
                                    behaviorTree.relativeChildIndex.Add(data.parentTask.Children.Count);
                                    behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
                                    behaviorTree.childrenIndex[data.parentIndex].Add(behaviorTree.taskList.Count);
                                    data.parentTask.AddChild(externalRootTask, data.parentTask.Children.Count);
                                }
                            }
                            hasExternalBehavior = true;
                            bool fromExternalTask = data.fromExternalTask;
                            data.fromExternalTask = true;
                            int status = 0;
                            if ((status = AddToTaskList(behaviorTree, externalRootTask, ref hasExternalBehavior, data)) < 0) {
                                return status;
                            }
                            // reset back to the original value
                            data.fromExternalTask = fromExternalTask;
                        } else {
                            return -2;
                        }
                    }
                    // remove any inherited fields that aren't at the same depth in the tree anymore.
                    if (data.inheritedFields != null) {
                        List<string> removedFields = null;
                        foreach (var field in data.inheritedFields) {
                            if (field.Value.Depth == data.depth) {
                                if (removedFields == null) {
                                    removedFields = new List<string>();
                                }
                                removedFields.Add(field.Key);
                            }
                        }
                        if (removedFields != null) {
                            for (int i = 0; i < removedFields.Count; ++i) {
                                data.inheritedFields.Remove(removedFields[i]);
                            }
                        }
                    }
                    data.depth--;
                } else {
                    return -2;
                }
            } else {
                // If the task is coming from an external behavior or prefab than create a new task. This is done to
                // keep all of the properties local and not override an external behavior or prefab with any property values.
                task.ReferenceID = behaviorTree.taskList.Count;
                behaviorTree.taskList.Add(task);
                if (data.fromExternalTask) {
                    if (data.inheritedFields != null) {
                        var fields = TaskUtility.GetAllFields(task.GetType());
                        for (int i = 0; i < fields.Length; ++i) {
                            var value = fields[i].GetValue(task);
                            if (data.inheritedFields.ContainsKey(fields[i].Name) && TaskUtility.HasAttribute(fields[i], typeof(InheritedFieldAttribute))) {
                                fields[i].SetValue(task, data.inheritedFields[fields[i].Name].Value);
                            } else if (value is SharedVariable) { // if the field name doesn't match then the shared variable name may match
                                var sharedVariable = value as SharedVariable;
                                var sharedVariableName = sharedVariable.Name;
                                if (data.inheritedFields != null && sharedVariableName != null && data.inheritedFields.ContainsKey(sharedVariableName)) {
                                    var inheritedSharedVariable = data.inheritedFields[sharedVariableName].Value;
                                    if (inheritedSharedVariable.GetType().Equals(fields[i].FieldType)) {
                                        fields[i].SetValue(task, inheritedSharedVariable);
                                    }
                                }
                            }
                        }
                    }

                    if (data.parentTask == null) { // A null parent task means the root task is the behavior tree reference task, so just replace that task
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        task.NodeData.Offset = behaviorTree.behavior.GetBehaviorSource().RootTask.NodeData.Offset;
#endif
                    } else {
                        int relativeChildIndex = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        int parentIndex = behaviorTree.parentIndex[behaviorTree.parentIndex.Count - 1];
                        if (behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]] is BehaviorReference) {
#endif
                        data.parentTask.ReplaceAddChild(task, relativeChildIndex);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        } else {
                            // if the original parent type is not a task reference then this task index is the task reference task
                            data.parentTask.ReplaceAddChild(behaviorTree.originalTaskList[behaviorTree.originalTaskList.Count - 1], relativeChildIndex);
                            task.NodeData.Offset = behaviorTree.originalTaskList[behaviorTree.originalTaskList.Count - 1].NodeData.Offset;
                        }
#endif
                    }
                }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.originalTaskList.Count == 0) {
                    behaviorTree.originalTaskList.Add(task);
                    behaviorTree.originalIndex.Add(0);
                } else {
                    if (!data.fromExternalTask) {
                        behaviorTree.originalTaskList.Add(task);
                    }
                    behaviorTree.originalIndex.Add(behaviorTree.originalTaskList.Count - 1);
                }
#endif
                if (task is ParentTask) {
                    var parentTask = task as ParentTask;
                    if (parentTask.Children == null || parentTask.Children.Count == 0) {
                        data.errorTask = behaviorTree.taskList.Count - 1;
                        data.errorTaskName = !string.IsNullOrEmpty(behaviorTree.taskList[data.errorTask].FriendlyName) ?
                                                behaviorTree.taskList[data.errorTask].FriendlyName : behaviorTree.taskList[data.errorTask].GetType().ToString();
                        return -1; // invalid tree
                    }
                    int status;
                    int parentIndex = behaviorTree.taskList.Count - 1;
                    behaviorTree.childrenIndex.Add(new List<int>());
                    behaviorTree.childConditionalIndex.Add(new List<int>());
                    // store the childCount ahead of time in case new external trees are added to the current parent
                    int childCount = parentTask.Children.Count;
                    for (int i = 0; i < childCount; ++i) {
                        behaviorTree.parentIndex.Add(parentIndex);
                        behaviorTree.relativeChildIndex.Add(i);
                        behaviorTree.childrenIndex[parentIndex].Add(behaviorTree.taskList.Count);
                        data.parentTask = task as ParentTask;
                        data.parentIndex = parentIndex;
                        if (task is Composite) {
                            data.compositeParentIndex = parentIndex;
                        }
                        behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
                        if ((status = AddToTaskList(behaviorTree, parentTask.Children[i], ref hasExternalBehavior, data)) < 0) {
                            if (status == -3) { // invalid task
                                data.errorTask = parentIndex;
                                data.errorTaskName = !string.IsNullOrEmpty(behaviorTree.taskList[data.errorTask].FriendlyName) ?
                                                        behaviorTree.taskList[data.errorTask].FriendlyName : behaviorTree.taskList[data.errorTask].GetType().ToString();
                            }
                            return status;
                        }
                    }
                } else { // the task isn't a parent so it doesn't have any children or condtional status
                    behaviorTree.childrenIndex.Add(null);
                    behaviorTree.childConditionalIndex.Add(null);
                    // mark the parent composite task as having a conditional task
                    if (task is Conditional) {
                        int taskIndex = behaviorTree.taskList.Count - 1;
                        int compositeParent = behaviorTree.parentCompositeIndex[taskIndex];
                        if (compositeParent != -1) {
                            behaviorTree.childConditionalIndex[compositeParent].Add(taskIndex);
                        }
                    }
                }
            }
            return 0;
        }

        public void DisableBehavior(Behavior behavior)
        {
            DisableBehavior(behavior, false);
        }

        public void DisableBehavior(Behavior behavior, bool paused)
        {
            if (!IsBehaviorEnabled(behavior)) {
                return;
            }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behavior.LogTaskChanges) {
                Debug.Log(string.Format("{0}: {1} {2}", RoundedTime(), (paused ? "Pausing" : "Disabling"), behavior.ToString()));
            }
#endif

            var behaviorTree = behaviorTreeMap[behavior];
            if (paused) {
                if (!pausedBehaviorTrees.ContainsKey(behavior)) {
                    pausedBehaviorTrees.Add(behavior, behaviorTree);

                    for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                        behaviorTree.taskList[i].OnPause(true);
                    }
                }
            } else {
                // pop all of the tasks so they receive the end callback
                var status = TaskStatus.Success;
                for (int i = behaviorTree.activeStack.Count - 1; i > -1; --i) {
                    while (behaviorTree.activeStack[i].Count > 0) {
                        int stackCount = behaviorTree.activeStack[i].Count;
                        PopTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref status, true, false);
                        if (stackCount == 1) {
                            break;
                        }
                    }
                }

                // Remove all of the conditional aborts that haven't had a chance to run yet
                RemoveChildConditionalReevaluate(behaviorTree, -1);

                for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                    behaviorTree.taskList[i].OnBehaviorComplete();
                }

                behaviorTreeMap.Remove(behavior);
            }
            behaviorTrees.Remove(behaviorTree);
        }

        public void RestartBehavior(Behavior behavior)
        {
            if (!IsBehaviorEnabled(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            // pop all of the tasks so they receive the end callback
            var status = TaskStatus.Success;
            for (int i = behaviorTree.activeStack.Count - 1; i > -1; --i) {
                while (behaviorTree.activeStack[i].Count > 0) {
                    int stackCount = behaviorTree.activeStack[i].Count;
                    PopTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref status, true, false);
                    if (stackCount == 1) {
                        break;
                    }
                }
            }
            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                behaviorTree.taskList[i].OnBehaviorComplete();
            }

            // start things again
            Restart(behaviorTree);
        }

        public bool IsBehaviorEnabled(Behavior behavior)
        {
            return behaviorTreeMap != null && behavior != null && behaviorTreeMap.ContainsKey(behavior);
        }

        public void Update()
        {
            Tick();
        }

        private IEnumerator CoroutineUpdate()
        {
            while (true) {
                Tick();
                yield return updateWait;
            }
        }

        // Tick all of the behavior trees
        public void Tick()
        {
            for (int i = 0; i < behaviorTrees.Count; ++i) {
                Tick(behaviorTrees[i]);
            }
        }

        // Manually tick a specific behavior tree
        public void Tick(Behavior behavior)
        {
            if (behavior == null || !IsBehaviorEnabled(behavior)) {
                return;
            }

            Tick(behaviorTreeMap[behavior]);
        }

        private void Tick(BehaviorTree behaviorTree)
        {
            ReevaluateDecoratorTasks(behaviorTree);
            ReevaluateConditionalTasks(behaviorTree);

            for (int j = behaviorTree.activeStack.Count - 1; j > -1; --j) {
                // ensure there are no interruptions within the hierarchy
                var status = TaskStatus.Inactive;
                int interruptedTask;
                if (j < behaviorTree.interruptionIndex.Count && (interruptedTask = behaviorTree.interruptionIndex[j]) != -1) {
                    behaviorTree.interruptionIndex[j] = -1;
                    while (behaviorTree.activeStack[j].Peek() != interruptedTask) {
                        int stackCount = behaviorTree.activeStack[j].Count;
                        PopTask(behaviorTree, behaviorTree.activeStack[j].Peek(), j, ref status, true);
                        if (stackCount == 1) {
                            break;
                        }
                    }
                    // pop the interrupt task. Performing a check to be sure the interrupted task is at the top of the stack because the interrupted task
                    // may be in a different stack and the active stack has completely been removed
                    if (j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0 && behaviorTree.taskList[interruptedTask] == behaviorTree.taskList[behaviorTree.activeStack[j].Peek()]) {
                        status = (behaviorTree.taskList[interruptedTask] as ParentTask).OverrideStatus();
                        PopTask(behaviorTree, interruptedTask, j, ref status, true);
                    }
                }
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                int count = 0;
#endif
                int startIndex = -1;
                int taskIndex;
                while (status != TaskStatus.Running && j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0) {
                    taskIndex = behaviorTree.activeStack[j].Peek();
                    // bail out if the index is the same as what it was before runTask was executed or the behavior is no longer enabled
                    if ((j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0 && startIndex == behaviorTree.activeStack[j].Peek()) || !IsBehaviorEnabled(behaviorTree.behavior)) {
                        break;
                    } else {
                        startIndex = taskIndex;
                    }
                    status = RunTask(behaviorTree, taskIndex, j, status);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    // While in the editor make sure we aren't in an infinite loop
                    if (++count > behaviorTree.taskList.Count) {
                        Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
                        DisableBehavior(behaviorTree.behavior);
                        break;
                    }
#endif
                }
            }
        }

        private void ReevaluateConditionalTasks(BehaviorTree behaviorTree)
        {
            // Loop through all of the conditional tasks that are currently being reevaluated
            for (int i = behaviorTree.conditionalReevaluate.Count - 1; i > -1; --i) {
                // The task may not be quite ready yet
                if (behaviorTree.conditionalReevaluate[i].compositeIndex == -1) {
                    continue;
                }

                int conditionalIndex = behaviorTree.conditionalReevaluate[i].index;
                var conditionalStatus = behaviorTree.taskList[conditionalIndex].OnUpdate();
                // stop the subsequent tasks from running if the status changed
                if (conditionalStatus != behaviorTree.conditionalReevaluate[i].taskStatus) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.behavior.LogTaskChanges) {
                        int compositeAbort = behaviorTree.parentCompositeIndex[conditionalIndex];
                        print(string.Format("{0}: {1}: Conditional abort with task {2} ({3}, index {4}) because of conditional task {5} ({6}, index {7}) with status {8}",
                                            RoundedTime(), behaviorTree.behavior.ToString(), behaviorTree.taskList[compositeAbort].FriendlyName, behaviorTree.taskList[compositeAbort].GetType(),
                                            compositeAbort, behaviorTree.taskList[conditionalIndex].FriendlyName, behaviorTree.taskList[conditionalIndex].GetType(),
                                            conditionalIndex, conditionalStatus));
                    }
#endif
                    int compositeIndex = behaviorTree.conditionalReevaluate[i].compositeIndex;
                    for (int j = behaviorTree.activeStack.Count - 1; j > -1; --j) {
                        if (behaviorTree.activeStack[j].Count > 0) {
                            int taskIndex = behaviorTree.activeStack[j].Peek();
                            int lcaIndex = FindLCA(behaviorTree, conditionalIndex, taskIndex);
                            // let the task continue to run if the LCA isn't the composite index. We don't want to pop a branch that isn't affected by the abort
                            if (lcaIndex != compositeIndex) {
                                continue;
                            }
                            // Don't pop anymore if there are no more tasks on a particular stack because that stack index has been removed
                            while (taskIndex != -1 && taskIndex != lcaIndex && j < behaviorTree.activeStack.Count) {
                                var status = TaskStatus.Failure;
                                PopTask(behaviorTree, taskIndex, j, ref status, false);
                                taskIndex = behaviorTree.parentIndex[taskIndex];
                            }

                        }
                    }

                    // Remove any conditional tasks within the same stack as well. They will be added again when the task gets pushed again
                    for (int j = behaviorTree.conditionalReevaluate.Count - 1; j > i - 1; --j) {
                        var jConditionalReval = behaviorTree.conditionalReevaluate[j];
                        if (FindLCA(behaviorTree, compositeIndex, jConditionalReval.index) == compositeIndex) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            behaviorTree.taskList[behaviorTree.conditionalReevaluate[j].index].NodeData.IsReevaluating = false;
#endif
                            ObjectPool.Return(behaviorTree.conditionalReevaluate[j]);
                            behaviorTree.conditionalReevaluateMap.Remove(behaviorTree.conditionalReevaluate[j].index);
                            behaviorTree.conditionalReevaluate.RemoveAt(j);
                        }
                    }

                    // Update the composite index for any tasks that have the same composite parent. The new index should be the child composite index (which has to first be found)
                    // This is done to allow multiple conditional tasks to exist under the same composite task and allow the first conditional task to cause an abort with the abort type
                    // of Lower Priority running
                    for (int j = i - 1; j > -1; --j) {
                        var jConditionalReval = behaviorTree.conditionalReevaluate[j];
                        if (behaviorTree.parentCompositeIndex[jConditionalReval.index] == behaviorTree.parentCompositeIndex[conditionalIndex]) {
                            for (int k = 0; k < behaviorTree.childrenIndex[compositeIndex].Count; ++k) {
                                if (IsParentTask(behaviorTree, behaviorTree.childrenIndex[compositeIndex][k], jConditionalReval.index)) {
                                    // The child index has to be a composite task
                                    var childIndex = behaviorTree.childrenIndex[compositeIndex][k];
                                    while (!(behaviorTree.taskList[childIndex] is Composite)) {
                                        if (behaviorTree.childrenIndex[childIndex] != null) {
                                            childIndex = behaviorTree.childrenIndex[childIndex][0]; // Decorator tasks can only have one child
                                        } else {
                                            break;
                                        }
                                    }
                                    if (behaviorTree.taskList[childIndex] is Composite) {
                                        jConditionalReval.compositeIndex = childIndex;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // get a listing of all of the parents, starting with the parent at the highest level
                    conditionalParentIndexes.Clear();
                    int parentIndex = behaviorTree.parentIndex[conditionalIndex];
                    while (parentIndex != compositeIndex) {
                        conditionalParentIndexes.Add(parentIndex);
                        parentIndex = behaviorTree.parentIndex[parentIndex];
                    }
                    if (conditionalParentIndexes.Count == 0) {
                        conditionalParentIndexes.Add(behaviorTree.parentIndex[conditionalIndex]);
                    }

                    // notify all of the parents of the conditional abort and push the parents
                    var parentTask = behaviorTree.taskList[compositeIndex] as ParentTask;
                    parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex[conditionalParentIndexes[conditionalParentIndexes.Count - 1]]);
                    for (int j = conditionalParentIndexes.Count - 1; j > -1; --j) {
                        parentTask = behaviorTree.taskList[conditionalParentIndexes[j]] as ParentTask;
                        if (j == 0) {
                            parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex[conditionalIndex]);
                        } else {
                            parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex[conditionalParentIndexes[j - 1]]);
                        }
                    }
                }
            }
        }

        private void ReevaluateDecoratorTasks(BehaviorTree behaviorTree)
        {
            for (int i = behaviorTree.decoratorReevaluate.Count - 1; i > -1; --i) {
                var decoratorReevaluateIndex = behaviorTree.decoratorReevaluate[i];
                if (behaviorTree.taskList[decoratorReevaluateIndex].OnUpdate() == TaskStatus.Failure) {
                    Interrupt(behaviorTree.behavior, behaviorTree.taskList[decoratorReevaluateIndex]);
                }
            }
        }

        private TaskStatus RunTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, TaskStatus previousStatus)
        {
            var task = behaviorTree.taskList[taskIndex];
            if (task == null)
                return previousStatus;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // If the task is disabled then return immediately with a status of success. Notify the parent task that the child task finished executing so it will move on to the next child
            if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                if (behaviorTree.behavior.LogTaskChanges) {
                    print(string.Format("{0}: {1}: Skip task {2} ({3}, index {4}) at stack index {5} (task disabled)", RoundedTime(), behaviorTree.behavior.ToString(),
                                            behaviorTree.taskList[taskIndex].FriendlyName, behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex));
                }
                if (behaviorTree.parentIndex[taskIndex] != -1) {
                    var parentTask = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
                    if (!parentTask.CanRunParallelChildren()) {
                        parentTask.OnChildExecuted(TaskStatus.Inactive);
                    } else {
                        parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], TaskStatus.Inactive);
                    }
                }
                return TaskStatus.Success;
            }
#endif

            var status = previousStatus;
            // If the task is non instant and the task has already completed executing then pop the task
            if (!task.IsInstant && (behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Failure || behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Success)) {
                status = behaviorTree.nonInstantTaskStatus[stackIndex];
                PopTask(behaviorTree, taskIndex, stackIndex, ref status, true);
                return status;
            }
            PushTask(behaviorTree, taskIndex, stackIndex);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (atBreakpoint) {
                return TaskStatus.Running;
            }
#endif

            if (task is ParentTask) {
                var parentTask = task as ParentTask;
                if (parentTask is Decorator) {
                    if (parentTask.CanReevaluate()) {
                        behaviorTree.decoratorReevaluate.Add(taskIndex);
                    }
                }
                if (!parentTask.CanRunParallelChildren() || parentTask.OverrideStatus(TaskStatus.Running) != TaskStatus.Running) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    int count = 0;
#endif
                    var childStatus = TaskStatus.Inactive;
                    // nest within a while loop so multiple child tasks can be run within a single update loop (such as conditions)
                    // also, if the parent is a parallel task, start running all of the children
                    int parentStack = stackIndex;
                    int prevChildIndex = -1;
                    while (parentTask.CanExecute() && (childStatus != TaskStatus.Running || parentTask.CanRunParallelChildren())) {
                        var childrenIndexes = behaviorTree.childrenIndex[taskIndex];
                        int childIndex = parentTask.CurrentChildIndex();
                        // bail out if the child index is the same as what it was before runTask was executed
                        if ((childIndex == prevChildIndex && status != TaskStatus.Running) || !IsBehaviorEnabled(behaviorTree.behavior)) {
                            status = TaskStatus.Running;
                            break;
                        }
                        prevChildIndex = childIndex;
                        if (parentTask.CanRunParallelChildren()) {
                            // need to create a new stack level
                            behaviorTree.activeStack.Add(ObjectPool.Get<Stack<int>>());
                            behaviorTree.interruptionIndex.Add(-1);
                            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
                            stackIndex = behaviorTree.activeStack.Count - 1;
                            parentTask.OnChildStarted(childIndex);
                        } else {
                            parentTask.OnChildStarted();
                        }
                        status = childStatus = RunTask(behaviorTree, childrenIndexes[childIndex], stackIndex, status);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        // While in the editor make sure we aren't in an infinite loop
                        if (++count > behaviorTree.taskList.Count) {
                            Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
                            DisableBehavior(behaviorTree.behavior);
                            break;
                        }
#endif
                    }
                    stackIndex = parentStack;
                }
                // let the parent task override the children status. The last child task could fail immediately and we don't want that to represent the entire task
                status = parentTask.OverrideStatus(status);
            } else {
                status = task.OnUpdate();
            }

            if (status != TaskStatus.Running) {
                // pop the task immediately if the task is instant. If the task is not instant then wait for the next update
                if (task.IsInstant) {
                    PopTask(behaviorTree, taskIndex, stackIndex, ref status, true);
                } else {
                    behaviorTree.nonInstantTaskStatus[stackIndex] = status;
                }
            }

            return status;
        }

        private void PushTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex)
        {
            if (!IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.Count) {
                return;
            }

            if (behaviorTree.activeStack[stackIndex].Count == 0 || behaviorTree.activeStack[stackIndex].Peek() != taskIndex) {
                behaviorTree.activeStack[stackIndex].Push(taskIndex);
                behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Running;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                // update the referenced task if this task is an external task
                if (behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]] is BehaviorReference) {
                    // this task needs to be the first external task
                    int parentIndex = behaviorTree.parentIndex[taskIndex];
                    if (parentIndex == -1 || !(behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]] is BehaviorReference)) {
                        behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = Time.realtimeSinceStartup;
                    }
                }
                behaviorTree.taskList[taskIndex].NodeData.PushTime = Time.realtimeSinceStartup;
                // reset the execution status of this task and all of its children if it has already been run
                SetInactiveExecutionStatus(behaviorTree, taskIndex);
                if (behaviorTree.taskList[taskIndex].NodeData.IsBreakpoint) {
                    atBreakpoint = true;
                    // let behavior designer know
                    if (onTaskBreakpoint != null) {
                        onTaskBreakpoint();
                    }
                }

                if (behaviorTree.behavior.LogTaskChanges) {
                    print(string.Format("{0}: {1}: Push task {2} ({3}, index {4}) at stack index {5}", RoundedTime(), behaviorTree.behavior.ToString(),
                                    behaviorTree.taskList[taskIndex].FriendlyName, behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex));
                }
#endif
                behaviorTree.taskList[taskIndex].OnStart();
            }
        }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        private void SetInactiveExecutionStatus(BehaviorTree behaviorTree, int taskIndex)
        {
            if (behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus != TaskStatus.Inactive && !behaviorTree.conditionalReevaluateMap.ContainsKey(taskIndex)) {
                behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = TaskStatus.Inactive;
                if (behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]] is BehaviorReference) {
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = TaskStatus.Inactive;
                }

                if (behaviorTree.taskList[taskIndex] is ParentTask) {
                    for (int i = 0; i < behaviorTree.childrenIndex[taskIndex].Count; ++i) {
                        SetInactiveExecutionStatus(behaviorTree, behaviorTree.childrenIndex[taskIndex][i]);
                    }
                }
            }
        }
#endif

        private void PopTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren)
        {
            PopTask(behaviorTree, taskIndex, stackIndex, ref status, popChildren, true);
        }

        private void PopTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren, bool notifyOnEmptyStack)
        {
            // return immediately if the behavior tree isn't enabled or the stack index is larger then the number of items on the stack.
            // this latter case can happen if you restart the behavior tree within a task.
            if (!IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.Count || behaviorTree.activeStack[stackIndex].Count == 0) {
                return;
            }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (taskIndex != behaviorTree.activeStack[stackIndex].Peek()) {
                print("error: popping " + taskIndex + " but " + behaviorTree.activeStack[stackIndex].Peek() + " is on top");
            }
#endif

            behaviorTree.activeStack[stackIndex].Pop();
            behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Inactive;
            // notify any third party plugin that the task has stopped
            StopThirdPartyTask(behaviorTree, taskIndex);
            behaviorTree.taskList[taskIndex].OnEnd();

            int parentIndex = behaviorTree.parentIndex[taskIndex];
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // update the referenced task if this task is an external task
            if (behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]] is BehaviorReference) {
                // this task needs to be the first external task
                if (parentIndex == -1 || !(behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]] is BehaviorReference)) {
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = -1;
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PopTime = Time.realtimeSinceStartup;
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = status;
                }
            }
            behaviorTree.taskList[taskIndex].NodeData.PushTime = -1;
            behaviorTree.taskList[taskIndex].NodeData.PopTime = Time.realtimeSinceStartup;
            behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = status;
            if (behaviorTree.behavior.LogTaskChanges) {
                print(string.Format("{0}: {1}: Pop task {2} ({3}, index {4}) at stack index {5} with status {6}", RoundedTime(), behaviorTree.behavior.ToString(),
                                    behaviorTree.taskList[taskIndex].FriendlyName, behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex, status));
            }
#endif

            // let the parent know
            if (parentIndex != -1) {
                if (behaviorTree.taskList[taskIndex] is Conditional) {
                    int compositeParentIndex = behaviorTree.parentCompositeIndex[taskIndex];
                    if (compositeParentIndex != -1) {
                        var compositeTask = behaviorTree.taskList[compositeParentIndex] as Composite;
                        if (compositeTask.AbortType != AbortType.None) {
                            // The abort type will be self until the composite task is popped
                            var conditionalReval = ObjectPool.Get<BehaviorTree.ConditionalReevaluate>();
                            conditionalReval.Initialize(taskIndex, status, stackIndex, (compositeTask.AbortType != AbortType.LowerPriority ? compositeParentIndex : -1));
                            behaviorTree.conditionalReevaluate.Add(conditionalReval);
                            behaviorTree.conditionalReevaluateMap.Add(taskIndex, conditionalReval);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            behaviorTree.taskList[taskIndex].NodeData.IsReevaluating = compositeTask.AbortType == AbortType.Self || compositeTask.AbortType == AbortType.Both;
#endif
                        }
                    }
                }
                var parentTask = behaviorTree.taskList[parentIndex] as ParentTask;
                if (!parentTask.CanRunParallelChildren()) {
                    parentTask.OnChildExecuted(status);
                    status = parentTask.Decorate(status);
                } else {
                    parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], status);
                }
            }

            if (behaviorTree.taskList[taskIndex] is Composite) {
                var compositeTask = behaviorTree.taskList[taskIndex] as Composite;

                // no longer observing if the type is self or there are no more tasks in the stack
                if (compositeTask.AbortType == AbortType.Self || compositeTask.AbortType == AbortType.None || behaviorTree.activeStack[stackIndex].Count == 0) {
                    RemoveChildConditionalReevaluate(behaviorTree, taskIndex);
                } else if (compositeTask.AbortType == AbortType.LowerPriority || compositeTask.AbortType == AbortType.Both) {
                    // the conditional task now becomes active so it will be reevaluated
                    for (int i = 0; i < behaviorTree.childConditionalIndex[taskIndex].Count; ++i) {
                        int conditionalIndex = behaviorTree.childConditionalIndex[taskIndex][i];
                        // the key may not exist if the stack is empty
                        if (behaviorTree.conditionalReevaluateMap.ContainsKey(conditionalIndex)) {
                            behaviorTree.conditionalReevaluateMap[conditionalIndex].compositeIndex = behaviorTree.parentCompositeIndex[taskIndex];
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            behaviorTree.taskList[conditionalIndex].NodeData.IsReevaluating = true;
#endif
                        }
                    }
                    // Update the composite index with the parent composite so the correct LCA between the active task and the conditional task will be found
                    for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                        if (behaviorTree.conditionalReevaluate[i].compositeIndex == taskIndex) {
                            behaviorTree.conditionalReevaluate[i].compositeIndex = behaviorTree.parentCompositeIndex[taskIndex];
                        }
                    }
                }
            } else if (behaviorTree.taskList[taskIndex] is Decorator) {
                var decoratorTask = behaviorTree.taskList[taskIndex] as Decorator;
                if (decoratorTask.CanReevaluate()) {
                    for (int i = behaviorTree.decoratorReevaluate.Count - 1; i > -1; --i) {
                        if (behaviorTree.decoratorReevaluate[i] == taskIndex) {
                            behaviorTree.decoratorReevaluate.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            // pop any task whose base parent is equal to the base parent of the current task being popped
            if (popChildren) {
                for (int i = behaviorTree.activeStack.Count - 1; i > stackIndex; --i) {
                    if (behaviorTree.activeStack[i].Count > 0) {
                        if (IsParentTask(behaviorTree, taskIndex, behaviorTree.activeStack[i].Peek())) {
                            var childStatus = TaskStatus.Failure;
                            int stackCount = behaviorTree.activeStack[i].Count;
                            while (stackCount > 0) {
                                PopTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref childStatus, false, notifyOnEmptyStack);
                                stackCount--;
                            }
                        }
                    }
                }
            }

            // If there are no more items in the stack, restart the tree (in the case of the root task) or remove the stack created by the parallel task
            if (behaviorTree.activeStack[stackIndex].Count == 0) {
                if (stackIndex == 0) {
                    // restart the tree
                    if (notifyOnEmptyStack) {
                        if (behaviorTree.behavior.RestartWhenComplete) {
                            Restart(behaviorTree);
                        } else {
                            DisableBehavior(behaviorTree.behavior);
                            behaviorTree.behavior.ExecutionStatus = status;
                        }
                    }
                    status = TaskStatus.Inactive;
                } else {
                    // don't remove the stack from the very first index
                    RemoveStack(behaviorTree, stackIndex);

                    // set the status to running to prevent the loop from running again within Update
                    status = TaskStatus.Running;
                }
            }
        }

        private void RemoveChildConditionalReevaluate(BehaviorTree behaviorTree, int compositeIndex)
        {
            for (int i = behaviorTree.conditionalReevaluate.Count - 1; i > -1; --i) {
                if (behaviorTree.conditionalReevaluate[i].compositeIndex == compositeIndex) {
                    ObjectPool.Return(behaviorTree.conditionalReevaluate[i]);
                    int conditionalIndex = behaviorTree.conditionalReevaluate[i].index;
                    behaviorTree.conditionalReevaluateMap.Remove(conditionalIndex);
                    behaviorTree.conditionalReevaluate.RemoveAt(i);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    behaviorTree.taskList[conditionalIndex].NodeData.IsReevaluating = false;
#endif
                }
            }
        }

        private void Restart(BehaviorTree behaviorTree)
        {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behaviorTree.behavior.LogTaskChanges) {
                Debug.Log(string.Format("{0}: Restarting {1}", RoundedTime(), behaviorTree.behavior.ToString()));
            }
#endif
            // Remove all of the conditional aborts that haven't had a chance to run yet
            RemoveChildConditionalReevaluate(behaviorTree, -1);

            PushTask(behaviorTree, 0, 0);
        }

        // returns if possibleParent is a parent of possibleChild
        private bool IsParentTask(BehaviorTree behaviorTree, int possibleParent, int possibleChild)
        {
            int parentIndex = 0;
            int childIndex = possibleChild;
            while (childIndex != -1) {
                parentIndex = behaviorTree.parentIndex[childIndex];
                if (parentIndex == possibleParent) {
                    return true;
                }
                childIndex = parentIndex;
            }
            return false;
        }

        // a task has been interrupted. Store the interrupted index so the update loop knows to stop executing tasks with a parent task equal to the interrupted task
        public void Interrupt(Behavior behavior, Task task)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            // determine the index of the task that is causing the interruption
            int interruptionIndex = -1;
            var behaviorTree = behaviorTreeMap[behavior];
            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                if (behaviorTree.taskList[i].ReferenceID == task.ReferenceID) {
                    interruptionIndex = i;
                    break;
                }
            }

            if (interruptionIndex > -1) {
                // loop through the active tasks. Mark any stack that has interruption index as its parent
                int taskIndex;
                for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                    if (behaviorTree.activeStack[i].Count > 0) {
                        taskIndex = behaviorTree.activeStack[i].Peek();
                        while (taskIndex != -1) {
                            if (taskIndex == interruptionIndex) {
                                behaviorTree.interruptionIndex[i] = interruptionIndex;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                                if (behavior.LogTaskChanges) {
                                    Debug.Log(string.Format("{0}: {1}: Interrupt task {2} ({3}) with interrupt index {4} at stack index {5}", RoundedTime(), behaviorTree.behavior.ToString(),
                                                            task.FriendlyName, task.GetType().ToString(), interruptionIndex, i));
                                }
#endif
                                break;
                            }
                            taskIndex = behaviorTree.parentIndex[taskIndex];
                        }
                    }
                }
            }
        }

        public void StopThirdPartyTask(BehaviorTree behaviorTree, int taskIndex)
        {
            // stop the third party task if it is running
            thirdPartyTaskCompare.Task = behaviorTree.taskList[taskIndex];
            if (taskObjectMap.ContainsKey(thirdPartyTaskCompare)) {
                var thirdPartyObjectType = objectTaskMap[taskObjectMap[thirdPartyTaskCompare]].ThirdPartyObjectType;
                if (invokeParameters == null) {
                    invokeParameters = new object[1];
                }
                invokeParameters[0] = behaviorTree.taskList[taskIndex];
                switch (thirdPartyObjectType) {
                    case ThirdPartyObjectType.PlayMaker:
                        PlayMakerStopMethod.Invoke(null, invokeParameters);
                        break;
                    case ThirdPartyObjectType.uScript:
                        UScriptStopMethod.Invoke(null, invokeParameters);
                        break;
                    case ThirdPartyObjectType.DialogueSystem:
                        DialogueSystemStopMethod.Invoke(null, invokeParameters);
                        break;
                    case ThirdPartyObjectType.uSequencer:
                        USequencerStopMethod.Invoke(null, invokeParameters);
                        break;
                    case ThirdPartyObjectType.AIForMecanim:
                        AIForMecanimStopMethod.Invoke(null, invokeParameters);
                        break;
                    case ThirdPartyObjectType.SimpleWaypointSystem:
                        SimpleWaypointSystemStopMethod.Invoke(null, invokeParameters);
                        break;
                }

                RemoveActiveThirdPartyTask(behaviorTree.taskList[taskIndex]);
            }
        }

        public void RemoveActiveThirdPartyTask(Task task)
        {
            thirdPartyTaskCompare.Task = task;
            if (taskObjectMap.ContainsKey(thirdPartyTaskCompare)) {
                var obj = taskObjectMap[thirdPartyTaskCompare];
                taskObjectMap.Remove(thirdPartyTaskCompare);
                objectTaskMap.Remove(obj);
            }
        }

        // remove the stack at stackIndex
        private void RemoveStack(BehaviorTree behaviorTree, int stackIndex)
        {
            var stack = behaviorTree.activeStack[stackIndex];
            stack.Clear();
            ObjectPool.Return(stack);
            behaviorTree.activeStack.RemoveAt(stackIndex);
            behaviorTree.interruptionIndex.RemoveAt(stackIndex);
            behaviorTree.nonInstantTaskStatus.RemoveAt(stackIndex);
        }

        // Find the LCA of the two tasks
        private int FindLCA(BehaviorTree behaviorTree, int taskIndex1, int taskIndex2)
        {
            var set = ObjectPool.Get<HashSet<int>>();
            set.Clear();
            int parentIndex = taskIndex1;
            while (parentIndex != -1) {
                set.Add(parentIndex);
                parentIndex = behaviorTree.parentIndex[parentIndex];
            }

            parentIndex = taskIndex2;
            while (!set.Contains(parentIndex)) {
                parentIndex = behaviorTree.parentIndex[parentIndex];
            }

            return parentIndex;
        }

        public List<Task> GetActiveTasks(Behavior behavior)
        {
            if (!IsBehaviorEnabled(behavior)) {
                return null;
            }

            var activeTasks = new List<Task>();
            var behaviorTree = behaviorTreeMap[behavior];
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                var task = behaviorTree.taskList[behaviorTree.activeStack[i].Peek()];
                if (!(task is BehaviorDesigner.Runtime.Tasks.Action)) {
                    continue;
                }
                activeTasks.Add(task);
            }
            return activeTasks;
        }

        // Forward the collision/trigger callback to the active task
        public void BehaviorOnCollisionEnter(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionEnter(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionEnter(collision);
            }
        }

        public void BehaviorOnCollisionExit(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionExit(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionExit(collision);
            }
        }

        public void BehaviorOnCollisionStay(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionStay(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionStay(collision);
            }
        }

        public void BehaviorOnTriggerEnter(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerEnter(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerEnter(other);
            }
        }

        public void BehaviorOnTriggerExit(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerExit(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerExit(other);
            }
        }

        public void BehaviorOnTriggerStay(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerStay(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerStay(other);
            }
        }

#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        public void BehaviorOnCollisionEnter2D(Collision2D collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionEnter2D(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionEnter2D(collision);
            }
        }

        public void BehaviorOnCollisionExit2D(Collision2D collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionExit2D(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionExit2D(collision);
            }
        }

        public void BehaviorOnCollisionStay2D(Collision2D collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnCollisionStay2D(collision);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnCollisionStay2D(collision);
            }
        }

        public void BehaviorOnTriggerEnter2D(Collider2D other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerEnter2D(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerEnter2D(other);
            }
        }

        public void BehaviorOnTriggerExit2D(Collider2D other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerExit2D(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerExit2D(other);
            }
        }

        public void BehaviorOnTriggerStay2D(Collider2D other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                while (taskIndex != -1) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                        break;
                    }
#endif
                    behaviorTree.taskList[taskIndex].OnTriggerStay2D(other);
                    taskIndex = behaviorTree.parentIndex[taskIndex];
                }
            }

            for (int i = 0; i < behaviorTree.conditionalReevaluate.Count; ++i) {
                taskIndex = behaviorTree.conditionalReevaluate[i].index;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                    continue;
                }
#endif
                behaviorTree.taskList[taskIndex].OnTriggerStay2D(other);
            }
        }
#endif

        // third party support
        public bool MapObjectToTask(object objectKey, Task task, ThirdPartyObjectType objectType)
        {
            if (objectTaskMap.ContainsKey(objectKey)) {
                string thirdPartyName = "";
                switch (objectType) {
                    case ThirdPartyObjectType.PlayMaker:
                        thirdPartyName = "PlayMaker FSM";
                        break;
                    case ThirdPartyObjectType.uScript:
                        thirdPartyName = "uScript Graph";
                        break;
                    case ThirdPartyObjectType.DialogueSystem:
                        thirdPartyName = "Dialogue System";
                        break;
                    case ThirdPartyObjectType.uSequencer:
                        thirdPartyName = "uSequencer sequence";
                        break;
                    case ThirdPartyObjectType.AIForMecanim:
                        thirdPartyName = "AI For Mecanim state machine";
                        break;
                    case ThirdPartyObjectType.SimpleWaypointSystem:
                        thirdPartyName = "Simple Waypoint System waypoint";
                        break;
                }
                Debug.LogError(string.Format("Only one behavior can be mapped to the same instance of the {0}.", thirdPartyName));
                return false;
            }
            var thirdPartyTask = new ThirdPartyTask(task, objectType);
            objectTaskMap.Add(objectKey, thirdPartyTask);
            taskObjectMap.Add(thirdPartyTask, objectKey);
            return true;
        }

        public Task TaskForObject(object objectKey)
        {
            if (!objectTaskMap.ContainsKey(objectKey)) {
                return null;
            }
            return objectTaskMap[objectKey].Task;
        }

        private decimal RoundedTime()
        {
            return Math.Round((decimal)Time.time, 5, MidpointRounding.AwayFromZero);
        }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public List<Task> GetTaskList(Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return null;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            return behaviorTree.taskList;
        }

        public bool SetShouldShowExternalTree(Behavior behavior, bool show)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior))
                return false;

            showExternalTrees = show;
            bool dirty = false;
            var behaviorTree = behaviorTreeMap[behavior];
            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
#pragma warning disable 0618
                if (behaviorTree.originalTaskList[behaviorTree.originalIndex[i]] is BehaviorReference) {
                    int parentIndex = behaviorTree.parentIndex[i];
                    // swap out the root
                    if (parentIndex == -1) {
                        var task = (showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]]);
                        behavior.GetBehaviorSource().RootTask = task;
                        dirty = true;
                    } else if (!(behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]] is BehaviorReference)) {
                        (behaviorTree.taskList[parentIndex] as ParentTask).Children[behaviorTree.relativeChildIndex[i]] = (showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]]);
                        dirty = true;
                    }
                }
#pragma warning restore 0618
            }

            return dirty;
        }
#endif
    }
}