using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static com.spacepuppyeditor.EditorHelpers;
using HarmonyLib;
using Ex.Kingmaker.Localization;
using Ex.Kingmaker.Blueprints;

namespace MyOwlcatModification
{
    [CustomPropertyDrawer(typeof(ConditionalAttribute), true)]
    public class ConditionalAttributeCustomAttributeDrawer : PropertyDrawer
    {
        bool setup = false;
        object target;
        bool Visible;
        MemberInfo ConditionProperty;
        LocalizedStringPropertyDrawer cachedLocStringDrawer;
        BlueprintReferenceDrawer cachedBpRefDrawer;

        void SetUp(SerializedProperty property)
        {
            //Debug.Log($"ShowIfCustomAttributeDrawer SetUp - {property.propertyPath}");
            string conditionSource = (attribute as ConditionalAttribute)?.ConditionSource;
           
           int index = property.propertyPath.LastIndexOf('.');
           if (index == -1)
               target = property.serializedObject.targetObject;
           else
           {
               var path = property.propertyPath.Substring(0, index);
               //Debug.Log($"ShowIfCustomAttributeDrawer SetUp - {property.propertyPath} - searching for parent at the path {path}");
               target = GetTargetObjectOfProperty(property.serializedObject.FindProperty(path));
           }
           if (target == null)
           {
               //Debug.LogError($"ShowIfCustomAttributeDrawer SetUp - {property.propertyPath} has NULL target!");
                setup = true;
               return;
           }
           Type type = target.GetType();
           ConditionProperty = type.GetProperty(conditionSource, typeof(bool));
           if (ConditionProperty == null)
               ConditionProperty = type
                   .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                   .Where(f => f.FieldType == typeof(bool) && f.Name == conditionSource)
                   .FirstOrDefault();
           if (ConditionProperty == null)
               ConditionProperty = type
                   .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                   .Where(m => m.ReturnType == typeof(bool) && m.Name == conditionSource)
                   .FirstOrDefault();

           if (ConditionProperty == null)
           {
               //Debug.LogError($"ShowIfCustomAttributeDrawer SetUp - {property.propertyPath} on target {target} has NULL ConditionProperty!");
                setup = true;
               return;
            }

            VisibilityCheck(property);
        }
        void VisibilityCheck(SerializedProperty property)
        {
            if (target == null || ConditionProperty == null)
            {
                //Debug.LogError($"ShowIfCustomAttributeDrawer VisibilityCheck - {property.propertyPath} has NULL target or condition property!");
                return;
            }
            string conditionSource = (attribute as ConditionalAttribute)?.ConditionSource;

            if (attribute is HideAttribute || conditionSource.IsNullOrEmpty())
            {
                Visible = false;
                return;
            }

            if (ConditionProperty as PropertyInfo != null)
                Visible = (bool)(ConditionProperty as PropertyInfo).GetValue(target, null) == (attribute as ConditionalAttribute).ValueForVisible;
            else if (ConditionProperty as FieldInfo != null)
                Visible = (bool)(ConditionProperty as FieldInfo).GetValue(target) == (attribute as ConditionalAttribute).ValueForVisible;
            else if (ConditionProperty as MethodInfo != null)
                Visible = (bool)(ConditionProperty as FieldInfo).GetValue(target) == (attribute as ConditionalAttribute).ValueForVisible;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Debug.Log($"ShowIfCustomAttributeDrawer OnGUI - property {property.displayName} at path {property.propertyPath}. " +
            //    $"Attribute is {attribute.GetType()}");
            if (!setup)
                SetUp(property);


            VisibilityCheck(property);

            if (!Visible)
                return;
            if (typeof(LocalizedString).IsAssignableFrom(fieldInfo.FieldType))
            {
                position.height = EditorGUIUtility.singleLineHeight;
                if (cachedLocStringDrawer == null)
                    cachedLocStringDrawer = new LocalizedStringPropertyDrawer();
                cachedLocStringDrawer.OnGUI(position, property, label);
            }
            else if (typeof(BlueprintReferenceBase).IsAssignableFrom(fieldInfo.FieldType))
            {
                position.height = EditorGUIUtility.singleLineHeight;
                if (cachedBpRefDrawer == null)
                    cachedBpRefDrawer = new BlueprintReferenceDrawer();
                cachedBpRefDrawer.OnGUI(position, property, label);
            }
            else
                EditorGUI.PropertyField(position, property, label, property.isExpanded);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Debug.Log($"ShowIfCustomAttributeDrawer GetPropertyHeight. Property is {property.displayName}. " +
            //    $"Set Up? {setup}.");
            if (!setup)
                SetUp(property);

            //Debug.Log($"ShowIfCustomAttributeDrawer GetPropertyHeight. Property is {property.displayName}. " +
            //    $"Visible? {Visible}. Has Visible children? {property.hasVisibleChildren}. Is Expanded? {expanded}." +
            //    $"Sum is {sum}");
            if (!Visible)
                return 0;

            float sum;
            if (typeof(LocalizedString).IsAssignableFrom(fieldInfo.FieldType))
            {
                if (cachedLocStringDrawer == null)
                    cachedLocStringDrawer = new LocalizedStringPropertyDrawer();
                sum = cachedLocStringDrawer.GetPropertyHeight(property, label);
            }
            else
            if (typeof(BlueprintReferenceBase).IsAssignableFrom(fieldInfo.FieldType))
            {
                if (cachedBpRefDrawer == null)
                    cachedBpRefDrawer = new BlueprintReferenceDrawer();
                sum = cachedBpRefDrawer.GetPropertyHeight(property, label);
            }
            else
                sum = EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
            return sum;

            //if (!property.hasVisibleChildren || !property.isExpanded)
            //    return sum;




            //if (typeof(BlueprintReferenceBase).IsAssignableFrom(fieldInfo.FieldType))
            //{
            //    if (cachedBpRefDrawer == null)
            //        cachedBpRefDrawer = new BlueprintReferenceDrawer();
            //    return cachedBpRefDrawer.GetPropertyHeight(property, label);
            //}
            //    string text = label.text;
            //string tooltip = label.tooltip;
            //var child = property.Copy();
            //var end = child.GetEndProperty();
            //child.NextVisible(true);
            //bool flag1 = true;
            //bool flag2 = true;
            //do
            //{
            //    sum += EditorGUI.GetPropertyHeight(child);
            //    flag1 = child.NextVisible(false);
            //    flag2 = !SerializedProperty.EqualContents(child, end);
            //}           
            //while (flag1  && flag2) ;
            //label.text = text;
            //label.tooltip = tooltip;
            //return sum +3f;
        }
    }
}
