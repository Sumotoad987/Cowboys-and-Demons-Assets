using System;
using System.Text;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Ex.Kingmaker.ElementsSystem;

namespace Kingmaker.ElementsSystemEx
{
    public static class ElementsDescription
    {
        public static string Conditions(bool extended, ConditionsChecker checker, string caption = "Conditions", int indent = 0)
        {
            if (!extended)
            {
                int num = checker.Conditions.Length;
                return string.Format("{0} ({1})", caption, num);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendIndent(indent);
            stringBuilder.Append(caption + ElementsDescription.CheckerOperation(checker) + ":\n");
            foreach (ElementsSystem.Condition element in checker.Conditions)
            {
                ElementsDescription.AppendElement(stringBuilder, element, indent + 1);
            }
            return stringBuilder.ToString().TrimEnd();
        }

        public static string Actions(bool extended, params ActionList[] actionLists)
        {
            return ElementsDescription.Actions(extended, "Actions", 0, actionLists);
        }

        public static string Actions(bool extended, string caption, int indent, params ActionList[] actionLists)
        {
            if (!extended)
            {
                int num = 0;
                foreach (ActionList actionList in actionLists)
                {
                    num += actionList.Actions.Length;
                }
                return string.Format("{0} ({1})", caption, num);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendIndent(indent);
            stringBuilder.Append(caption + ":\n");
            for (int i = 0; i < actionLists.Length; i++)
            {
                foreach (ElementsSystem.GameAction element in actionLists[i].Actions)
                {
                    ElementsDescription.AppendElement(stringBuilder, element, indent + 1);
                }
            }
            return stringBuilder.ToString().TrimEnd();
        }

        public static void AppendIndent(this StringBuilder sb, int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                sb.Append("    ");
            }
        }

        private static void AppendElement(StringBuilder sb, ElementsSystem.Element element, int indent)
        {
            sb.AppendIndent(indent);
            sb.Append(((element != null) ? element.GetCaption() : null) ?? "").Append('\n');
            OrAndLogic orAndLogic = element as OrAndLogic;
            if (orAndLogic != null)
            {
                foreach (var element2 in orAndLogic.ConditionsChecker.Conditions)
                {
                    ElementsDescription.AppendElement(sb, element2, indent + 1);
                }
            }
            Conditional conditional = element as Conditional;
            if (conditional != null)
            {
                sb.AppendIndent(indent);
                sb.Append("If" + ElementsDescription.CheckerOperation(conditional.ConditionsChecker) + "\n");
                foreach (var element3 in conditional.ConditionsChecker.Conditions)
                {
                    ElementsDescription.AppendElement(sb, element3, indent + 1);
                }
                sb.AppendIndent(indent);
                sb.Append("Then\n");
                foreach (var element4 in conditional.IfTrue.Actions)
                {
                    ElementsDescription.AppendElement(sb, element4, indent + 1);
                }
                if (conditional.IfFalse.Actions.Length != 0)
                {
                    sb.AppendIndent(indent);
                    sb.Append("Else\n");
                    foreach (var element5 in conditional.IfFalse.Actions)
                    {
                        ElementsDescription.AppendElement(sb, element5, indent + 1);
                    }
                }
            }
            RandomAction randomAction = element as RandomAction;
            if (randomAction != null)
            {
                foreach (ActionAndWeight actionAndWeight in randomAction.Actions)
                {
                    if (actionAndWeight.Action.HasActions)
                    {
                        sb.AppendIndent(indent + 1);
                        sb.AppendLine(string.Format("Weight: {0}", actionAndWeight.Weight));
                        if (actionAndWeight.Conditions.HasConditions)
                        {
                            sb.AppendLine(ElementsDescription.Conditions(true, actionAndWeight.Conditions, "Conditions", indent + 1));
                        }
                        sb.AppendLine(ElementsDescription.Actions(true, "Actions", indent + 1, new ActionList[]
                        {
                            actionAndWeight.Action
                        }));
                        sb.AppendLine();
                    }
                }
            }
        }

        private static string CheckerOperation(ConditionsChecker checker)
        {
            if (checker.Operation == ElementsSystem.Operation.And || checker.Conditions.Length <= 1)
            {
                return "";
            }
            return string.Format(" [{0}]", checker.Operation);
        }
    }
}
