using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyOwlcatModification
{
    [CustomPropertyDrawer(typeof(LongAsEnumAttribute), true)]
    public class LongAsEnumAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var type = (attribute as LongAsEnumAttribute).EnumType;
            var result = EditorGUI.EnumPopup(position, label, Enum.ToObject(type, property.longValue) as Enum);
            property.longValue = (long)Enum.ToObject(type, result);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
