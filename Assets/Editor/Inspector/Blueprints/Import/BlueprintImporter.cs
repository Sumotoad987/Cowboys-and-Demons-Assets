using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Debug;
using UnityEditor.AssetImporters;
using Ex.Kingmaker.Blueprints.JsonSystem;
using System.Runtime.CompilerServices;
using Ex.Kingmaker.Blueprints.Facts;
using Ex.Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Owlcat.QA.Validation;
using UnityEditor;

namespace MyOwlcatModification
{
    [ScriptedImporter(1, "jbp")]
    public class BlueprintImporter : ScriptedImporter
    {
        [SerializeField]
        internal BlueprintJsonWrapper wrapper;
        internal bool DontReimport;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!Launcher.Launched)
            {
                Debug.LogError("BlueprintImporter - failed to Launch minimal services. Aborting.");
            }
            if (DontReimport)
            {
                DontReimport = false;
                return;
            }
            try
            {
                wrapper = BlueprintJsonWrapper.Load(ctx.assetPath);
            }
            catch(Exception ex)
            {
                LogException(ex);
                return;
            }
            if (wrapper is null)
            {
                LogError($"Failed to import a blueprint from path {ctx.assetPath}. Wrapper is null.");
                return;
            }
            if (wrapper.Data is null)
            {
                LogError($"Failed to import a blueprint from path {ctx.assetPath}. Data is null.");
                return;
            }
            if (wrapper.Data.AssetGuid == BlueprintGuid.Empty)
            {
                wrapper.Data.AssetGuid = BlueprintGuid.Parse(AssetDatabase.AssetPathToGUID(ctx.assetPath));
            }
            wrapper.AssetId = wrapper.Data.AssetGuid.ToString();
            //if (Guid.TryParse(wrapper.AssetId, out Guid guid))
            //wrapper.Data.AssetGuid = new BlueprintGuid(guid);
            Debug.Log($"BlueprintImporter - AssetID of the imported blueprint {wrapper.Data.name} has been set to {wrapper.Data.AssetGuid}");
            var obj = wrapper.Data;
            Sprite icon = null;
            Texture2D texture = null;
            if (obj is BlueprintUnitFact feature)
                icon = feature.Icon;
            if (obj is BlueprintItem item)
                icon = item.Icon;
            if (icon != null && icon.texture != null)
                texture = icon.texture.isReadable ? icon.texture : icon.texture.DuplicateTexture();
            ctx.AddObjectToAsset("blueprint", wrapper.Data, texture);
            ctx.SetMainObject(wrapper.Data);
            //var Scriptable = wrapper.Data as Ex.Kingmaker.Blueprints.BlueprintScriptableObject;
            //if (Scriptable != null)
            //    foreach(var component in Scriptable.ComponentsArray)
            //      try  
            //      {
            //          var wrapped = BlueprintComponentEditorWrapper.Wrap(component);
            //          ctx.AddObjectToAsset(wrapped.name, wrapped);
            //      }
            //      catch(Exception ex)
            //      {
            //          Debug.Log($"Failed to wrap a component {component?.name ?? "NO_FCKING_NAME"} when importing blueprint at path {ctx.assetPath}");
            //          Debug.LogException(ex);
            //      }
            try
            {
                //Debug.Log($"Hide Flags is {ctx.mainObject.hideFlags}");
                //var prop = new SerializedObject(obj).GetIterator();
                //prop.Next(true);
                //for (int i =0; i < 4; i++)
                //if (prop.NextVisible(false))
                //    Debug.Log($"{prop.name} is editable? {prop.editable}.");
                //    else
                //    {
                //        Debug.Log($"Failed to enters)");
                //        break;
                //    }
            }
            catch (Exception ex)
            {
                Debug.Log("Failed");
                LogException(ex);
                return;
            }
            if (ResourcesLibrary.BlueprintsCache?.m_PackSerializer != null && wrapper.Data.AssetGuid != BlueprintGuid.Empty)
                ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(wrapper.Data.AssetGuid, wrapper.Data);

            
        }

        public void Save(Ex.Kingmaker.Blueprints.SimpleBlueprint target)
        {
            if (!Launcher.Launched)
            {
                Debug.LogWarning($"Canceled saving the blueprint, because services are not Launched");
                return;
            }
            var id = wrapper.AssetId;
            wrapper.Data.Become(target);
            wrapper.Data.AssetGuid = BlueprintGuid.Parse(id);
            DontReimport = true;
            wrapper.Save(wrapper.LoadedFromPath);

        }

    }
}
