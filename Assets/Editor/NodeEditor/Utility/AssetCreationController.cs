using System;
using System.IO;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Window;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class AssetCreationController
	{
		public static void Update(Graph graph)
		{
			//if (graph == null)
			//	return;

			//var node = graph.SelectedNode;
			//if (node == null)
			//	return;

			//if (Event.current.commandName == "Duplicate")
			//{
			//	if (Event.current.type == EventType.ValidateCommand)
			//		if (CanDuplicateNode(node))
			//			Event.current.Use();
			//	if (Event.current.type == EventType.ExecuteCommand)
			//		DuplicateNode(node);
			//	return;
			//}

			//if (ShowAssetCreationMenu(node))
			//	return;

			//var type = NodeEditorAssetType.GetCreationType(Event.current, node);
			//if (type == null)
			//	return;

			//Event.current.Use();

			//CreateAssetWithParent(node, type);
		}

		//private static bool ShowAssetCreationMenu(EditorNode parent)
		//{
		//	if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
		//	{
		//		var menu = new GenericMenu();

  //              foreach (var at in NodeEditorAssetType.AllTypes)
  //              {
  //                  if (at.Template is BlueprintBookPage)
  //                  {
  //                      var dialog = parent.Graph.RootAsset as BlueprintDialog;
  //                      if (dialog == null || dialog.Type == DialogType.Common)
  //                          continue;
  //                  }

  //                  if (parent.CanAddReference(at.Template.GetType(), at.Template))
  //                  {
  //                      var atSafe = at;
  //                      menu.AddItem(new GUIContent("Add " + at.Name), false, () => CreateAssetWithParent(parent, atSafe));
  //                  }
  //              }

  //              var virtualNode = parent as VirtualNode;
  //              if (virtualNode != null)
  //                  menu.AddItem(new GUIContent("Move real node here"), false, () => virtualNode.MakeReal());

  //              if (menu.GetItemCount() > 0)
		//		{
		//			Event.current.Use();
		//			menu.ShowAsContext();
		//			return true;
		//		}
		//	}

		//	return false;
		//}

		//private static void CreateAssetWithParent(EditorNode parent, NodeEditorAssetType type)
		//{
		//	ScriptableObject newAsset = CreateAsset(type, parent.Graph);
		//	parent.AddReferencedAsset(newAsset);
		//	parent.Graph.AddNode(newAsset);
		//	parent.Graph.Layout();
		//}

		//private static bool CanDuplicateNode(EditorNode node)
		//{
		//	if (node is VirtualNode)
		//		return false;
		//	if (node is DialogNode)
		//		return false;
		//	return true;
		//}

		//private static void DuplicateNode(EditorNode node)
		//{
		//	if (!CanDuplicateNode(node))
		//		return;

  //          var bp = node.GetAsset() as SimpleBlueprint;
  //          ScriptableObject newAsset;
  //          if (bp != null)
  //          {
  //              string sourcePath = BlueprintsDatabase.GetAssetPath(bp);
  //              string copyPath = CreateDuplicatePath(sourcePath);

  //              var bp2 = BlueprintsDatabase.CopyAsset(bp, copyPath);
  //              newAsset = bp2;
  //              if (newAsset == null)
  //              {
  //                  Debug.LogError("Could not duplicate asset " + sourcePath);
  //                  return;
  //              }
  //              // todo: Undo?
		//	}
		//	else
  //          {
  //              string sourcePath = AssetDatabase.GetAssetPath(node.GetAsset());
  //              string copyPath = CreateDuplicatePath(sourcePath);

  //              AssetDatabase.CopyAsset(sourcePath, copyPath);
  //              newAsset = AssetDatabase.LoadMainAssetAtPath(copyPath) as ScriptableObject;
  //              if (newAsset == null)
  //              {
  //                  Debug.LogError("Could not duplicate asset " + sourcePath);
  //                  return;
  //              }

  //              Undo.RegisterCreatedObjectUndo(newAsset, "Copy " + node.GetName());
  //          }

  //          EditorNode newNode = node.Graph.AddNode(newAsset);
		//	if (newNode != null)
		//	{
		//		if (node.Parent != null)
		//		{
		//			node.Parent.AddReferencedAsset(newAsset);
		//			newNode.Parent = node.Parent;
		//		}
		//		newNode.RemoveReferencedAssets((o) => true);
		//	}

		//	Event.current.Use();
		//}

		//private static ScriptableObject CreateAsset(NodeEditorAssetType type, Graph graph)
  //      {
  //          type.Template.AssetGuid = BlueprintGuid.NewGuid();
  //          var newBP = (SimpleBlueprint)JsonUtility.FromJson(JsonUtility.ToJson(type.Template), type.Template.GetType());
		//	SaveNewAsset(type, newBP, graph);
		//	return newBP;
		//}

		//private static void SaveNewAsset(NodeEditorAssetType type, SimpleBlueprint asset, Graph graph)
		//{
		//	string dialogPath = BlueprintsDatabase.GetAssetPath(graph.RootAsset as SimpleBlueprint);
		//	string directoryName = Path.GetDirectoryName(dialogPath);
		//	if (directoryName == null)
		//		return;

		//	string assetPath = Path.Combine(directoryName, type.Prefix + "_");
		//	string id = (graph.NextAssetId++).ToString("0000");
		//	assetPath += id;
		//	assetPath += ".jbp";

		//	//Undo.RegisterCreatedObjectUndo(asset, "New " + asset.GetType().Name);
		//	//AssetDatabase.CreateAsset(asset, assetPath);
  //          BlueprintsDatabase.CreateAsset(asset, assetPath);
		//	UndoManager.Instance.RegisterUndo("New " + asset.GetType().Name, () => BlueprintsDatabase.DeleteAsset(asset));
		//}

		//private static string CreateDuplicatePath(string path)
		//{
		//	string s = path;
		//	int attempt = 0;
		//	while (File.Exists(s))
		//	{
		//		attempt++;
		//		s = Regex.Replace(
		//			s,
		//			@"( \d+)?\.jbp",
		//			match =>
		//			{
		//				if (match.Groups[1].Value == "")
		//					return " 1.jbp";
		//				int i = Convert.ToInt32(match.Groups[1].Value);
		//				return " " + (i + 1).ToString() + ".jbp";
		//			}
		//		);
		//		if (attempt > 10000)
		//		{
		//			Debug.LogError("Could not generate duplication path for " + path);
		//			break;
		//		}
		//	}
		//	return s;
		//}
	}
}