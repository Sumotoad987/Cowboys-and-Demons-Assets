using Kingmaker.Blueprints;
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
    internal static class BlueprintQuestObjectiveEx
    {
        public static void AddAddendum(this BlueprintQuestObjective objective0, BlueprintQuestObjective objective)
        {
            if (objective == objective0)
            {
                PFLog.Default.Error("Objective can't be addendum for itself!");
                return;
            }
            if (objective0.Addendums.Contains(objective))
            {
                PFLog.Default.Warning("Addendum already added");
                return;
            }
            objective0.Addendums.Add(objective);
            objective.SetIsAddendum(true);
            if (objective0.Quest != null)
            {
                objective0.Quest.LinkObjective(objective);
            }
            EditorUtility.SetDirty(objective0);
            //objective0.SetDirty();
        }
        public static void SetIsAddendum(this BlueprintQuestObjective objective, bool isAddendum)
        {
            if (isAddendum && objective.m_Type == BlueprintQuestObjective.Type.Objective)
            {
                objective.m_Type = BlueprintQuestObjective.Type.Addendum;
            }
            else if (!isAddendum)
            {
                objective.m_Type = BlueprintQuestObjective.Type.Objective;
            }
            EditorUtility.SetDirty(objective);
            //objective0.SetDirty();
        }
        public static void RemoveAddendum(this BlueprintQuestObjective objective0, BlueprintQuestObjective objective)
        {
            if (!objective0.Addendums.Contains(objective))
            {
                PFLog.Default.Warning("Addendum not found");
                return;
            }
            objective0.Addendums.Remove(objective);
            EditorUtility.SetDirty(objective0);
            //objective0.SetDirty();
        }
        public static void SetIsFinishParent(this BlueprintQuestObjective objective, bool finishParent)
        {
            if (finishParent == objective.m_FinishParent)
            {
                return;
            }
            objective.m_FinishParent = finishParent;
            EditorUtility.SetDirty(objective);
            //objective0.SetDirty();
        }
        public static void AddNextObjective(this BlueprintQuestObjective objective0, BlueprintQuestObjective objective)
        {
            if (objective == objective0)
            {
                PFLog.Default.Error("Objective can't be next for itself!");
                return;
            }
            if (objective0.m_NextObjectives.Any(r => r.Is(objective)))
            {
                PFLog.Default.Warning("Objective already is next");
                return;
            }
            objective0.m_NextObjectives.Add(objective.ToReference<BlueprintQuestObjectiveReference>());
            if (objective0.Quest != null)
            {
                objective0.Quest.LinkObjective(objective);
            }
            EditorUtility.SetDirty(objective0);
            //objective0.SetDirty();
        }
        public static void RemoveNextObjective(this BlueprintQuestObjective objective0, BlueprintQuestObjective objective)
        {
            if (!objective0.m_NextObjectives.Any((BlueprintQuestObjectiveReference r) => r.Is(objective)))
            {
                PFLog.Default.Warning("Objective not next");
                return;
            }
            objective0.m_NextObjectives.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(objective));
            EditorUtility.SetDirty(objective0);
            //objective0.SetDirty();
        }
        public static void RemoveNullReferences(this BlueprintQuestObjective objective)
        {
            int num = objective.m_NextObjectives.RemoveAll((BlueprintQuestObjectiveReference i) => i == null);
            if (num > 0)
            {
                PFLog.Default.Warning((ICanBeLogContext)objective, "Quest objective '{0}': removed {1} missing next objectives", new object[]
                {
            objective,
            num
                });
            }
            int num2 = objective.m_Addendums.RemoveAll((BlueprintQuestObjectiveReference i) => i == null);
            if (num2 > 0)
            {
                PFLog.Default.Warning((ICanBeLogContext)objective, "Quest objective '{0}': removed {1} missing addendums", new object[]
                {
            objective,
            num2
                });
            }
        }
        public static void SetQuest(this BlueprintQuestObjective objective, BlueprintQuest quest)
        {
            if (objective.Quest == quest)
            {
                return;
            }
            objective.m_Quest = quest.ToReference<BlueprintQuestReference>();
            EditorUtility.SetDirty(objective);
            //objective0.SetDirty();
        }
    }
}
