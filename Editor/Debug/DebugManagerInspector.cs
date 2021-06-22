﻿using UnityEditor;
using UnityEngine;

namespace HT.Framework
{
    [CustomEditor(typeof(DebugManager))]
    [GiteeURL("https://gitee.com/SaiTingHu/HTFramework")]
    [GithubURL("https://github.com/SaiTingHu/HTFramework")]
    [CSDNBlogURL("https://wanderer.blog.csdn.net/article/details/102570194")]
    internal sealed class DebugManagerInspector : InternalModuleInspector<DebugManager, IDebugHelper>
    {
        protected override string Intro
        {
            get
            {
                return "Debug Manager, this is a runtime debugger for games!";
            }
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            Toggle(Target.IsEnableDebugger, out Target.IsEnableDebugger, "Enable Debugger");
            GUILayout.EndHorizontal();

            if (Target.IsEnableDebugger)
            {
                GUILayout.BeginHorizontal();
                ObjectField(Target.DebuggerSkin, out Target.DebuggerSkin, false, "Debugger Skin");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Toggle(Target.IsChinese, out Target.IsChinese, "Chinese");
                GUILayout.EndHorizontal();
            }
        }
        protected override void OnInspectorRuntimeGUI()
        {
            base.OnInspectorRuntimeGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("No Runtime Data!");
            GUILayout.EndHorizontal();
        }
    }
}