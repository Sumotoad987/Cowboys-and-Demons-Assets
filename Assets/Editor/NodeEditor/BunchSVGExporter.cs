using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.Quests;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.Editor.Utility;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor
{
	public static class BunchSvgExporter
	{
		//[UsedImplicitly]
		//private static void ExportAllDialogsBuilder()
		//{
		//	EditorGUIUtility.Load("Assets/QA/Corks/Cork.png");
		//	Thread.Sleep(10000);
		//	ExportDialogsAllLocales();
		//	EditorApplication.Exit(0);
		//}

		//[MenuItem("Tools/Narrative and Text/SVG Export/Export dialogs (EN, RU)")]
		//public static void ExportDialogsEnRu()
		//{
		//	var locales = new[] {Locale.ruRU, Locale.enGB};
		//	EditorCoroutine.Start(ExportAllDialogsRoutine("DialogMaps", locales, false, false, false));
		//}

		//[MenuItem("Tools/Narrative and Text/SVG Export/Export dialogs in all locales")]
		//public static void ExportDialogsAllLocales()
  //      {
  //          var locales = Enum.GetValues(typeof(Locale))
  //              .Cast<Locale>()
  //              .Where(l => l != Locale.Sound)
  //              .ToArray();
		//	EditorCoroutine.Start(ExportAllDialogsRoutine("DialogMaps", locales, false, false, true));
		//}

		//[MenuItem("Tools/Narrative and Text/SVG Export/Export dialogs for walkthrough")]
		//public static void ExportDialogsWalkthrough()
		//{
		//	var locales = new[] {Locale.enGB};
		//	EditorCoroutine.Start(ExportAllDialogsRoutine("WalkthroughDialogs", locales, true, true, true));
		//}

		//[MenuItem("Tools/Narrative and Text/SVG Export/Export quests for walkthrough")]
		//public static void ExportQuestsWalkthrough()
		//{
		//	var locales = new[] {Locale.enGB};
		//	EditorCoroutine.Start(ExportAllQuestsRoutine("WalkthroughQuests", locales, true, true));
		//}

		//public static IEnumerator ExportAllDialogsRoutine(string folder, Locale[] locales, bool markers, bool expandMarkers, bool deleteOld)
		//{
		//	PFLog.Default.Log("ExportAllDialogsRoutine started");
		//	try
		//	{
		//		if (EditorUtility.DisplayCancelableProgressBar("SVG Export", "Collecting dialogs...", 0f))
		//			yield break;

		//		List<BlueprintDialog> dialogs= Kingmaker.Blueprints.ResourcesLibrary.GetBlueprints<BlueprintDialog>().ToList();
		//		int processedCount = 0;
		//		var validSvgPaths = new HashSet<string>();
		//		foreach (var dialog in dialogs)
		//		{
		//			processedCount++;
		//			if (EditorUtility.DisplayCancelableProgressBar(
		//				"SVG Export",
		//				$"Processing dialog: {dialog}...",
		//				1f * processedCount / dialogs.Count))
		//				yield break;

  //                  DialogEditor.FocusAsset(dialog, dialog, false); 
  //                  var window = EditorWindow.GetWindow<DialogEditor>();

		//			var pathTemplate = AssetPathUtility.GetFilePath(dialog)
		//				.Replace("/Blueprints/", $"/{folder}/")
		//				.Replace(".jbp", "");

		//			foreach (var locale in locales)
		//			{
		//				LocalizationManager.CurrentLocale = locale;
		//				window.Graph.ReloadGraph();
		//				window.Graph.ShowExtendedMarkers = expandMarkers;
		//				window.Repaint();
		//				yield return null;

		//				var svgPath = pathTemplate + "_" + locale + ".svg";
		//				svgPath = new Uri(svgPath).LocalPath.Replace("\\", "/");
		//				validSvgPaths.Add(svgPath);

		//				var routine = window.SvgExportCoroutine(svgPath, markers);
		//				while (routine.MoveNext())
		//				{
		//					yield return null;
		//				}
		//			}
		//		}

		//		if (deleteOld)
		//		{
		//			if (EditorUtility.DisplayCancelableProgressBar("SVG Export", "Deleting outdated SVG maps...", 0f))
		//				yield break;

  //                  var dialogMapsPath = (Application.dataPath + "/")
  //                      .Replace("\\", "/")
  //                      .Replace("/Blueprints/", $"/{folder}/");

		//			var existingSvgPaths = Directory.GetFiles(dialogMapsPath, "*.svg", SearchOption.AllDirectories)
		//				.Select(p => new Uri(p).LocalPath.Replace("\\", "/"))
		//				.ToArray();

		//			foreach (var svgPath in existingSvgPaths)
		//			{
		//				if (!validSvgPaths.Contains(svgPath))
		//					File.Delete(svgPath);
		//			}
		//		}
		//	}
		//	finally
		//	{
		//		EditorUtility.ClearProgressBar();
		//	}
		//}

		//public static IEnumerator ExportAllQuestsRoutine(string folder, Locale[] locales, bool markers, bool expandMarkers)
		//{
		//	PFLog.Default.Log("ExportAllQuestsRoutine started");
		//	try
		//	{
		//		if (EditorUtility.DisplayCancelableProgressBar("SVG Export", "Collecting quests...", 0f))
		//			yield break;

		//		List<BlueprintQuest> quests = Kingmaker.Blueprints.ResourcesLibrary.GetBlueprints<BlueprintQuest>().ToList();
		//		int processedCount = 0;
		//		var validSvgPaths = new HashSet<string>();
		//		foreach (var quest in quests)
		//		{
		//			processedCount++;
		//			if (EditorUtility.DisplayCancelableProgressBar(
		//				"SVG Export",
		//				$"Processing quest: {quest}...",
		//				1f * processedCount / quests.Count))
		//				yield break;

  //                  QuestEditor.Focus(quest, null); 
  //                  var window = EditorWindow.GetWindow<QuestEditor>();

		//			var pathTemplate = AssetPathUtility.GetFilePath(quest)
  //                      .Replace("/Blueprints/", $"/{folder}/")
  //                      .Replace(".jbp", "");

		//			foreach (var locale in locales)
		//			{
		//				LocalizationManager.CurrentLocale = locale;
		//				window.Graph.ReloadGraph();
		//				window.Graph.ShowExtendedMarkers = expandMarkers;
		//				window.Repaint();
		//				yield return null;

		//				var svgPath = pathTemplate + "_" + locale + ".svg";
		//				svgPath = new Uri(svgPath).LocalPath.Replace("\\", "/");
		//				validSvgPaths.Add(svgPath);

		//				var routine = window.SvgExportCoroutine(svgPath, markers);
		//				while (routine.MoveNext())
		//				{
		//					yield return null;
		//				}
		//			}
		//		}

		//		if (EditorUtility.DisplayCancelableProgressBar("SVG Export", "Deleting outdated SVG maps...", 0f))
		//			yield break;

  //              var questMapPaths = (Application.dataPath + "/")
  //                  .Replace("\\", "/")
  //                  .Replace("/Blueprints/", $"/{folder}/");

		//		var existingSvgPaths = Directory.GetFiles(questMapPaths, "*.svg", SearchOption.AllDirectories)
		//			.Select(p => new Uri(p).LocalPath.Replace("\\", "/"))
		//			.ToArray();

		//		foreach (var svgPath in existingSvgPaths)
		//		{
		//			if (!validSvgPaths.Contains(svgPath))
		//				File.Delete(svgPath);
		//		}
		//	}
		//	finally
		//	{
		//		EditorUtility.ClearProgressBar();
		//	}
		//}
	}
}