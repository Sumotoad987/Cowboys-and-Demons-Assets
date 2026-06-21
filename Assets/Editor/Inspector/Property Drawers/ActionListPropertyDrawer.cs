using Ex.Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using Diag = System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using static MyOwlcatModification.Assets.Editor.Inspector.Advanced_Dropdown.Game_Action.AddGameActionWindow;
using static Kingmaker.GameModes.GameModeType;
using static com.spacepuppyeditor.EditorHelpers;
using System.Text.RegularExpressions;

namespace MyOwlcatModification
{
    public class SeamatosReorderableListWrapperHackeroni
    {
        public SerializedProperty Property { get; }

        public SeamatosReorderableListWrapperHackeroni(SerializedProperty property, GUIContent label, bool reorderable = true) {
            m_ReorderableList = new ReorderableList(property.serializedObject, property.Copy(), reorderable, false, true, true);
            Property = property;
        }

        public void Draw(GUIContent label, Rect r, Rect visibleArea, string tooltip, bool includeChildren) { }
        public int GetHeight() { return 42; }

        public ReorderableList m_ReorderableList;
    }

    [CustomPropertyDrawer(typeof(ActionList), true)]
    public class ActionListPropertyDrawer : PropertyDrawer
    {
        Diag.Stopwatch watch = new Diag.Stopwatch();
        float cachedHeight = EditorGUIUtility.singleLineHeight;

        SeamatosReorderableListWrapperHackeroni /*ReorderableListWrapper*/ reorderableList;
        SerializedProperty cachedProperty;
        static readonly Rect infinityRect = new Rect(0, 0, float.PositiveInfinity, float.PositiveInfinity);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //watch.Restart();
            {
                if (reorderableList == null || reorderableList.Property != cachedProperty)
                {
                    cachedProperty = property.FindPropertyRelative("Actions");
                    reorderableList = new SeamatosReorderableListWrapperHackeroni(cachedProperty, label);
                    reorderableList.m_ReorderableList.onAddDropdownCallback += OnAddDropdownCallback;
                    reorderableList.m_ReorderableList.drawElementCallback += OnDrawElementCallback;

                }
                reorderableList.Draw(label, position, infinityRect, string.Empty, true);
            }
            //watch.Stop();
            //Debug.Log($"OnGUI. Event type - {Event.current.type}. Elapsed {watch.Elapsed}");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //watch.Restart();
            if (Event.current.type == EventType.Layout)
            {
                cachedHeight = reorderableList?.GetHeight() ?? EditorGUIUtility.singleLineHeight;
            }
            //watch.Stop();
            //Debug.Log($"GetPropertyHeight Event type - {Event.current.type}. Elapsed {watch.Elapsed}");
            return cachedHeight;
        }

        void OnAddDropdownCallback (Rect rect, ReorderableList list)
        {
            AddGameActionAdvancedDropdownWindow.Show(rect, list);
        }
        void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var property = reorderableList.m_ReorderableList.serializedProperty;
            //Debug.Log($"Property at {index} is null? {property == null}");
            if (property == null)
            {
                if (reorderableList.m_ReorderableList != null)
                    goto noProperty;
                else return;
            }
            else
            {
                var arr = reorderableList.m_ReorderableList.serializedProperty;
                if (!arr.isArray || arr.arraySize <= index)
                    return;
            }
            property = property.GetArrayElementAtIndex(index);
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = rect.xMax * 0.45f - 35f - (property.depth * 15) - (Regex.Matches(property.propertyPath, ".Array.data").Count * 10); ;
            EditorGUI.PropertyField(rect, property, new GUIContent((GetTargetObjectOfProperty(property) as Kingmaker.ElementsSystem.Element).GetCaption() ?? property.displayName), property.isExpanded);
            EditorGUIUtility.labelWidth = labelWidth;


            ;noProperty:;
        }
    }


}
