using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingmaker.Editor.DragDrop
{
	public class DragManager
	{
		public delegate void DragWindowGui(DraggedWindow window);

		public static readonly DragManager Instance = new DragManager();

		public Object[] UnityObjects { get; set; }
		private bool m_DragInProgress;
		private DraggedWindow m_DraggedWindow;
		private EditorWindow m_MouseOverWindow;

		public bool DragInProgress { get; private set; }

        public bool IsDragging<T>()
        {
            return DragInProgress && DraggedObject is T;
        }

		private DragManager()
		{
		}

		public Vector2 ScreenDropPoint { get; private set; }

		public Vector2 DropPoint
		{
			get { return GUIUtility.ScreenToGUIPoint(ScreenDropPoint); }
		}

		public object DraggedObject { get; private set; }
		public KingmakerWindowBase DragSourceWindow { get; private set; }

		public void BeginDrag(DragWindowGui guiCallback, object draggedObject, Vector2 size = default(Vector2))
		{
			if (m_DraggedWindow)
			{
				m_DraggedWindow.Close();
				Object.DestroyImmediate(m_DraggedWindow);
			}

			m_DraggedWindow = ScriptableObject.CreateInstance<DraggedWindow>();
			if (!m_DraggedWindow)
			{
				Debug.LogErrorFormat("WTF!");
				return;
			}

			DragSourceWindow = EditorWindow.focusedWindow as KingmakerWindowBase;

			ScreenDropPoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			if (size == Vector2.zero)
				size = new Vector2(100, 20);

			m_DraggedWindow.ShowAsDropDown(new Rect(ScreenDropPoint, Vector2.one), size);

			DragInProgress = true;
			DraggedObject = draggedObject;
			m_DraggedWindow.Callback = guiCallback;

            Event.current.Use();
		}

		public void UpdateDrag()
		{
			//If drag is in process and unity lost info about it - reset drag process
			if (DragInProgress && Event.current.type == EventType.MouseMove)
			{
				m_DraggedWindow.Close();
				Object.DestroyImmediate(m_DraggedWindow);
				DraggedObject = null;
				DragInProgress = false;
				
				return;
			}
			
			if (!m_DraggedWindow || !DragInProgress)
				return;

			if (Event.current.type == EventType.MouseDrag)
			{
				var previousMo = m_MouseOverWindow;
				m_MouseOverWindow = EditorWindow.mouseOverWindow;

				//Small offset is necessary, otherwise drag and drop events might get caught by the window itself.
				ScreenDropPoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) + new Vector2(-20, 20);
				m_DraggedWindow.position = new Rect(ScreenDropPoint, m_DraggedWindow.position.size);

				if (m_MouseOverWindow != null)
				{
					m_MouseOverWindow.SendEvent(new Event(Event.current) {type = EventType.DragUpdated});
					if (previousMo != null && previousMo != m_MouseOverWindow)
					{
						previousMo.SendEvent(new Event(Event.current) {type = EventType.DragExited});
						previousMo.Repaint();
					}
				}

				Event.current.Use();
			}

			if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
			{
				bool handledByUnity = false;
				m_DraggedWindow.Close();
				Object.DestroyImmediate(m_DraggedWindow);

				var mo = EditorWindow.mouseOverWindow;
				if (mo)
				{
					mo.Focus();

					var evt = new Event(Event.current)
					{
						type = EventType.DragPerform,
					};
					evt.mousePosition += new Vector2(0, 20); // hack: this is probably required b/c of window header height not being correctly applied on Send

					// hacK: to drop Unity Objects to places that expect them, but still be able to use DragManager full fuctionalty,
					// we create a drag operation right before drop and send it to the drop target window.
					// This breaks our own windows that expect only DragPerform though, so we don't use this with KingmakeWindowBases
					if (UnityObjects != null && UnityObjects.Length > 0 && !(mo is KingmakerWindowBase))
					{
						//UberDebug.Log("Drop UnityObjects to " + mo+" pos="+Event.current.mousePosition);

						Event.current.type = EventType.MouseDrag;
						DragAndDrop.PrepareStartDrag();
						DragAndDrop.StartDrag("DragManager"); // <- this actually creates a giant bunch of events that are handled by SendEvent
						DragAndDrop.objectReferences = UnityObjects;
						Event.current.type = EventType.MouseUp;
						handledByUnity = true;
					}
					try
					{
						if (!handledByUnity)
						{
							mo.SendEvent(evt);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}

				EditorApplication.delayCall += () => // do not clear just now - SendEvent is now delayed until next frame
				{
					DragInProgress = false;
					DraggedObject = null;
					mo?.Repaint();
					//UberDebug.Log("Cleared drag");
				};

				if (!handledByUnity)
				{
					Event.current.Use();
				}
			}
		}
	}
}