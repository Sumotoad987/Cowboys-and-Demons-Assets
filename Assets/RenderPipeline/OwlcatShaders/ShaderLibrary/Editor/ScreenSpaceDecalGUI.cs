using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenSpaceDecalGUI : ShaderGUIBase
{
    public enum MaterialType
    {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    private MaterialProperty m_MaterialType;

    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        //m_Groups.Add(new KeywordPropertyGroup()
        //{
        //    HideToggle = true,
        //    InspectorName = "Cutout",
        //    HideTags = new string[] { "cutout" },
        //    Keyword = "CUTOUT_ON"
        //});

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "tex1", "uv1" },
            InspectorName = "Texture1",
            PropertyName = "TEXTURE1_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "noise0" },
            InspectorName = "Noise0",
            PropertyName = "NOISE0_ON",
        });

        var noise0Enabled = (m_MaterialEditor.target as Material).IsKeywordEnabled("NOISE0_ON");
        var noise1Enabled = (m_MaterialEditor.target as Material).IsKeywordEnabled("NOISE1_ON");
        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = (!noise0Enabled && !noise1Enabled),
            HideTags = new[] { "correction" },
            InspectorName = "Noise UV Correction",
            PropertyName = "NOISE_UV_CORRECTION",
            IndentLevel = 1,
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "noise1" },
            InspectorName = "Noise1",
            PropertyName = "NOISE1_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Emission",
            HideTags = new string[] { "emission" },
            PropertyName = "EMISSION_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Slope fade",
            HideTags = new string[] { "slope" },
            PropertyName = "SLOPE_FADE_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Gradient fade",
            HideTags = new string[] { "gradient" },
            PropertyName = "GRADIENT_FADE_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Color Alpha Ramp",
            HideTags = new string[] { "ramp" },
            PropertyName = "COLOR_ALPHA_RAMP"
        });
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialType = properties.FirstOrDefault(p => p.name == "_Type");

        EditorGUI.BeginChangeCheck();
        MaterialTypePopup();
        materialEditor.RenderQueueField();

        base.OnGUI(materialEditor, properties);
        if (EditorGUI.EndChangeCheck())
        {
            MaterialChanged(materialEditor.target as Material);
        }
    }

    private void MaterialTypePopup()
    {
        EditorGUI.showMixedValue = m_MaterialType.hasMixedValue;
        MaterialType materialType1 = (MaterialType)m_MaterialType.floatValue;
        EditorGUI.BeginChangeCheck();
        MaterialType materialType2 = (MaterialType)EditorGUILayout.EnumPopup("Material Type", materialType1);
        if (EditorGUI.EndChangeCheck())
        {
            m_MaterialEditor?.RegisterPropertyChangeUndo("Rendering Mode");
            m_MaterialType.floatValue = (float)materialType2;
        }
        EditorGUI.showMixedValue = false;
    }

    protected override void MaterialChanged(Material material)
    {
        MaterialType type = (MaterialType)m_MaterialType.floatValue;
        SetupShaderWithType(material, type);
    }

    private void SetupShaderWithType(Material material, MaterialType type)
    {
        //material.SetInt("_StencilRef", (int)Kingmaker.Import.StencilRef.DecalReceiver);
        switch (type)
        {
            case MaterialType.Opaque:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetOverrideTag("Reflection", "None");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.DisableKeyword("CUTOUT_ON");
                material.DisableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                break;

            case MaterialType.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetOverrideTag("Reflection", "None");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.EnableKeyword("CUTOUT_ON");
                material.DisableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                break;

            case MaterialType.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("Reflection", "None");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.DisableKeyword("CUTOUT_ON");
                material.EnableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                break;

            case MaterialType.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("Reflection", "None");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.DisableKeyword("CUTOUT_ON");
                material.DisableKeyword("ALPHABLEND_ON");
                material.EnableKeyword("ALPHAPREMULTIPLY_ON");
                break;

                //case MaterialType.Fade:
                //case MaterialType.Transparent:
                //    material.SetOverrideTag("RenderType", "Transparent");
                //    material.SetOverrideTag("Reflection", "None");
                //    material.SetInt("_SrcBlend", (int)BlendMode.One);
                //    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                //    material.DisableKeyword("CUTOUT_ON");
                //    material.DisableKeyword("ALPHABLEND_ON");
                //    material.EnableKeyword("ALPHAPREMULTIPLY_ON");
                //    break;
        }
    }
}