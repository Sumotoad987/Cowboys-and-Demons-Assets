using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class DragAndDropController
	{
		private DragAndDropController()
		{
		}

		public static void UpdateStartDrag()
		{
			var node = DragAndDrop.GetGenericData("node") as EditorNode;
			if (node == null)
				// drag not planned
				return;

			var e = Event.current;
			switch (e.type)
			{
				case EventType.MouseUp:
				case EventType.DragExited:
					// cleanup drag data
					DragAndDrop.PrepareStartDrag();
					break;
				case EventType.MouseDrag:
					DragAndDrop.StartDrag(node.GetName());
					Event.current.Use();
					break;
			}
		}

		public static void Update(EditorNode node)
		{
			TryStart(node);
			UpdateState(node);
			TryAccept(node);
		}

		private static bool CanAccept(EditorNode node)
		{
			if (DragAndDrop.objectReferences.Length == 0)
				return false;

			EditorNode droppedNode = DragAndDrop.GetGenericData("node") as EditorNode;
			if (droppedNode == node)
				return false;

			if (droppedNode != null)
			{
				if (droppedNode.GroupId != node.GroupId)
				{
					// when relinking node to another group, all previous links are cleared.
					// we cant simply add a new link to dropped node which holding 'control' means
					return !Event.current.control;
				}
				if (Event.current.control && !droppedNode.CanBeShared())
				{
					return false;
				}
			}

			foreach (Object droppedObject in DragAndDrop.objectReferences)
			{
				if (droppedObject == null)
					continue;

                var type = droppedObject.GetType();
                
				if (!node.CanAddReference(type, droppedObject as SimpleBlueprint))
					return false;
				if (!(node is CheckNode))
					if (node.GetAllReferencedAssets().Any(a => a == droppedObject))
						return false;
			}
			return true;
		}

		private static void UpdateState(EditorNode node)
		{
			if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.DragUpdated)
			{
				if (CanAccept(node))
				{
					if (DragAndDrop.GetGenericData("node") == null || Event.current.control)
						DragAndDrop.visualMode = DragAndDropVisualMode.Link;
					else
						DragAndDrop.visualMode = DragAndDropVisualMode.Move;
				}
				else
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.None;
				}
			}
		}

		private static void TryAccept(EditorNode node)
		{
			if (Event.current.type != EventType.DragPerform)
				return;
			if (!CanAccept(node))
				return;

			DragAndDrop.AcceptDrag();
			Undo.RecordObject(node.GetAsset(), "Drag-and-drop link to " + node.GetName());

			var droppedNode = DragAndDrop.GetGenericData("node") as EditorNode;
			if (droppedNode != null)
				TryAcceptNode(node, droppedNode);
			else
				TryAcceptShared(node);

			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData("node", null);
			DragAndDrop.objectReferences = new Object[] {};
			node.Graph.Layout();
		}

		private static void TryAcceptNode(EditorNode node, EditorNode droppedNode)
		{
			if (Event.current.control)
			{
				TryAcceptShared(node);
				return;
			}

			var asset = droppedNode.GetAsset();
			if (asset == null)
				return;

			HandleGroupChange(asset, node);

			EditorNode prevParent = droppedNode.Parent;
			if (prevParent != null)
				prevParent.RemoveReferencedAsset(asset, true);

			node.AddReferencedAsset(asset);
			droppedNode.Parent = node;

			UndoManager.Instance.RegisterUndo(
				"Node " + droppedNode.GetName() + " move",
				() => droppedNode.Parent = prevParent);
		}

		private static void TryAcceptShared(EditorNode node)
		{
			foreach (Object o in DragAndDrop.objectReferences)
			{
				ScriptableObject asset = o as ScriptableObject;
				if (asset == null)
					continue;

				HandleGroupChange(asset, node);
				node.AddReferencedAsset(asset);
				if (node.Graph.ContainsNode(asset))
					node.AddVirtualChild(node.Graph.GetNode(asset));
				else
					node.Graph.AddNode(asset);
			}

			DragAndDrop.PrepareStartDrag();
			Event.current.Use();
		}

		private static void TryStart(EditorNode node)
		{
			if (Event.current.button != 0)
				return;

			switch (Event.current.type)
			{
				case EventType.MouseDown:
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.SetGenericData("node", node);
					DragAndDrop.objectReferences = new Object[] {node.GetAsset()};
					break;
			}
		}

		private static void HandleGroupChange(ScriptableObject asset, EditorNode newParent)
		{
			var graph = newParent.Graph;
			if (!graph.ContainsNode(asset))
				return;

			bool groupChanged = newParent.GroupId != graph.GetGroupId(asset);
			if (newParent is CueSequenceNode)
				groupChanged = true;

            if (newParent is BookPageNode && asset as DialogSystem.Blueprints.BlueprintCueBase !=null)
                groupChanged = true;

            if (!groupChanged)
				return;

			var visited = new HashSet<EditorNode>();
			var visitedAssets = new HashSet<ScriptableObject>();
			var queue = new Queue<EditorNode>();
			queue.Enqueue(graph.GetNode(asset));
			visited.Add(graph.GetNode(asset));
			visitedAssets.Add(asset);
			while (queue.Count > 0)
			{
				EditorNode n = queue.Dequeue();
				n.ClearVirtualChildren();

				foreach (EditorNode child in n.GetReferencedNodes())
				{
					if (visited.Contains(child))
						continue;

					visited.Add(child);
					visitedAssets.Add(child.GetAsset());
					queue.Enqueue(child);
				}
			}

			foreach (var n in graph.Nodes)
			{
				if (visited.Contains(n))
					continue;
				foreach (var va in visitedAssets)
					n.RemoveReferencedAsset(va);
			}
		}
	}
}