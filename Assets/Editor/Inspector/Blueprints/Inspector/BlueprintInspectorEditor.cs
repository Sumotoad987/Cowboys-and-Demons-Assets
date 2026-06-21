using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using static UnityEngine.GUILayout;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(SimpleBlueprint), true)]
    public class BlueprintInspectorEditor : UnityEditor.Editor
    {
        static GUIContent AddComponentContent = new GUIContent("Add Component");

        BlueprintComponentEditorWrapper[] componentWrappers;
        bool[] componentFoldout;
        UnityEditor.Editor[] componentEditors;
        bool? fromDatabase = null;

        Dictionary<string, BlueprintComponentPropertyDrawer> componentDrawers = new Dictionary<string, BlueprintComponentPropertyDrawer>();
        public override void OnInspectorGUI()
        {
            //Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - start blueprint {target.name}. Event Type {Event.current.type}");
            var stopwatach = new System.Diagnostics.Stopwatch();
            stopwatach.Start();
            GUI.enabled = true;
            if (!fromDatabase.HasValue)
                fromDatabase = AssetDatabase.Contains(target);

            EditorGUI.BeginChangeCheck();
            BeginHorizontal();
            FlexibleSpace();
            if (fromDatabase.Value)
                Label($"Blueprint is imported!");
            else
            if (Button("Import!"))
            {
                var another = Import(target as SimpleBlueprint, true);
                if (another != null)
                {
                    //Debug.Log("Setting imported blueprint as current selection");
                    Selection.activeObject = another;
                    return;
                }
            }
            var imported = EditorGUI.EndChangeCheck();
            serializedObject.Update();
            FlexibleSpace();
            EndHorizontal();
            var beforeDefault = stopwatach.Elapsed;
            DrawDefaultInspector();
            var afterDefault = stopwatach.Elapsed;
            if (imported && !fromDatabase.Value)
            {
                serializedObject.UpdateIfRequiredOrScript();
                string path = AssetDatabase.GetAssetPath(target);
                //Debug.Log($"OnInspectorGUI - has been changed. Asset is at path {path}");
                var wrapper = (AssetImporter.GetAtPath(path) as BlueprintImporter).wrapper;
                var id = wrapper.AssetId;
                wrapper.Data.Become(target as SimpleBlueprint);
                wrapper.Data.AssetGuid = Kingmaker.Blueprints.BlueprintGuid.Parse(id);
                wrapper.Save(path);
                // Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - Forced blueprint wrapper to serialize the blueprint into jbp.");
            }

            //===========================
            /*
            EditorGUILayout.Space(5);
            if (componentWrappers == null && flag)
            {
                componentWrappers = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(target)).OfType<BlueprintComponentEditorWrapper>().ToArray();
                componentFoldout = new bool[componentWrappers.Length];
                componentEditors = componentWrappers.Select(wrapper => UnityEditor.Editor.CreateEditor(wrapper, typeof(BlueprintComponentEditorWrapperInspectorEditor))).ToArray();
            }

            var components = serializedObject.FindProperty("Components");
            for (int i = 0; i < componentFoldout.Length; i++)
            {
                componentFoldout[i] = EditorGUILayout.InspectorTitlebar(componentFoldout[i], componentEditors[i]);
                Rect last = GUILayoutUtility.GetLastRect();
                last.xMin += EditorGUIUtility.singleLineHeight * 2 + 5f;
                var component = componentWrappers[i].Component;
                component.Disabled = EditorGUI.Toggle(last, component.Disabled);
                if (componentFoldout[i])
                    componentEditors[i].OnInspectorGUI();
            }
            */


            //================================
            ///*
            var beforeComponents = stopwatach.Elapsed;
            var components = serializedObject.FindProperty("Components");
            var componentsDebug = new (string componentName, string duration)[components.arraySize];
            TimeSpan beforeApply;
            if (components == null)
            {
                //Debug.Log("BlueprintInspectorEditor - no components");
                goto AfterComponents;
            }
            if (components.arraySize == 0)
            {
                goto AfterComponents;
            }
            for (int i = 0; i < components.arraySize; i++)
            {
                var componentProperty = components.GetArrayElementAtIndex(i);
                //Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - drawing component {i}. Its type is {components.GetArrayElementAtIndex(i).managedReferenceFullTypename}");
                var r = EditorGUILayout.GetControlRect(false, 0f);
                var deleteButtonRect = new Rect(r.xMax - EditorGUIUtility.singleLineHeight, r.yMin + 4, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                bool deleteComponent = GUI.Button(deleteButtonRect, "X");
                if (EditorGUI.EndChangeCheck() && deleteComponent)
                {
                    Undo.RecordObject(componentProperty.serializedObject.targetObject, $"Deleted {componentProperty.GetType()} on {target.name ?? "NULL"}");
                    components.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                return;
                }

                else
                {
                    //if (!componentDrawers.TryGetValue(componentProperty.propertyPath, out var drawer))
                    //{
                    //    drawer = new BlueprintComponentPropertyDrawer();
                    //    componentDrawers[componentProperty.propertyPath] = drawer;
                    //}
                    //    var componentHeight = drawer.GetPropertyHeight(componentProperty, GUIContent.none);
                    //    var componentRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, componentHeight);
                    //    drawer.OnGUI(componentRect, componentProperty, GUIContent.none);
                        EditorGUILayout.PropertyField(componentProperty, false);
                    componentsDebug[i] = ("component " + i, stopwatach.Elapsed.ToString());
                }
            //Debug.Log($"EventType: {Event.current.type}. Before default: {beforeDefault}. After default: {afterDefault}. Before components: {beforeComponents}. " +
            //    $"{string.Join(". ", componentsDebug.Select(e => e.componentName + " " + e.duration))}. BeforeApply: {beforeApply}. End: {stopwatach.Elapsed}.");
        }
            AfterComponents:;

            EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
            var butRect = GUILayoutUtility.GetRect(AddComponentContent, "AC Button"); 
            bool but = GUI.Button(butRect, AddComponentContent);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            bool butPressed = EditorGUI.EndChangeCheck();
            if (but && butPressed)
            {
                var AddComponentDropdownRect = new Rect(butRect);
                AddBlueprintComponentAdvancedDropdownWindow.Show(AddComponentDropdownRect, target as BlueprintScriptableObject);
                /*
                    var bp = (target as BlueprintScriptableObject);
                    var newComp = new Kingmaker.Blueprints.BlueprintComponent() { name = $"BlueprintComponent${bp.AssetGuid}", OwnerBlueprint = bp };
                    Undo.RecordObject(target, $"New {newComp.GetType()} on {(target as SimpleBlueprint).name}");
                    int i = components.arraySize;
                    components.InsertArrayElementAtIndex(i);
                    components.GetArrayElementAtIndex(i).managedReferenceValue = newComp;
                */

                //bp.ComponentsArray = bp.ComponentsArray.Append(newComp).ToArray();

            //Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - Blueprint has been set dirty and saved.");
            }
            beforeApply = stopwatach.Elapsed;

            if (serializedObject.ApplyModifiedProperties())
            {
                Debug.Log("Property changed");
            };
            serializedObject.Update();
                stopwatach.Stop();

            if (GUI.changed && AssetDatabase.Contains(target))
            {
                string path = AssetDatabase.GetAssetPath(target);
                //Debug.Log($"OnInspectorGUI - has been changed. Asset is at path {path}");
                var importer = (AssetImporter.GetAtPath(path) as BlueprintImporter);
                importer.Save(target as SimpleBlueprint);


                   // Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - Forced blueprint wrapper to serialize the blueprint into jbp.");
            }




            //Debug.Log($"BlueprintInspectorEditor OnInspectorGUI - End painting.");
            //*/
            //serializedObject.Update();
            //if (serializedObject.ApplyModifiedProperties())
            //{
            //    Debug.Log($"ModifiedProperties");
            //};
            //serializedObject.Update();
            //serializedObject.ApplyModifiedProperties();
        }



        static readonly string directory = Path.Combine("Assets", "Blueprints Library");
        static internal SimpleBlueprint Import(SimpleBlueprint bp, bool doReturn = false)
        {
            try
            {
                if (bp == null)
                    throw new System.ArgumentNullException("Trying to import a blueprint, but it was type cast to null!");

                if (!AssetDatabase.IsValidFolder(directory))
                    AssetDatabase.CreateFolder("", directory);
                //Debug.Log("Import - 1");
                string name = $"{bp.name}_{bp.AssetGuid}_{bp.GetType().Name}.jbp";
                //Debug.Log($"Import - path is {name}");
                string path = Path.Combine(directory, name);
                new BlueprintJsonWrapper(bp).Save(path);
                //AssetDatabase.CreateAsset(bp, Path.Combine(directory, name));
                AssetDatabase.Refresh();
                return doReturn ? AssetDatabase.LoadAssetAtPath<SimpleBlueprint>(path) : null;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

    }
}
