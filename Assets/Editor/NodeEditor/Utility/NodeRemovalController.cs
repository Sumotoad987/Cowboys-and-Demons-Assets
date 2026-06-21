using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Nodes.Quest;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.Utility;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class NodeRemovalController
	{
		public static void Update(Graph graph)
		{
			if (graph == null)
				return;
			if (graph.SelectedNode == null)
				return;

			if (Event.current.type == EventType.KeyDown
				&& Event.current.keyCode == KeyCode.Delete)
			{
				EditorNode parent = graph.SelectedNode.Parent;
				var asset = graph.SelectedNode.GetAsset();

                // todo: [bp] handle removal of elements etc here

                if (parent != null)
				{					
					graph.SelectedNode.Parent = null;
					graph.SelectedNode.SetParentAsset(null);

					if (graph.SelectedNode is ObjectiveNode)
						graph.Nodes.ForEach(n => n.RemoveReferencedAsset(asset));
					else
						parent.RemoveReferencedAsset(asset);					

					Event.current.Use();
					graph.Layout();
					return;
				}

				if (Event.current.control)
				{
					//if (AssetDatabase.Contains(asset))
					//	AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
					
					graph.ReloadGraph();
					Event.current.Use();
				}
			}
		}
	}
}