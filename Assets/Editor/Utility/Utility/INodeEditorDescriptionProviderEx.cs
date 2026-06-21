using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ex.Kingmaker.Designers.Quests.Common;
using Kingmaker.ElementsSystemEx;
using Ex.Kingmaker.ElementsSystem;
using Ex.Kingmaker.Designers.EventConditionActionSystem.ObjectiveEvents;

namespace Kingmaker.Blueprints.Quests.Logic
{
    public static class INodeEditorDescriptionProviderEx
    {
        public static string GetDescription(this INodeEditorDescriptionProvider instance)
        {
            if (instance is QuestObjectiveCallback q)
                return QuestObjectiveCallback(q);
            else if (instance is ObjectiveStatusTrigger s)
                return ObjectiveStatusTrigger(s);
            else if (instance is ChangeObjectiveOnUnlockTrigger c)
                return ChangeObjectiveOnUnlockTriggerr(c);
            else if (instance is GiveUnlockOnObjectiveTrigger g)
                return GiveUnlockOnObjectiveTrigger(g);
            else if (instance is SummonPoolCountTrigger p)
                return SummonPoolCountTrigger(p);
            else return "No Description";
        }



        static string QuestObjectiveCallback(QuestObjectiveCallback instance)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (instance.m_OnComplete.HasActions)
            {
                stringBuilder.Append("On Complete ");
                stringBuilder.Append(ElementsDescription.Actions(true, new ActionList[]
                {
                    instance.m_OnComplete
                }));
            }
            if (instance.m_OnFail.HasActions)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append("\n");
                }
                stringBuilder.Append("On Fail ");
                stringBuilder.Append(ElementsDescription.Actions(true, new ActionList[]
                {
                    instance.m_OnFail
                }));
            }
            return stringBuilder.ToString();

        }
        static string ObjectiveStatusTrigger(ObjectiveStatusTrigger instance)
        {
            StringBuilder stringBuilder = new StringBuilder(string.Format("When {0}", instance.objectiveState));
            if (instance.Conditions.HasConditions)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(ElementsDescription.Conditions(true, instance.Conditions, "Conditions", 0));
            }
            stringBuilder.Append("\n");
            stringBuilder.Append(ElementsDescription.Actions(true, new ActionList[]
            {
                instance.Actions
            }));
            return stringBuilder.ToString();

        }
        static string ChangeObjectiveOnUnlockTriggerr(ChangeObjectiveOnUnlockTrigger instance)
        {
            string text = (instance.OwnerBlueprint as BlueprintQuestObjective == instance.targetObjective) ? "" : string.Format(" {0}", instance.targetObjective);
            string text2 = (instance.unlockStatus == ChangeObjectiveOnUnlockTrigger.UnlockStatus.OnGain) ? "unlocked" : "locked";
            return string.Format("{0}{1} when {2} is {3}", new object[]
            {
                instance.setStatus,
                text,
                instance.unlock,
                text2
            });
        }
        static string GiveUnlockOnObjectiveTrigger(GiveUnlockOnObjectiveTrigger instance)
        {
            return string.Format("Unlock Flag {0} when {1}", instance.unlock, instance.objectiveState);
        }

        static string SummonPoolCountTrigger(SummonPoolCountTrigger instance)
        {
            return string.Format("{0} when {1} count is {2}", instance.setStatus, instance.summonPool, instance.count);
        }
    }
}
