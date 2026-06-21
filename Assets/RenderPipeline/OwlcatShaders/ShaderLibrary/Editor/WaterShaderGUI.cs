using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public class WaterShaderGUI : ShaderGUIBase
{
    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        var foamGroup = new PropertyGroup()
        {
            InspectorName = "Foam",
            HideTags = new string[] {"foamramp", "foamcolor", "maintex", "foamstrength"},
            PropertyName = "FOAM_ON"
        };
        m_Groups.Add(foamGroup);

        var showFoamSettings = (m_MaterialEditor.target as Material).IsKeywordEnabled("FOAM_ON");
        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Foam Depth",
            HideToggle = !showFoamSettings,
            HideTags = new string[] {"foampower"},
            PropertyName = "FOAM_DEPTH",
            IndentLevel = 1,
            ParentGroup = foamGroup,
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Foam Mask",
            HideToggle = !showFoamSettings,
            HideTags = new string[] { "foammaskscale" },
            PropertyName = "FOAM_MASK",
            IndentLevel = 1,
            ParentGroup = foamGroup,
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Flow Map",
            HideTags = new string[] { "flownoise" },
            PropertyName = "FLOW_ON"
        });

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Screen Space Reflections",
            HideTags = new string[]
            {
                "backwardsrays",
                "_MaxRayTraceDistance",
                "_LayerThickness",
                "_MaxSteps",
                "_RayStepSize",
                "_FadeDistance",
                "_FresnelFade",
                "_ScreenEdgeFading"
            },
            PropertyName = "SCREEN_SPACE_REFLECTIONS",
        });
    }
}