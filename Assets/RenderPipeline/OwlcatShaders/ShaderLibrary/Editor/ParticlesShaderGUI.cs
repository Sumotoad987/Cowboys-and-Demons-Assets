using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;

public class ParticlesShaderGUI : ShaderGUIBase
{
    public enum BlendMode
    {
        Opaque,
        Fade,
        Transparent,
        Blend,
        BlendPremultiplyAlpha,
        Add,
        AddSoft,
        Multiply,
    }
    
    private MaterialProperty m_BlendMode;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_BlendMode = properties.FirstOrDefault(p => p.name == "_Type");
        EditorGUI.BeginChangeCheck();
        BlendModePopup();
        base.OnGUI(materialEditor, properties);
        materialEditor.RenderQueueField();
        if (EditorGUI.EndChangeCheck())
        {
            MaterialChanged(materialEditor.target as Material);
        }
    }

    private void BlendModePopup()
    {
        EditorGUI.showMixedValue = m_BlendMode.hasMixedValue;
        BlendMode blendMode1 = (BlendMode)m_BlendMode.floatValue;
        EditorGUI.BeginChangeCheck();
        BlendMode blendMode2 = (BlendMode)EditorGUILayout.EnumPopup("Material Type", blendMode1);
        if (EditorGUI.EndChangeCheck())
        {
            this.m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            m_BlendMode.floatValue = (float)blendMode2;
        }
        EditorGUI.showMixedValue = false;
    }

    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "ramp" },
            InspectorName = "Color Alpha Ramp",
            PropertyName = "COLOR_ALPHA_RAMP",
        });

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
            HideTags = new[] { "texsheetenabled" },
            InspectorName = "Texture Sheet Animation Enabled (Automatic when ParticleSystem used. Disable manually when MeshRenderer)",
            PropertyName = "_TexSheetEnabled",
            IndentLevel = 1,
            Type = PropertyGroup.GroupType.Float,
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = (!noise0Enabled && !noise1Enabled),
            HideTags = new[] { "correction" },
            InspectorName = "Noise UV Correction (NEED UV2)",
            PropertyName = "NOISE_UV_CORRECTION",
            IndentLevel = 1,
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = (!noise0Enabled && !noise1Enabled),
            HideTags = new [] {"randomizenoise"},
            InspectorName = "Randomize Noise Offset (Check Particle Material Controller If Enabled",
            PropertyName = "_RandomizeNoiseOffset",
            IndentLevel = 1,
            Type = PropertyGroup.GroupType.Float,
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "noise1" },
            InspectorName = "Noise1",
            PropertyName = "NOISE1_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "distortion" },
            InspectorName = "Distortion",
            PropertyName = "DISTORTION_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "bump" },
            InspectorName = "Bump",
            PropertyName = "BUMP_ON",
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "softness", "substractsoftness" },
            InspectorName = "Soft Particles",
            PropertyName = "SOFT_PARTICLES"
        });

        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "falloff" },
            InspectorName = "Opacity Falloff",
            PropertyName = "OPACITY_FALLOFF"
        });

        var opacityFalloff = (m_MaterialEditor.target as Material).IsKeywordEnabled("OPACITY_FALLOFF");
        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = !opacityFalloff,
            HideTags = new string[] { "empty" },
            InspectorName = "Invert Opacity Falloff",
            PropertyName = "INVERT_OPACITY_FALLOFF",
            IndentLevel = 1
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Particles Lighting",
            HideTags = new string[] { "lighting" },
            PropertyName = "PARTICLES_LIGHTING_ON",
        });

        var lightingEnabled = (m_MaterialEditor.target as Material).IsKeywordEnabled("PARTICLES_LIGHTING_ON");
        m_Groups.Add(new PropertyGroup()
        {
            HideToggle = !lightingEnabled,
            HideTags = new string[] { "normals" },
            InspectorName = "Billboard normals (for ParticleSystem trails)",
            PropertyName = "OVERRIDE_NORMAL_ON",
            IndentLevel = 1,
        });

        //m_Groups.Add(new PropertyGroup()
        //{
        //    HideToggle = !lightingEnabled,
        //    HideTags = new string[] { "lightprobes" },
        //    InspectorName = "Don't use vertex lights",
        //    PropertyName = "LIGHT_PROBES_ONLY",
        //    IndentLevel = 1,
        //});

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Fluid Fog",
            HideTags = new[] {"fluid"},
            PropertyName = "FLUID_FOG",
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Snap UV to World XZ",
            HideTags = new[] { "worldxz" },
            PropertyName = "WORLD_UV_XZ",
        });
    }

    protected override void MaterialChanged(Material material)
    {
        BlendMode type = (BlendMode)m_BlendMode.floatValue;
        SetupMaterialKeywords(material, type);
    }

    private void SetupMaterialKeywords(Material material, BlendMode type)
    {
        switch (type)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.SetInt("_CullMode", (int)CullMode.Back);
                material.DisableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = -1;
                break;

            case BlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.EnableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.DisableKeyword("ALPHABLEND_ON");
                material.EnableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.Blend:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.EnableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.BlendPremultiplyAlpha:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.DisableKeyword("ALPHABLEND_ON");
                material.EnableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.Add:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.EnableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.AddSoft:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.DisableKeyword("ALPHABLEND_ON");
                material.EnableKeyword("ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;

            case BlendMode.Multiply:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_CullMode", (int)CullMode.Off);
                material.DisableKeyword("ALPHABLEND_ON");
                material.DisableKeyword("ALPHAPREMULTIPLY_ON");
                material.EnableKeyword("ALPHABLENDMULTIPLY_ON");
                //material.renderQueue = 3001;
                break;
        }
    }
}