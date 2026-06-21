using UnityEditor;
using UnityEditor.AssetImporters;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(BlueprintImporter))]
    public class BlueprintImporterSettings : AssetImporterEditor
    {
        protected override bool needsApplyRevert => false;
    }
}
