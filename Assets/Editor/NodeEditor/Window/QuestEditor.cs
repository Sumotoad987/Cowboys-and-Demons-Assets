using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.PropertyUtility;
using Ex.Kingmaker.Blueprints.Quests;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class QuestEditor : NodeEditorBase
	{
		public QuestEditor()
		{
			titleContent = new GUIContent("Quest Editor");
		}

        [BlueprintContextMenu("Open in Quest Editor", BlueprintType = typeof(BlueprintQuest))]
		public static void OpenAssetInQuestEditor(BlueprintQuest bp)
		{
			Focus(bp, null);
		}

		public static void Focus([CanBeNull] BlueprintQuest quest, Object asset)
		{
            if (quest == null)
            {
                quest = (asset as BlueprintQuestObjective)?.Quest;
            }

            if (quest == null)
            {
                PFLog.Default.Error("Quest is missing");
                return;
            }

            var window = GetWindow<QuestEditor>("Quest Editor", false);
            window.OpenAsset(quest, asset); 
        }

		[MenuItem("Design/Quest Editor", false, 2004)]
		public static void ShowWindow()
		{
			GetWindow<QuestEditor>().Show();
		}

		protected override Type GetOpenType()
		{
			return typeof(Kingmaker.Blueprints.Quests.BlueprintQuest);
		}
	}
}