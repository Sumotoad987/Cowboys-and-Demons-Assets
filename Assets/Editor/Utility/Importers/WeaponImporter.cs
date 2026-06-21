using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View.Equipment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace MyOwlcatModification
{
    class WeaponImporter : EditorWindow
    {
        static EditorWindow wnd;
        protected static string inputGuid = "de1fc233ad934a0a93a17ebed3ec0cfb";
        protected string buffer = "";
        protected bool showError = false;

        [MenuItem("Importers/Import Weapon")]
        public static void MenuButton()
        {
            if (!ModAssetImporter.Launched) return;
            inputGuid = "de1fc233ad934a0a93a17ebed3ec0cfb";
            wnd = GetWindow<WeaponImporter>(true, "Import Weapon", true);
            //wnd.titleContent = new GUIContent("Weapon importer");
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
                GUILayout.Label($"Failed to find blueprint of a type BlueprintItemWeapon with guid {inputGuid}!");
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
                var bp = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(inputGuid);
                showError = bp is null;
                if (!showError)
                    ModAssetImporter.DoImportWeapon(bp as BlueprintItemWeapon);
            }
            GUILayout.FlexibleSpace();
            EndHorizontal();


        }
        
    }

}
