using System;
using System.Collections.Generic;
using System.IO;
using Kingmaker.Localization;
using Kingmaker.Utility;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Editor
{
    public static class EditorLocalizationManager
    {
		public static bool ShowTextStatus = false;
#if UNITY_EDITOR && EDITOR_FIELDS

        public readonly struct StringData : IEquatable<StringData>
        {
            public readonly LocalizedString String;
            public readonly SerializedProperty Property;
            public readonly string AssetPath;

            public StringData(LocalizedString s, SerializedProperty property, string assetPath)
            {
                String = s;
                Property = property;
                AssetPath = assetPath;
            }

            public bool Equals(StringData other)
                => Equals(String, other.String);

            public override bool Equals(object obj)
                => obj is StringData other && Equals(other);

            public override int GetHashCode()
                => String != null ? String.GetHashCode() : 0;

            public static bool operator ==(StringData a, StringData b)
                => a.Equals(b);

            public static bool operator !=(StringData a, StringData b)
                => !(a == b);
        }

        [JetBrains.Annotations.NotNull]
        public static readonly Dictionary<string, StringData> AllStringsByKey = new();
        
        [JetBrains.Annotations.NotNull]
        public static readonly Dictionary<string, HashSet<StringData>> AllStringsByPath = new();
        
        public static void ClearRegistry()
        {
            AllStringsByPath.Clear();
            AllStringsByKey.Clear();
        }

        private static T DemandStrings<T>(
            string path, Dictionary<string, T> strings) where T: new()
        {
            if (strings.TryGetValue(path, out var data)) 
                return data;
            data = new T();
            strings.Add(path, data);

            return data;
        }

        public static void Register(SerializedProperty property, LocalizedString localizedString, string path)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (localizedString.Shared != null)
            {
                return;
            }

            var newStringData = new StringData(localizedString, property, path);
            
            if (!localizedString.IsTrulyEmpty)
            {
                AllStringsByKey.TryGetValue(localizedString.Key, out var current);

                if (current.String == localizedString)
                {
                    var stringData = new StringData(current.String, property, path);
                    AllStringsByKey[localizedString.Key] = stringData;
                    DemandStrings(path, AllStringsByPath).Remove(stringData);
                    DemandStrings(path, AllStringsByPath).Add(stringData);
                    return;
                }

                if (current.String != null && current.Property.serializedObject.targetObject != null)
                {
                    LocalizedString.Logger.Error(
                        "Two localized strings with same keys (key = {0}, asset1 = {1}[{2}][{3}], asset2 = {4}[{5}][{6}])",
                        localizedString.Key,
                        LocalizedString.CalcOwnerPath(property),
                        property.serializedObject.targetObject.name,
                        localizedString.OwnerPropertyPath,
                        LocalizedString.CalcOwnerPath(current.Property),
                        current.Property.serializedObject.targetObject.name,
                        current.String.OwnerPropertyPath
                    );
                }

                AllStringsByKey[localizedString.Key] = newStringData;
            }

            while (!string.IsNullOrEmpty(path))
            {
                path = path.Replace("\\", "/");
                DemandStrings(path, AllStringsByPath).Add(newStringData);
                path = Path.GetDirectoryName(path);
            }
        }

        public static void Unregister(LocalizedString localizedString)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (localizedString.Shared != null)
            {
                return;
            }

            var strData = AllStringsByKey.Get(localizedString.Key);
            string path = strData.AssetPath;
            AllStringsByKey.Remove(localizedString.Key);
            while (!string.IsNullOrEmpty(path))
            {
                path = path.Replace("\\", "/");
                DemandStrings(path, AllStringsByPath).Remove(strData);
                path = Path.GetDirectoryName(path);
            }

            localizedString.Unload();
        }
#endif
    }
}
