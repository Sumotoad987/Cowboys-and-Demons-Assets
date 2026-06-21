using System;
using UnityEditor;
using UnityEngine;

namespace Owlcat.Editor.Core.Utility
{
    public static class GuiScopes
    {
        public static IDisposable Color(Color c)
        {
            return new GuiScopes.SetColorScope(c);
        }

        public static IDisposable LabelWidth(float w)
        {
            return new GuiScopes.SetLabelWidthScope(w);
        }

        public static IDisposable Indent(int n = 1)
        {
            return new GuiScopes.SetIndentScope(n);
        }

        public static IDisposable UpdateObject(SerializedObject o)
        {
            return new GuiScopes.UpdateObjectScope(o);
        }

        public static IDisposable UpdateObject(SerializedProperty p)
        {
            return new GuiScopes.UpdateObjectScope(p.serializedObject);
        }

        public static IDisposable FixedWidth(float labelWidth, float fieldWidth)
        {
            return new GuiScopes.SetFixedWidthScope(labelWidth, fieldWidth);
        }

        public static IDisposable Horizontal(params GUILayoutOption[] options)
        {
            return new GUILayout.HorizontalScope(options);
        }

        public static IDisposable Vertical(params GUILayoutOption[] options)
        {
            return new GUILayout.VerticalScope(options);
        }

        private class SetColorScope : IDisposable
        {
            public SetColorScope(Color c)
            {
                this.m_Color = GUI.color;
                GUI.color = c;
            }

            public void Dispose()
            {
                GUI.color = this.m_Color;
            }

            private readonly Color m_Color;
        }

        private class SetIndentScope : IDisposable
        {
            public SetIndentScope(int width)
            {
                this.m_Width = width;
                EditorGUI.indentLevel += width;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel -= this.m_Width;
            }

            private readonly int m_Width;
        }

        private class SetLabelWidthScope : IDisposable
        {
            public SetLabelWidthScope(float width)
            {
                this.m_Width = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = width;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = this.m_Width;
            }

            private readonly float m_Width;
        }

        private class SetFixedWidthScope : IDisposable
        {
            public SetFixedWidthScope(float labelWidth, float fieldWidth)
            {
                this.m_OldIndentLevel = EditorGUI.indentLevel;
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUIUtility.fieldWidth = fieldWidth;
                EditorGUI.indentLevel = 0;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = 0f;
                EditorGUIUtility.fieldWidth = 0f;
                EditorGUI.indentLevel = this.m_OldIndentLevel;
            }

            private int m_OldIndentLevel;
        }

        private class UpdateObjectScope : IDisposable
        {
            public UpdateObjectScope(SerializedObject serializedObject)
            {
                this.m_SerializedObject = serializedObject;
                this.m_SerializedObject.Update();
            }

            public void Dispose()
            {
                this.m_SerializedObject.ApplyModifiedProperties();
            }

            private readonly SerializedObject m_SerializedObject;
        }
    }
}
