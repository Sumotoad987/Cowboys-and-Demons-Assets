using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor.Blueprints
{
	public class UndoManager : ScriptableObject
	{
		private static UndoManager s_Instance;
		public readonly List<Action> Callbacks = new List<Action>();
		public int UndoCount;

		public event Action UndoRedoPerformed;

		static UndoManager()
		{
			Undo.undoRedoPerformed += OnUndoPerformed;
		}

		public static UndoManager Instance
		{
			get
			{
				if (s_Instance == null)
				{
					foreach (var u in Resources.FindObjectsOfTypeAll<UndoManager>())
						DestroyImmediate(u);

					s_Instance = CreateInstance<UndoManager>();
					if (s_Instance == null)
						throw new Exception("What the fcuk!");
				}
				return s_Instance;
			}
		}

		private static void OnUndoPerformed()
		{
			Instance.HandleUndo();
		}


		private void HandleUndo()
		{
			while (Callbacks.Count > UndoCount)
			{
				var cb = Callbacks[Callbacks.Count - 1];
				try
				{
					cb();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				Callbacks.RemoveAt(Callbacks.Count - 1);
			}

			if (UndoRedoPerformed != null)
			{
				UndoRedoPerformed();
			}
		}

		public void RegisterUndo(string desc, Action callback)
		{
			Undo.FlushUndoRecordObjects();
			Undo.RegisterCompleteObjectUndo(this, desc);

			Callbacks.Add(callback);
			UndoCount++;
		}
	}
}