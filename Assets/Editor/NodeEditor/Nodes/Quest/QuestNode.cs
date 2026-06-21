using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.Classes.Experience;
using Ex.Kingmaker.Blueprints.Quests;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.Enums;
using Kingmaker.Localization;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Nodes.Quest
{
	public class QuestNode : EditorNode<BlueprintQuest>
	{	
		public QuestNode(Graph graph, BlueprintQuest asset) : base(graph, asset, new Vector2(200, 50))
		{
		}

		public override EditorNode AddVirtualChild(EditorNode referencedNode)
		{
			return null;
		}

		public override string GetText()
		{
			return Asset.Description;
		}

		protected override void DrawContent()
		{
			SerializedObject.Update();

#if true || (UNITY_EDITOR && EDITOR_FIELDS)
			GUILayout.Label("Title");
			var title = FindProperty("Title");
            var ww = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            EditorGUILayout.TextArea(Asset.Title, ww);

			GUILayout.Label("Description");
			var description = FindProperty("Description");
            EditorGUILayout.TextArea(Asset.Description, ww);
#endif

			SerializedObject.ApplyModifiedProperties();
		}

        protected override IEnumerable<Ex.Kingmaker.Blueprints.SimpleBlueprint> GetAllReferencedAssetsInternal()
		{
			var list = Asset.Objectives.ToList();
			foreach (var o in Asset.Objectives)
			{
				if (o == null)
					continue;
				foreach (var next in o.NextObjectives)
				{
					list.Remove(next);
				}
			}
			return list.Where(o => !o.IsAddendum);
		}

		public override IEnumerable<string> GetMarkers(bool extended)
		{
			if (extended)
			{
				foreach (var xp in Kingmaker.Blueprints.BlueprintExtenstions.GetComponents<Experience>(Asset))
				{
					yield return $"XP (CR={xp.CR}, {xp.Encounter})";
				}
			}
		}

		protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
        {
            if (r is BlueprintQuestObjective objective && objective.IsAddendum)
				return null;

			if (typeof(BlueprintQuestObjective).IsAssignableFrom(type))
				return FindProperty("m_Objectives");
			return null;
		}

		public override void AddReferencedAsset(ScriptableObject asset)
        {
            var objective = asset as BlueprintQuestObjective;
			if (objective == null)
				return;
			objective.m_Type = BlueprintQuestObjective.Type.Addendum;
			LinkObjective(Asset, objective);
		}

		static void LinkObjective(BlueprintQuest quest, BlueprintQuestObjective objective)
        {
            if (quest.m_Type == QuestType.Errand)
            {
                using (List<Ex.Kingmaker.Blueprints.BlueprintQuestObjectiveReference>.Enumerator enumerator = quest.m_Objectives.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Get().IsHidden)
                        {
                            objective.IsHidden = true;
                            break;
                        }
                    }
                }
            }
            if (objective.Quest == quest && quest.m_Objectives.Any((Ex.Kingmaker.Blueprints.BlueprintQuestObjectiveReference r) => r.Is(objective)))
            {
                return;
            }
            if (objective.Quest != null)
            {
                PFLog.Default.Warning("Objective already linked to quest, link changed");
                UnlinkObjective(objective.Quest, objective);
            }
            quest.m_Objectives.Add(objective.ToReference<Ex.Kingmaker.Blueprints.BlueprintQuestObjectiveReference>());
            SetQuest(objective, quest);

        }
        public static void SetQuest(BlueprintQuestObjective objective, BlueprintQuest quest)
        {
            if (objective.Quest == quest)
            {
                return;
            }
            objective.m_Quest = quest.ToReference<Ex.Kingmaker.Blueprints.BlueprintQuestReference>();
        }

        public override void RemoveReferencedAsset(ScriptableObject asset, bool move = false)
		{
            var objective =  asset as BlueprintQuestObjective;
			if (objective == null)
				return;
			UnlinkObjective(Asset, objective);
		}
        public static void UnlinkObjective(BlueprintQuest quest, BlueprintQuestObjective objective)
        {
            if (!quest.m_Objectives.Any((Ex.Kingmaker.Blueprints.BlueprintReferenceBase r) => r.Equals(objective)))
            {
                PFLog.Default.Warning("Quest doesn't contains this objective");
            }
            quest.m_Objectives.RemoveAll((Ex.Kingmaker.Blueprints.BlueprintQuestObjectiveReference r) => r.Equals(objective));
            SetQuest(objective, null);
        }
    }
}