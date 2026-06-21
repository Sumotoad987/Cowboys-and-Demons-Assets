using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using static Kingmaker.Blueprints.BlueprintExtenstions;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.Localization;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Nodes.Quest
{
    public class ObjectiveNode : EditorNode<BlueprintQuestObjective>
    {
        public int AddendumsGroup = 0;

        public ObjectiveNode(Graph graph, BlueprintQuestObjective asset) : base(graph, asset, new Vector2(200, 50))
        {

        }

		public override string GetText()
		{
			return Asset.Description;
		}

        public override EditorNode AddVirtualChild(EditorNode referencedNode)
        {
            return null;
        }

        public override Color GetWindowColor()
        {
            return Asset.IsAddendum ? Color.yellow : Color.green;
        }

        protected override void DrawContent()
        {
            SerializedObject.Update();

#if true || (UNITY_EDITOR && EDITOR_FIELDS)
            if (!Asset.IsErrandObjective)
            {
                GUILayout.Label("Title");
                var title = FindProperty("Title");
                var ww = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                EditorGUILayout.TextArea(Asset.Title, ww);

                GUILayout.Label("Description");
                var description = FindProperty("Description");
                EditorGUILayout.TextArea(Asset.Title, ww);
            }
#endif

            SerializedObject.ApplyModifiedProperties();
        }

		public override IEnumerable<string> GetMarkers(bool extended)
        {
            if (Asset.IsHidden)
                yield return "Hidden";
            if (Asset.IsAutomaticallyStartingAddendum)
                yield return "Auto-Start";
            if (Asset.IsFinishParent)
            {
                if (Asset.NextObjectives.Count == 0)
                    yield return "Complete";
				else
					yield return "Fail";
            }

			if (extended)
			{
				foreach (var xp in Asset.GetComponents<Experience>())
				{
					yield return string.Format($"XP (CR={xp.CR}, {xp.Encounter})");
                }

				foreach (var logic in Asset.GetComponents<INodeEditorDescriptionProvider>())
				{
					yield return logic.GetDescription();
				}
			}
        }

        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
        {
        var firstAddendums = Asset.Addendums.ToList();
            foreach (var o in Asset.Addendums)
            {
                if (o == null)
                    continue;
                foreach (var next in o.NextObjectives)
                {
                    firstAddendums.Remove(next);
                }
            }

            return Asset.NextObjectives.Concat(firstAddendums);
        }

        protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
        {
            var objective =  r as BlueprintQuestObjective;
            if (objective == null)
                return null;

            if (!Asset.IsAddendum)
            {
                if (objective.IsAddendum)
                    return FindProperty("m_Addendums");
                else
                    return FindProperty("m_NextObjectives");
            }
            else
            {
                if (objective.IsAddendum)
                    return FindProperty("m_NextObjectives");
            }

            return null;
        }

        public override void RemoveReferencedAsset(ScriptableObject asset, bool move = false)
        {
            var objective = (BlueprintQuestObjective)(asset);//asset as BlueprintQuestObjective;
            if (objective == null)
                return;

            if (Asset.Addendums.Contains(objective))
                foreach(var o in GetAddendumsTree(objective))
                    Asset.RemoveAddendum(o);
            if (Asset.NextObjectives.Contains(objective))
                Asset.RemoveNextObjective(objective);
        }

        public override void AddReferencedAsset(ScriptableObject asset)
        {
            var objective = asset as BlueprintQuestObjective;//asset as BlueprintQuestObjective;
            if (objective == null)
                return;

            if (Asset.IsAddendum)
            {
                objective.SetIsAddendum(true);
                Asset.AddNextObjective(objective);

                ObjectiveNode parentObjective = this;
                while (parentObjective != null && parentObjective.Asset.IsAddendum)
                    parentObjective = parentObjective.Parent as ObjectiveNode;

                if (parentObjective != null)
                    foreach(var o in GetAddendumsTree(objective))
                        parentObjective.Asset.AddAddendum(o);
            }
            else
            {
                if (objective.IsAddendum)
                    foreach (var o in GetAddendumsTree(objective))
                        Asset.AddAddendum(o);
                else
                    Asset.AddNextObjective(objective);
            }
        }

        private static IEnumerable<BlueprintQuestObjective> GetAddendumsTree(BlueprintQuestObjective firstAddendum)
        {
            var queue = new Queue<BlueprintQuestObjective>();
            var visited = new HashSet<BlueprintQuestObjective>();
            queue.Enqueue(firstAddendum);
            while (queue.Count > 0)
            {
                var addendum = queue.Dequeue();
                visited.Add(addendum);
                foreach(var o in addendum.NextObjectives.Where(n => !visited.Contains(n)))
                  queue.Enqueue(o);
            }

            return visited;
        }
    }
}