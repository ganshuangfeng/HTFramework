﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace HT.Framework
{
    [CustomEditor(typeof(Main))]
    [GiteeURL("https://gitee.com/SaiTingHu/HTFramework")]
    [GithubURL("https://github.com/SaiTingHu/HTFramework")]
    [CSDNBlogURL("https://wanderer.blog.csdn.net/article/details/102956756")]
    internal sealed class MainInspector : InternalModuleInspector<Main, IMainHelper>
    {
        private static Page _currentPage = Page.None;

        protected override string Intro
        {
            get
            {
                return "HTFramework Main Module!";
            }
        }

        protected override void OnDefaultEnable()
        {
            base.OnDefaultEnable();

            ScriptingDefineEnable();
            MainDataEnable();
            LicenseEnable();
            ParameterEnable();
            SettingEnable();
        }
        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();
            
            ScriptingDefineGUI();
            MainDataGUI();
            LicenseGUI();
            ParameterGUI();
            SettingGUI();
        }
        protected override void OnInspectorRuntimeGUI()
        {
            base.OnInspectorRuntimeGUI();

            GUILayout.BeginHorizontal();
            Target.Pause = EditorGUILayout.Toggle("Pause", Target.Pause);
            GUILayout.EndHorizontal();
        }

        #region Style
        private GUIStyle GetStyle(Page page)
        {
            if (_currentPage == page)
                return "SelectionRect";
            else
                return "Box";
        }
        #endregion

        #region ScriptingDefine
        private AnimBool _scriptingDefineABool;
        private ScriptingDefine _currentScriptingDefine;
        private bool _isNewDefine = false;
        private string _newDefine = "";

        private void ScriptingDefineEnable()
        {
            _scriptingDefineABool = new AnimBool(false, new UnityAction(Repaint));
            _currentScriptingDefine = new ScriptingDefine();
        }
        private void ScriptingDefineGUI()
        {
            GUILayout.BeginVertical(GetStyle(Page.ScriptingDefine));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            bool oldValue = _currentPage == Page.ScriptingDefine;
            _scriptingDefineABool.target = EditorGUILayout.Foldout(oldValue, "Scripting Define", true);
            if (_scriptingDefineABool.target != oldValue)
            {
                if (_scriptingDefineABool.target) _currentPage = Page.ScriptingDefine;
                else _currentPage = Page.None;
            }
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_scriptingDefineABool.faded))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Defined");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(_currentScriptingDefine.Defined);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.enabled = _currentScriptingDefine.IsAnyDefined;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Clear", EditorStyles.miniButtonLeft))
                {
                    _currentScriptingDefine.ClearDefines();
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("New", EditorStyles.miniButtonMid))
                {
                    _isNewDefine = !_isNewDefine;
                    _newDefine = "";
                }
                if (GUILayout.Button("Apply", EditorStyles.miniButtonRight))
                {
                    _currentScriptingDefine.Apply();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

                if (_isNewDefine)
                {
                    GUILayout.BeginHorizontal();
                    _newDefine = EditorGUILayout.TextField(_newDefine);
                    if (GUILayout.Button("OK", EditorStyles.miniButtonLeft, GUILayout.Width(30)))
                    {
                        if (_newDefine != "")
                        {
                            _currentScriptingDefine.AddDefine(_newDefine);
                            _isNewDefine = false;
                            _newDefine = "";
                        }
                        else
                        {
                            Log.Error("输入的宏定义不能为空！");
                        }
                    }
                    if (GUILayout.Button("NO", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        _isNewDefine = false;
                        _newDefine = "";
                    }
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Historical record");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Clear record", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        _currentScriptingDefine.ClearDefinesRecord();
                    }
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < _currentScriptingDefine.DefinedsRecord.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.Label(_currentScriptingDefine.DefinedsRecord[i], "PR PrefabLabel");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Use", EditorStyles.miniButton, GUILayout.Width(30)))
                        {
                            _newDefine += _currentScriptingDefine.DefinedsRecord[i] + ";";
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
        
        private sealed class ScriptingDefine
        {
            private BuildTargetGroup _buildTargetGroup = BuildTargetGroup.Standalone;

            public List<string> DefinedsRecord { get; } = new List<string>();
            public List<string> Defineds { get; } = new List<string>();
            public string DefinedRecord { get; private set; } = "";
            public string Defined { get; private set; } = "";
            public bool IsAnyDefined
            {
                get
                {
                    return Defined != "";
                }
            }

            public ScriptingDefine()
            {
                AddDefineRecord(EditorPrefs.GetString(EditorPrefsTable.ScriptingDefine_Record, ""));

                _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                AddDefine(PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup));
            }
            public bool IsDefined(string define)
            {
                return Defineds.Contains(define);
            }
            public void AddDefine(string define)
            {
                if (string.IsNullOrEmpty(define) || define == "")
                {
                    return;
                }

                string[] defines = define.Split(';');
                for (int i = 0; i < defines.Length; i++)
                {
                    if (defines[i] != "" && !Defineds.Contains(defines[i]))
                    {
                        Defined += defines[i] + ";";
                        Defineds.Add(defines[i]);
                    }
                }
                AddDefineRecord(define);
            }
            public void ClearDefines()
            {
                Defined = "";
                Defineds.Clear();
            }
            public void Apply()
            {
                _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, Defined);
            }

            public void ClearDefinesRecord()
            {
                DefinedRecord = "";
                DefinedsRecord.Clear();
                EditorPrefs.SetString(EditorPrefsTable.ScriptingDefine_Record, DefinedRecord);
            }
            private void AddDefineRecord(string define)
            {
                if (string.IsNullOrEmpty(define) || define == "")
                {
                    return;
                }

                string[] defines = define.Split(';');
                for (int i = 0; i < defines.Length; i++)
                {
                    if (defines[i] != "" && !DefinedsRecord.Contains(defines[i]))
                    {
                        DefinedRecord += defines[i] + ";";
                        DefinedsRecord.Add(defines[i]);
                    }
                }
                EditorPrefs.SetString(EditorPrefsTable.ScriptingDefine_Record, DefinedRecord);
            }
        }
        #endregion

        #region MainData
        private AnimBool _mainDataABool;

        private void MainDataEnable()
        {
            _mainDataABool = new AnimBool(false, new UnityAction(Repaint));
        }
        private void MainDataGUI()
        {
            GUILayout.BeginVertical(GetStyle(Page.MainData));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            bool oldValue = _currentPage == Page.MainData;
            _mainDataABool.target = EditorGUILayout.Foldout(oldValue, "Main Data", true);
            if (_mainDataABool.target != oldValue)
            {
                if (_mainDataABool.target) _currentPage = Page.MainData;
                else _currentPage = Page.None;
            }
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_mainDataABool.faded))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("MainData", GUILayout.Width(LabelWidth));
                if (GUILayout.Button(Target.MainDataType, EditorGlobalTools.Styles.MiniPopup))
                {
                    GenericMenu gm = new GenericMenu();
                    List<Type> types = ReflectionToolkit.GetTypesInRunTimeAssemblies(type =>
                    {
                        return type.IsSubclassOf(typeof(MainDataBase));
                    });
                    gm.AddItem(new GUIContent("<None>"), Target.MainDataType == "<None>", () =>
                    {
                        Undo.RecordObject(target, "Set Main Data");
                        Target.MainDataType = "<None>";
                        HasChanged();
                    });
                    for (int i = 0; i < types.Count; i++)
                    {
                        int j = i;
                        gm.AddItem(new GUIContent(types[j].FullName), Target.MainDataType == types[j].FullName, () =>
                        {
                            Undo.RecordObject(target, "Set Main Data");
                            Target.MainDataType = types[j].FullName;
                            HasChanged();
                        });
                    }
                    gm.ShowAsContext();
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
        #endregion

        #region License
        private AnimBool _licenseABool;

        private void LicenseEnable()
        {
            _licenseABool = new AnimBool(false, new UnityAction(Repaint));
        }
        private void LicenseGUI()
        {
            GUILayout.BeginVertical(GetStyle(Page.License));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            bool oldValue = _currentPage == Page.License;
            _licenseABool.target = EditorGUILayout.Foldout(oldValue, "License", true);
            if (_licenseABool.target != oldValue)
            {
                if (_licenseABool.target) _currentPage = Page.License;
                else _currentPage = Page.None;
            }
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_licenseABool.faded))
            {
                GUILayout.BeginHorizontal();
                Toggle(Target.IsPermanentLicense, out Target.IsPermanentLicense, "Permanent License");
                GUILayout.EndHorizontal();

                if (!Target.IsPermanentLicense)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Licenser", GUILayout.Width(LabelWidth));
                    if (GUILayout.Button(Target.LicenserType, EditorGlobalTools.Styles.MiniPopup))
                    {
                        GenericMenu gm = new GenericMenu();
                        List<Type> types = ReflectionToolkit.GetTypesInRunTimeAssemblies(type =>
                        {
                            return type.IsSubclassOf(typeof(LicenserBase)) && !type.IsAbstract;
                        });
                        gm.AddItem(new GUIContent("<None>"), Target.LicenserType == "<None>", () =>
                        {
                            Undo.RecordObject(target, "Set Licenser");
                            Target.LicenserType = "<None>";
                            HasChanged();
                        });
                        for (int i = 0; i < types.Count; i++)
                        {
                            int j = i;
                            gm.AddItem(new GUIContent(types[j].FullName), Target.LicenserType == types[j].FullName, () =>
                            {
                                Undo.RecordObject(target, "Set Licenser");
                                Target.LicenserType = types[j].FullName;
                                HasChanged();
                            });
                        }
                        gm.ShowAsContext();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
        #endregion

        #region Parameter
        private AnimBool _parameterABool;
        private SerializedProperty _mainParameters;
        private ReorderableList _parameterList;        

        private void ParameterEnable()
        {
            _parameterABool = new AnimBool(false, new UnityAction(Repaint));
            _mainParameters = GetProperty("MainParameters");
            _parameterList = new ReorderableList(serializedObject, _mainParameters, true, false, true, true);
            _parameterList.headerHeight = 2;
            _parameterList.elementHeight = 65;
            _parameterList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Parameter");
            };
            _parameterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty mainParameter = _mainParameters.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty = mainParameter.FindPropertyRelative("Name");
                SerializedProperty typeProperty = mainParameter.FindPropertyRelative("Type");
                
                Rect subrect = rect;

                subrect.Set(rect.x, rect.y + 2, 50, 16);
                GUI.Label(subrect, "Name:");
                if (isActive)
                {
                    subrect.Set(rect.x + 50, rect.y + 2, rect.width - 50, 16);
                    nameProperty.stringValue = EditorGUI.TextField(subrect, nameProperty.stringValue);
                }
                else
                {
                    subrect.Set(rect.x + 50, rect.y + 2, rect.width - 50, 16);
                    GUI.Label(subrect, nameProperty.stringValue);
                }

                subrect.Set(rect.x, rect.y + 22, 50, 16);
                GUI.Label(subrect, "Type:");
                subrect.Set(rect.x + 50, rect.y + 22, rect.width - 50, 16);
                typeProperty.enumValueIndex = (int)(MainParameter.ParameterType)EditorGUI.EnumPopup(subrect, (MainParameter.ParameterType)typeProperty.enumValueIndex);

                subrect.Set(rect.x, rect.y + 42, 50, 16);
                GUI.Label(subrect, "Value:");
                subrect.Set(rect.x + 50, rect.y + 42, rect.width - 50, 16);
                SerializedProperty valueProperty;
                switch ((MainParameter.ParameterType)typeProperty.enumValueIndex)
                {
                    case MainParameter.ParameterType.String:
                        valueProperty = mainParameter.FindPropertyRelative("StringValue");
                        valueProperty.stringValue = EditorGUI.TextField(subrect, valueProperty.stringValue);
                        break;
                    case MainParameter.ParameterType.Integer:
                        valueProperty = mainParameter.FindPropertyRelative("IntegerValue");
                        valueProperty.intValue = EditorGUI.IntField(subrect, valueProperty.intValue);
                        break;
                    case MainParameter.ParameterType.Float:
                        valueProperty = mainParameter.FindPropertyRelative("FloatValue");
                        valueProperty.floatValue = EditorGUI.FloatField(subrect, valueProperty.floatValue);
                        break;
                    case MainParameter.ParameterType.Boolean:
                        valueProperty = mainParameter.FindPropertyRelative("BooleanValue");
                        valueProperty.boolValue = EditorGUI.Toggle(subrect, valueProperty.boolValue);
                        break;
                    case MainParameter.ParameterType.Vector2:
                        valueProperty = mainParameter.FindPropertyRelative("Vector2Value");
                        valueProperty.vector2Value = EditorGUI.Vector2Field(subrect, "", valueProperty.vector2Value);
                        break;
                    case MainParameter.ParameterType.Vector3:
                        valueProperty = mainParameter.FindPropertyRelative("Vector3Value");
                        valueProperty.vector3Value = EditorGUI.Vector3Field(subrect, "", valueProperty.vector3Value);
                        break;
                    case MainParameter.ParameterType.Color:
                        valueProperty = mainParameter.FindPropertyRelative("ColorValue");
                        valueProperty.colorValue = EditorGUI.ColorField(subrect, valueProperty.colorValue);
                        break;
                    case MainParameter.ParameterType.DataSet:
                        valueProperty = mainParameter.FindPropertyRelative("DataSet");
                        valueProperty.objectReferenceValue = EditorGUI.ObjectField(subrect, valueProperty.objectReferenceValue, typeof(DataSetBase), false);
                        break;
                    case MainParameter.ParameterType.Prefab:
                        valueProperty = mainParameter.FindPropertyRelative("PrefabValue");
                        valueProperty.objectReferenceValue = EditorGUI.ObjectField(subrect, valueProperty.objectReferenceValue, typeof(GameObject), true);
                        break;
                    case MainParameter.ParameterType.Texture:
                        valueProperty = mainParameter.FindPropertyRelative("TextureValue");
                        valueProperty.objectReferenceValue = EditorGUI.ObjectField(subrect, valueProperty.objectReferenceValue, typeof(Texture), false);
                        break;
                    case MainParameter.ParameterType.AudioClip:
                        valueProperty = mainParameter.FindPropertyRelative("AudioClipValue");
                        valueProperty.objectReferenceValue = EditorGUI.ObjectField(subrect, valueProperty.objectReferenceValue, typeof(AudioClip), false);
                        break;
                    case MainParameter.ParameterType.Material:
                        valueProperty = mainParameter.FindPropertyRelative("MaterialValue");
                        valueProperty.objectReferenceValue = EditorGUI.ObjectField(subrect, valueProperty.objectReferenceValue, typeof(Material), false);
                        break;
                }
            };
            _parameterList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle gUIStyle = (index % 2 != 0) ? "CN EntryBackEven" : "CN EntryBackodd";
                    gUIStyle = (!isActive && !isFocused) ? gUIStyle : "RL Element";
                    gUIStyle.Draw(rect, false, isActive, isActive, isFocused);
                }
            };
        }
        private void ParameterGUI()
        {
            GUILayout.BeginVertical(GetStyle(Page.Parameter));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            bool oldValue = _currentPage == Page.Parameter;
            _parameterABool.target = EditorGUILayout.Foldout(oldValue, "Parameter", true);
            if (_parameterABool.target != oldValue)
            {
                if (_parameterABool.target) _currentPage = Page.Parameter;
                else _currentPage = Page.None;
            }
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_parameterABool.faded))
            {
                _parameterList.DoLayoutList();
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
        #endregion

        #region Setting
        private AnimBool _settingABool;

        private void SettingEnable()
        {
            _settingABool = new AnimBool(false, new UnityAction(Repaint));
        }
        private void SettingGUI()
        {
            GUILayout.BeginVertical(GetStyle(Page.Setting));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            bool oldValue = _currentPage == Page.Setting;
            _settingABool.target = EditorGUILayout.Foldout(oldValue, "Setting", true);
            if (_settingABool.target != oldValue)
            {
                if (_settingABool.target) _currentPage = Page.Setting;
                else _currentPage = Page.None;
            }
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_settingABool.faded))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Log", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Toggle(Target.IsEnabledLogInfo, out Target.IsEnabledLogInfo, "Enabled Log Info");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Toggle(Target.IsEnabledLogWarning, out Target.IsEnabledLogWarning, "Enabled Log Warning");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Toggle(Target.IsEnabledLogError, out Target.IsEnabledLogError, "Enabled Log Error");
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
        #endregion
        
        /// <summary>
        /// 分页
        /// </summary>
        private enum Page
        {
            None,
            ScriptingDefine,
            MainData,
            License,
            Parameter,
            Setting
        }
    }
}