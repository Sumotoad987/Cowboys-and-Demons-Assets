using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.DragDrop
{
	public class DraggedWindow : EditorWindow
	{
		private Vector2 m_LastMousePos;
		public DragManager.DragWindowGui Callback { get; set; }

		private void OnGUI()
		{
			if (Callback != null)
				Callback(this);
		}
	}
}