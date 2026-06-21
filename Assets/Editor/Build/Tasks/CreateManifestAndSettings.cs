using System.IO;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Modding;
using Kingmaker.BundlesLoading;
using MyOwlcatModification.Editor.Build.Context;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace MyOwlcatModification.Editor.Build.Tasks
{
	public class CreateManifestAndSettings : IBuildTask
	{
#pragma warning disable 649
		[InjectContext(ContextUsage.In)]
		private IBuildParameters m_BuildParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationParameters m_ModificationParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationRuntimeSettings m_ModificationSettings;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildContent m_BundleBuildContent;

        [InjectContext]
        IBundleBuildResults m_BundleBuildResults;
#pragma warning restore 649

        public int Version
			=> 1;

		public ReturnCode Run()
		{
			string buildFolderPath = m_BuildParameters.GetOutputFilePathForIdentifier("");

			Debug.Log(m_ModificationParameters.SourcePath);
			//var blueprintPatches =
			//	AssetDatabase.FindAssets($"t:{nameof(BlueprintPatches)}", new[] {m_ModificationParameters.SourcePath})
			//		.Select(AssetDatabase.GUIDToAssetPath)
			//		.Select(AssetDatabase.LoadAssetAtPath<BlueprintPatches>)
			//		.FirstOrDefault();
			//if (blueprintPatches != null)
			//{
			//	m_ModificationSettings.Settings.BlueprintPatches = blueprintPatches.Entries.ToList();
			//}
   //         else
   //             Debug.Log("blueprintPatches is null");

            if (m_BundleBuildResults.BundleInfos.Count == 0)
				goto noDeps;
			
			m_ModificationSettings.Settings.BundleDependencies.BundleToDependencies = m_BundleBuildResults.BundleInfos.ToDictionary(
					key => key.Key.Substring(8), 
					value => value.Value.Dependencies.Select(s => s.Substring(8)).ToList());
            
			noDeps:
			string manifestJsonFilePath = Path.Combine(buildFolderPath, Kingmaker.Modding.OwlcatModification.ManifestFileName);
			string manifestJsonContent = JsonUtility.ToJson(m_ModificationParameters.Manifest, true);
			File.WriteAllText(manifestJsonFilePath, manifestJsonContent);
			
			string settingsJsonFilePath = Path.Combine(buildFolderPath, Kingmaker.Modding.OwlcatModification.SettingsFileName);
			string settingsJsonContent = JsonUtility.ToJson(m_ModificationSettings.Settings, true);
			File.WriteAllText(settingsJsonFilePath, settingsJsonContent);
			
			return ReturnCode.Success;
		}
	}
}