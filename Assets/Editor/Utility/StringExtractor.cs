using Kingmaker.Blueprints.JsonSystem;
using
#if UNITY_EDITOR
            Ex. 
#endif
    Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathfinding.Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using static UnityEngine.Debug;
using Kingmaker.Utility;

namespace WotRStringExtarctor
{
    internal class WotRStringExtractor
    {
        public static List<Tuple<LocalizedStringExtended, LocalizedStringData>> m_entries = new List<Tuple<LocalizedStringExtended, LocalizedStringData>>();

        //[MenuItem("Examples/Strings")]
        static void Main()
        {
            string wotr = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Pathfinder Second Adventure";
            string blueprintsPath = Path.Combine(wotr, "blueprints.zip");
            if (!File.Exists(blueprintsPath))
            {
                LogError($"Did not find blueprints.zip at {blueprintsPath}");
                //Console.Write($"Did not find blueprints.zip at {blueprintsPath}");
                //WaitForResponse();

                return;
            }

            _ = Kingmaker.Cheats.Utilities.GetAllBlueprints();
            if (Kingmaker.Cheats.Utilities.s_BlueprintMap == null || Kingmaker.Cheats.Utilities.s_BlueprintMap.Count < 0)
            {
                LogError("Empty BlueprintsMap!");
                return;
            }

            GuidClassBinder guidClassBinder = (GuidClassBinder)Json.Serializer.Binder;
            foreach (Type type in Assembly.GetAssembly(typeof(TypeIdAttribute)).GetTypes())
            {
                TypeIdAttribute customAttribute = type.GetCustomAttribute<TypeIdAttribute>();
                if (customAttribute != null)
                {
                    guidClassBinder.AddToCache(type, customAttribute.GuidString);
                }
            }
            if (!Json.Serializer.Converters.Any(c => c is LocalizedStringConverter))
                Json.Serializer.Converters.Add(new LocalizedStringConverter());

            if (!Kingmaker.Localization.LocalizationManager.Initialized)
                Kingmaker.Localization.LocalizationManager.Init();
            var conv = Json.Serializer.Converters;
            try
            {
                Ex.Kingmaker.Blueprints.JsonSystem.BlueprintJsonWrapper blueprintJsonWrapper;
                var zipArchive = ZipFile.Read(blueprintsPath);
                var entries = zipArchive.Entries;

                int i = 0;
                foreach (var zipEntry in entries.Where(e => !e.IsDirectory && e.FileName.EndsWith(".jbp")))
                {
                    EditorUtility.DisplayProgressBar("Building bp addresses cache", $"Processed {i} entries out of {entries.Count}", (float)(i / entries.Count));
                    try
                    {
                        using (var memoryStream = zipEntry.OpenReader())
                        using (StreamReader streamReader = new StreamReader(memoryStream))
                        using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                            blueprintJsonWrapper = Json.Serializer.Deserialize<Ex.Kingmaker.Blueprints.JsonSystem.BlueprintJsonWrapper>(jsonTextReader);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        EditorUtility.ClearProgressBar();
                        while (ex.InnerException != null)
                            ex = ex.InnerException;

                        LogException(ex);
                        continue;
                    }
                }

                foreach (var locale in LocaleExtensions.GameLanguageValues)
                {
                    Kingmaker.Localization.LocalizationManager.CurrentLocale = locale;
                    Kingmaker.Localization.LocalizationManager.CurrentPack = Kingmaker.Localization.LocalizationManager.LoadPack(locale);
                    try
                    {
                        i = 0;
                        foreach (var data in m_entries)
                        {
                            EditorUtility.DisplayProgressBar("Building bp addresses cache", $"Loading strings from locale {locale}. \n" +
                                $"Processed {i} entries out of {m_entries.Count}", (float)(i / entries.Count));
                            if (Kingmaker.Localization.LocalizationManager.CurrentPack.m_Strings.ContainsKey(data.Item1.Key))
                                data.Item2.Languages.Add(
                                            new LocaleData(Kingmaker.Localization.LocalizationManager.CurrentLocale)
                                            {
                                                Text = data.Item1.ToString()
                                            });
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        EditorUtility.ClearProgressBar();
                        continue;
                    }

                }



                JsonSerializerSettings serialSettings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    Converters = conv
                };
                JsonSerializer serializer = JsonSerializer.Create(serialSettings);
                i = 0;
                foreach (var entry in m_entries.NotNull())
                {
                    EditorUtility.DisplayProgressBar("Building bp strings folder", $"Serializing strings. \n" +
                        $"Processed {i} strings out of {m_entries.Count}", (float)(i / m_entries.Count));
                    try
                    {
                        var localizedString = entry.Item1;
                        if (localizedString == null)
                        {
                            LogError("Null String!");
                            i++;
                            continue;
                        }

                        if (localizedString.m_JsonPath.IsNullOrEmpty())
                        {
                            LogError($"Null m_JsonPath for string {localizedString.m_Key ?? "Null key!!"}!");
                            i++;
                            continue;
                        }

                        var path = localizedString.m_JsonPath.Split('/');
                        if (path.Length < 2)
                        {
                            LogError($"m_JsonPath for string {localizedString.m_Key ?? "Null key"} is too short!");
                            i++;
                            continue;
                        }
                        if (path[0] != "Strings")
                        {
                            LogError($"m_JsonPath for string {localizedString.m_Key ?? "Null key"} is not in Strings!");
                            i++;
                            continue;
                        }
                        if (!localizedString.m_JsonPath.EndsWith("json"))
                        {
                            LogError($"m_JsonPath for string {localizedString.m_Key ?? "Null key"} is {localizedString.m_JsonPath} and doesn't end with 'json'!");
                            i++;
                            continue;
                        }

                        var pathRooted = Path.Combine("E:/", localizedString.m_JsonPath);
                        var DirectoryName = Path.GetDirectoryName(pathRooted);
                        if (!Directory.Exists(DirectoryName))
                            Directory.CreateDirectory(DirectoryName);
                        if (!File.Exists(pathRooted))
                            File.Create(pathRooted).Close();
                        //using (var fileStream = File.Open(pathRooted, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        using (var streamReader = new StreamWriter(pathRooted, false))
                        using (var jsonTextReader = new JsonTextWriter(streamReader))
                            serializer.Serialize(jsonTextReader, entry.Item2);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        i++;
                        continue;
                    }
                }

                EditorUtility.ClearProgressBar();
                return;

                //try
                //{
                //    using (var writer = new JsonTextWriter(new StreamWriter(filePath)) { Formatting = Formatting.Indented })
                //        JsonSerializer.CreateDefault().Serialize(writer, m_entries);
                //}
                //catch (Exception)
                //{
                //    Debug.LogError("error when trying to serialize the dictionary");
                //}

            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                LogException(ex);
                return;
            }
        }

        //static void WaitForResponse()
        //{
        //    while (Console.Read() != 'q') { };
        //}
    }

    internal class LocalizedStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(LocalizedString) && objectType != typeof(LocalizedStringExtended));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var localizedString = new LocalizedStringExtended();
            var jObject = JObject.Load(reader);
            var m_Key = (string)jObject["m_Key"];
            var m_OwnerString = (string)jObject["m_OwnerString"];
            var m_OwnerPropertyPath = (string)jObject["m_OwnerPropertyPath"];
            var m_JsonPath = (string)jObject["m_JsonPath"];
            if (m_Key != null)
                localizedString.Key = m_Key;
            if (m_OwnerString != null)
                localizedString.m_OwnerString = m_OwnerString;
            if (m_OwnerPropertyPath != null)
                localizedString.m_OwnerPropertyPath = m_OwnerPropertyPath;
            if (m_JsonPath != null)
                localizedString.m_JsonPath = m_JsonPath;

            if (jObject[nameof(LocalizedString.Shared)].Type == JTokenType.Null
                && !m_JsonPath.IsNullOrEmpty())
            {

                var data = new LocalizedStringData()
                {
                    Key = localizedString.Key,
                    OwnerGuid = localizedString.m_OwnerString,
                };

                if (!localizedString.m_OwnerString.IsNullOrEmpty())
                {
                    string key = "";
                    if (Guid.TryParse(localizedString.m_OwnerString, out var parseResult))
                        key = parseResult.ToString("N");
                    else
                    {
                        LogWarning($"Failed to parse owner guid {localizedString.m_OwnerString} for string {localizedString.Key}");
                    }
                    if (Kingmaker.Cheats.Utilities.s_BlueprintMap.TryGetValue(key, out var mapEntry))
                    {
                        Kingmaker.DialogSystem.Blueprints.BlueprintCue bp = null ;
                        if (mapEntry != null)
                        {
                            bool flag = mapEntry.Type.IsOrSubclassOf<Ex.Kingmaker.DialogSystem.Blueprints.BlueprintCue>();
                            if (flag)
                                bp = Kingmaker.Cheats.Utilities.GetBlueprintByGuid<Kingmaker.DialogSystem.Blueprints.BlueprintCue>(localizedString.m_OwnerString);
                        }
                        if (bp != null && !bp.Speaker.NoSpeaker && bp.Speaker.Blueprint != null)
                        {
                            var speaker = bp.Speaker.Blueprint;
                            data.Speaker = speaker.CharacterName.ToString();
                            data.SpeakerGender = speaker.Gender.ToString();
                        }
                    }
                    else
                    {
                        LogWarning($"Failed to find an entry in the blueprintsMap for guid {key}");
                    }
                }
                WotRStringExtractor.m_entries.Add(new Tuple<LocalizedStringExtended, LocalizedStringData>(localizedString, data));
            }
            return localizedString;
        }
    }

    internal class LocalizedStringExtended : LocalizedString
    {
        //public string m_OwnerString;
        //public string m_OwnerPropertyPath;
        //public string m_JsonPath;
        //public LocalizedStringData Data;
    }
}
