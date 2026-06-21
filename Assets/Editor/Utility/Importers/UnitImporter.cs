//using Kingmaker.Blueprints;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.EditorGUILayout;
using Kingmaker.Editor.Blueprints;
using Kingmaker.Blueprints;

namespace MyOwlcatModification
{
    public class UnitImporter : EditorWindow
    {

        public static UnitImporter wnd;
        static string inputGuid = "de1fc233ad934a0a93a17ebed3ec0cfb";
        //static string buffer = "";
        //static bool showError = false;

        [MenuItem("Importers/Import Unit")]
        public static void MenuButton()
        {
            if (!ModAssetImporter.Launched) return;
            inputGuid = "9e8727d008bec6e47842ba13df87d939";
            //wnd = GetWindow<UnitImporter>(false, "Import Unit", true);
            BlueprintPicker.ShowAssetPicker<BlueprintUnit>
                (bp => ModAssetImporter.DoImportUnit(bp.AssetGuid.ToString(), false), 
                Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.BlueprintUnit>(Kingmaker.Blueprints.BlueprintGuid.Parse(inputGuid)), 
                null, true);
            //wnd.titleContent = new GUIContent("Unit importer");
        }


        private void OnGUI()
        {
            #region Old
            //BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //GUILayout.Label("Guid to search");
            //GUILayout.FlexibleSpace();
            //EndHorizontal();
            //BeginHorizontal();
            //GUILayout.FlexibleSpace();

            //buffer = TextField(inputGuid);
            //GUILayout.FlexibleSpace();
            //EndHorizontal();
            //if (buffer != inputGuid)
            //{
            //    showError = false;
            //}
            //inputGuid = buffer;
            //if (showError)
            //{
            //    BeginHorizontal();
            //    GUILayout.FlexibleSpace();
            //    GUILayout.Label($"Failed to find blueprint of a unit with guid {inputGuid}!");
            //    GUILayout.FlexibleSpace();
            //    EndHorizontal();
            //    BeginHorizontal();
            //    GUILayout.FlexibleSpace();
            //    GUILayout.Label("Can't import");
            //    GUILayout.FlexibleSpace();
            //    EndHorizontal();
            //}
            //BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //if (GUILayout.Button("Import"))
            //{
            //    if (!Launcher.Launched)
            //        return;
            //    var bp = ResourcesLibrary.TryGetBlueprint<Ex.Kingmaker.Blueprints.BlueprintUnit>(inputGuid);
            //    showError = bp is null;
            //    if (bp != null)
            //        ModAssetImporter.DoImportUnit(inputGuid, false);
            //}
            //GUILayout.FlexibleSpace();
            //EndHorizontal();  
            #endregion
            
        }

    }
}
