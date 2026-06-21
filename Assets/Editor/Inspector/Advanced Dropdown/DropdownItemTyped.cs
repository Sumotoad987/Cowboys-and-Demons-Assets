using UnityEditor.IMGUI.Controls;
using System;

namespace MyOwlcatModification
{
    public class DropdownItemTyped : AdvancedDropdownItem
    {
        public DropdownItemTyped (Type t) : base(t.Name)
        {
            selfType = t;
        }
        public readonly Type selfType;
    }
}
