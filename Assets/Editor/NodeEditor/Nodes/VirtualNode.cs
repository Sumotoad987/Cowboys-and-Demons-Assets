using System;
using Kingmaker.Blueprints;
using Kingmaker.Editor.NodeEditor.Utility;
using Kingmaker.Editor.NodeEditor.Window;
using UnityEditor;
using UnityEngine;
using KM = Ex.Kingmaker;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
	public class VirtualNode : EditorNode
	{
		private readonly EditorNode m_ReferencedNode;

		public VirtualNode(Graph graph, EditorNode referencedNode) : base(graph, new Vector2(200, 10))
		{
			m_ReferencedNode = referencedNode;
		}

		public override Color GetWindowColor()
		{
			return m_ReferencedNode.GetWindowColor() * Color.grey;
		}

		public override EditorNode AddVirtualChild(EditorNode referencedNode)
		{
			return null;
		}

		public override string GetName()
		{
			return m_ReferencedNode.GetName();
		}

		public override ScriptableObject GetAsset()
		{
			return m_ReferencedNode.GetAsset();
		}

		public override ScriptableObject GetParentAsset()
		{
			return m_ReferencedNode.GetParentAsset();
		}

		public override void SetParentAsset(ScriptableObject parent)
		{
		}

		public override SerializedObject GetSerializedObject()
		{
			return m_ReferencedNode.GetSerializedObject();
		}

		public EditorNode GetReferencedNode()
		{
			return m_ReferencedNode;
		}

		protected override void DrawContent()
		{
			GUILayout.Space(16);
		}

		public override void DrawConnections(CanvasView view)
		{
			if (Graph.SelectedNode == this
				|| Graph.SelectedNode == m_ReferencedNode
				|| Graph.ShowAllVirtualLinks)
			{
				DrawFunctions.Connection(view, m_ReferencedNode, 8, this, 8, Colors.VirtualLink);
			}
		}

		public void MakeReal()
		{
			if (Parent == null)
				return;

			m_ReferencedNode.Parent = Parent;
			Parent.VirtualChildren.Remove(GetAsset());
			Graph.Layout();
		}

		protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
		{
			return null;
		}
    }
}