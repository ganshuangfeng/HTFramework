﻿using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HT.Framework
{
    /// <summary>
    /// 任务内容基类
    /// </summary>
    public abstract class TaskContentBase : ScriptableObject
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string GUID;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 任务详细介绍
        /// </summary>
        public string Details;
        /// <summary>
        /// 任务目标
        /// </summary>
        public TaskGameObject Target;
        /// <summary>
        /// 所有任务点
        /// </summary>
        public List<TaskPointBase> Points = new List<TaskPointBase>();
        /// <summary>
        /// 所有任务依赖
        /// </summary>
        public List<TaskDepend> Depends = new List<TaskDepend>();

        public TaskContentBase()
        {
            GUID = "";
            Name = "New Task";
            Details = "New Task";
        }

#if UNITY_EDITOR
        private static readonly int _width = 200;
        private int _height = 0;

        internal void OnEditorGUI()
        {
            GUILayout.BeginVertical("ChannelStripBg", GUILayout.Width(_width), GUILayout.Height(_height));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Task Property:");
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            _height = 20;

            _height += OnPropertyGUI();
            
            GUILayout.EndVertical();
        }

        public virtual int OnPropertyGUI()
        {
            int height = 0;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID:", GUILayout.Width(50));
            GUID = EditorGUILayout.TextField(GUID);
            GUILayout.EndHorizontal();

            height += 20;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(50));
            Name = EditorGUILayout.TextField(Name);
            GUILayout.EndHorizontal();

            height += 20;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Details:", GUILayout.Width(50));
            Details = EditorGUILayout.TextField(Details);
            GUILayout.EndHorizontal();

            height += 20;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Points:", GUILayout.Width(50));
            GUILayout.Label(Points.Count.ToString());
            GUILayout.EndHorizontal();
            
            height += 20;

            GUILayout.BeginHorizontal();
            TaskGameObjectField(ref Target, "Target:", 50);
            GUILayout.EndHorizontal();

            height += 20;

            return height;
        }
        
        public bool IsExistDepend(int originalPoint, int dependPoint)
        {
            for (int i = 0; i < Depends.Count; i++)
            {
                if (Depends[i].OriginalPoint == originalPoint && Depends[i].DependPoint == dependPoint)
                {
                    return true;
                }
            }
            return false;
        }

        public void DisconnectDepend(int originalPoint, int dependPoint)
        {
            for (int i = 0; i < Depends.Count; i++)
            {
                if (Depends[i].OriginalPoint == originalPoint && Depends[i].DependPoint == dependPoint)
                {
                    Depends.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public void ConnectDepend(int originalPoint, int dependPoint)
        {
            Depends.Add(new TaskDepend(originalPoint, dependPoint));
        }

        protected void TaskGameObjectField(ref TaskGameObject taskGameObject, string name, float nameWidth)
        {
            if (taskGameObject == null)
            {
                taskGameObject = new TaskGameObject();
            }

            GUIContent gUIContent = new GUIContent(name);
            gUIContent.tooltip = "GUID: " + taskGameObject.GUID;
            GUILayout.Label(gUIContent, GUILayout.Width(nameWidth));

            GUI.color = taskGameObject.Entity ? Color.white : Color.gray;
            GameObject newEntity = EditorGUILayout.ObjectField(taskGameObject.Entity, typeof(GameObject), true, GUILayout.Width(_width - nameWidth - 35)) as GameObject;
            if (newEntity != taskGameObject.Entity)
            {
                if (newEntity != null)
                {
                    TaskTarget target = newEntity.GetComponent<TaskTarget>();
                    if (!target)
                    {
                        target = newEntity.AddComponent<TaskTarget>();
                    }
                    if (target.GUID == "<None>")
                    {
                        target.GUID = Guid.NewGuid().ToString();
                    }
                    taskGameObject.Entity = newEntity;
                    taskGameObject.GUID = target.GUID;
                    taskGameObject.Path = newEntity.transform.FullName();
                }
            }
            GUI.color = Color.white;

            if (taskGameObject.Entity == null && taskGameObject.GUID != "<None>")
            {
                taskGameObject.Entity = GameObject.Find(taskGameObject.Path);
                if (taskGameObject.Entity == null)
                {
                    TaskTarget[] targets = FindObjectsOfType<TaskTarget>();
                    foreach (TaskTarget target in targets)
                    {
                        if (taskGameObject.GUID == target.GUID)
                        {
                            taskGameObject.Entity = target.gameObject;
                            taskGameObject.Path = target.transform.FullName();
                            break;
                        }
                    }
                }
                else
                {
                    TaskTarget target = taskGameObject.Entity.GetComponent<TaskTarget>();
                    if (!target)
                    {
                        target = taskGameObject.Entity.AddComponent<TaskTarget>();
                        target.GUID = taskGameObject.GUID;
                    }
                }
            }

            gUIContent = EditorGUIUtility.IconContent("TreeEditor.Trash");
            gUIContent.tooltip = "Delete Target";
            GUI.enabled = taskGameObject.GUID != "<None>";
            if (GUILayout.Button(gUIContent, "InvisibleButton", GUILayout.Width(20), GUILayout.Height(20)))
            {
                taskGameObject.Entity = null;
                taskGameObject.GUID = "<None>";
                taskGameObject.Path = "";
            }
            GUI.enabled = true;
        }

        internal static void GenerateSerializeSubObject(UnityEngine.Object obj, UnityEngine.Object mainAsset)
        {
            obj.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(obj, mainAsset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainAsset));
        }

        internal static void DestroySerializeSubObject(UnityEngine.Object obj, UnityEngine.Object mainAsset)
        {
            AssetDatabase.RemoveObjectFromAsset(obj);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainAsset));
        }
#endif
    }
}