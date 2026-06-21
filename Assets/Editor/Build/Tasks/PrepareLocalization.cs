using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Blueprints.JsonSystem;
using MyOwlcatModification.Editor.Build.Context;
using Newtonsoft.Json;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using Kingmaker.Utility;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

namespace MyOwlcatModification.Editor.Build.Tasks
{
	public class PrepareLocalization : IBuildTask
	{
#pragma warning disable 649
		[InjectContext(ContextUsage.In)]
		private IBuildParameters m_BuildParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationParameters m_ModificationParameters;
#pragma warning restore 649
		
		public int Version
			=> 1;
		
		public ReturnCode Run()
		{
			//string[] localeFiles = Enum.GetNames(typeof(Locale)).Select(i => i + ".json").ToArray();
			string originDirectory = m_ModificationParameters.LocalizationPath;
			string destinationDirectory = m_BuildParameters.GetOutputFilePathForIdentifier(BuilderConsts.OutputLocalization);

			string stringsPath = m_ModificationParameters.SourcePath.Replace("Assets", "Assets/../Strings");
			var files = Directory.EnumerateFiles(stringsPath, "*.LocalizedString.json", SearchOption.AllDirectories);
			var ReadFiles = ReadStrings(files).ToArray(); 
			if (files.Count() < 1)
				return ReturnCode.SuccessNotRun;
			if (!Directory.Exists(destinationDirectory))
				Directory.CreateDirectory(destinationDirectory);
			DefaultJsonSettings.Initialize();
			var serializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);
			serializer.Formatting = Formatting.Indented;
			serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
			foreach(var locale in Enum.GetValues(typeof(Locale)).Cast<Locale>().Where(loc => loc != Locale.Sound))
			{
				var pack = new LocalizationPack() 
				{ 
					Locale = locale, 
					m_Strings = ReadFiles.ToDictionary(data => data.Key, data => new LocalizationPack.StringEntry() { Text = GetRealText(data, locale) }) 
				};
				using var writer = File.CreateText(Path.Combine(destinationDirectory, $"{locale}.json"));
				using var jsonWriter = new JsonTextWriter(writer);
                serializer.Serialize (jsonWriter, pack);
			}


			//BuilderUtils.CopyFilesWithFoldersStructure(
			//	originDirectory, destinationDirectory, i =>
			//	{
			//		string filename = Path.GetFileName(i);
			//		return localeFiles.Contains(filename);
			//	});
			
			return ReturnCode.Success;

			string GetRealText(LocalizedStringData data, Locale locale)
			{
				var text = data.GetText(locale);
				if (locale != Locale.enGB && text.IsNullOrEmpty())
				{
					text = data.GetText(Locale.enGB);
					if (text.IsNullOrEmpty())
						text = data.GetText(Locale.ruRU);
				}
				return text;
            }
		}

		static IEnumerable<LocalizedStringData> ReadStrings(IEnumerable<string> filePaths)
		{
			foreach (var path in filePaths)
			{
				LocalizedStringData loc = null;
				try
				{
					using (var stream = File.OpenRead(path))
					{
						using (var reader = new StreamReader(stream))
						{
                            using (var jsonReader = new JsonTextReader(reader))
                            {
								loc = Json.Serializer.Deserialize<LocalizedStringData>(jsonReader);
                            }
                        }
                    }
				}
				catch(Exception ex)
				{
					Debug.Log("Exception when reading a localized string " + path);
					Debug.LogException(ex);
				}
				if (loc != null)
					yield return loc;
			}
		}

	}
}