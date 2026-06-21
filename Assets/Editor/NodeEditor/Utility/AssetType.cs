using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class NodeEditorAssetType
	{
		[NotNull]
		public readonly string Name;

		[NotNull]
		public readonly SimpleBlueprint Template;

		public readonly KeyCode CreateHotkey;

		public readonly string Prefix;

		[NotNull]
		private static readonly List<NodeEditorAssetType> s_AllTypes = new List<NodeEditorAssetType>();

		public static IEnumerable<NodeEditorAssetType> AllTypes
		{
			get
			{
				CheckTemplates();
				return s_AllTypes;
			}
		}

		static NodeEditorAssetType()
		{
			Init();
		}

		private static void Init()
		{
			s_AllTypes.Clear();
			s_AllTypes.Add(new NodeEditorAssetType("Dialog", typeof(BlueprintDialog), KeyCode.None));
			s_AllTypes.Add(new NodeEditorAssetType("Answer", typeof(BlueprintAnswer), KeyCode.None));
			s_AllTypes.Add(new NodeEditorAssetType("Answers list", typeof(BlueprintAnswersList), KeyCode.None));
			s_AllTypes.Add(new NodeEditorAssetType("Cue", typeof(BlueprintCue), KeyCode.Q));
			s_AllTypes.Add(new NodeEditorAssetType("Check", typeof(BlueprintCheck), KeyCode.E));
			s_AllTypes.Add(new NodeEditorAssetType("Book page", typeof(BlueprintBookPage), KeyCode.None));
			s_AllTypes.Add(new NodeEditorAssetType("Cue sequence", typeof(BlueprintCueSequence), KeyCode.None));
			s_AllTypes.Add(new NodeEditorAssetType("Cue sequence exit", typeof(BlueprintSequenceExit), KeyCode.None));

			// quests
			s_AllTypes.Add(new NodeEditorAssetType("Objective", typeof(BlueprintQuestObjective), KeyCode.Q, "Objective"));
            BlueprintQuestObjective addendum = ScriptableObject.CreateInstance<BlueprintQuestObjective>();
            addendum.SetIsAddendum(true);
            s_AllTypes.Add(new NodeEditorAssetType("Addendum", addendum, KeyCode.W, "Addendum"));

            // encyclopedia
            s_AllTypes.Add(new NodeEditorAssetType("Encyclopedia Page", typeof(BlueprintEncyclopediaPage), KeyCode.Q, "Page"));
		}

		private static void CheckTemplates()
		{
			if (s_AllTypes.Select(t => t.Template).Any(t => t == null))
				Init();
		}

		public static NodeEditorAssetType GetCreationType([NotNull] Event evt, [NotNull] EditorNode parent)
		{
			CheckTemplates();
			if (evt.type == EventType.KeyUp && evt.control)
			{
				foreach (var at in s_AllTypes)
				{
					if (at.CreateHotkey == KeyCode.None)
						continue;
					if (at.CreateHotkey != evt.keyCode)
						continue;
					if (parent.CanAddReference(at.Template.GetType(), at.Template))
						return at;
				}
			}
			return null;
		}

		private NodeEditorAssetType(string name, [NotNull] Type type, KeyCode createHotkey, string prefix = "")
		{
			Name = name;
			Template = (SimpleBlueprint)ScriptableObject.CreateInstance(type);
			CreateHotkey = createHotkey;
			Prefix = prefix != "" ? prefix : type.Name.Replace("Blueprint", "");
		}

		private NodeEditorAssetType(string name, BlueprintScriptableObject template, KeyCode createHotkey, string prefix = "")
		{
			Name = name;
			Template = template;
			CreateHotkey = createHotkey;
			Prefix = prefix != "" ? prefix : template.GetType().Name.Replace("Blueprint", "");
		}
	}
}