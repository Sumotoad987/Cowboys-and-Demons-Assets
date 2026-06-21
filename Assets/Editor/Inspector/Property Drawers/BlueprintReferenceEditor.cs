using UnityEngine;
using Ex.Kingmaker.Blueprints;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using static com.spacepuppyeditor.EditorHelpers;
using Kingmaker.Utility;
using System.Linq;
using UnityEngine.ProBuilder;

namespace MyOwlcatModification
{
    [CustomPropertyDrawer(typeof(BlueprintReferenceBase), true)]
    public class BlueprintReferenceDrawer : PropertyDrawer
    {
        static GUIStyle styleRed = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        Type blueprintType;
        BlueprintReferenceBase inspectedReference;
        bool searched;

        SimpleBlueprint _Cached;
        string _guid;
        GUIContent cachedContent = new GUIContent(" ");
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            inspectedReference = GetTargetObjectOfProperty(property) as BlueprintReferenceBase;
            if (searched && blueprintType != null) goto CheckCache;
            else if (searched && blueprintType == null)
            {
                EditorGUI.BeginProperty(position, new GUIContent(fieldInfo.Name), property);
                //EditorGUILayout.LabelField("Failed to extract the type from the reference!", styleRed);
                EditorGUI.EndProperty();
                return;
            }
            else try
            {
                //Debug.Log("Searching");
                Type genericType = inspectedReference.GetType();
                Type bpType = null;
                while (genericType != typeof(BlueprintReferenceBase))
                {
                    var generics = genericType.GetGenericArguments();
                    if (generics.Length > 0)
                        if (null != (bpType = generics.FirstOrDefault(g => g.IsOrSubclassOf<SimpleBlueprint>())))
                            break;

                    genericType = genericType.BaseType;
                }
                blueprintType = bpType;
                searched = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            
            ; CheckCache:

            string s = inspectedReference.deserializedGuid.ToString().IsNullOrEmpty() ? inspectedReference.guid : inspectedReference.deserializedGuid.ToString();
            if (s == _guid)
                goto Draw;
            //Debug.Log($"Property is {label.text}, guid is {inspectedReference.guid}, deserialized guid is {inspectedReference.deserializedGuid}");
            cachedContent.text = label.text;
                //property.displayName;
            if (cachedContent.text.IsNullOrEmpty() && fieldInfo != null)
                cachedContent.text = fieldInfo.Name;
            _guid = s;
            var c = inspectedReference.Cached;
            if (c != null)
                _Cached = c;
            else if (s.IsNullOrEmpty())
                _Cached = null;
            
            else if (!Launcher.Launched)
            {
                EditorGUI.BeginProperty(position, cachedContent, property);
                //EditorGUILayout.LabelField("<color = red>Failed to launch the game and retrieve the blueprint!<color>");
                EditorGUI.EndProperty();
                SetValues();
                return;
            }

            else
            {
                var path = AssetDatabase.GUIDToAssetPath(s);
                //Debug.Log($"path for guid {s} is {path}");
                if (!path.IsNullOrEmpty())
                {
                    _Cached = AssetDatabase.LoadAssetAtPath<SimpleBlueprint>(path);
                    //Debug.Log($"Tried to load from Database. Is null? {_Cached == null}");
                }
                if (_Cached == null)
                _Cached = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse(_guid));
            }


            SetValues();

            ; Draw:
            var TheLabel = EditorGUI.BeginProperty(position, cachedContent, property);
            var temp = EditorGUI.ObjectField(new Rect(position.xMin, position.yMin, position.width, 20), TheLabel, _Cached, blueprintType ?? typeof(SimpleBlueprint), true) as SimpleBlueprint;
            //Debug.Log($"BlueprintReferenceDrawer - temp is null? {temp == null}. {temp?.name ?? "null"}. {temp?.AssetGuid.ToString() ?? "null"}.");
            //Debug.Log($"BlueprintReferenceDrawer - event type is {Event.current.type}. Cached is {_Cached?.AssetGuid.ToString() ?? "null"}. Temp is {temp?.AssetGuid.ToString() ?? "null"}");
            EditorGUI.LabelField(new Rect(position.xMin, position.yMin + 20, position.width, 20), "     guid", _guid);
            //EditorGUI.LabelField(new Rect(position.xMin, position.yMin + 40, position.width, 20), "     name", _Cached != null ? _Cached.name : "");
            if (temp != null && temp != _Cached)
            {
                _Cached = temp;
                if (_Cached == null)
                    _guid = string.Empty;
                else
                {
                    _guid = _Cached.AssetGuid.ToString();
                    if (_guid.IsNullOrEmpty())
                        _guid = (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(temp)) as BlueprintImporter)?.wrapper?.AssetId;
                }
                SetValues();
                var newprop = property.FindPropertyRelative("guid");
                //Debug.Log($"BlueprintReferenceDrawer - newprop is null? {newprop == null}. guid is {_guid}");
                if (newprop != null)
                    newprop.stringValue = _guid;
                property.serializedObject.Update();
                //property.serializedObject.ApplyModifiedProperties();
            }
                
            EditorGUI.EndProperty();
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.Repaint && currentEvent.type != EventType.Layout 
                && currentEvent.button == 1 && position.Contains(currentEvent.mousePosition))
            {
                Rect contextRect = GUILayoutUtility.GetLastRect();
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Look for a blueprint in cheat data"), false, () => { });
                //if (inspectedReference?.Cached == null)
                //    contextMenu.AddItem(new GUIContent("Try resolve guid"), false, ResolveGuid);
                contextMenu.ShowAsContext();
                currentEvent.Use();
            }
            //Debug.Log(property.serializedObject.targetObject.name);
        }


        void SetValues()
        {
            if (_Cached == null)
            {
                inspectedReference.Cached = null;
                inspectedReference.deserializedGuid = Kingmaker.Blueprints.BlueprintGuid.Empty;
                inspectedReference.guid = "";
                //cachedContent.text = " ";
            }
            else
            {
                if (AssetDatabase.Contains(_Cached))
                    inspectedReference.Cached = _Cached;
                inspectedReference.deserializedGuid = Kingmaker.Blueprints.BlueprintGuid.Parse(_guid);
                inspectedReference.guid = _guid;
                //cachedContent.text = fieldInfo.Name;
            }
        }

        //void ResolveGuid()
        //{
        //    if (!Launcher.Launched)
        //    {
        //        Debug.LogError("Launcher has failed to launch! Can not resolve blueprint reference");
        //        return;
        //    }
        //    if (inspectedReference is null)
        //    {
        //        Debug.LogError("Reference instance is null! Can not resolve blueprint reference");
        //        return;
        //    }
        //    if (!Guid.TryParse(inspectedReference.guid, out Guid g))
        //    {
        //        Debug.Log("failed to parse guid");
        //        return;
        //    }
        //    var bp = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(new Kingmaker.Blueprints.BlueprintGuid(g));
        //    if (bp == null)
        //    {
        //        Debug.Log("did not find the blueprint");
        //        return;
        //    }
        //    if (blueprintType == null)
        //    {
        //        Debug.LogWarning("We don't know the type of reference");
        //        return;
        //    }
        //    if (!blueprintType.IsAssignableFrom(bp.GetType()))
        //    {
        //        Debug.LogWarning($"Blueprint {bp.name} is not assignable from type {blueprintType.Name}");
        //        return;
        //    }
        //    inspectedReference.Cached = bp;
        //    //inspectedReference.GetBlueprint();
        //}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 +2 ;
        }


    }
}