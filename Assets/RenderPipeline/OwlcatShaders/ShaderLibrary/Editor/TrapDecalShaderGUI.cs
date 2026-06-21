using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrapDecalShaderGUI : ShaderGUIBase
{
    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        m_Groups.Add(new PropertyGroup()
        {
            HideTags = new string[] { "nothing" },
            InspectorName = "Icon",
            PropertyName = "TRAP_ICON",
        });
    }
}
