using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CircleDecalShaderGUI : DynamicDecalShaderGUI
{
    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        base.CreatePropertyGroups(properties);

        m_Groups.Add(new PropertyGroup()
        {
            InspectorName = "Fill Center",
            HideTags = new string[] { "center" },
            PropertyName = "FILL_CENTER_ON"
        });
    }
}