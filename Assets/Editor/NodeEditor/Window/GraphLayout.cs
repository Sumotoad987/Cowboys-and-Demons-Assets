using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Nodes.Quest;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class GraphLayout
	{
		private const int HeightDist = 16;
		private const int WidthDist = 30;

		protected Graph graph;
		protected readonly Dictionary<EditorNode, int> layers = new Dictionary<EditorNode, int>();

		private readonly HashSet<EditorNode> visited = new HashSet<EditorNode>();

		public readonly List<EditorNode> VisitOrder = new List<EditorNode>();

		private float nextLeafHeight = 1f;
		private float currentWidth;

		private int m_NextGroupId;

		public GraphLayout(Graph graph)
		{
			this.graph = graph;
		}

		public void DoLayout()
		{
			graph.Nodes.ForEach(n => n.LoadParentNode());
			graph.ResetGroups();
			graph.Root.GroupId = 0;
			FillLayers(graph.Root);
			LayoutRecursive(graph.Root);

			HashSet<EditorNode> unreferencedNodes = new HashSet<EditorNode>(graph.Nodes);
			unreferencedNodes.RemoveWhere(n => visited.Contains(n));
			foreach (var node in graph.Nodes)
				node.GetReferencedNodes().ForEach(n => unreferencedNodes.Remove(n));

			while (unreferencedNodes.Count > 0)
			{
				var node = unreferencedNodes.First();
				unreferencedNodes.Remove(node);

				node.GroupId = 0;
				FillLayers(node);
				LayoutRecursive(node);

				unreferencedNodes.RemoveWhere(n => visited.Contains(n));
			}

			while (true)
			{
				EditorNode unvisitedNode = null;
				foreach (EditorNode node in graph.Nodes)
					if (node.GetAsset() != null && !visited.Contains(node))
						unvisitedNode = node;

				if (unvisitedNode == null)
					break;

				unvisitedNode.GroupId = 0;
				FillLayers(unvisitedNode);
				LayoutRecursive(unvisitedNode);
			}
		}

		// returns node height
		private float LayoutRecursive(EditorNode node)
		{
			if (visited.Add(node))
			{
				VisitOrder.Add(node);
			}

			int childrenCount = 0;
			float childrenHeightSum = 0f;
			float minHeight = nextLeafHeight;

			float widthBackup = currentWidth;
			currentWidth += node.Size.x + WidthDist;

			foreach (var child in node.GetReferencedNodes())
			{
				EditorNode actualChild = child;
				bool isVirtual;
				if (child.Parent != null)
					isVirtual = child.Parent != node;
				else
					isVirtual = visited.Contains(child) || layers[child] <= layers[node];

				if (isVirtual)
				{
					var virtualChild = node.AddVirtualChild(child);
					if (virtualChild != null)
					{
						layers[virtualChild] = layers[node] + 1;
						actualChild = virtualChild;
					}
				}

				if (actualChild == null)
					continue;

				actualChild.GroupId = CalculateChildGroup(node, child);

				actualChild.Parent = node;
				childrenCount++;
                if (!visited.Contains(actualChild))
				    childrenHeightSum += LayoutRecursive(actualChild);
			}

			currentWidth = widthBackup;
			node.Center.x = currentWidth + node.Size.x / 2;

			if (childrenCount == 0)
			{
				node.Center.y = nextLeafHeight + node.Size.y / 2;
				nextLeafHeight += node.Size.y + HeightDist;
				return node.Center.y;
			}

			float height = childrenHeightSum / childrenCount;
			if (height < minHeight + node.Size.y / 2)
				height = minHeight + node.Size.y / 2;

			node.Center.y = height;
			nextLeafHeight = Mathf.Max(nextLeafHeight, height + node.Size.y / 2 + HeightDist);
			return height;
		}

		protected virtual void FillLayers(EditorNode root)
		{
			Queue<EditorNode> queue = new Queue<EditorNode>();
			queue.Enqueue(root);
			layers[root] = 0;

			while (!queue.Empty())
			{
				EditorNode node = queue.Dequeue();

				int layer = layers[node];
				foreach (var child in node.GetReferencedNodes())
				{
					if (layers.ContainsKey(child))
						continue;
					layers[child] = layer + 1;
					queue.Enqueue(child);
				}
			}
		}

		private int CalculateChildGroup(EditorNode node, EditorNode child)
		{
            if (node.GetBlueprint() is BlueprintCueSequence)
            {
                if (child is CueSequenceExitNode)
                    return node.GroupId;
                else
                    return ++m_NextGroupId;
            }

            if (node.GetBlueprint() is BlueprintBookPage)
            {
                if (child.GetBlueprint() is BlueprintAnswerBase)
                    return node.GroupId;
                else
                    return ++m_NextGroupId;
            } 


            var nodeObjective = node.GetBlueprint() as BlueprintQuestObjective;
            var childObjective = child.GetBlueprint() as BlueprintQuestObjective;
            if (nodeObjective != null && childObjective != null)
            {
                if (!nodeObjective.IsAddendum && childObjective.IsAddendum)
                {
                    var questNode = node as ObjectiveNode;
                    if (questNode != null)
                    {
                        if (questNode.AddendumsGroup <= 0)
                            questNode.AddendumsGroup = ++m_NextGroupId;
                        return questNode.AddendumsGroup;
                    }
                }
            } 

            return node.GroupId;
		}
	}
}