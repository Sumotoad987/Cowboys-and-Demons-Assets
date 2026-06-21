using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PFStandardDynamicShaderGUI : ShaderGUIBase
{
    private MaterialProperty m_MaterialType;
    private MaterialProperty m_QueueOffset;
    private MaterialProperty m_StencilRef;
    private PropertyGroup m_SimpleVertexAnimGroup;
    //private PropertyGroup m_ComplexVertexAnimGroup;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialType = properties.FirstOrDefault(p => p.name == "_Type");
        m_QueueOffset = properties.FirstOrDefault(p => p.name == "_QueueOffset");
        m_StencilRef = properties.FirstOrDefault(p => p.name == "_StencilRef");

        m_MaterialEditor = materialEditor;

        EditorGUI.BeginChangeCheck();
        MaterialTypePopup();
        //QueueOffsetField();
        StencilRefField();
        materialEditor.EnableInstancingField();

        base.OnGUI(materialEditor, properties);
        if (EditorGUI.EndChangeCheck())
        {
            MaterialChanged(materialEditor.target as Material);
        }
    }

    private void StencilRefField()
    {
        //int stencilRef1 = (int)m_StencilRef.floatValue;
        //EditorGUI.BeginChangeCheck();
        //int stencilRef2 = EditorGUILayout.MaskField("Stencil Reference", stencilRef1, Enum.GetNames(typeof(StencilRef)));
        ////StencilRef stencilRef2 = (StencilRef)EditorGUILayout.EnumPopup("Stencil Reference", stencilRef1);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    this.m_MaterialEditor.RegisterPropertyChangeUndo("Stencil Ref");
        //    m_StencilRef.floatValue = stencilRef2;
        //}
    }

    private void QueueOffsetField()
    {
        //int offset1 = (int)m_QueueOffset.floatValue;
        //EditorGUI.BeginChangeCheck();
        //int offset2 = EditorGUILayout.IntField("Queue Offset", offset1);
        //offset2 = Mathf.Max(offset2, 0);
        //var mat = m_MaterialEditor.target as Material;
        //EditorGUILayout.LabelField("Render Queue", mat.renderQueue.ToString());
        //if (EditorGUI.EndChangeCheck())
        //{
        //    this.m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Queue");
        //    mat.renderQueue = Kingmaker.Import.Editor.AssetImportPostprocessor.GetRenderQueue((MaterialType)m_MaterialType.floatValue) + offset2;
        //    m_QueueOffset.floatValue = (float)offset2;
        //}
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
            InspectorName = "Dissolve",
            HideTags = new string[] { "dissolve" },
            PropertyName = "DISSOLVE_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Petrification",
            HideTags = new string[] { "petrification" },
            PropertyName = "PETRIFICATION_ON"
        });

        //m_Groups.Add(new PropertyGroup()
        //{
        //    InspectorName = "Fog Of War Dissolve",
        //    HideTags = new string[] { "fogofwardissolve" },
        //    PropertyName = "FOG_OF_WAR_DISSOLVE_ON",
        //});

        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    InspectorName = "Rim Lighting Global",
        //    HideTags = new string[] { "_rimglobal" },
        //    Keyword = "RIM_LIGHTING_GLOBAL_ON"
        //});

        //var rimLightingGlobalNdotL = (m_MaterialEditor.target as Material).IsKeywordEnabled("RIM_LIGHTING_GLOBAL_ON");
        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    HideToggle = !rimLightingGlobalNdotL,
        //    InspectorName = "Rim Global Shade NdotL",
        //    HideTags = new string[] { "_Shadeglobal" },
        //    Keyword = "RIM_LIGHTING_GLOBAL_SHADE_NDOTL",
        //    IndentLevel = 1
        //});

        m_Groups.Add(new PropertyGroup()
        {
            Type = PropertyGroup.GroupType.Float,
            InspectorName = "Rim Lighting",
            HideTags = new string[] { "_rimcolor", "_rimpower", "_rimshade" },
            PropertyName = "_RimLighting"
        });

        //var rimLightingNdotL = (m_MaterialEditor.target as Material).IsKeywordEnabled("RIM_LIGHTING_ON");
        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    HideToggle = !rimLightingNdotL,
        //    InspectorName = "Rim Shade NdotL",
        //    HideTags = new string[] { "_Shade" },
        //    Keyword = "RIM_LIGHTING_SHADE_NDOTL",
        //    IndentLevel = 1
        //});

        //m_Groups.Add(new PropertyGroup()
        //{
        //    InspectorName = "Vertex Animation",
        //    HideTags = new string[] { "animation", "interaction" },
        //    PropertyName = "VERTEX_ANIMATION_ON"
        //});

        //m_SimpleVertexAnimGroup = new PropertyGroup()
        //{
        //    HideToggle = true,
        //    InspectorName = "Simple Vertex Animation",
        //    HideTags = new string[] { "simple", "ground", "grass" },
        //    PropertyName = "VERTEX_ANIMATION_SIMPLE_ON"
        //};
        //m_Groups.Add(m_SimpleVertexAnimGroup);

        //m_ComplexVertexAnimGroup = new PropertyGroup()
        //{
        //    HideToggle = true,
        //    InspectorName = "Complex Vertex Animation",
        //    HideTags = new string[] { "complex" },
        //    PropertyName = "VERTEX_ANIMATION_COMPLEX_ON"
        //};
        //m_Groups.Add(m_ComplexVertexAnimGroup);

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Reflections",
            HideTags = new string[] { "reflections" },
            PropertyName = "REFLECTIONS_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Use Light Probe Proxy Volume",
            HideTags = new string[] { "proxy" },
            PropertyName = "USE_LIGHT_PROBE_PROXY_VOLUME"
        });
    }

    protected override void MaterialChanged(Material material)
    {
        //MaterialType type = (MaterialType)m_MaterialType.floatValue;
        //var dissolveCutout = material.GetFloat("_DissolveCutout");
        //if (dissolveCutout > 0 && type == MaterialType.Opaque)
        //{
        //    type = MaterialType.Cutout;
        //}

        //Kingmaker.Import.Editor.AssetImportPostprocessor.SetupMaterialWithType(material, type, null, null, null);
    }
}