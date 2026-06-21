using System;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Window;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class DrawFunctions
	{
		private DrawFunctions()
		{
		}

		public static void Connection(CanvasView view, EditorNode n1, EditorNode n2, Color color)
		{
			Connection(view, n1, n1.Size.y / 2, n2, n2.Size.y / 2, color);
		}

		public static void Connection(CanvasView view, EditorNode n1, float y1, EditorNode n2, Color color)
		{
			Connection(view, n1, y1, n2, n2.Size.y / 2, color);
		}

		public static void Connection(CanvasView view, EditorNode n1, float y1, EditorNode n2, float y2, Color color)
		{
			Vector2 p1 = n1.Center;
			p1.x += n1.Size.x/2;
			p1.y -= n1.Size.y/2;
			p1.y += y1;
			Vector2 p2 = n2.Center;
			p2.x -= n2.Size.x/2;
			p2.y -= n2.Size.y / 2;
			p2.y += y2;

			Vector2 sp1 = view.ToScreen(p1);
			Vector2 sp2 = view.ToScreen(p2);
			Vector2 tg1 = sp1 + 0.5f * new Vector2(Math.Abs(p2.x - p1.x), 0);
			Vector2 tg2 = sp2 - 0.5f * new Vector2(Math.Abs(p2.x - p1.x), 0);

			float width = 5 / view.Scale;
			Handles.DrawBezier(sp1, sp2, tg1, tg2, color, null, width);
		}
	}
}