
using System;
using Ex.Kingmaker.Blueprints;
//using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility;
//using Kingmaker.Utility.DotNetExtensions;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase
{
	public class BlueprintEditorUtility
	{
		public static event Action<Ex.Kingmaker.Blueprints.SimpleBlueprint> OnPing;

		public static Ex.Kingmaker.Blueprints.SimpleBlueprint ObjectField(Rect position, GUIContent label, Ex.Kingmaker.Blueprints.SimpleBlueprint bp, Type type, bool allowSceneObjects = false)
		{
			ProfileScope profileScope = ProfileScope.New("ObjectField", null) as ProfileScope;
            Ex.Kingmaker.Blueprints.SimpleBlueprint result;
			try
			{
				int controlID = GUIUtility.GetControlID(4386533, FocusType.Keyboard, position);
				position = EditorGUI.PrefixLabel(position, controlID, label);
				Event current = Event.current;
				EventType type2 = Event.current.type;
				if (type2 <= EventType.Repaint)
				{
					if (type2 != EventType.MouseDown)
					{
						if (type2 != EventType.Repaint)
						{
							goto IL_32D;
						}
						bool? flag = null;
						if (bp == null)
						{
							flag = null;
						}
						//else
						//{
						//	BlueprintEditorMetadata meta = bp.GetMeta();
						//	flag = ((meta != null) ? new bool?(meta.ShadowDeleted) : null);
						//}
						bool? flag2 = flag;
						bool valueOrDefault = flag2.GetValueOrDefault();
						GUIContent content = new GUIContent((bp == null) ? ("None [" + type.Name + "]") : (valueOrDefault ? string.Concat(new string[]
						{
							"DELETED ",
							bp.name,
							" [",
							bp.GetType().Name,
							"]"
						}) : (bp.name + " [" + bp.GetType().Name + "]")));
						ProfileScope profileScope2 = ProfileScope.New("Paint", null) as ProfileScope;
						try
						{
							GUIStyle guistyle = "ObjectFieldButton";
							EditorStyles.objectField.Draw(position, content, controlID, DragAndDrop.activeControlID == controlID, position.Contains(Event.current.mousePosition));
							if (valueOrDefault)
							{
								Rect rect = position;
								rect.y += rect.height / 2f;
								rect.height = 1f;
								EditorGUI.DrawRect(rect, Color.red);
							}
							Rect position2 = guistyle.margin.Remove(new Rect(position.xMax - 19f, position.y, 19f, position.height));
							guistyle.Draw(position2, GUIContent.none, controlID, DragAndDrop.activeControlID == controlID, position2.Contains(Event.current.mousePosition));
							goto IL_32D;
						}
						finally
						{
							if (profileScope2 != null)
							{
								((IDisposable)profileScope2).Dispose();
							}
						}
					}
					else
					{
						if (Event.current.button != 0 || !position.Contains(Event.current.mousePosition))
						{
							goto IL_32D;
						}
						Rect rect2 = new Rect(position.xMax - 19f, position.y, 19f, position.height);
						EditorGUIUtility.editingTextField = false;
						if (!rect2.Contains(Event.current.mousePosition) && Event.current.clickCount == 1)
						{
							GUIUtility.keyboardControl = controlID;
							Action<Ex.Kingmaker.Blueprints.SimpleBlueprint> onPing = BlueprintEditorUtility.OnPing;
							if (onPing != null)
							{
								onPing(bp);
							}
							current.Use();
							goto IL_32D;
						}
						goto IL_32D;
					}
				}
				else if (type2 - EventType.DragUpdated > 1)
				{
					if (type2 != EventType.DragExited)
					{
						goto IL_32D;
					}
					if (GUI.enabled)
					{
						HandleUtility.Repaint();
						goto IL_32D;
					}
					goto IL_32D;
				}
				if (position.Contains(Event.current.mousePosition) && GUI.enabled)
				{
                    var simpleBlueprint = DragAndDrop.objectReferences.FirstOrDefault<UnityEngine.Object>() as Ex.Kingmaker.Blueprints.SimpleBlueprint;
					if (type.IsInstanceOfType(simpleBlueprint))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
						if (type2 == EventType.DragPerform)
						{
							bp = simpleBlueprint;
							GUI.changed = true;
							DragAndDrop.AcceptDrag();
							DragAndDrop.activeControlID = 0;
						}
						else
						{
							DragAndDrop.activeControlID = controlID;
						}
						Event.current.Use();
					}
				}
				IL_32D:
				result = bp!;
			}
			finally
			{
				if (profileScope != null)
				{
					((IDisposable)profileScope).Dispose();
				}
			}
			return result;
		}

		public static Ex.Kingmaker.Blueprints.SimpleBlueprint ObjectField(string label, SimpleBlueprint bp, Type type, bool allowSceneObjects = false)
		{
			return BlueprintEditorUtility.ObjectField(GUILayoutUtility.GetRect(0f, 1000f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), new GUIContent(label), bp, type, false);
		}

		public static Ex.Kingmaker.Blueprints.SimpleBlueprint ObjectField(SimpleBlueprint bp, Type type, bool allowSceneObjects = false)
		{
			return BlueprintEditorUtility.ObjectField(GUILayoutUtility.GetRect(0f, 1000f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), GUIContent.none, bp, type, false);
		}

		public static void ShowType(string label, Type t)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 1000f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
			GUI.Label(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height), new GUIContent(label));
			rect.xMin += EditorGUIUtility.labelWidth + 1f;
			GUI.Label(rect, t?.Name ?? "not set", EditorStyles.objectField);
			if (t != null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				//string text;
				//string text2;
				//BlueprintsDatabase.Binder.BindToName(t, out text, out text2);
				UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath(AssetDatabase.FindAssets(t.Name).FirstOrDefault(), typeof(MonoScript));
				if (Event.current.clickCount > 1)
				{
					Selection.activeObject = @object;
				}
				EditorGUIUtility.PingObject(@object);
				Event.current.Use();
			}
		}
	}
}
