using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class KeywordGroupDecorator : MaterialPropertyDrawer
{
    private string m_Keyword;
    private string m_Header;

    public KeywordGroupDecorator(string keyword, string header)
    {
        m_Keyword = keyword;
        m_Header = header;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 24;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        position.y += 8f;
        position = EditorGUI.IndentedRect(position);

        Material firstMaterial = (Material)prop.targets[0];
        bool enabled = firstMaterial.IsKeywordEnabled(m_Keyword);

        Rect toggleRect = position;
        toggleRect.width = 20;

        var newValue = GUI.Toggle(toggleRect, enabled, string.Empty);

        Rect labelRect = position;
        labelRect.width = position.width - toggleRect.width;
        labelRect.x += toggleRect.width;
        GUI.Label(labelRect, m_Header, EditorStyles.boldLabel);

        if (enabled != newValue)
        {
            SetKeyword(prop, newValue);
        }
    }

    private void SetKeyword(MaterialProperty prop, bool enabled)
    {
        foreach (Material material in prop.targets)
        {
            if (enabled)
            {
                material.EnableKeyword(m_Keyword);
            }
            else
            {
                material.DisableKeyword(m_Keyword);
            }
        }
    }
}