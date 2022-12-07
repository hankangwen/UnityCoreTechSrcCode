using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    public abstract class SharedVariable
    {
        public bool IsShared { get { return mIsShared; } set { mIsShared = value; } }
        [SerializeField]
        private bool mIsShared = false;

        public bool IsGlobal { get { return mIsGlobal; } set { mIsGlobal = value; } }
        [SerializeField]
        private bool mIsGlobal = false;

        public string Name { get { return mName; } set { mName = value; } }
        [SerializeField]
        private string mName;

        public bool IsNone { get { return mIsShared && string.IsNullOrEmpty(mName); } }

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }
}