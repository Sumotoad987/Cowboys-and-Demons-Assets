using Kingmaker.Blueprints;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.EditorGUILayout;
using Kingmaker.BundlesLoading;
using UnityEditor.VersionControl;

namespace MyOwlcatModification
{
    public class PrefabImporter : EditorWindow
    {

        static string inputGuid = "aa448b28b377b1c49b136d88fa346600";
        static string buffer = "";
        static bool showError = false;

        [MenuItem("Importers/ImportPrefab")]
        public static void MenuButton()
        {
            if (!ModAssetImporter.Launched) return;
            inputGuid = "9e8727d008bec6e47842ba13df87d939";
            GetWindow<PrefabImporter>(true, "Import Unit", true);
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
                GUILayout.Label($"Failed to find blueprint of a unit with guid {inputGuid}!");
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
                showError = ModAssetImporter.DoImportPrefab(inputGuid) == null;
            }
            GUILayout.FlexibleSpace();
            EndHorizontal();
        }

    }
}
