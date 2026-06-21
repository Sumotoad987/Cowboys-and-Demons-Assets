using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyOwlcatModification
{
    public class ProjectFilePostprocessor : UnityEditor.AssetPostprocessor
    {
        public static string OnGeneratedSlnSolution(string path, string content)
        {
            return content;
        }

        public static string OnGeneratedCSProject(string path, string content)
        {
            //content = content.Replace("DebugType>full</DebugType>", "DebugType>portable</DebugType>");
            //content = content.Replace("<LangVersion>8.0</LangVersion>", "<LangVersion>latest</LangVersion>");

            return content;
        }
    }
}
