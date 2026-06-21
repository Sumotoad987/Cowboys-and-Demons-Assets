using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using UnityEditor;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase
{
    internal class BlueprintDatabaseEx
    {
        private static string FullPathPrefix = Directory.GetCurrentDirectory() + "\\";
        public static List<Tuple<string, string>> SearchByFolderEx (string path)
        {
            IEnumerable<string> enumerable = Directory.EnumerateFiles(path, "*.jbp", SearchOption.AllDirectories);
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            foreach (string text in enumerable)
            {
                string text1 = Path.IsPathRooted(text) ? FullToRelativePath(text) : text;
                list.Add(new Tuple<string, string>(AssetDatabase.LoadAssetAtPath<Ex.Kingmaker.Blueprints.SimpleBlueprint>( text1).AssetGuid.ToString(), text1));
            }
            return list;
        }

        public static string FullToRelativePath(string fullPath)
        {
            if (!Path.IsPathRooted(fullPath))
            {
                PFLog.Default.Warning(fullPath + " is not actually full");
                return fullPath;
            }
            return fullPath.Substring(FullPathPrefix.Length);
        }

    }
}
