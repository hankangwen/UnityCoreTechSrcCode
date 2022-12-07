#if UNITY_WINRT && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    [Flags]
    public enum BindingFlags
    {
        Default,
        Public,
        Instance,
        InvokeMethod,
        NonPublic,
        Static,
        FlattenHierarchy,
        DeclaredOnly
    }

    public static class ReflectionExtensions
    {
        public static bool IsEnum(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static FieldInfo GetField(this Type type, string name)
        {
#if NETFX_CORE
            return type.GetRuntimeField(name);
#else
            return type.GetField(name);
#endif
        }

        public static FieldInfo[] GetFields(this Type type)
        {
            return GetFields(type, BindingFlags.Default);
        }

        public static FieldInfo[] GetFields(this Type t, BindingFlags flags)
        {
#if NETFX_CORE
            if (!flags.HasFlag(BindingFlags.Instance) && !flags.HasFlag(BindingFlags.Static)) return null;

            var ti = t.GetTypeInfo();
            var origFields = ti.DeclaredFields;
            var results = new List<FieldInfo>();
            foreach (var field in origFields)
            {
                var isValid = (flags.HasFlag(BindingFlags.Public) && field.IsPublic) || (flags.HasFlag(BindingFlags.NonPublic) && !field.IsPublic);
                isValid &= (flags.HasFlag(BindingFlags.Static) && field.IsStatic) || (flags.HasFlag(BindingFlags.Instance) && !field.IsStatic);
                if (flags.HasFlag(BindingFlags.DeclaredOnly))
                    isValid &= field.DeclaringType == t;

                results.Add(field);
            }
            return results.ToArray();
#else
            return t.GetFields(flags);
#endif
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetDeclaredMethod(name);
#else
            return type.GetMethod(name, flags);
#endif
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] types)
        {
#if NETFX_CORE
            var allMethods = type.GetTypeInfo().GetDeclaredMethods(name);
            if (allMethods == null) {
                return null;
            }
            foreach (var method in allMethods) {
                var parameters = method.GetParameters();
                if (types.Length != parameters.Length) {
                    continue;
                }
                bool match = true;
                for (int j = 0; j < parameters.Length; ++j) {
                    if (!types[j].Equals(parameters[j].ParameterType)) {
                        match = false;
                        break;
                    }
                }
                if (match) {
                    return method;
                }
            }
            return null;
#else
            return type.GetMethod(name, types);
#endif
        }

        public static MethodInfo GetMethod(this Type type, string name)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetDeclaredMethod(name);
#else
            return type.GetMethod(name);
#endif
        }

        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
#else
            return type.GetProperty(propertyName);
#endif
        }

        public static Type[] GetGenericArguments(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static bool IsAssignableFrom(this Type type, Type toCompare)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsAssignableFrom(toCompare.GetTypeInfo());
#else
            return type.IsAssignableFrom(toCompare);
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETFX_CORE
            if (type == typeof(Boolean)) return true;
            if (type == typeof(Byte)) return true;
            if (type == typeof(SByte)) return true;
            if (type == typeof(Int16)) return true;
            if (type == typeof(UInt16)) return true;
            if (type == typeof(Int32)) return true;
            if (type == typeof(UInt32)) return true;
            if (type == typeof(Int64)) return true;
            if (type == typeof(UInt64)) return true;
            if (type == typeof(IntPtr)) return true;
            if (type == typeof(UIntPtr)) return true;
            if (type == typeof(Char)) return true;
            if (type == typeof(Double)) return true;
            if (type == typeof(Single)) return true;
            return false;
#else
            return type.IsPrimitive;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        /**
         * Missing IsSubclassOf, this works well
         */
        public static bool IsSubclassOf(this Type type, System.Type parent)
        {
#if NETFX_CORE
            return parent.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
#else
            return type.IsSubclassOf(parent);
#endif
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
#if NETFX_CORE
            return propertyInfo.GetMethod;
#else
            return propertyInfo.GetGetMethod();
#endif
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
        {
#if NETFX_CORE
            return propertyInfo.SetMethod;
#else
            return propertyInfo.GetSetMethod();
#endif
        }
    }
}
#endif