using System;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class BlueprintNodeEditor : NodeEditorBase
	{
		public BlueprintNodeEditor()
		{
			titleContent = new GUIContent("Node Editor");
		}

		[MenuItem("Design/Node Editor", false, 2002)]
		public static void ShowWindow()
		{
			GetWindow<BlueprintNodeEditor>().Show();
		}

		public static void OpenAssetInNodeEditor()
		{
			var rootAsset = Selection.activeObject as ScriptableObject;
			if (!rootAsset)
				return;

			var window = GetWindow<BlueprintNodeEditor>("Node Editor", false);
			window.OpenAsset(rootAsset);
		}

		public static void OpenNewAsset([NotNull] ScriptableObject rootAsset)
		{
			var window = GetWindow<BlueprintNodeEditor>("Node Editor", false);
			window.OpenAsset(rootAsset);
		}
		public static void OpenNewAsset([CanBeNull] SimpleBlueprint blueprint)
		{
			var window = GetWindow<BlueprintNodeEditor>("Node Editor", false);
			window.OpenAsset(blueprint);
		}

		public static void CheckForNewNodes()
		{
			var window = GetWindow<BlueprintNodeEditor>();
			if (!window)
				return;
			if (window.Graph == null)
				return;
			window.Graph.CheckForNewNodes();
		}

		protected override Type GetOpenType()
		{
			return typeof(BlueprintScriptableObject);
		}
	}
}