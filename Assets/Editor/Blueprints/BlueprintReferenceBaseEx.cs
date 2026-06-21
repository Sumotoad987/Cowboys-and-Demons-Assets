using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Ex.Kingmaker.Blueprints
{
    internal static class BlueprintReferenceBaseEx
    {
        public static void SetPropertyValue(SerializedProperty p, BlueprintScriptableObject bp)
        {
            p.FindPropertyRelative("guid").stringValue = bp != null ? bp.AssetGuid.ToString() : "";
        }
        public static IEnumerable<T> Dereference<T>(this IEnumerable<BlueprintReference<T>> list) where T : SimpleBlueprint
        {
            return list.Select(delegate (BlueprintReference<T> r)
            {
                if (r == null)
                {
                    return default(T);
                }
                return r.Get();
            });
        }
    }
}
