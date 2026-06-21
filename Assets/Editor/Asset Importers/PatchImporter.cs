using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor.AssetImporters;
using Ex.Kingmaker.Blueprints;
using System.IO;

namespace MyOwlcatModification.Assets.Editor.Utility.Importers
{

    [ScriptedImporter(1, "patch")]
    internal class PatchImporter : ScriptedImporter
    {
        [SerializeField]
        public BlueprintScriptableObjectReference TargetBluepint;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            try
            {
                var obj = new TextAsset(File.ReadAllText(ctx.assetPath));
                ctx.AddObjectToAsset("Patch as text", obj);
                ctx.SetMainObject(obj);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
