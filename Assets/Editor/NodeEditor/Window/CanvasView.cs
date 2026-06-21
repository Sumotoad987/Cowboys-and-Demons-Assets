using System;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class CanvasView
	{
		public Vector2 Pan;
		private int m_ZoomMode = 0;

		public float Scale
		{
			get
			{
				switch (m_ZoomMode)
				{
					case 0:
						return 1f;
					case 1:
						return 0.5f;
					case 2:
						return 0.25f;
					case 3:
						return 0.125f;
					default:
						return 0.125f;
				}
			}
		}

		public bool NeedsScale
		{
			get { return m_ZoomMode > 0; }
		}

		private bool m_CanvasDrag;
		private Vector2 m_StartDrag;
		private Vector2 m_StartDragPan;

		public Rect VisibleScreenArea { get; private set; }

		public Rect ToScreen(Rect canvasRect)
		{
			Rect result = canvasRect;
			result.position += Pan;
			return result;
		}

		public Rect ToCanvas(Rect screenRect)
		{
			Rect result = screenRect;
			result.position -= Pan;
			return result;
		}

		public Vector2 ToScreen(Vector2 canvasPoint)
		{
			return canvasPoint + Pan;
		}

		public Vector2 ToCanvas(Vector2 screenPoint)
		{
			return screenPoint - Pan;
		}

		public void CenterOn(Vector2 canvasPoint, Rect windowRect)
		{
			Pan = -canvasPoint + (windowRect.size / 2) / Scale;
		}

		public void Update(EditorWindow window)
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 2) // middle mouse button
				{
					m_CanvasDrag = true;
					m_StartDrag = Event.current.mousePosition;
					m_StartDragPan = Pan;
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.MouseUp)
			{
				if (Event.current.button == 2) // middle mouse button
				{
					m_CanvasDrag = false;
					Event.current.Use();
				}
			}

			if (m_CanvasDrag && Event.current.type == EventType.MouseDrag)
			{
				Pan = m_StartDragPan + (Event.current.mousePosition - m_StartDrag)/Scale;
				Event.current.Use();
			}

			if (Event.current.type == EventType.ScrollWheel)
			{
				float prevScale = Scale;
				int prevZoomMode = m_ZoomMode;
				Vector2 canvasMouse = ToCanvas(Event.current.mousePosition / Scale);

				m_ZoomMode += Event.current.delta.y < 0 ? -1 : 1;
				m_ZoomMode = Math.Max(0, Math.Min(3, m_ZoomMode));

				if (prevZoomMode == m_ZoomMode)
					return;

				float scale = Scale/prevScale;
				Pan += canvasMouse;
				Pan /= scale;
				Pan -= canvasMouse;

				Event.current.Use();
			}

			var size = window.position.size / Scale;
			VisibleScreenArea = new Rect(Vector2.zero, size);
		}
	}
}