using System.Reflection;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingmaker.Editor.Utility
{
	public static class EditorUtilityEx
	{
		private static FieldInfo s_LastControlIdField;
		private static FieldInfo s_ActiveEditor;

		static EditorUtilityEx()
		{
			var egu = typeof(EditorGUIUtility);
			/*
			var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			*/
			s_LastControlIdField = egu.GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
			//    private static EditorGUI.RecycledTextEditor activeEditor;
			var eg = typeof(EditorGUI);
			s_ActiveEditor = eg.GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic);
		}

		public static TextEditor GetActiveTextEditor()
		{
			return s_ActiveEditor.GetValue(null) as TextEditor;
		}

        public static bool IsDirty(Object obj)
        {
            return EditorUtility.IsDirty(obj);
        }

        //public static bool IsDirty(SimpleBlueprint obj)
        //{
        //    return BlueprintsDatabase.IsDirty(obj.AssetGuid);
        //}
        
        public static int GetLastControlId()
		{
			return (int)s_LastControlIdField.GetValue(null);
		}

		public static void SetIconForObject(GameObject obj, int idx)
		{
			var ic = EditorGUIUtility.IconContent("sv_label_" + idx);
			var icon = ic.image;
			//EditorGUIUtility.SetIconForObject(obj, icon as Texture2D);
		}

	    public static int SpinnerControl(Rect rect, int value)
	    {
	        const int buttonWidth = 11;
	        rect.xMax -= buttonWidth * 2;
	        var i = EditorGUI.indentLevel;
	        EditorGUI.indentLevel = 0;
	        value = EditorGUI.IntField(rect, value);
	        EditorGUI.indentLevel = i;
            if (GUI.Button(new Rect(rect.xMax, rect.y, buttonWidth, rect.height), "▼", EditorStyles.miniLabel))
                value--;
            if (GUI.Button(new Rect(rect.xMax + buttonWidth, rect.y, buttonWidth, rect.height), "▲", EditorStyles.miniLabel))
                value++;
	        return value;
	    }

		private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
		{
			GUIContent[] array = new GUIContent[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
			}
			return array;
		}
	}
}