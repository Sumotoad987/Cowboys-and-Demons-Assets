using System;
using System.Collections.Generic;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Utility;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.ElementsSystemEx;
using Kingmaker.Enums;
using UnityEditor;
using UnityEngine;
using KM = Ex.Kingmaker;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
	public class AnswersListNode : EditorNode<BlueprintAnswersList>
	{
		public AnswersListNode(Graph graph, BlueprintAnswersList asset) : base(graph, asset, new Vector2(200, 80))
		{
		}

		public override Color GetWindowColor()
		{
			return Colors.AnswerListWindow;
		}

		protected override void DrawContent()
		{
			GUILayout.Space(50);
		}

		public override IEnumerable<string> GetMarkers(bool extended)
		{
			if (Asset.ShowOnce)
				yield return "Once";
			if (Asset.Conditions.HasConditions)
				yield return ElementsDescription.Conditions(extended, Asset.Conditions);
			//if (Asset.SoulMarkRequirement.Empty)
			//	yield return  $"{Asset.SoulMarkRequirement.Direction.ToString()} + {Asset.SoulMarkRequirement.Value}";
		}
		
        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
		{
			return Asset.Answers.Dereference();
		}
        
		protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
		{
			if (type == typeof(BlueprintAnswer))
				return FindProperty("Answers");
			return null;
		}
	}
}