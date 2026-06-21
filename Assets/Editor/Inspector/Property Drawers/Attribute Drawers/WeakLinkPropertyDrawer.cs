using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUI;
using static UnityEngine.GUI;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints;
using static com.spacepuppyeditor.EditorHelpers;
using System.Linq;
using System;
using Kingmaker.Utility;
using Kingmaker.BundlesLoading;
using Pathfinding.Util;
using UnityEditor.VersionControl;
using System.Reflection;

namespace MyOwlcatModification
{

    [CustomPropertyDrawer(typeof(WeakResourceLink<>), true)]
    public class WeakLinkPropertyDrawer : PropertyDrawer
    {

        WeakResourceLink _cachedLink;
        Type _cachedType;
        UnityEngine.Object _cachedObject;


        float cachedViewWidth = float.NegativeInfinity;
        float cachedTextHeight = EditorGUIUtility.singleLineHeight;

        static readonly GUIContent _EmptyContent = new GUIContent(" ");
        static readonly GUIContent ContentFind = new GUIContent("Find");
        static readonly GUIContent ContentForce = new GUIContent("Force");

        string InputField = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            position.height = cachedTextHeight;
            var assetIdProperty = property.FindPropertyRelative(nameof(WeakResourceLink.AssetId));
            var _label = EditorGUI.BeginProperty(new Rect (position.x, position.y, position.width, property.isExpanded ? cachedTextHeight + (EditorGUIUtility.singleLineHeight + 3) * 2 + 3 : cachedTextHeight), label, property);
            var tryObject = ObjectField(position, _EmptyContent, _cachedObject, _cachedType, false);


            if (tryObject != null && tryObject != _cachedObject)
            {
                string tryAssetPath = AssetDatabase.GetAssetPath(tryObject);
                if (!tryAssetPath.IsNullOrEmpty())
                {
                    //assetIdProperty.stringValue = AssetDatabase.AssetPathToGUID(tryAssetPath);
                    SetUpCache(property, AssetDatabase.AssetPathToGUID(tryAssetPath));
                }
            }
            if (!(property.isExpanded = Foldout(position, property.isExpanded, _label)))
                return;

            position.y += position.height + 3;
            position.height = EditorGUIUtility.singleLineHeight;

            if (InputField is null)
                InputField = assetIdProperty.stringValue;
            string controlName = property.serializedObject.targetObject.name + property.displayName;
            GUI.SetNextControlName(controlName);
            EditorGUI.indentLevel++;
            var m_InputField = TextField(position, nameof(WeakResourceLink.AssetId), InputField);
            EditorGUI.indentLevel--;
            InputField = m_InputField;
            position.y += EditorGUIUtility.singleLineHeight + 3;

            float labelPlace = EditorGUIUtility.labelWidth + (EditorGUI.indentLevel) * 15f + 2;
            float width = EditorGUIUtility.currentViewWidth - labelPlace;

            if (width > 100f)
            {
                position.x += labelPlace;
                float widthEnd = position.x + position.width;
                position.width = GUI.skin.textField.CalcSize(ContentFind).x + 8;
                DoButtonFind(position, property);

                    position.x += position.width + 10f;
                position.width = GUI.skin.textField.CalcSize(ContentForce).x + 8;
                //position.x = widthEnd - position.width;
                DoButtonForce(position, property);
            }
            else
            {
                position.x += labelPlace;
                float widthEnd = position.x + position.width;
                position.width = GUI.skin.textField.CalcSize(ContentFind).x + 8;
                //GUI.SetNextControlName(property.serializedObject.targetObject.name + property.name);
                DoButtonFind(position, property);
                position.y += position.height + 3;

                position.width = GUI.skin.textField.CalcSize(ContentForce).x + 8;
                DoButtonForce(position, property);
            }
            if (Event.current.type == EventType.MouseUp && GUI.GetNameOfFocusedControl() != controlName)
                InputField = assetIdProperty.stringValue;
            EditorGUI.EndProperty();
        }



        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Layout)
            {
                SetUpCache(property);
                if (EditorGUIUtility.currentViewWidth != cachedViewWidth)
                {
                    cachedViewWidth = EditorGUIUtility.currentViewWidth;
                    //cachedTextHeight = EditorStyles.textArea.CalcHeight(_CachedContent, cachedViewWidth - EditorGUIUtility.labelWidth - (EditorGUI.indentLevel + 1) * 15f) + 4;

                }
            }
            if (!property.isExpanded)
                return cachedTextHeight;
            else
                return cachedTextHeight + (EditorGUIUtility.singleLineHeight + 3f) * 2f + 3f;
        }

        void SetUpCache(SerializedProperty property, string ForceRenewForId = null)
        {
            if (!Launcher.Launched)
                return;
;
            var assetIdProperty = property.FindPropertyRelative(nameof(WeakResourceLink.AssetId));
            //var a = _cachedLink;
            var c = GetTargetObjectOfProperty(property) as WeakResourceLink; 
            var b = TypeCache.GetTypesDerivedFrom(typeof(WeakResourceLink<>)).FirstOrDefault(t => t.Name == property.type);
            var t = b?.BaseType.GetGenericArguments()[0];

            bool NeedReloadObject = false;

            if (ForceRenewForId != null)
            {
                c.AssetId = ForceRenewForId;
                var handle = (b.GetProperty("m_Handle", BindingFlags.Instance | BindingFlags.Public).GetMethod.Invoke(c, null) as IDisposable);
                if (handle != null)
                    handle.Dispose();
                assetIdProperty.stringValue = _cachedLink.AssetId = ForceRenewForId;
                NeedReloadObject = true;
                //InputField = assetIdProperty.stringValue;
            }

            else if (_cachedLink == null || assetIdProperty.stringValue != _cachedLink.AssetId || _cachedType != t)
            {
                _cachedLink = GetTargetObjectOfProperty(property) as WeakResourceLink;
                _cachedType = t;
                cachedTextHeight = t != typeof(Sprite) ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 5;
                InputField = assetIdProperty.stringValue;
                NeedReloadObject = true;
            }
            if (assetIdProperty.stringValue.IsNullOrEmpty())
            {
                _cachedObject = null;
            }
            else if (NeedReloadObject)
            {
                var o = _cachedLink.LoadObject();
                if (o == null && (GUID.TryParse(_cachedLink.AssetId, out var guid)))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.IsNullOrEmpty())
                        o = AssetDatabase.LoadAssetAtPath(path, t);
                }
                _cachedObject = o;
            }

            
        }


        void DoButtonFind(Rect position, SerializedProperty propertyLink)
        {
            if (Button(position, ContentFind))
            {
                if (!InputField.IsNullOrEmpty() && ((GUID.TryParse(InputField, out var guid) && !AssetDatabase.GUIDToAssetPath(guid).IsNullOrEmpty())
                    || (!BundlesLoadService.Instance?.GetBundleNameForAsset(InputField).IsNullOrEmpty() ?? false)))
                {
                    SetUpCache(propertyLink, InputField);
                    EditorUtility.SetDirty(propertyLink.serializedObject.targetObject);
                    propertyLink.serializedObject.Update();
                }
            }
        }

        void DoButtonForce(Rect position, SerializedProperty propertyLink)
        {
            if (Button(position, ContentForce))
            {
                //propertyID.stringValue = InputField;
                SetUpCache(propertyLink, InputField);
                EditorUtility.SetDirty(propertyLink.serializedObject.targetObject);
                propertyLink.serializedObject.Update();
            }
        }

    }
}
