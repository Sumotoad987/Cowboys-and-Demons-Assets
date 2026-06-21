using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Owlcat.Editor.Core.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Kingmaker.ElementsSystemEx;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
	public class BookPageNode : EditorNode<BlueprintBookPage>
	{
		public BookPageNode(Graph graph, BlueprintBookPage asset) : base(graph, asset, new Vector2(200, 200))
		{
		}

		protected override void DrawContent()
		{
			if (Asset.ImageLink.Load())
				GUILayout.Label(new GUIContent(Asset.ImageLink.Load().texture), GUILayout.MaxWidth(200), GUILayout.MaxHeight(200));
			else
				GUILayout.Space(200);

		    BlueprintDialog dialog = Graph.RootAsset as BlueprintDialog;
		    if (dialog == null || dialog.Type != Kingmaker.DialogSystem.Blueprints.DialogType.Epilogue)
		    {
		        return;
		    }

		    using (GuiScopes.UpdateObject(SerializedObject))
            {
#if UNITY_EDITOR && EDITOR_FIELDS
                Profiler.BeginSample("Find Text Property");
                var property = FindProperty("Title");
                Profiler.EndSample();

                if (!Asset.ShowMainCharacterName)
                {
	                GUILayout.Label(Asset.Companion.Get()?.CharacterName);
                }
#endif
	            
            }
        }

		public override IEnumerable<string> GetMarkers(bool extended)
		{
			if (Asset.ShowOnce)
				yield return "Once";
			if (Asset.Conditions.HasConditions)
				yield return ElementsDescription.Conditions(extended, Asset.Conditions);
			if (Asset.OnShow.HasActions)
				yield return ElementsDescription.Actions(extended, Asset.OnShow);
		}

        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
		{
			return Asset.Cues.Dereference()
                .Cast<SimpleBlueprint>()
				.Concat(Asset.Answers.Dereference());
		}
        
		protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
		{
			if (type == typeof(BlueprintBookPage))
				return null;

			if (typeof(DialogSystem.Blueprints.BlueprintCueBase).IsAssignableFrom(type))
				return FindProperty("Cues");
			if (typeof(DialogSystem.Blueprints.BlueprintAnswerBase).IsAssignableFrom(type))
				return FindProperty("Answers");
			return null;
		}
	}
}