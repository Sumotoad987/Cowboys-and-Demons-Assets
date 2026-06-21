using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Kingmaker.Editor;

public abstract class ShaderGUIBase : ShaderGUI
{
    public class PropertyGroup
    {
        public enum GroupType
        {
            Keyword,
            Float,
        }

        public bool Enabled;
        public GroupType Type = GroupType.Keyword;
        public int IndentLevel;
        public bool HideToggle;
        public string PropertyName;
        public string InspectorName;
        public string[] HideTags;
        public PropertyGroup ParentGroup;

        public bool CheckHideTags(string input)
        {
            return HideTags.Any(tag => Regex.IsMatch(input, tag, RegexOptions.IgnoreCase));
        }
    }

    protected List<PropertyGroup> m_Groups = new List<PropertyGroup>();
    protected MaterialEditor m_MaterialEditor;
    private bool m_FirstTimeApply = true;

    public abstract void CreatePropertyGroups(MaterialProperty[] properties);

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialEditor = materialEditor;

        CreatePropertyGroupsInternal(properties);

        if (m_FirstTimeApply)
        {
            foreach (Material material in materialEditor.targets)
            {
                MaterialChanged(material);
            }
            
            m_FirstTimeApply = false;
        }

        MaterialGUI(properties);
    }

    private void CreatePropertyGroupsInternal(MaterialProperty[] properties)
    {
        m_Groups.Clear();
        CreatePropertyGroups(properties);

        Material mat = m_MaterialEditor.target as Material;

        foreach (var item in m_Groups)
        {
            switch (item.Type)
            {
                case PropertyGroup.GroupType.Keyword:
                    item.Enabled = mat.IsKeywordEnabled(item.PropertyName);
                    break;

                case PropertyGroup.GroupType.Float:
                    item.Enabled = mat.GetFloat(item.PropertyName) > 0;
                    break;
            }
        }
    }

    private void MaterialGUI(MaterialProperty[] properties)
    {
        EditorGUIUtility.fieldWidth = 64;

        // сначала рисуем свойства, которые не относятся ни к одной из групп
        var nonGroupedProperties = (from p in properties
                                    where !m_Groups.Any(g => g.CheckHideTags(p.name) || p.name == g.PropertyName)
                                    select p);

        foreach (var property in nonGroupedProperties)
		{
			ShaderProperty(property);
        }

        // затем рисуем сгруппированные свойства
        foreach (var group in m_Groups)
        {
            var groupProperties = (from p in properties
                                   where @group.CheckHideTags(p.name)
                                   select p);

            EditorGUILayout.Separator();

            EditorGUI.indentLevel = group.IndentLevel;

            if (group.HideToggle)
            {
                EditorGUILayout.BeginVertical();
            }
            else
            {
                group.Enabled = EditorGUILayout.BeginToggleGroup($"{group.InspectorName} ({group.Type})", group.Enabled);
            }

            if (group.Enabled)
            {
                foreach (Material material in m_MaterialEditor.targets)
                {
                    switch (group.Type)
                    {
                        case PropertyGroup.GroupType.Keyword:
                            material.EnableKeyword(group.PropertyName);
                            break;

                        case PropertyGroup.GroupType.Float:
                            material.SetFloat(group.PropertyName, 1);
                            break;
                    }
                }

                if (group.ParentGroup == null || group.ParentGroup.Enabled)
                {
                    foreach (var property in groupProperties)
                    {
						ShaderProperty(property);
                    }
                }
            }
            else
            {
                foreach (Material material in m_MaterialEditor.targets)
                {
                    switch (group.Type)
                    {
                        case PropertyGroup.GroupType.Keyword:
                            material.DisableKeyword(group.PropertyName);
                            break;

                        case PropertyGroup.GroupType.Float:
                            material.SetFloat(group.PropertyName, 0);
                            break;
                    }
                }
            }

            if (group.HideToggle)
            {
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.EndToggleGroup();
            }
        }
    }

	private void ShaderProperty(MaterialProperty property)
	{
		if ((property.flags & MaterialProperty.PropFlags.HideInInspector) != MaterialProperty.PropFlags.None)
		{
			return;
		}

		if (property.type != MaterialProperty.PropType.Texture)
		{
			m_MaterialEditor.ShaderProperty(property, property.displayName);
		}
		else
		{
			var height = m_MaterialEditor.GetPropertyHeight(property);
			var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
			m_MaterialEditor.ShaderProperty(rect, property, property.displayName);

			var pickerFieldRect = new Rect();
			pickerFieldRect.position = rect.position + new Vector2(16, EditorGUIUtility.singleLineHeight);

			// left indent + texture preview size
			float width = rect.width - 16 - 70;
			pickerFieldRect.size = new Vector2(width, EditorGUIUtility.singleLineHeight);

			//AssetPicker.ShowObjectField(
			//	pickerFieldRect, 
			//	property.textureValue, 
			//	t => property.textureValue = t as Texture,
			//	new GUIContent(), 
			//	typeof(Texture), 
			//	typeof(ScriptableObject),
			//	null,
			//	true
			//);
		}
	}

    protected virtual void MaterialChanged(Material material)
    {

    }
}