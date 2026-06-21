using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using Ex.Kingmaker.Blueprints;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
    public static class UtilExtension
    {
        public static void BlueprintReferenceBaseSetPropertyValue(SerializedProperty p, BlueprintScriptableObject bp)
        {
            p.FindPropertyRelative("guid").stringValue = (((bp != null) ? bp.AssetGuid.ToString() : null) ?? "");
        }
        public static BlueprintScriptableObject BlueprintReferenceBaseGetPropertyValue(SerializedProperty p)
        {
            //UnityEngine.Debug.Log($"trying to get bp reference  at path {p.propertyPath} (type is {p.type})");
            try
            {
                var a = p.FindPropertyRelative("guid").stringValue;
                var b = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(a);
                return b;
            }
            catch (Exception ex)
            {
                PFLog.Default.Exception(ex, $"when trying to get bp reference at path {p.propertyPath}");
                return null;
            }
        }
    }
}
