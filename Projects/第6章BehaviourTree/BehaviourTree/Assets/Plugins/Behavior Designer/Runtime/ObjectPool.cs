using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public static class ObjectPool
    {
        private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();

        public static T Get<T>()
        {
            if (poolDictionary.ContainsKey(typeof(T))) {
                var pooledObjects = poolDictionary[typeof(T)] as List<T>;
                if (pooledObjects.Count > 0) {
                    var obj = pooledObjects[0];
                    pooledObjects.RemoveAt(0);
                    return obj;
                }
            }
            return Activator.CreateInstance<T>();
        }

        public static void Return<T>(T obj)
        {
            if (poolDictionary.ContainsKey(typeof(T))) {
                var pooledObjects = poolDictionary[typeof(T)] as List<T>;
                pooledObjects.Add(obj);
            } else {
                var pooledObjects = new List<T>();
                pooledObjects.Add(obj);
                poolDictionary.Add(typeof(T), pooledObjects);
            }
        }
    }
}