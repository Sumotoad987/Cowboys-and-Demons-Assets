using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Window;
using UnityEditor;
using UnityEngine;
using Kingmaker.ElementsSystemEx;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
	public class DialogNode : EditorNode<BlueprintDialog>
	{
		public DialogNode(Graph graph, BlueprintDialog asset) : base(graph, asset, new Vector2(100, 50))
		{
		}

		protected override void DrawContent()
		{
			GUILayout.Label(Asset.name);
		}

        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
		{
			return Asset.FirstCue.Cues.Select(r => r.Get());
		}

        protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
		{
			if (typeof(DialogSystem.Blueprints.BlueprintCueBase).IsAssignableFrom(type))
				return FindProperty("FirstCue.Cues");
			return null;
		}

		public override IEnumerable<string> GetMarkers(bool extended)
		{
			if (Asset.Conditions.HasConditions)
				yield return ElementsDescription.Conditions(extended, Asset.Conditions);
			if (Asset.ReplaceActions.HasActions)
				yield return ElementsDescription.Actions(extended, "Replace Actions", 0, Asset.ReplaceActions);
			if (Asset.StartActions.HasActions)
				yield return ElementsDescription.Actions(extended, "Start Actions", 0, Asset.StartActions);
			if (Asset.FinishActions.HasActions)
				yield return ElementsDescription.Actions(extended, "Finish Actions", 0, Asset.FinishActions);
		}


        public static void SearchDialogRecursive(SimpleBlueprint bp, ref List<Kingmaker.Blueprints.BlueprintGuid> Ids)
		{
            if (bp == null)
            {
                Debug.LogError("NULL BP");
            }
			if (Ids.Contains(bp.AssetGuid))
				return;
			Ids.Add(bp.AssetGuid);
			if (bp is BlueprintCue cue)
			{
				foreach (var a in cue.Answers.Select(r => r.Get()))
					SearchDialogRecursive(a, ref Ids);
				foreach (var a in cue.Continue.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
            else if (bp is BlueprintBookPage bookPage)
            {
                foreach (var a in bookPage.Answers.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
                foreach (var a in bookPage.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
            else if (bp is BlueprintCueSequence sequence)
            {
                foreach (var a in sequence.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
				SearchDialogRecursive(sequence.Exit, ref Ids);
            }
			else if (bp is BlueprintSequenceExit exit)
            {
                foreach (var a in exit.Answers.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
                foreach (var a in exit.Continue.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
            else if (bp is BlueprintDialog dialog)
            {
                foreach (var a in dialog.FirstCue.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
            else if (bp is BlueprintCheck check)
            {
                SearchDialogRecursive(check.Fail, ref Ids);
                SearchDialogRecursive(check.Success, ref Ids);
            }
            else if (bp is BlueprintAnswersList answerList)
            {
                foreach (var a in answerList.Answers.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
            else if (bp is BlueprintAnswer answer)
            {
                foreach (var a in answer.NextCue.Cues.Select(r => r.Get()))
                    SearchDialogRecursive(a, ref Ids);
            }
        }

    }
}