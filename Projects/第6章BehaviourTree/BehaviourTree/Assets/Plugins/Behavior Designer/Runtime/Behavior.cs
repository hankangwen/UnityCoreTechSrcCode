using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public abstract class Behavior : MonoBehaviour, IBehavior
    {
        [SerializeField]
        private bool startWhenEnabled = true;
        public bool StartWhenEnabled { get { return startWhenEnabled; } set { startWhenEnabled = value; } }
        [SerializeField]
        private bool pauseWhenDisabled = false;
        public bool PauseWhenDisabled { get { return pauseWhenDisabled; } set { pauseWhenDisabled = value; } }
        [SerializeField]
        private bool restartWhenComplete = false;
        public bool RestartWhenComplete { get { return restartWhenComplete; } set { restartWhenComplete = value; } }
        [SerializeField]
        private bool logTaskChanges = false;
        public bool LogTaskChanges { get { return logTaskChanges; } set { logTaskChanges = value; } }
        [SerializeField]
        private int group = 0;
        public int Group { get { return group; } set { group = value; } }
        // reference to an external behavior tree, useful if creating a behavior tree from script
        [SerializeField]
        private ExternalBehavior externalBehavior;
        public ExternalBehavior ExternalBehavior { get { return externalBehavior; } set { if (externalBehavior != value) mBehaviorSource.HasSerialized = false; externalBehavior = value; } }
        public string BehaviorName { get { return mBehaviorSource.behaviorName; } set { mBehaviorSource.behaviorName = value; } }
        public string BehaviorDescription { get { return mBehaviorSource.behaviorDescription; } set { mBehaviorSource.behaviorDescription = value; } }

        [SerializeField]
        private BehaviorSource mBehaviorSource;
        public BehaviorSource GetBehaviorSource() { return mBehaviorSource; }
        public void SetBehaviorSource(BehaviorSource behaviorSource) { mBehaviorSource = behaviorSource; }
        public UnityEngine.Object GetObject() { return this; }
        public string GetOwnerName() { return gameObject.name; }
        [SerializeField]
        private bool isSceneObject;
        public bool IsSceneObject { get { return isSceneObject; } set { isSceneObject = value; } }

        private bool mIsPaused = false;
        private TaskStatus mExecutionStatus = TaskStatus.Inactive;
        public TaskStatus ExecutionStatus { get { return mExecutionStatus; } set { mExecutionStatus = value; } }
        private bool mInitialized = false;
                
        // coroutines
        private Dictionary<string, List<TaskCoroutine>> activeTaskCoroutines = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public bool showBehaviorDesignerGizmo = true;
#endif

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public void OnDrawGizmosSelected()
        {
            if (showBehaviorDesignerGizmo) {
                Gizmos.DrawIcon(transform.position, "Behavior Designer Scene Icon.png");
            }
        }
#endif

        public Behavior()
        {
            mBehaviorSource = new BehaviorSource(this);
        }

#pragma warning disable 0618
        public void Start()
        {
            if (startWhenEnabled) {
                EnableBehavior();
            }
            mInitialized = true;
        }

        public void EnableBehavior()
        {
            // create the behavior manager if it doesn't already exist
            CreateBehaviorManager();
            BehaviorManager.instance.EnableBehavior(this);
        }

        public void DisableBehavior()
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.DisableBehavior(this, pauseWhenDisabled);
                mIsPaused = pauseWhenDisabled;
            }
        }

        public void DisableBehavior(bool pause)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.DisableBehavior(this, pause);
                mIsPaused = pause;
            }
        }

        public void OnEnable()
        {
            if (BehaviorManager.instance != null && mIsPaused) {
                BehaviorManager.instance.EnableBehavior(this);
                mIsPaused = false;
            } else if (startWhenEnabled && mInitialized) {
                EnableBehavior();
            }
        }
#pragma warning restore 0618

        public void OnDisable()
        {
            DisableBehavior();
        }
        
        // Support blackboard variables:
        public SharedVariable GetVariable(string name)
        {
            CheckForSerialization();
            return mBehaviorSource.GetVariable(name);
        }

        public void SetVariable(string name, SharedVariable item)
        {
            CheckForSerialization();
            mBehaviorSource.SetVariable(name, item);
        }

        public List<SharedVariable> GetAllVariables()
        {
            CheckForSerialization();
            return mBehaviorSource.GetAllVariables();
        }

        private void CheckForSerialization()
        {
            if (ExternalBehavior != null) {
                externalBehavior.BehaviorSource.Owner = externalBehavior;
                externalBehavior.BehaviorSource.CheckForSerialization(!isSceneObject, mBehaviorSource);
            } else {
                mBehaviorSource.CheckForSerialization(!isSceneObject);
            }
        }

        // Support collisions/triggers:
        public void OnCollisionEnter(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionEnter(collision, this);
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionExit(collision, this);
            }
        }

        public void OnCollisionStay(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionStay(collision, this);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerEnter(other, this);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerExit(other, this);
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerStay(other, this);
            }
        }

#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionEnter2D(collision, this);
            }
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionExit2D(collision, this);
            }
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionStay2D(collision, this);
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerEnter2D(other, this);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerExit2D(other, this);
            }
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerStay2D(other, this);
            }
        }
#endif

        public void OnDrawGizmos()
        {
            OnDrawGizmos(mBehaviorSource.RootTask);

            var detachedTasks = mBehaviorSource.DetachedTasks;
            if (detachedTasks != null) {
                for (int i = 0; i < detachedTasks.Count; ++i) {
                    OnDrawGizmos(detachedTasks[i]);
                }
            }
        }

        private void OnDrawGizmos(Task task)
        {
            if (task == null) {
                return;
            }
            task.OnDrawGizmos();

            if (task is ParentTask) {
                var parentTask = task as ParentTask;
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        OnDrawGizmos(parentTask.Children[i]);
                    }
                }
            }
        }

        public T FindTask<T>() where T : Task
        {
            return FindTask<T>( mBehaviorSource.RootTask);
        }

        private T FindTask<T>(Task task) where T : Task
        {
            if (task.GetType().Equals(typeof(T))) {
                return (T)task;
            }

            ParentTask parentTask;
            if ((parentTask = task as ParentTask) != null) {
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        T foundTask = null;
                        if ((foundTask = FindTask<T>(parentTask.Children[i])) != null) {
                            return foundTask;
                        }
                    }
                }
            }

            return null;
        }

        public List<T> FindTasks<T>() where T : Task
        {
            List<T> tasks = new List<T>();
            FindTasks<T>(mBehaviorSource.RootTask, ref tasks);
            return tasks;
        }

        private void FindTasks<T>(Task task, ref List<T> taskList) where T : Task
        {
            if (task.GetType().Equals(typeof(T))) {
                taskList.Add((T)task);
            }

            ParentTask parentTask;
            if ((parentTask = task as ParentTask) != null) {
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        FindTasks<T>(parentTask.Children[i], ref taskList);
                    }
                }
            }
        }

        public Task FindTaskWithName(string taskName)
        {
            return FindTaskWithName(taskName, mBehaviorSource.RootTask);
        }

        private Task FindTaskWithName(string taskName, Task task)
        {
            if (task.FriendlyName.Equals(taskName)) {
                return task;
            }

            ParentTask parentTask;
            if ((parentTask = task as ParentTask) != null) {
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        Task foundTask = null;
                        if ((foundTask = FindTaskWithName(taskName, parentTask.Children[i])) != null) {
                            return foundTask;
                        }
                    }
                }
            }

            return null;
        }

        public List<Task> FindTasksWithName(string taskName)
        {
            List<Task> tasks = new List<Task>();
            FindTasksWithName(taskName, mBehaviorSource.RootTask, ref tasks);
            return tasks;
        }

        private void FindTasksWithName(string taskName, Task task, ref List<Task> taskList)
        {
            if (task.FriendlyName.Equals(taskName)) {
                taskList.Add(task);
            }

            ParentTask parentTask;
            if ((parentTask = task as ParentTask) != null) {
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        FindTasksWithName(taskName, parentTask.Children[i], ref taskList);
                    }
                }
            }
        }

        public List<Task> GetActiveTasks()
        {
            if (BehaviorManager.instance == null) {
                return null;
            }
            return BehaviorManager.instance.GetActiveTasks(this);
        }

        // ScriptableObjects don't normally support coroutines. Add that support here.
        public Coroutine StartTaskCoroutine(Task task, string methodName)
        {
#if !UNITY_EDITOR && UNITY_WINRT
            var method = task.GetType().GetMethod(methodName, System.BindingFlags.Public |System.BindingFlags.NonPublic | System.BindingFlags.Instance);
#else
            var method = task.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
            if (method == null) {
                Debug.LogError("Unable to start coroutine " + methodName + ": method not found");
                return null;
            }
            if (activeTaskCoroutines == null) {
                activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
            }
            var taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[] { }), methodName);
            if (activeTaskCoroutines.ContainsKey(methodName)) {
                var taskCoroutines = activeTaskCoroutines[methodName];
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines[methodName] = taskCoroutines;
            } else {
                var taskCoroutines = new List<TaskCoroutine>();
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines.Add(methodName, taskCoroutines);
            }
            return taskCoroutine.Coroutine;
        }

        public Coroutine StartTaskCoroutine(Task task, string methodName, object value)
        {
#if !UNITY_EDITOR && UNITY_WINRT
            var method = task.GetType().GetMethod(methodName, System.BindingFlags.Public |System.BindingFlags.NonPublic | System.BindingFlags.Instance);
#else
            var method = task.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
            if (method == null) {
                Debug.LogError("Unable to start coroutine " + methodName + ": method not found");
                return null;
            }
            if (activeTaskCoroutines == null) {
                activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
            }
            var taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[] { value }), methodName);
            if (activeTaskCoroutines.ContainsKey(methodName)) {
                var taskCoroutines = activeTaskCoroutines[methodName];
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines[methodName] = taskCoroutines;
            } else {
                var taskCoroutines = new List<TaskCoroutine>();
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines.Add(methodName, taskCoroutines);
            }
            return taskCoroutine.Coroutine;
        }

        public void StopTaskCoroutine(string methodName)
        {
            if (!activeTaskCoroutines.ContainsKey(methodName)) {
                return;
            }

            var taskCoroutines = activeTaskCoroutines[methodName];
            for (int i = 0; i < taskCoroutines.Count; ++i) {
                taskCoroutines[i].Stop();
            }
        }

        public void StopAllTaskCoroutines()
        {
            StopAllCoroutines();

            foreach (var entry in activeTaskCoroutines) {
                var taskCoroutines = entry.Value;
                for (int i = 0; i < taskCoroutines.Count; ++i) {
                    taskCoroutines[i].Stop();
                }
            }
        }

        public void TaskCoroutineEnded(TaskCoroutine taskCoroutine, string coroutineName)
        {
            if (activeTaskCoroutines.ContainsKey(coroutineName)) {
                var taskCoroutines = activeTaskCoroutines[coroutineName];
                if (taskCoroutines.Count == 1) {
                    activeTaskCoroutines.Remove(coroutineName);
                } else {
                    taskCoroutines.Remove(taskCoroutine);
                    activeTaskCoroutines[coroutineName] = taskCoroutines;
                }
            }
        }

        public override string ToString()
        {
            return mBehaviorSource.ToString();
        }

        public static BehaviorManager CreateBehaviorManager()
        {
            if (BehaviorManager.instance == null) {
                var behaviorManager = new GameObject();
                //behaviorManager.hideFlags = HideFlags.HideAndDontSave;
                behaviorManager.name = "Behavior Manager";
                return behaviorManager.AddComponent<BehaviorManager>();
            }
            return null;
        }
    }
}