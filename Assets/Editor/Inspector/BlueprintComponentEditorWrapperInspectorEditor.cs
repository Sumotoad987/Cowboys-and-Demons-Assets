using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(BlueprintComponentEditorWrapper), true)]
    public  class BlueprintComponentEditorWrapperInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            bool guiBackup = GUI.enabled;
            GUI.enabled = true;

            GUILayout.Label($"BlueprintComponentEditorWrapperInspectorEditor test -  {target.name}");
            DrawDefaultInspector();



            GUI.enabled = guiBackup;
        }
    }
}
