using Ex.Kingmaker.ElementsSystem;
using MyOwlcatModification.Editor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEditorInternal;

namespace MyOwlcatModification.Assets.Editor.Inspector.Advanced_Dropdown.Game_Action
{
    internal class AddGameActionWindow : AdvancedDropdown
    {
        event Action<DropdownItemTyped> OnItemSelected;
        internal ReorderableList actionList;
        public AddGameActionWindow(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new Vector2(170f, 315f);
            OnItemSelected += onTargetSelected;
        }
        protected override AdvancedDropdownItem BuildRoot()
        {
            return BlueprintTypesCache.CachedAdvancedDropdownActionItems;
        }
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            OnItemSelected.Invoke(item as DropdownItemTyped);
        }
        protected void onTargetSelected(DropdownItemTyped selected)
        {
            actionList.GrabKeyboardFocus();
            bool flag2 = actionList.serializedProperty != null;
            if (flag2)
            {
                bool flag3 = actionList.serializedProperty.hasMultipleDifferentValues && !EditorUtility.DisplayDialog(L10n.Tr("Changing size of the array will copy new value to all other selected objects."), L10n.Tr("Unique values in the different selected object array size properties will be lost"), L10n.Tr("OK"), L10n.Tr("Cancel"));
                if (flag3)
                {
                    return;
                }
                SerializedProperty serializedProperty = actionList.serializedProperty.FindPropertyRelative("Array.size");
                SerializedProperty serializedProperty2 = serializedProperty;
                int intValue = serializedProperty2.intValue;
                serializedProperty2.intValue = intValue + 1;
                serializedProperty.serializedObject.ApplyModifiedProperties();
                actionList.index = actionList.serializedProperty.arraySize - 1;
                bool flag4 = selected?.selfType != null;
                if (flag4)
                {
                    var element = Kingmaker.ElementsSystem.Element.CreateInstance(selected.selfType);
                    actionList.serializedProperty.GetArrayElementAtIndex(actionList.index).managedReferenceValue = element;
                    var obj = actionList.serializedProperty.serializedObject;
                    obj.ApplyModifiedProperties();
                    obj.Update();
                    var target = obj.targetObject as Ex.Kingmaker.Blueprints.SimpleBlueprint;
                    if (!target)
                        return;
                    string path = AssetDatabase.GetAssetPath(target);
                    var importer = (AssetImporter.GetAtPath(path) as BlueprintImporter);
                    importer.Save(target);
                }
                    Undo.SetCurrentGroupName($"Add {(flag4 ? selected.selfType.Name : "Action")} to list");

            }
        }


        public class AddGameActionAdvancedDropdownWindow : EditorWindow
        {

            AddGameActionWindow dropdown = new AddGameActionWindow(new AdvancedDropdownState());

            //static AddGameActionAdvancedDropdownWindow instance = ScriptableObject.CreateInstance<AddGameActionAdvancedDropdownWindow>();
            internal static bool Show(Rect rect, ReorderableList actionList)
            {
                //instance.Close();
                AddGameActionAdvancedDropdownWindow addComponentWindow = ScriptableObject.CreateInstance<AddGameActionAdvancedDropdownWindow>();
                addComponentWindow.dropdown.actionList = actionList;
                addComponentWindow.dropdown.Show(rect);
                return true;
            }
        }
    }
}
