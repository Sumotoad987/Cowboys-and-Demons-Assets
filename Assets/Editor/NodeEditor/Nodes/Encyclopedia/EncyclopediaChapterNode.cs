using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.Localization;
using UnityEditor;
using UnityEngine;
using KM = Ex.Kingmaker;

namespace Kingmaker.Editor.NodeEditor.Nodes.Encyclopedia
{
    public class EncyclopediaChapterNode : EditorNode<BlueprintEncyclopediaChapter>
    {
        public EncyclopediaChapterNode(Graph graph, BlueprintEncyclopediaChapter asset) : base(graph, asset, new Vector2(200, 50))
        {
            
        }
        public override EditorNode AddVirtualChild(EditorNode referencedNode)
        {
            return null;
        }
        protected override void DrawContent()
        {
            SerializedObject.Update();

#if true || (UNITY_EDITOR && EDITOR_FIELDS)
            GUILayout.Label("Title");
            var title = FindProperty("Title");
            var ww = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            EditorGUILayout.TextArea(Asset.Title, ww);
#endif

            SerializedObject.ApplyModifiedProperties();
        }

        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
        {
            return Asset.ChildPages.Select(p => p.Get()).ToList();
        }
        
        protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
        {
			if (type == typeof(BlueprintEncyclopediaPage))
				return FindProperty("ChildPages");
			return null;
		}
    }
}
