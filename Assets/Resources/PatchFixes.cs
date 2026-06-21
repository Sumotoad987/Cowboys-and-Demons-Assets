using HarmonyLib;
using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using System.Reflection;
using Unity.Jobs;
using UnityEngine;

namespace OwlcatModification.Assets.Editor
{
    public static class PatchFixes
    {
        private static FieldInfo _jobFieldInfo;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _jobFieldInfo = AccessTools.Field(typeof(ClusteredLights), "m_SetupJobsHandle");

            var harmony = new Harmony("com.hambeard.PatchFixes");
            harmony.PatchAll();
        }

        [HarmonyPatch]
        public static class ClusteredLightsPatch
        {
            private static void Complete(ClusteredLights instance)
            {
                var jobHandleValue = _jobFieldInfo.GetValue(instance);

                if (jobHandleValue != null)
                    ((JobHandle)jobHandleValue).Complete();
            }

            [HarmonyPatch(typeof(ClusteredLights), "InitializeSingleThreadedParameters")]
            [HarmonyPrefix]
            public static void PatchISTP(ClusteredLights __instance) => Complete(__instance);

            [HarmonyPatch(typeof(ClusteredLights), nameof(ClusteredLights.Dispose))]
            [HarmonyPrefix]
            public static void PatchDispose(ClusteredLights __instance) => Complete(__instance);
        }
    }
}
