using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.EditorGUILayout;
using UnityEditor.VersionControl;

namespace MyOwlcatModification
{
    public class EquipmentEntityImporter : EditorWindow
    {
        static string inputGuid = "a88163dba67a85941946a58aa294d094";//"e7c86166041c1e04a92276abdab68afa";
        static string buffer = "";
        static bool showError = false;

        [MenuItem("Importers/Import EE")]
        public static void MenuButton()
        {
            if (!ModAssetImporter.Launched) return;
            GetWindow<EquipmentEntityImporter>(true, "Import Equipment Entity", true);
            //wnd.titleContent = new GUIContent("Unit importer");
        }


        private void OnGUI()
        {
            BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Guid to search");
            GUILayout.FlexibleSpace();
            EndHorizontal();
            BeginHorizontal();
            GUILayout.FlexibleSpace();

            buffer = TextField(inputGuid);
            GUILayout.FlexibleSpace();
            EndHorizontal();
            if (buffer != inputGuid)
            {
                showError = false;
            }
            inputGuid = buffer;
            if (showError)
            {
                BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Failed to find Equipment Entity with asset Id {inputGuid}!");
                GUILayout.FlexibleSpace();
                EndHorizontal();
                BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Can't import");
                GUILayout.FlexibleSpace();
                EndHorizontal();
            }
            BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import"))
            { 
               showError = !ModAssetImporter.DoImportEE(inputGuid);
            }
            GUILayout.FlexibleSpace();
            EndHorizontal();
        }
    }
}
