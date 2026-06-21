using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Window;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class AssetReorderController
	{
		public static void Update(Graph graph)
		{
			if (graph == null)
				return;

			if (graph.SelectedNode == null)
				return;

			if (graph.SelectedNode.Parent == null)
				return;

			if (Event.current.control && Event.current.type == EventType.KeyDown)
			{
				switch (Event.current.keyCode)
				{
					case KeyCode.UpArrow:						
						Reorder(graph.SelectedNode, -1);
						break;
					case KeyCode.DownArrow:
						Reorder(graph.SelectedNode, 1);
						break;
				}
			}
		}

		private static void Reorder(EditorNode node, int shift)
		{
			if (node.Parent == null)
				return;

			node.Parent.ReorderReferrencedAsset(node.GetAsset(), shift);
			node.Graph.Layout();
			Event.current.Use();
		}
	}
}