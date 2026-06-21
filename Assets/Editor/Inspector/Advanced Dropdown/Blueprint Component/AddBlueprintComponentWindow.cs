using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using MyOwlcatModification.Editor.Utility;
using Ex.Kingmaker.Blueprints;
using HarmonyLib;

namespace MyOwlcatModification
{
    public class AddBlueprintComponentAdvancedDropdown :  AdvancedDropdown
    {
        event Action<DropdownItemTyped> OnItemSelected;
        internal BlueprintScriptableObject targetBlueprint;

        public AddBlueprintComponentAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new Vector2(80f, 315f);
            OnItemSelected += onTargetSelected;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {         
            return BlueprintTypesCache.CachedAdvancedDropdownComponentItems;
        }
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            OnItemSelected.Invoke(item as DropdownItemTyped);
        }

        protected void onTargetSelected(DropdownItemTyped selected)
        {
            Debug.Log($"OnTargetSelected {targetBlueprint != null} {selected?.selfType?.Name ?? "null"}");
            if (targetBlueprint == null || selected == null)
                return;
            var component = Activator.CreateInstance(selected.selfType) as BlueprintComponent;
            component.name = $"${selected.selfType.Name}${Guid.NewGuid()}"; 
            component.OwnerBlueprint = targetBlueprint;
            targetBlueprint.ComponentsArray = targetBlueprint.ComponentsArray.AddToArray(component); 
            string path = AssetDatabase.GetAssetPath(targetBlueprint);
            //Debug.Log($"OnInspectorGUI - has been changed. Asset is at path {path}");
            var importer = (AssetImporter.GetAtPath(path) as BlueprintImporter);
            var wrapper = importer.wrapper;
            var id = wrapper.AssetId;
            wrapper.Data.Become(targetBlueprint);
            wrapper.Data.AssetGuid = Kingmaker.Blueprints.BlueprintGuid.Parse(id);
            importer.DontReimport = true;
            wrapper.Save(path);
        }
    }
    public class AddBlueprintComponentAdvancedDropdownWindow : EditorWindow
    {
        //string searchString = string.Empty;

        AddBlueprintComponentAdvancedDropdown dropdown = new AddBlueprintComponentAdvancedDropdown(new AdvancedDropdownState());

        //static AddBlueprintComponentAdvancedDropdownWindow instance = ScriptableObject.CreateInstance<AddBlueprintComponentAdvancedDropdownWindow>();
        internal static bool Show(Rect rect, BlueprintScriptableObject blueprint)
        {
            //instance.Close();
            AddBlueprintComponentAdvancedDropdownWindow addComponentWindow = ScriptableObject.CreateInstance<AddBlueprintComponentAdvancedDropdownWindow>();
            addComponentWindow.dropdown.targetBlueprint = blueprint;
            //addComponentWindow.searchString = EditorPrefs.GetString("ComponentSearchString", "");
            addComponentWindow.dropdown.Show(rect);
            return true;
        }
    }
}
