using MyOwlcatModification.Assets.Editor.Utility.Importers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace MyOwlcatModification.Assets.Editor.Asset_Importers
{
    [CustomEditor(typeof(PatchImporter))]
    internal class PatchImporterSettings : AssetImporterEditor
    {
        protected override bool needsApplyRevert => false;
    }
}
