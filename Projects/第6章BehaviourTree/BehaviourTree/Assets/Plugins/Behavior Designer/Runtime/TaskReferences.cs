using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class TaskReferences : MonoBehaviour
    {
        // Check the references from the inspector with a behavior
        public static void CheckReferences(BehaviorSource behaviorSource)
        {
            if (behaviorSource.RootTask != null) {
                CheckReferences(behaviorSource, behaviorSource.RootTask);
            }

            if (behaviorSource.DetachedTasks != null) {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; ++i) {
                    CheckReferences(behaviorSource, behaviorSource.DetachedTasks[i]);
                }
            }
        }

        private static void CheckReferences(BehaviorSource behaviorSource, Task task)
        {
            var fieldInfo = TaskUtility.GetAllFields(task.GetType());
            for (int i = 0; i < fieldInfo.Length; ++i) {
                if (!fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.Equals(typeof(Task)) || fieldInfo[i].FieldType.IsSubclassOf(typeof(Task)))) {
                    // find the new task
                    var referencedTask = fieldInfo[i].GetValue(task) as Task;
                    if (referencedTask != null) {
                        var newTask = FindReferencedTask(behaviorSource, referencedTask);
                        if (newTask != null) {
                            fieldInfo[i].SetValue(task, newTask);
                        }
                    }
                } else if (fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.GetElementType().Equals(typeof(Task)) || fieldInfo[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) {
                    var referencedTasks = fieldInfo[i].GetValue(task) as Task[];
                    if (referencedTasks != null) {
                        var referencedTasksList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldInfo[i].FieldType.GetElementType())) as IList;
                        for (int j = 0; j < referencedTasks.Length; ++j) {
                            var newTask = FindReferencedTask(behaviorSource, referencedTasks[j]);
                            if (newTask != null) {
                                referencedTasksList.Add(newTask);
                            }
                        }
                        // copy to an array so SetValue will accept the new value
                        var referencedTasksArray = Array.CreateInstance(fieldInfo[i].FieldType.GetElementType(), referencedTasksList.Count);
                        referencedTasksList.CopyTo(referencedTasksArray, 0);
                        fieldInfo[i].SetValue(task, referencedTasksArray);
                    }
                }
            }

            if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                var parentTask = task as ParentTask;
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        CheckReferences(behaviorSource, parentTask.Children[i]);
                    }
                }
            }
        }

        private static Task FindReferencedTask(BehaviorSource behaviorSource, Task referencedTask)
        {
            int referencedTaskID = referencedTask.ID;
            Task task;
            if (behaviorSource.RootTask != null && (task = FindReferencedTask(behaviorSource.RootTask, referencedTaskID)) != null) {
                return task;
            }

            if (behaviorSource.DetachedTasks != null) {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; ++i) {
                    if ((task = FindReferencedTask(behaviorSource.DetachedTasks[i], referencedTaskID)) != null) {
                        return task;
                    }
                }
            }
            return null;
        }

        private static Task FindReferencedTask(Task task, int referencedTaskID)
        {
            if (task.ID == referencedTaskID) {
                return task;
            }

            if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                var parentTask = task as ParentTask;
                if (parentTask.Children != null) {
                    Task childTask;
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        if ((childTask = FindReferencedTask(parentTask.Children[i], referencedTaskID)) != null) {
                            return childTask;
                        }
                    }
                }
            }
            return null;
        }

        // Check the references from the Behavior Manager with a task list
        public static void CheckReferences(Behavior behavior, List<Task> taskList)
        {
            for (int i = 0; i < taskList.Count; ++i) {
                CheckReferences(behavior, taskList[i], taskList);
            }
        }

        private static void CheckReferences(Behavior behavior, Task task, List<Task> taskList)
        {
            if (TaskUtility.CompareType(task.GetType(), "BehaviorDesigner.Runtime.Tasks.ConditionalEvaluator")) {
                var conditionalTaskField = task.GetType().GetField("conditionalTask");
                var conditionalTask = conditionalTaskField.GetValue(task);
                if (conditionalTask != null) {
                    task = conditionalTask as Task;
                }
            }
            var fieldInfo = TaskUtility.GetAllFields(task.GetType());
            for (int i = 0; i < fieldInfo.Length; ++i) {
                if (!fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.Equals(typeof(Task)) || fieldInfo[i].FieldType.IsSubclassOf(typeof(Task)))) {
                    // find the new task
                    var referencedTask = fieldInfo[i].GetValue(task) as Task;
                    if (referencedTask != null && !referencedTask.Owner.Equals(behavior)) {
                        var newTask = FindReferencedTask(referencedTask, taskList);
                        if (newTask != null) {
                            fieldInfo[i].SetValue(task, newTask);
                        }
                    }
                } else if (fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.GetElementType().Equals(typeof(Task)) || fieldInfo[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) {
                    var referencedTasks = fieldInfo[i].GetValue(task) as Task[];
                    if (referencedTasks != null) {
                        var referencedTasksList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldInfo[i].FieldType.GetElementType())) as IList;
                        for (int j = 0; j < referencedTasks.Length; ++j) {
                            var newTask = FindReferencedTask(referencedTasks[j], taskList);
                            if (newTask != null) {
                                referencedTasksList.Add(newTask);
                            }
                        }
                        // copy to an array so SetValue will accept the new value
                        var referencedTasksArray = Array.CreateInstance(fieldInfo[i].FieldType.GetElementType(), referencedTasksList.Count);
                        referencedTasksList.CopyTo(referencedTasksArray, 0);
                        fieldInfo[i].SetValue(task, referencedTasksArray);
                    }
                }
            }
        }

        private static Task FindReferencedTask(Task referencedTask, List<Task> taskList)
        {
            int referencedTaskID = referencedTask.ReferenceID;
            for (int i = 0; i < taskList.Count; ++i) {
                if (taskList[i].ReferenceID == referencedTaskID) {
                    return taskList[i];
                }
            }

            return null;
        }
    }
}