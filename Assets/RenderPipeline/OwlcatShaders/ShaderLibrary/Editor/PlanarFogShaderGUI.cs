using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public class PlanarFogShaderGUI : ShaderGUIBase
{
    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Noise",
            HideTags = new string[] {"noise"},
            PropertyName = "NOISE_ON"
        });
    }
}