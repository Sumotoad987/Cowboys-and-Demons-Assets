using Kingmaker.Editor.DragDrop;
using UnityEditor;

namespace Kingmaker.Editor
{
	public class KingmakerWindowBase : EditorWindow
	{
		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		protected virtual void OnGUI()
		{
			if (DragManager.Instance.DragInProgress)
				DragManager.Instance.UpdateDrag();
		}
	}
}