using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owlcat.Runtime.Core.Logging;
using UnityEditor;

namespace Ex.Kingmaker.Blueprints.Quests
{
    internal static class BlueprintQuestEx
    {
        public static void LinkObjective(this BlueprintQuest quest, BlueprintQuestObjective objective)
        {
            if (quest.m_Type == QuestType.Errand)
            {
                using (List<BlueprintQuestObjectiveReference>.Enumerator enumerator = quest.m_Objectives.GetEnumerator())
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
            if (objective.Quest == quest && quest.m_Objectives.Any((BlueprintQuestObjectiveReference r) => r.Is(objective)))
            {
                return;
            }
            if (objective.Quest != null)
            {
                PFLog.Default.Warning("Objective already linked to quest, link changed");
                objective.Quest.UnlinkObjective(objective);
            }
            quest.m_Objectives.Add(objective.ToReference<BlueprintQuestObjectiveReference>());
            objective.SetQuest(quest);
            EditorUtility.SetDirty(quest);
            //quest.SetDirty();
        }

        public static void UnlinkObjective(this BlueprintQuest quest, BlueprintQuestObjective objective)
        {
            if (!quest.m_Objectives.Any((BlueprintQuestObjectiveReference r) => r.Is(objective)))
            {
                PFLog.Default.Warning("Quest doesn't contains this objective");
            }
            quest.m_Objectives.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(objective));
            objective.SetQuest(null);
            EditorUtility.SetDirty(quest);
            //quest.SetDirty();
        }

        private static void UpdateObjectives(this BlueprintQuest quest)
        {
            quest.m_Objectives.ForEach(delegate (BlueprintQuestObjectiveReference objective)
            {
                objective.Get().RemoveNullReferences();
                objective.Get().SetQuest(quest);
                if (quest.m_Type == QuestType.Errand && !objective.Get().IsHidden)
                {
                    objective.Get().SetIsFinishParent(true);
                }
            });
            List<BlueprintQuestObjectiveReference> list = quest.m_Objectives.ToList<BlueprintQuestObjectiveReference>();
            using (IEnumerator<BlueprintQuestObjective> enumerator = quest.Addendums.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BlueprintQuestObjective addendum = enumerator.Current;
                    addendum.SetIsAddendum(true);
                    list.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(addendum));
                }
            }
            foreach (BlueprintQuestObjectiveReference blueprintQuestObjectiveReference in list)
            {
                blueprintQuestObjectiveReference.Get().SetIsAddendum(false);
            }
            quest.m_Objectives.ForEach(delegate (BlueprintQuestObjectiveReference objective)
            {
                foreach(var addendum in objective.Get().Addendums)
                {
                    addendum.SetIsAddendum(true);
                };
            });
            EditorUtility.SetDirty(quest);
            //quest.SetDirty();
        }

        public static void UpdateAndValidateObjectives(this BlueprintQuest quest)
        {
            quest.RemoveNullReferences();
            quest.UpdateObjectives();
            EditorUtility.SetDirty(quest);
            //quest.SetDirty();
        }
        private static void RemoveNullReferences(this BlueprintQuest quest)
        {
            int num = quest.m_Objectives.RemoveAll((BlueprintQuestObjectiveReference objective) => objective == null);
            if (num > 0)
            {
                PFLog.Default.Warning((ICanBeLogContext)quest, "Quest '{0}': removed {1} missing objectives", new object[]
                {
            quest,
            num
                });
            }
        }
    }
}
