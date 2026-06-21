using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GlobalMapShaderGUI : ShaderGUIBase
{
    private MaterialProperty m_MaterialType;

    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = true,
            InspectorName = "Cutout",
            HideTags = new string[] { "_cutout" },
            PropertyName = "CUTOUT_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Metallness",
            HideTags = new string[] { "metallic" },
            PropertyName = "METALLNESS_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Bump",
            HideTags = new string[] { "bump" },
            PropertyName = "BUMP_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Specular",
            HideTags = new string[] { "specular", "smoothness" },
            PropertyName = "SPECULAR_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Emission",
            HideTags = new string[] { "_emission" },
            PropertyName = "EMISSION_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Rim Lighting Global",
            HideTags = new string[] { "_rimglobal" },
            PropertyName = "RIM_LIGHTING_GLOBAL_ON"
        });

        var rimLightingGlobalNdotL = (m_MaterialEditor.target as Material).IsKeywordEnabled("RIM_LIGHTING_GLOBAL_ON");
        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = !rimLightingGlobalNdotL,
            InspectorName = "Rim Global Shade NdotL",
            HideTags = new string[] { "_Shadeglobal" },
            PropertyName = "RIM_LIGHTING_GLOBAL_SHADE_NDOTL",
            IndentLevel = 1
        });

        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    InspectorName = "Rim Lighting",
        //    HideTags = new string[] { "_rimcolor", "_rimpower" },
        //    Keyword = "RIM_LIGHTING_ON"
        //});

        //var rimLightingNdotL = (m_MaterialEditor.target as Material).IsKeywordEnabled("RIM_LIGHTING_ON");
        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    HideToggle = !rimLightingNdotL,
        //    InspectorName = "Rim Shade NdotL",
        //    HideTags = new string[] { "_Shade" },
        //    Keyword = "RIM_LIGHTING_SHADE_NDOTL",
        //    IndentLevel = 1
        //});

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Reflections",
            HideTags = new string[] { "reflections" },
            PropertyName = "REFLECTIONS_ON"
        });
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialType = properties.FirstOrDefault(p => p.name == "_Type");

        EditorGUI.BeginChangeCheck();
        MaterialTypePopup();

        base.OnGUI(materialEditor, properties);
        if (EditorGUI.EndChangeCheck())
        {
            MaterialChanged(materialEditor.target as Material);
        }
    }

    private void MaterialTypePopup()
    {
        //EditorGUI.showMixedValue = m_MaterialType.hasMixedValue;
        //MaterialType materialType1 = (MaterialType)m_MaterialType.floatValue;
        //EditorGUI.BeginChangeCheck();
        //MaterialType materialType2 = (MaterialType)EditorGUILayout.EnumPopup("Material Type", materialType1);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    this.m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
        //    m_MaterialType.floatValue = (float)materialType2;
        //}
        //EditorGUI.showMixedValue = false;
    }
}