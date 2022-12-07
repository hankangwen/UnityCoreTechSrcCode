using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class DeserializeJSON : UnityEngine.Object
    {
        private struct TaskField
        {
            public TaskField(Task t, FieldInfo f) { task = t; fieldInfo = f; }
            public Task task;
            public FieldInfo fieldInfo;
        }

        private static Dictionary<TaskField, List<int>> taskIDs = null;
        private static GlobalVariables globalVariables = null;

        // Convert over to just TaskSerializationData as a parameter as soon as the old JSON serialization code is removed
        public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
        {
            var IDtoTask = new Dictionary<int, Task>();

            var dict = MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>;
            if (dict == null) {
                Debug.Log("Failed to deserialize");
                return;
            }
            taskIDs = new Dictionary<TaskField, List<int>>();

            // deserialize the variables first so the tasks can reference them
            DeserializeVariables(behaviorSource, dict, taskData.fieldSerializationData.unityObjects);

            if (dict.ContainsKey("EntryTask")) {
                behaviorSource.EntryTask = DeserializeTask(behaviorSource, dict["EntryTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
            }

            if (dict.ContainsKey("RootTask")) {
                behaviorSource.RootTask = DeserializeTask(behaviorSource, dict["RootTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
            }

            if (dict.ContainsKey("DetachedTasks")) {
                var detachedTasks = new List<Task>();
                foreach (Dictionary<string, object> detachedTaskDict in (dict["DetachedTasks"] as IEnumerable)) {
                    detachedTasks.Add(DeserializeTask(behaviorSource, detachedTaskDict, ref IDtoTask, taskData.fieldSerializationData.unityObjects));
                }
                behaviorSource.DetachedTasks = detachedTasks;
            }

            // deserialization is complete besides assigning the correct tasks based off of the id
            if (taskIDs != null && taskIDs.Count > 0) {
                foreach (TaskField taskField in taskIDs.Keys) {
                    var idList = taskIDs[taskField] as List<int>;
                    if (taskField.fieldInfo.FieldType.IsArray) { // task array
                        var taskList = Activator.CreateInstance(typeof(List<>).MakeGenericType(taskField.fieldInfo.FieldType.GetElementType())) as IList;
                        for (int i = 0; i < idList.Count; ++i) {
                            var task = IDtoTask[idList[i]];
                            if (task.GetType().Equals(taskField.fieldInfo.FieldType.GetElementType()) || task.GetType().IsSubclassOf(taskField.fieldInfo.FieldType.GetElementType())) {
                                taskList.Add(task);
                            }
                        }
                        var taskArray = Array.CreateInstance(taskField.fieldInfo.FieldType.GetElementType(), taskList.Count);
                        taskList.CopyTo(taskArray, 0);
                        taskField.fieldInfo.SetValue(taskField.task, taskArray);
                    } else { // single task
                        var task = IDtoTask[idList[0]];
                        if (task.GetType().Equals(taskField.fieldInfo.FieldType) || task.GetType().IsSubclassOf(taskField.fieldInfo.FieldType)) {
                            taskField.fieldInfo.SetValue(taskField.task, task);
                        }
                    }
                }
                taskIDs = null;
            }
        }

        public static void Load(string serialization, GlobalVariables globalVariables)
        {
            if (globalVariables == null)
                return;

            var dict = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
            if (dict == null) {
                Debug.Log("Failed to deserialize");
                return;
            }

            if (globalVariables.VariableData == null) {
                globalVariables.VariableData = new VariableSerializationData();
            }

            DeserializeVariables(globalVariables, dict, globalVariables.VariableData.fieldSerializationData.unityObjects);
        }

        private static void DeserializeVariables(IVariableSource variableSource, Dictionary<string, object> dict, List<UnityEngine.Object> unityObjects)
        {
            if (dict.ContainsKey("Variables")) {
                var variables = new List<SharedVariable>();
                var variablesList = dict["Variables"] as IList;
                for (int i = 0; i < variablesList.Count; ++i) {
                    var sharedVariable = DeserializeSharedVariable(variablesList[i] as Dictionary<string, object>, variableSource, true, unityObjects);
                    variables.Add(sharedVariable);
                }
                variableSource.SetAllVariables(variables);
            }
        }

        private static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, List<UnityEngine.Object> unityObjects)
        {
            Task task = null;
            try {
                var type = TaskUtility.GetTypeWithinAssembly(dict["ObjectType"] as string);
                // Change the type to an unknown type if the type doesn't exist anymore.
                if (type == null) {
                    if (dict.ContainsKey("Children")) {
                        type = typeof(UnknownParentTask);
                    } else {
                        type = typeof(UnknownTask);
                    }
                }
                task = Activator.CreateInstance(type) as Task;
            }
            catch (Exception /*e*/) { }

            // What happened?
            if (task == null) {
                Debug.Log("Error: task is null of type " + dict["ObjectType"]);
                return null;
            }

            task.Owner = behaviorSource.Owner.GetObject() as Behavior;
            task.ID = Convert.ToInt32(dict["ID"]);
            if (task.ID == -1) {
                // for tasks that were saved with a really old BD version
                task.ID = IDtoTask.Count;
            }
            if (dict.ContainsKey("Name")) {
                task.FriendlyName = (string)dict["Name"];
            }
            if (dict.ContainsKey("Instant")) {
                task.IsInstant = (bool)Convert.ChangeType(dict["Instant"], typeof(bool));
            }
            IDtoTask.Add(task.ID, task);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            task.NodeData = DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);

            // give a little warning if the task is an unknown type
            if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask))) {
                if (!task.FriendlyName.Contains("Unknown ")) {
                    task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
                }
                if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?")) {
                    task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", (task.NodeData.Comment.Equals("") ? "" : string.Format("\0{0}", task.NodeData.Comment)));
                }
            }
#endif
            DeserializeObject(task, task, dict, behaviorSource, unityObjects);

            if (task is ParentTask && dict.ContainsKey("Children")) {
                var parentTask = task as ParentTask;
                if (parentTask != null) {
                    foreach (Dictionary<string, object> childDict in (dict["Children"] as IEnumerable)) {
                        var child = DeserializeTask(behaviorSource, childDict, ref IDtoTask, unityObjects);
                        int index = (parentTask.Children == null ? 0 : parentTask.Children.Count);
                        parentTask.AddChild(child, index);
                    }
                }
            }

            return task;
        }
        
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
        {
            var nodeData = new NodeData();
            if (dict.ContainsKey("Position")) {
                nodeData.Position = StringToVector2((string)dict["Position"]);
            }
            if (dict.ContainsKey("Offset")) {
                nodeData.Offset = StringToVector2((string)dict["Offset"]);
            }
            if (dict.ContainsKey("FriendlyName")) {
                task.FriendlyName = (string)dict["FriendlyName"];
            }
            if (dict.ContainsKey("Comment")) {
                nodeData.Comment = (string)dict["Comment"];
            }
            if (dict.ContainsKey("IsBreakpoint")) {
                nodeData.IsBreakpoint = Convert.ToBoolean(dict["IsBreakpoint"]);
            }
            if (dict.ContainsKey("Collapsed")) {
                nodeData.Collapsed = Convert.ToBoolean(dict["Collapsed"]);
            }
            if (dict.ContainsKey("Disabled")) {
                nodeData.Disabled = Convert.ToBoolean(dict["Disabled"]);
            }
            if (dict.ContainsKey("ColorIndex")) {
                nodeData.ColorIndex = Convert.ToInt32(dict["ColorIndex"]);
            }
            if (dict.ContainsKey("WatchedFields")) {
                nodeData.WatchedFieldNames = new List<string>();
                nodeData.WatchedFields = new List<FieldInfo>();

                var objectValues = dict["WatchedFields"] as IList;
                for (int i = 0; i < objectValues.Count; ++i) {
                    var field = task.GetType().GetField((string)objectValues[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null) {
                        nodeData.WatchedFieldNames.Add(field.Name);
                        nodeData.WatchedFields.Add(field);
                    }
                }
            }
            return nodeData;
        }
#endif

        // If the serialized variable is from the source then just create a new variable and don't check to see if it already exists
        private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, IVariableSource variableSource, bool fromSource, List<UnityEngine.Object> unityObjects)
        {
            if (dict == null) {
                return null;
            }

            SharedVariable sharedVariable = null;
            // the shared variable may be referencing the variable within the behavior
            if (!fromSource && variableSource != null && dict.ContainsKey("Name")) {
                if (!dict.ContainsKey("IsGlobal") || (bool)Convert.ChangeType(dict["IsGlobal"], typeof(bool)) == false) {
                    sharedVariable = variableSource.GetVariable(dict["Name"] as string);
                } else {
                    if (globalVariables == null) {
                        globalVariables = GlobalVariables.Instance;
                    }
                    if (globalVariables != null) {
                        sharedVariable = globalVariables.GetVariable(dict["Name"] as string);
                    }
                }
            }

            var variableType = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
            if (variableType == null) {
                return null;
            }
            bool typesEqual = true;
            if (sharedVariable == null || !(typesEqual = sharedVariable.GetType().Equals(variableType))) {
                sharedVariable = Activator.CreateInstance(variableType) as SharedVariable;
                // In a future release SharedVariable will no longer derive from ScriptableObject so .name will be invalid. Start deserializing the correct Name
                sharedVariable.Name = dict["Name"] as string;
                sharedVariable.IsShared = dict.ContainsKey("IsShared") && (bool)Convert.ChangeType(dict["IsShared"], typeof(bool));
                sharedVariable.IsGlobal = dict.ContainsKey("IsGlobal") && (bool)Convert.ChangeType(dict["IsGlobal"], typeof(bool));
                // if the types are not equal then this shared variable used to be a different type so it should be shared
                if (!typesEqual) {
                    sharedVariable.IsShared = true;
                }

                DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
            }

            return sharedVariable;
        }

        private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
        {
            if (dict == null)
                return;

            var fields = TaskUtility.GetAllFields(obj.GetType());
            for (int i = 0; i < fields.Length; ++i) {
                string key = "";
                if (dict.ContainsKey((key = fields[i].FieldType + "," + fields[i].Name)) || dict.ContainsKey((key = fields[i].Name))) {
                    if (typeof(IList).IsAssignableFrom(fields[i].FieldType)) {
                        var objectValues = dict[key] as IList;
                        if (objectValues != null) {
                            Type type;
                            if (fields[i].FieldType.IsArray) {
                                type = fields[i].FieldType.GetElementType();
                            } else {
                                type = fields[i].FieldType.GetGenericArguments()[0];
                            }
                            bool isIDList = type.Equals(typeof(Task)) || type.IsSubclassOf(typeof(Task));
                            var genericType = type;
                            if (isIDList) {
                                genericType = typeof(int);
                            } else if (type.IsSubclassOf(typeof(Enum))) {
                                genericType = typeof(Enum);
                            }
                            var objectList = Activator.CreateInstance(typeof(List<>).MakeGenericType(isIDList ? typeof(int) : genericType)) as IList;
                            for (int j = 0; j < objectValues.Count; ++j) {
                                // If the type is a Task then the task ID was stored. Because the task may not exist yet, wait to reference the task until all deserialization has been completed
                                if (isIDList) {
                                    objectList.Add(Convert.ToInt32(objectValues[j]));
                                } else {
                                    objectList.Add(ValueToObject(task, type, objectValues[j], variableSource, unityObjects));
                                }
                            }

                            if (isIDList) {
                                if (taskIDs != null) {
                                    taskIDs.Add(new TaskField(task, fields[i]), objectList as List<int>);
                                }
                            } else {
                                if (fields[i].FieldType.IsArray) {
                                    // copy to an array so SetValue will accept the new value
                                    var objectArray = Array.CreateInstance(type, objectList.Count);
                                    objectList.CopyTo(objectArray, 0);
                                    fields[i].SetValue(obj, objectArray);
                                } else {
                                    fields[i].SetValue(obj, objectList);
                                }
                            }
                        }
                    } else {
                        var type = fields[i].FieldType;
                        // If the type is a Task then the task ID was stored. Because the task may not exist yet, wait to reference the task until all deserialization has been completed
                        if (type.Equals(typeof(Task)) || type.IsSubclassOf(typeof(Task))) {
                            if (TaskUtility.HasAttribute(fields[i], typeof(InspectTaskAttribute))) {
                                var referencedTaskDict = dict[fields[i].Name] as Dictionary<string, object>;
                                var referencedTaskType = TaskUtility.GetTypeWithinAssembly(referencedTaskDict["ObjectType"] as string);
                                if (referencedTaskType != null) {
                                    var referencedTask = Activator.CreateInstance(referencedTaskType) as Task;
                                    DeserializeObject(referencedTask, referencedTask, referencedTaskDict, variableSource, unityObjects);
                                    fields[i].SetValue(task, referencedTask);
                                }
                            } else if (taskIDs != null) {
                                var idList = new List<int>();
                                idList.Add(Convert.ToInt32(dict[key]));
                                taskIDs.Add(new TaskField(task, fields[i]), idList);
                            }
                        } else {
                            fields[i].SetValue(obj, ValueToObject(task, type, dict[key], variableSource, unityObjects));
                        }
                    }
                } else if (typeof(SharedVariable).IsAssignableFrom(fields[i].FieldType)) {
                    // This is ugly but it works. In version 1.4 a lot of the main tasks had their fields changed from the concrete type to the SharedVariable type.
                    // Instead of doing a two step conversion to the SharedVariable this will convert the concrete type to the SharedVariable type while preserving the value.
                    // This code should be removed in the future.
                    var concreteType = TaskUtility.SharedVariableToConcreteType(fields[i].FieldType);
                    if (concreteType == null) {
                        return;
                    }
                    key = concreteType + "," + fields[i].Name;
                    if (dict.ContainsKey(key)) {
                        var sharedVariable = Activator.CreateInstance(fields[i].FieldType) as SharedVariable;
                        sharedVariable.SetValue(ValueToObject(task, concreteType, dict[key], variableSource, unityObjects));
                        fields[i].SetValue(obj, sharedVariable);
                    }
                }
            }
        }

        private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
        {
            if (type.Equals(typeof(SharedVariable)) || type.IsSubclassOf(typeof(SharedVariable))) {
                var value = DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects);
                if (value == null || !value.GetType().Equals(type)) {
                    // Create a blank shared variable so a value is assigned
                    value = Activator.CreateInstance(type) as SharedVariable;
                }
                return value;
            } else if (type.Equals(typeof(UnityEngine.Object)) || type.IsSubclassOf(typeof(UnityEngine.Object))) {
                return IndexToUnityObject(Convert.ToInt32(obj), unityObjects);
#if UNITY_WINRT && !UNITY_EDITOR
            } else if (type.IsPrimitive() || type.Equals(typeof(string))) {
#else
            } else if (type.IsPrimitive || type.Equals(typeof(string))) {
#endif
                try {
                    return Convert.ChangeType(obj, type);
                }
                catch (Exception /*e*/) {
                    return null;
                }
            } else if (type.IsSubclassOf(typeof(Enum))) {
                try {
                    return Enum.Parse(type, (string)obj);
                }
                catch (Exception /*e*/) {
                    return null;
                }
            } else if (type.Equals(typeof(Vector2))) {
                return StringToVector2((string)obj);
            } else if (type.Equals(typeof(Vector3))) {
                return StringToVector3((string)obj);
            } else if (type.Equals(typeof(Vector4))) {
                return StringToVector4((string)obj);
            } else if (type.Equals(typeof(Quaternion))) {
                return StringToQuaternion((string)obj);
            } else if (type.Equals(typeof(Matrix4x4))) {
                return StringToMatrix4x4((string)obj);
            } else if (type.Equals(typeof(Color))) {
                return StringToColor((string)obj);
            } else if (type.Equals(typeof(Rect))) {
                return StringToRect((string)obj);
            } else if (type.Equals(typeof(LayerMask))) {
                return ValueToLayerMask(Convert.ToInt32(obj));
            } else {
                var unknownObj = Activator.CreateInstance(type);
                DeserializeObject(task, unknownObj, obj as Dictionary<string, object>, variableSource, unityObjects);
                return unknownObj;
            }
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            var stringSplit = vector2String.Substring(1, vector2String.Length - 2).Split(',');
            return new Vector2(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]));
        }

        private static Vector3 StringToVector3(string vector3String)
        {
            var stringSplit = vector3String.Substring(1, vector3String.Length - 2).Split(',');
            return new Vector3(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]));
        }

        private static Vector4 StringToVector4(string vector4String)
        {
            var stringSplit = vector4String.Substring(1, vector4String.Length - 2).Split(',');
            return new Vector4(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Quaternion StringToQuaternion(string quaternionString)
        {
            var stringSplit = quaternionString.Substring(1, quaternionString.Length - 2).Split(',');
            return new Quaternion(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Matrix4x4 StringToMatrix4x4(string matrixString)
        {
            var stringSplit = matrixString.Split(null);
            var matrix = new Matrix4x4();
            matrix.m00 = float.Parse(stringSplit[0]);
            matrix.m01 = float.Parse(stringSplit[1]);
            matrix.m02 = float.Parse(stringSplit[2]);
            matrix.m03 = float.Parse(stringSplit[3]);
            matrix.m10 = float.Parse(stringSplit[4]);
            matrix.m11 = float.Parse(stringSplit[5]);
            matrix.m12 = float.Parse(stringSplit[6]);
            matrix.m13 = float.Parse(stringSplit[7]);
            matrix.m20 = float.Parse(stringSplit[8]);
            matrix.m21 = float.Parse(stringSplit[9]);
            matrix.m22 = float.Parse(stringSplit[10]);
            matrix.m23 = float.Parse(stringSplit[11]);
            matrix.m30 = float.Parse(stringSplit[12]);
            matrix.m31 = float.Parse(stringSplit[13]);
            matrix.m32 = float.Parse(stringSplit[14]);
            matrix.m33 = float.Parse(stringSplit[15]);
            return matrix;
        }

        private static Color StringToColor(string colorString)
        {
            var stringSplit = colorString.Substring(5, colorString.Length - 6).Split(',');
            return new Color(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Rect StringToRect(string rectString)
        {
            var stringSplit = rectString.Substring(1, rectString.Length - 2).Split(',');
            return new Rect(float.Parse(stringSplit[0].Substring(2, stringSplit[0].Length - 2)), //x:0.00
                            float.Parse(stringSplit[1].Substring(3, stringSplit[1].Length - 3)), // y:0.00
                            float.Parse(stringSplit[2].Substring(7, stringSplit[2].Length - 7)), // width:0.00
                            float.Parse(stringSplit[3].Substring(8, stringSplit[3].Length - 8))); // height:0.00
        }

        private static LayerMask ValueToLayerMask(int value)
        {
            var layerMask = new LayerMask();
            layerMask.value = value;
            return layerMask;
        }

        private static UnityEngine.Object IndexToUnityObject(int index, List<UnityEngine.Object> unityObjects)
        {
            if (index < 0 || index >= unityObjects.Count) {
                return null;
            }

            if (unityObjects[index].Equals(null)) {
                return null;
            }

            return unityObjects[index];
        }

    }
}