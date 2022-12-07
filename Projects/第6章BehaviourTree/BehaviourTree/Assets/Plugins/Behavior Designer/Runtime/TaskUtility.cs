using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class TaskUtility
    {
        [System.NonSerialized]
        private static Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();

        public static FieldInfo[] GetAllFields(Type t)
        {
            var fieldList = new List<FieldInfo>();
            GetAllFields(t, ref fieldList);
            return fieldList.ToArray();
        }

        private static void GetAllFields(Type t, ref List<FieldInfo> fieldList)
        {
            if (t == null || t.Equals(typeof(ParentTask)) || t.Equals(typeof(Task)) || t.Equals(typeof(SharedVariable)))
                return;

#if UNITY_WINRT && !UNITY_EDITOR
            var flags = System.BindingFlags.Public | System.BindingFlags.NonPublic | System.BindingFlags.Instance | System.BindingFlags.DeclaredOnly;
#else
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
#endif
            var fields = t.GetFields(flags);
            for (int i = 0; i < fields.Length; ++i) {
                fieldList.Add(fields[i]);
            }
#if UNITY_WINRT && !UNITY_EDITOR
            GetAllFields(t.BaseType(), ref fieldList);
#else
            GetAllFields(t.BaseType, ref fieldList);
#endif
        }

        public static Type GetTypeWithinAssembly(string typeName)
        {
            // cache the results for quicker repeated lookup
            if (typeDictionary.ContainsKey(typeName)) {
                return typeDictionary[typeName];
            }

            var type = Type.GetType(typeName);
            // if type is null try a different namespace
            if (type == null) {
                for (int i = 0; i < 6; ++i) {
                    type = Type.GetType(string.Format("{0}, {1}", typeName, GetAssemblyName(i)));
                    if (type != null)
                        break;
                }
            }
            if (type != null) {
                typeDictionary.Add(typeName, type);
            }
            return type;
        }

        public static string GetAssemblyName(int index)
        {
            switch (index) {
                case 0:
                    return "Assembly-CSharp";
                case 1:
                    return "Assembly-CSharp-firstpass";
                case 2:
                    return "Assembly-UnityScript";
                case 3:
                    return "Assembly-UnityScript-firstpass";
                case 4:
                    return "Assembly-Boo";
                case 5:
                    return "Assembly-Boo-firstpass";
            }
            return "";
        }

        public static bool CompareType(Type t, string typeName)
        {
            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) {
                type = System.Type.GetType(typeName + ", Assembly-CSharp-firstpass");
            }
            return t.Equals(type);
        }

        public static bool HasAttribute(FieldInfo field, Type attribute)
        {
#if !UNITY_EDITOR && UNITY_WINRT && NETFX_CORE
            return field.GetCustomAttribute(attribute) != null;
#else
            return field.GetCustomAttributes(attribute, false).Length > 0;
#endif
        }

        public static Type SharedVariableToConcreteType(Type sharedVariableType)
        {
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedInt"))) {
                return typeof(int);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedFloat"))) {
                return typeof(float);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedBool"))) {
                return typeof(bool);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString"))) {
                return typeof(string);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedVector2"))) {
                return typeof(Vector2);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedVector3"))) {
                return typeof(Vector3);
            }
            if (sharedVariableType.Equals(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedGameObject"))) {
                return typeof(GameObject);
            }
            return null;
        }
    }
}