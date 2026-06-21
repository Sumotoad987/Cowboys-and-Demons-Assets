using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public static class EditorZoomArea
	{
		private static readonly Stack<Matrix4x4> s_PreviousMatrices = new Stack<Matrix4x4>();

		public static void Begin(float zoomScale, Rect screenCoordsArea)
		{
			GUI.EndGroup();
			s_PreviousMatrices.Push(GUI.matrix);
			GUIUtility.ScaleAroundPivot(Vector2.one * zoomScale, new Vector2(0, 40));
		}

		public static void End()
		{
			GUI.matrix = s_PreviousMatrices.Pop();
			GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
		}
	}
}