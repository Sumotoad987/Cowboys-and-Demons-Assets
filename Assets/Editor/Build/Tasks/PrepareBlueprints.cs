using System;
using System.IO;
using System.Linq;
using Ex.Kingmaker.Modding;
using Kingmaker.Utility;
using MyOwlcatModification.Assets.Editor.Utility.Importers;
using MyOwlcatModification.Editor.Build.Context;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace MyOwlcatModification.Editor.Build.Tasks
{
	public class PrepareBlueprints : IBuildTask
	{
#pragma warning disable 649
		[InjectContext(ContextUsage.In)]
		private IBuildParameters m_BuildParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationParameters m_ModificationParameters;

        [InjectContext(ContextUsage.In)]
        private IModificationRuntimeSettings m_ModificationSettings;
#pragma warning restore 649

        public int Version
			=> 1;
		
		public ReturnCode Run()
		{
			string originDirectory = m_ModificationParameters.BlueprintsPath;
			string destinationDirectory = m_BuildParameters.GetOutputFilePathForIdentifier(BuilderConsts.OutputBlueprints);
            string fullDestinationPath = Path.GetFullPath(destinationDirectory);
            BuilderUtils.CopyFilesWithFoldersStructure(
				originDirectory, destinationDirectory, i => i.EndsWith(".jbp"));
			var maybePatches = Directory.EnumerateFiles(m_ModificationParameters.SourcePath, "*.patch", SearchOption.AllDirectories);

            string fullOriginPath = Path.GetFullPath(m_ModificationParameters.SourcePath);
            int originPathLength = fullOriginPath.Length+1;
            var patchesWithTarget = maybePatches.Select(GetPatch).Where(o => o != default);
			foreach(var patch in patchesWithTarget)
			{
				var rootedpath = Path.GetFullPath(patch.patchPath);
                var filePath = rootedpath.Substring(originPathLength);
				var destination = Path.Combine( destinationDirectory, filePath);
				var dest = Path.GetDirectoryName(destination);
				if (!Directory.Exists(dest))
					Directory.CreateDirectory(dest);
				File.Copy(patch.patchPath, destination);
				m_ModificationSettings.Settings.BlueprintPatches.Add(new OwlcatModificationSettings.BlueprintPatch()
				{
					Filename = filePath,
					Guid = patch.targetID
				});
            }

			return ReturnCode.Success;
		}

		(string patchPath, string targetID) GetPatch(string patchPath)
		{
			var importer = AssetImporter.GetAtPath(patchPath) as PatchImporter;
			var target = importer?.TargetBluepint?.Guid.ToString();
			if (!target.IsNullOrEmpty())
				return (patchPath, target);
			else
			{
				Debug.LogWarning($"Could not find target Id for the blueprint patch at " + patchPath);
				return default;
			}

		}
	}
}