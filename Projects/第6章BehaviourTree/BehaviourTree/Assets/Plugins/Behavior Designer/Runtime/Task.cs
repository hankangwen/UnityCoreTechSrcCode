using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public enum TaskStatus
    {
        Inactive,
        Failure,
        Success,
        Running
    }

    public abstract class Task
    {
        // OnAwake is called once when the behavior tree is enabled. Think of it as a constructor
        public virtual void OnAwake() { }

        // OnStart is called immediately before execution. It is used to setup any variables that need to be reset from the previous run
        public virtual void OnStart() { }

        // OnUpdate runs the actual task
        public virtual TaskStatus OnUpdate() { return TaskStatus.Success; }

        // OnEnd is called after execution on a success or failure.
        public virtual void OnEnd() { }

        // OnPause is called when the behavior is paused and resumed
        public virtual void OnPause(bool paused) { }

        // The priority select will need to know this tasks priority of running
        public virtual float GetPriority() { return 0; }

        // OnBehaviorRestart is called after a complete behavior execution and the behavior is going to restart
        [System.Obsolete("Task.OnBehaviorRestart has been deprecated. Use Task.OnBehaviorComplete() instead")]
        public virtual void OnBehaviorRestart() { }

        // OnBehaviorComplete is called after the behavior tree finishes executing
        public virtual void OnBehaviorComplete() { }

        // OnReset is called by the inspector to reset the public properties
        public virtual void OnReset() { }

        // Same as Editor.OnSceneGUI except this is executed on a runtime class
        [System.Obsolete("Task.OnSceneGUI has been deprecated. Use Task.OnDrawGizmos() instead")]
        public virtual void OnSceneGUI() { }

        // Allow OnDrawGizmos to be called from the tasks
        public virtual void OnDrawGizmos() { }

        // Support coroutines within the task
        protected void StartCoroutine(string methodName) { Owner.StartTaskCoroutine(this, methodName); }
        protected Coroutine StartCoroutine(System.Collections.IEnumerator routine) { return Owner.StartCoroutine(routine); }
        protected Coroutine StartCoroutine(string methodName, object value) { return Owner.StartTaskCoroutine(this, methodName, value); }
        protected void StopCoroutine(string methodName) { Owner.StopTaskCoroutine(methodName); }
        protected void StopAllCoroutines() { Owner.StopAllTaskCoroutines(); }

        // Support collision/trigger callbacks:
        public virtual void OnCollisionEnter(Collision collision) { }
        public virtual void OnCollisionExit(Collision collision) { }
        public virtual void OnCollisionStay(Collision collision) { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        public virtual void OnCollisionEnter2D(Collision2D collision) { }
        public virtual void OnCollisionExit2D(Collision2D collision) { }
        public virtual void OnCollisionStay2D(Collision2D collision) { }
        public virtual void OnTriggerEnter2D(Collider2D other) { }
        public virtual void OnTriggerExit2D(Collider2D other) { }
        public virtual void OnTriggerStay2D(Collider2D other) { }
#endif

        // MonoBehaviour components:
#pragma warning disable 0618
        public Animation Animation { set { animation = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.animation is deprecated. You should cache this reference within the task.")]
        protected Animation animation;
        public AudioSource Audio { set { audio = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.audio is deprecated. You should cache this reference within the task.")]
        protected AudioSource audio;
        public Camera Camera { set { camera = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.camera is deprecated. You should cache this reference within the task.")]
        protected Camera camera;
        public Collider Collider { set { collider = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.collider is deprecated. You should cache this reference within the task.")]
        protected Collider collider;
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        public Collider2D Collider2D { set { collider2D = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.collider2D is deprecated. You should cache this reference within the task.")]
        protected Collider2D collider2D;
#endif
        public ConstantForce ConstantForce { set { constantForce = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.constantForce is deprecated. You should cache this reference within the task.")]
        protected ConstantForce constantForce;
        public GameObject GameObject { set { gameObject = value; } }
        protected GameObject gameObject;
        public GUIText GUIText { set { guiText = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.guiText is deprecated. You should cache this reference within the task.")]
        protected GUIText guiText;
        public GUITexture GUITexture { set { guiTexture = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.guiTexture is deprecated. You should cache this reference within the task.")]
        protected GUITexture guiTexture;
        public HingeJoint HingeJoint { set { hingeJoint = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.hingeJoint is deprecated. You should cache this reference within the task.")]
        protected HingeJoint hingeJoint;
        public Light Light { set { light = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.light is deprecated. You should cache this reference within the task.")]
        protected Light light;
#if !UNITY_WINRT && !DLL_DEBUG && !DLL_RELEASE
        public NetworkView NetworkView { set { networkView = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.networkView is deprecated. You should cache this reference within the task.")]
        protected NetworkView networkView;
#endif
        public ParticleEmitter ParticleEmitter { set { particleEmitter = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.particleEmitter is deprecated. You should cache this reference within the task.")]
        protected ParticleEmitter particleEmitter;
        public ParticleSystem ParticleSystem { set { particleSystem = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.particleSystem is deprecated. You should cache this reference within the task.")]
        protected ParticleSystem particleSystem;
        public Renderer Renderer { set { renderer = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.renderer is deprecated. You should cache this reference within the task.")]
        protected Renderer renderer;
        public Rigidbody Rigidbody { set { rigidbody = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.rigidbody is deprecated. You should cache this reference within the task.")]
        protected Rigidbody rigidbody;
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        public Rigidbody2D Rigidbody2D { set { rigidbody2D = value; } }
        [System.Obsolete("To ensure compatibility with Unity 5, Task.rigidbody2D is deprecated. You should cache this reference within the task.")]
        protected Rigidbody2D rigidbody2D;
#endif
        public Transform Transform { set { transform = value; } }
        protected Transform transform;
#pragma warning restore 0618

        protected T GetComponent<T>() where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        // Return the inputted GameObject if it is not null, otherwise return the current GameObject component
        protected GameObject GetDefaultGameObject(GameObject go)
        {
            if (go == null) {
                return gameObject;
            }
            return go;
        }

        // NodeData contains properties used by the editor
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        [SerializeField]
        private NodeData nodeData = null;
        public NodeData NodeData { get { return nodeData; } set { nodeData = value; } }
#endif

        // Keep a reference to the behavior that owns this task
        [SerializeField]
        private Behavior owner = null;
        public Behavior Owner { get { return owner; } set { owner = value; } }

        // The unique id of the task
        [SerializeField]
        private int id = -1;
        public int ID { get { return id; } set { id = value; } }

        [SerializeField]
        private string friendlyName = "";
        public string FriendlyName { get { return friendlyName; } set { friendlyName = value; } }

        [SerializeField]
        private bool instant = true;
        public bool IsInstant { get { return instant; } set { instant = value; } }

        private int referenceID = -1;
        public int ReferenceID { get { return referenceID; } set { referenceID = value; } }
    }
}