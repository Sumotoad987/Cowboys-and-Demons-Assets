using Ex.Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static com.spacepuppyeditor.EditorHelpers;
using MyOwlcatModification.Editor.Utility;
using Kingmaker.Utility;
using Kingmaker.Blueprints.JsonSystem;
using HarmonyLib;
using System.Reflection;
using com.spacepuppyeditor;
using Diag = System.Diagnostics;

namespace MyOwlcatModification
{
    [CustomPropertyDrawer(typeof(BlueprintComponent), true)]
    public class BlueprintComponentPropertyDrawer : PropertyDrawer
    {


        //static MethodInfo TitleBarMethod =
        //    typeof(EditorGUI)
        //    .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic)
        //    .FindOrDefault(m => m.Name == "GetInspectorTitleBarObjectFoldoutRenderRect" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(UnityEngine.Rect));
        static GUIStyle TitleBarStyle = "Titlebar Foldout";
        //typeof(EditorStyles)
        //.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic)
        //.FindOrDefault(m => m.Name == "titlebarFoldout" && m.PropertyType == typeof(GUIStyle))
        //?.GetValue(null) as GUIStyle;

        static GUIStyle BoxStyle = "FrameBox";
            //AccessTools
            //.Property(typeof(EditorStyles), "frameBox")
            //.GetValue(null) as GUIStyle;


        static GUIContent Assign = new GUIContent() { text = "Try Assign" };
        static GUIContent IDContent = new GUIContent("TypeID");
        BlueprintComponent instance;
        Type type;
        Guid TypeId;
        string buffer = "";
        float cachedHeight;
        float[] cachedPropertiesHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Launcher.Launched)
                return;
            if (instance == null)
            {
                instance = GetTargetObjectOfProperty(property) as BlueprintComponent;
                if (instance != null)
                {
                    var t = instance.GetType();
                    if ((Json.Serializer.Binder as GuidClassBinder).m_TypeToGuidCache.TryGetValue(t, out var s))
                    {
                        type = t;
                        if (Guid.TryParse(s, out var guid))
                            TypeId = guid;
                        buffer = s;
                    }
                }
                if (instance == null)
                    return;
            }
            //var stopwatch = new Diag.Stopwatch();
            //stopwatch.Start();
            Rect foldoutRect = new Rect(position.xMin + 3f, position.yMin + 2f, position.width - EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
            Rect flagsPosition = new Rect(foldoutRect.xMin + EditorGUIUtility.singleLineHeight, foldoutRect.yMin + 2, EditorGUIUtility.singleLineHeight - 4f, EditorGUIUtility.singleLineHeight- 4);
            Rect boxRect = new Rect(foldoutRect.xMin - EditorGUIUtility.singleLineHeight, foldoutRect.yMin, foldoutRect.width + EditorGUIUtility.singleLineHeight -2f, foldoutRect.height);
            
            if (Event.current.type != EventType.Layout)
            {
                GUI.Box(boxRect, new GUIContent(), BoxStyle);
                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUI.Toggle(flagsPosition, !instance.Disabled);
                position.xMin -= 3f;
                if (
                    EditorGUI.EndChangeCheck() &&
                    enabled == instance.Disabled)
                {
                    Undo.RecordObject(property.serializedObject.targetObject, $"{(enabled ? "Enabled" : "Disabled")} {instance.GetType().Name} on {instance.OwnerBlueprint?.name ?? "NULL"}");
                    instance.Disabled = !enabled;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.BeginChangeCheck();
            bool tempExp = EditorGUI.Foldout(foldoutRect, property.isExpanded, "          " + instance?.name ?? label.text, TitleBarStyle);
            if (EditorGUI.EndChangeCheck())
                property.isExpanded = tempExp;
            if (!property.isExpanded)
                return;


            //position.x += EditorGUIUtility.singleLineHeight;
            var AssignWidth = EditorStyles.toolbarButton.CalcSize(Assign).x;
            var newPosition = new Rect(position.xMin, position.yMin + EditorGUIUtility.singleLineHeight + 5f, position.width - 1, EditorGUIUtility.singleLineHeight);
            if (buffer.IsNullOrEmpty())
                buffer = TypeId.ToString();
            Rect typeRect = new Rect(newPosition.xMin, newPosition.yMin, newPosition.width - AssignWidth, newPosition.height);
            buffer = EditorGUI.TextField(typeRect, IDContent, buffer);
            EditorGUI.BeginChangeCheck();
            bool tempButt = GUI.Button(new Rect(typeRect.xMax, newPosition.yMin, AssignWidth, EditorGUIUtility.singleLineHeight), Assign);
            if (EditorGUI.EndChangeCheck() && tempButt)
            {
                Debug.Log("BlueprintComponentPropertyDrawer - button");
                if (Guid.TryParse(buffer, out Guid g))
                {
                    Debug.Log("BlueprintComponentPropertyDrawer - parsing succeeded");
                    if((Json.Serializer.Binder as GuidClassBinder).m_GuidToTypeCache.TryGetValue(g.ToString("N"), out Type t))
                    {
                        Debug.Log($"BlueprintComponentPropertyDrawer - found type {t.Name}");
                        var a = Activator.CreateInstance(t);
                        if (property.propertyType == SerializedPropertyType.ManagedReference)
                        {
                            Debug.Log($"BlueprintComponentPropertyDrawer - managed reference");
                            Undo.RecordObject(property.serializedObject.targetObject, "Changed component type");
                            property.managedReferenceValue = a;
                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();

                        }
                        else
                        {
                            Debug.Log($"BlueprintComponentPropertyDrawer - property type is {property.propertyType}");
                        }
                    }
                }
                else
                {
                    Debug.Log("BlueprintComponentPropertyDrawer - parsing failed");

                }
            }
            else
            {
                //Debug.Log("BlueprintComponentPropertyDrawer - NOT button");

            }
            newPosition.y += EditorGUIUtility.singleLineHeight;

            if (cachedPropertiesHeight == null)
                this.GetPropertyHeight(property.Copy(), label);
            var child = property.Copy();
            var noMore = !child.Next(false);
            property = property.FindPropertyRelative("m_PrototypeLink.ComponentName");
            int i = 0;
            while (property.Next(false) && (noMore || !SerializedProperty.EqualContents(child, property)))
            {
                EditorGUI.PropertyField(newPosition, property, true);
                newPosition.y += cachedPropertiesHeight[i];
                i++;
                    //EditorGUI.GetPropertyHeight(property);


                //if (type.Name.StartsWith("Action"))
                //    Debug.Log($"action list property {property.name} - elapsed {stopwatch.Elapsed}");
            };
            //var beforeApply = stopwatch.Elapsed;
            if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
            {
                var debug = property.serializedObject.ApplyModifiedProperties();
            }
            //stopwatch.Stop();
            //Debug.Log($"Event type {Event.current.type}, property {label.text}. Elapsed total {stopwatch.Elapsed}. Before Apply {beforeApply}");
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float sum = EditorGUIUtility.singleLineHeight + 3f;
            if ( !property.isExpanded)
                return sum;
            //var stopwatch = new Diag.Stopwatch();
            //    stopwatch.Start();
            if (Event.current.type != EventType.Layout && cachedPropertiesHeight != null)
            {
             //   stopwatch.Stop();
                //Debug.Log($"Event type {Event.current.type}, property {label.text}. Elapsed {stopwatch.Elapsed}");
                return cachedHeight;

            }
            sum *= 2f;

            var child = property.Copy();
            var noMore = !child.Next(false);
            property = property.FindPropertyRelative("m_PrototypeLink.ComponentName");
            if (cachedPropertiesHeight == null)
            {
                var temp = property.Copy();
                int tempI = 0;
                while (temp.Next(false) && (noMore || !SerializedProperty.EqualContents(child, temp)))
                {
                    tempI++;
                };
                if (tempI != 0)
                    cachedPropertiesHeight = new float[tempI];
            }
            int i = 0;
            while (property.Next(false) && (noMore || !SerializedProperty.EqualContents(child, property)))
            {
                cachedPropertiesHeight[i] = EditorGUI.GetPropertyHeight(property, property.isExpanded);
                sum += (cachedPropertiesHeight[i]);
                i++;
            };
            //stopwatch.Stop();
            //Debug.Log($"Event type {Event.current.type}, property {label.text}. Elapsed {stopwatch.Elapsed}");
            cachedHeight= sum;
            return sum;
        }
    }
}
