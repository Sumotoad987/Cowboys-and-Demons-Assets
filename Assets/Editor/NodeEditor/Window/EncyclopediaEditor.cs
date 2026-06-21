using System;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.JsonSystem.PropertyUtility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingmaker.Editor.NodeEditor.Window
{
    public class EncyclopediaEditor : NodeEditorBase
    {
        public EncyclopediaEditor()
        {
            titleContent = new GUIContent("Encyclopedia Editor");
        }

        [BlueprintContextMenu("Open in Encyclopedia Editor", BlueprintType = typeof(BlueprintEncyclopediaChapter))]
        public static void OpenAssetInEncyclopediaEditor(BlueprintEncyclopediaChapter bp)
        {
            Focus(bp, null);
        }

        public static void Focus([CanBeNull] BlueprintEncyclopediaChapter chapter, Object asset)
        {
            if (chapter == null)
            {
                PFLog.Default.Error("Node is missing");
                return;
            }
            var window = GetWindow<EncyclopediaEditor>("Encyclopedia Editor", false);
            window.OpenAsset(chapter, asset);
        }

        [MenuItem("Design/Encyclopedia Editor", false, 2004)]
        public static void ShowWindow()
        {
            GetWindow<EncyclopediaEditor>().Show();
        }

        protected override Type GetOpenType()
        {
            return typeof(Kingmaker.Blueprints.Encyclopedia.BlueprintEncyclopediaChapter);
        }
    }
}
