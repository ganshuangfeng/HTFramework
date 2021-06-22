﻿using UnityEditor;
using UnityEngine;

namespace HT.Framework
{
    [CustomEditor(typeof(AspectTrackManager))]
    [GiteeURL("https://gitee.com/SaiTingHu/HTFramework")]
    [GithubURL("https://github.com/SaiTingHu/HTFramework")]
    [CSDNBlogURL("https://wanderer.blog.csdn.net/article/details/85617377")]
    internal sealed class AspectTrackManagerInspector : InternalModuleInspector<AspectTrackManager, IAspectTrackHelper>
    {
        protected override string Intro
        {
            get
            {
                return "Aspect Track Manager, you can track code calls anywhere in the program, or intercept him.";
            }
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            Toggle(Target.IsEnableAspectTrack, out Target.IsEnableAspectTrack, "Enable Track");
            GUILayout.EndHorizontal();

            if (Target.IsEnableAspectTrack)
            {
                GUILayout.BeginHorizontal();
                Toggle(Target.IsEnableIntercept, out Target.IsEnableIntercept, "Enable Intercept");
                GUILayout.EndHorizontal();
            }
        }
        protected override void OnInspectorRuntimeGUI()
        {
            base.OnInspectorRuntimeGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Intercept Conditions: ");
            GUILayout.EndHorizontal();

            foreach (var condition in Target.InterceptConditions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(condition.Key);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Conditions"))
            {
                Target.InterceptConditions.Clear();
            }
            GUILayout.EndHorizontal();
        }
    }
}