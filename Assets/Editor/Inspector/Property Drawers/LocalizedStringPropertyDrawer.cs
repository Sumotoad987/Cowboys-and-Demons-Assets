using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUI;
using static com.spacepuppyeditor.EditorHelpers;
using Ex.Kingmaker.Localization;
using Loc = Kingmaker.Localization;
using System.Text;
using System;
using Kingmaker.Utility;
using UnityEngine.UIElements;
using System.IO;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using Kingmaker.Blueprints.JsonSystem;
using Ex.Kingmaker.DialogSystem.Blueprints;
using System.IO.Pipes;
using HarmonyLib;

namespace MyOwlcatModification
{
    [CustomPropertyDrawer(typeof(LocalizedString), true)]
    public class LocalizedStringPropertyDrawer : PropertyDrawer
    {
        static FieldInfo m_OwnerString = AccessTools.DeclaredField(typeof(LocalizedString), "m_OwnerString");
        static FieldInfo m_OwnerPropertyPath = AccessTools.DeclaredField(typeof(LocalizedString), "m_OwnerPropertyPath");
        static FieldInfo m_JsonPath = AccessTools.DeclaredField(typeof(LocalizedString), "m_JsonPath");
        public static FieldInfo[] fields = new FieldInfo[3] { m_OwnerString, m_OwnerPropertyPath, m_JsonPath };


        static Locale LastLocale;
        static readonly Texture2D BackgroundNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor Default Resources/Icons/NewButton/BackgroundNormal.png");
        static readonly Texture2D BackgroundHover = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor Default Resources/Icons/NewButton/BackgroundHover.png");

        static readonly GUIContent _EmptyContent = new GUIContent(" ");
        static readonly GUIContent _PlusContent = new GUIContent("+");
        static readonly GUIContent _MinusContent = new GUIContent("-");
        static readonly GUIContent _LabelKeyContent = new GUIContent("Key");
        static readonly GUIContent _KeyContent = new GUIContent("");
        static readonly GUIStyle styleRed = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        static readonly GUIStyle ObjectFieldStyle = "ObjectField";
        static readonly GUIStyle FileButtonStyle = new GUIStyle((GUIStyle)"ObjectFieldButton")
        {
            name = "PlusMinusStyleForLocalizedStrings",
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            contentOffset = new Vector2(0, -2),
            normal = new GUIStyleState()
            {
                background = BackgroundNormal,
                textColor = new Color(0.823f, 0.823f, 0.823f, 1)
            },

            hover = new GUIStyleState()
            {
                background = BackgroundHover,
                textColor = new Color(0.9f, 0.9f, 0.9f, 1)
            },
            stretchWidth = false
        };

        string _Cached;
        LocalizedString localizedString;
        GUIContent _CachedContent = new GUIContent("");

        float cachedViewWidth = float.NegativeInfinity;
        float cachedTextHeight;
        string controlname;
        DateTime lastFileUpdate = DateTime.MinValue;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //BeginProperty(position, _EmptyContent, property);
            //StringBuilder sb2 = new StringBuilder();
            try
            {
                //sb2.Append($"Launcher is launched? {Launcher.Launched}. ");
                //sb2.Append($"Localization manager initialized? {Loc.LocalizationManager.Initialized}. ");
                //sb2.Append($"Current pack is null? {Loc.LocalizationManager.CurrentPack is null}. ");
                //sb2.Append($"Its file is null? {Loc.LocalizationManager.CurrentPack?.m_PackFile is null}. ");
                //sb2.Append($"Contains {Loc.LocalizationManager.CurrentPack?.m_Strings.Count.ToString() ?? "null"} strings. ");
                //sb2.Append($"Current locale is {Loc.LocalizationManager.CurrentLocale}.  Different from Sound? {Loc.LocalizationManager.CurrentLocale != Loc.Shared.Locale.Sound}. ");
                //sb2.Append($"Using json localization? {Kingmaker.Utility.BuildModeUtility.Data.UsePackedLocalization == false}. ");
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                //Debug.Log(sb2);
            }

            if (!Launcher.Launched 
                || !Loc.LocalizationManager.Initialized
                || Loc.LocalizationManager.CurrentPack is null 
                || Loc.LocalizationManager.CurrentLocale == Locale.Sound)
            {
                property.isExpanded = false;
                LabelField(position, property.displayName, "Failed to load the localization pack", styleRed);
                return;
            }
            SetUpCache(property);
            //Debug.Log($"Cached localized string is {_Cached}");
            position.y += 2;
            EditorStyles.textField.wordWrap = true;
            bool oldStyleWrap = EditorStyles.textField.wordWrap;
            Rect TextPosition = position;
            position.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded = Foldout(position, property.isExpanded, property.displayName))
            {
                EditorGUI.indentLevel++;
                position.y += cachedTextHeight + 2 ;
                //PropertyField(position, property.FindPropertyRelative(nameof(LocalizedString.m_Key)));
                var lab = BeginProperty(position, _LabelKeyContent, property);
                LabelField(position, lab, _KeyContent, ObjectFieldStyle);
                bool hasKey = localizedString.m_Key.IsNullOrEmpty();
                BeginChangeCheck();
                if ( GUI.Button(
                    new Rect(position.x + position.width - EditorGUIUtility.singleLineHeight - 1, position.y +1, 
                    EditorGUIUtility.singleLineHeight, position.height -2),
                    hasKey ? _PlusContent : _MinusContent,
                    FileButtonStyle)
                    && EndChangeCheck())
                {
                    ChangeKey(property, hasKey);
                }
                EndProperty();
                position.y += position.height + 3;
                //PropertyField(position, property.FindPropertyRelative(nameof(LocalizedString.ShouldProcess)));
                //position.y += position.height + 3;
                PropertyField(position, property.FindPropertyRelative(nameof(LocalizedString.Shared)));
                EditorGUI.indentLevel--;
            };
            TextPosition.x = TextPosition.x + 2 + EditorGUIUtility.labelWidth;
            TextPosition.width = TextPosition.width -2 - EditorGUIUtility.labelWidth;
            TextPosition.height = cachedTextHeight;
            bool HasFile = localizedString.m_JsonPath != null;

                BeginDisabledGroup(!HasFile);
            var text = DelayedTextField(TextPosition, _Cached);
            if (text != _Cached && HasFile && localizedString.Data != null)
            {
                localizedString.Data.UpdateText(LastLocale, text);
                _Cached= text;
                _CachedContent.text = text;
                try
                {
                    using FileStream fileStream = File.Open(localizedString.m_JsonPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    using StreamWriter streamWriter = new StreamWriter(fileStream);
                    using JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter);
                    Json.Serializer.Serialize(jsonTextWriter, localizedString.Data);
                    SetUpCache(property);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }
            if (!HasFile)
            EndDisabledGroup();
            EditorStyles.textField.wordWrap = oldStyleWrap;

            //EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Layout)
                SetUpCache(property);
            if (!property.isExpanded)
                return cachedTextHeight + 3;
            else
                return cachedTextHeight + 2 * (EditorGUIUtility.singleLineHeight + 3) + 3;
        }

        void SetUpCache(SerializedProperty property, bool force = false)
        {
            if (!Launcher.Launched)
                return;

            DateTime? time = null;
            bool fileWasChanged = (!localizedString?.m_JsonPath.IsNullOrEmpty() ?? false && (time = File.GetLastWriteTime(localizedString.m_JsonPath)) > lastFileUpdate);
            bool needToRefresh = 
                force 
                || localizedString?.Key != property.FindPropertyRelative(nameof(LocalizedString.m_Key)).stringValue 
                || Loc.LocalizationManager.CurrentLocale != LastLocale
                || fileWasChanged;

            if (needToRefresh)
            {

                LastLocale = Loc.LocalizationManager.CurrentLocale;
                localizedString = GetTargetObjectOfProperty(property) as LocalizedString;
                if (time.HasValue)
                    lastFileUpdate = time.Value;

                _Cached = localizedString;
                _CachedContent.text = _Cached;
                _KeyContent.text = localizedString.Key;
                controlname = property.propertyPath;
            }
            if (needToRefresh || EditorGUIUtility.currentViewWidth != cachedViewWidth)
            {
                cachedViewWidth = EditorGUIUtility.currentViewWidth;
                cachedTextHeight = EditorStyles.textArea.CalcHeight(_CachedContent, cachedViewWidth - EditorGUIUtility.labelWidth - 7 - (EditorGUI.indentLevel + 2) * 15f) + 4;

            }
        }

        string GetSerializedStringPath(string ObjectPath, SerializedProperty property)
        {
            if (ObjectPath.StartsWith("Assets"))
                ObjectPath = ObjectPath.Insert(6, "/../Strings");
            else
                ObjectPath = Path.Combine("Assets", "..", "Strings", ObjectPath);
                ObjectPath = ObjectPath.Substring(0, ObjectPath.Length - Path.GetExtension(ObjectPath).Length);

            return Path.Combine(ObjectPath, property.propertyPath) + ".LocalizedString.json";
        }

        void ChangeKey(SerializedProperty property, bool hasKey)
        {
            string path = GetSerializedStringPath(AssetDatabase.GetAssetPath(property.serializedObject.targetObject), property);
            var key = property.FindPropertyRelative(nameof(LocalizedString.m_Key));
            var JsonPath = property.FindPropertyRelative(nameof(LocalizedString.m_JsonPath));
            var OwnerPropertyPath = property.FindPropertyRelative(nameof(LocalizedString.m_OwnerPropertyPath));
            var Owner = property.FindPropertyRelative(nameof(LocalizedString.m_OwnerString));
            if (!hasKey)
            {
                //Debug.Log($"Pressed Minus - {path}");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    key.stringValue = string.Empty;
                    JsonPath.stringValue = string.Empty;
                    OwnerPropertyPath.stringValue = string.Empty;
                    localizedString.Data = null;
                }
            }
            else
            {
                //Debug.Log($"Pressed Plus - {path}");
                if (File.Exists(path))
                    File.Delete(path);
                var DirectoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(DirectoryPath))
                    Directory.CreateDirectory(DirectoryPath);
                
                var data = new LocalizedStringData(LastLocale, Guid.NewGuid().ToString());
                data.UpdateText(LastLocale, "CreationPlaceholder");
                if (property.serializedObject.targetObject is SimpleBlueprint bp)
                {
                    data.OwnerGuid = bp.AssetGuid.ToString();
                    Owner.stringValue = bp.AssetGuid.ToString();
                    if (bp is BlueprintCue cue && cue.Speaker?.Blueprint is BlueprintUnit unit)
                    {
                        data.Speaker = unit.CharacterName;
                        data.SpeakerGender = unit.Gender.ToString();
                    }
                }
                using FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter streamWriter = new StreamWriter(fileStream);
                using JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter);
                Json.Serializer.Serialize(jsonTextWriter, data);

                key.stringValue = data.Key;
                JsonPath.stringValue = path;
                OwnerPropertyPath.stringValue = property.propertyPath;

                localizedString.Data = data;
                //localizedString.m_JsonPath = path;
                //localizedString.m_OwnerPropertyPath = property.propertyPath;
            }

        }

        void CreateDirectoryRecursive(string path)
        {
            string ObjectPath = Path.GetDirectoryName(path);
            var folders = ObjectPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var currentFolder = Path.Combine(folders[0], folders[1], folders[3]);
            for (int index = 3; index < folders.Length;)
            {
                var nextFolder = Path.Combine(currentFolder, folders[index]);
                if (!AssetDatabase.IsValidFolder(nextFolder))
                    AssetDatabase.CreateFolder(currentFolder, folders[index]);
                index++;
                currentFolder = nextFolder;
            }
            AssetDatabase.Refresh();

        }
    }
}
