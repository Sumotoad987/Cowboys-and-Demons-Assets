using UnityEngine;
using UnityEditor;

namespace MyOwlcatModification
{
    public class BlueprintPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in movedAssets)
            {
                var bpImporter = AssetImporter.GetAtPath(path) as BlueprintImporter;
                if (bpImporter != null)
                {
                    bpImporter.wrapper.LoadedFromPath = path;
                    Debug.Log($"BlueprintPostprocessor - moved asset path to {path}");

                }
            }
        }
    }
}
