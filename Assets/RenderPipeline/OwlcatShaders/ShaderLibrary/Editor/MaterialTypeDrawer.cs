using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MaterialTypeDrawer : MaterialPropertyDrawer
{
    private static bool IsPropertyTypeSuitable(MaterialProperty prop)
    {
        return prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        //if (!MaterialTypeDrawer.IsPropertyTypeSuitable(prop))
        //{
        //    GUIContent label1 = new GUIContent("KeywordEnum used on a non-float property: " + prop.name);
        //    EditorGUI.LabelField(position, label1, EditorStyles.helpBox);
        //}
        //else
        //{
        //    EditorGUI.BeginChangeCheck();
        //    EditorGUI.showMixedValue = prop.hasMixedValue;
        //    MaterialType mt = (MaterialType)prop.floatValue;
        //    MaterialType newValue = (MaterialType)EditorGUI.EnumPopup(position, label, mt);
        //    EditorGUI.showMixedValue = false;
        //    if (!EditorGUI.EndChangeCheck())
        //        return;
        //    prop.floatValue = (float)newValue;
        //    foreach (Material material in prop.targets)
        //    {
        //        Kingmaker.Import.Editor.AssetImportPostprocessor.SetupMaterialWithType(material, newValue, null, null, null);
        //    }
        //}
    }
}