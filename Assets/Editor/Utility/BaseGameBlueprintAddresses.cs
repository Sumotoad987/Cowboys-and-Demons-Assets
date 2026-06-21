using Kingmaker.BundlesLoading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
//using System.IO.Compression;
using Pathfinding.Ionic.Zip;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MyOwlcatModification.Assets.Editor.Utility
{
    static public class BaseGameBlueprintAddresses
    {
        static readonly string filePath = Path.Combine("Assets", "Editor", "Utility", "BaseGameBlueprintAddresses.json");
        //static readonly string blueprintsPath = Path.Combine(EditorPrefs.GetString("wotr_directory"), "blueprints.zip");
        //static readonly MethodInfo OpenInReadMode = typeof(ZipArchiveEntry).GetMethod("OpenInReadMode", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly object[] args = new object[] { false };


        static private Dictionary<string, string> m_entries;

        static public Dictionary<string, string> Entries
        {
            get
            {
                string wotr = EditorPrefs.GetString("wotr_directory");
                string blueprintsPath = Path.Combine(wotr, "blueprints.zip");
                if (!File.Exists(blueprintsPath))
                {
                    Debug.LogWarning($"Did not find blueprints.zip at {blueprintsPath}");
                    return m_entries;
                }
                try
                {
                    if (!File.Exists(filePath) || File.GetCreationTime(filePath) < File.GetCreationTime(blueprintsPath))
                    {

                        //using (var zipStream = File.Open(blueprintsPath, FileMode.Open, FileAccess.Read))
                        {
                        var zipArchive = ZipFile.Read(blueprintsPath);
                        m_entries = new Dictionary<string, string>();
                        var entries = zipArchive.Entries;
                        Debug.Log($"There are {entries.Count} entries");
                        int i = 0;
                        foreach (var zipEntry in entries.Where(e => !e.IsDirectory && e.FileName.EndsWith(".jbp")))
                        {
                            EditorUtility.DisplayProgressBar("Building bp addresses cache", $"Processed {i} entries out of {entries.Count}", (float)(i/entries.Count));
                            try
                            {
                                using (var memoryStream = zipEntry.OpenReader())
                                    using (StreamReader streamReader = new StreamReader(memoryStream))
                                    {
                                        streamReader.ReadLine();

                                        var b = streamReader.ReadLine();
                                        var semicolonIndex = b.IndexOf(':', 7);
                                        var guid = b.Substring(semicolonIndex +1).TrimStart(' ').Substring(1, 32);
                                        m_entries[guid] = zipEntry.FileName;
                                    }
                                i++;
                            }
                            catch (Exception)
                            {
                                Debug.LogError($"Error when reading file {zipEntry.FileName}");
                                    EditorUtility.ClearProgressBar();
                            }
                        }
                            EditorUtility.ClearProgressBar();
                            var a = m_entries;
                                

                        }
                        try
                        {
                            using (var writer = new JsonTextWriter(new StreamWriter(filePath)) { Formatting = Formatting.Indented})
                                JsonSerializer.CreateDefault().Serialize(writer, m_entries);
                        }
                        catch (Exception)
                        {
                            Debug.LogError("error when trying to serialize the dictionary");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return null;
                }
                if (m_entries == null)
                {
                    using (var reader = new JsonTextReader(new StreamReader(filePath)))
                        m_entries = JsonSerializer.CreateDefault().Deserialize<Dictionary<string, string>>(reader);
                }
                return m_entries;
            }
        }

        //[MenuItem("Examples/Cache")]
        static void Do()
        {
            try
            {
                Entries.Any();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
