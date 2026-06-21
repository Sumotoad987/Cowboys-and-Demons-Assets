using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UnlitShaderGUI : ShaderGUIBase
{
    private MaterialProperty m_StencilRef;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_StencilRef = properties.FirstOrDefault(p => p.name == "_StencilRef");
        EditorGUI.BeginChangeCheck();
        StencilRefField();

        base.OnGUI(materialEditor, properties);
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

    public override void CreatePropertyGroups(MaterialProperty[] properties)
    {
        
    }
}
