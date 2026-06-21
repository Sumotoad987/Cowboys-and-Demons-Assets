using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShaderWrapper;
#if UNITY_EDITOR
using UnityEditor; 
#endif
using System.Linq;

namespace MyOwlcatModification
{
    public class blahtest : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Bundle/AddScr")]
        static void b()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Kingmaker.SharedTypes.OwlcatModificationMaterialsInBundleAsset>(), "Assets\\Modifications\\Test/MaterialFix.asset");
        }
        [MenuItem("Bundle/Build")]
        static void bu()
        {
            try
            {
                AssetBundleBuild bun = new AssetBundleBuild()
                {
                    assetBundleName = "ArmagSword",
                    assetNames = new string[]
                    {
                        "Assets\\Modifications\\Test/ArmagSword.mat",
                        "Assets\\Modifications\\Test/MaterialFix.asset",
                        "Assets\\Modifications\\Test/WP_SwordBastardOversizedArmag.prefab",
                        "Assets\\Modifications\\Test/WP_SwordBastardOversizedArmag_Armag.png",
                        "Assets\\Modifications\\Test/WP_SwordBastardOversizedArmagScabbard.prefab"
                    },
                    addressableNames = new string[]
                    {
                        null,
                        "Material_Fix",
                        "WP_SwordBastardOversizedArmag",
                        null,
                        "WP_SwordBastardOversizedArmagScabbard.prefab"
                    }
                };
                BuildPipeline.BuildAssetBundles("Assets/AssetBundles", new AssetBundleBuild[] { bun }, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [MenuItem("Bundle/Wrapper")]
        static void script()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ScriptableShaderWrapper>(), "Assets\\Modifications\\Test/ScriptableShaderWrapperInstance.asset");
        }




        [MenuItem("Bundle/Build2")]
        static void bu2()
        {
            try
            {
                AssetBundleBuild bun = new AssetBundleBuild()
                {
                    assetBundleName = "ScriptableShaderWrapper",
                    assetNames = new string[]
                    {
                        "Assets\\Modifications\\Test/ScriptableShaderWrapperInstance.asset",
                    },
                    addressableNames = new string[]
                    {
                        "scriptableshaderwrapperinstance"
                    }
                };
                AssetBundleBuild bun2 = new AssetBundleBuild()
                {
                    assetBundleName = "shaders",
                    assetNames = new string[]
                    {
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Lit/Lit.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Decals/Projector.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Decals/ProjectorGUI.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Particles/Particles.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Terrain/Terrain.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Unlit/Unlit.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Skybox/Skybox-Cubed.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Water/Water.shader",
                       "Assets/RenderPipeline/OwlcatShaders/Shaders/Decals/FullScreen.shader",
                    },
                    addressableNames = new string[]
                    {
                        "Lit",
                        "Projector",
                        "ProjectorGUI",
                        "Particles",
                        "Terrain",
                        "Unlit",
                        "Skybox-Cubed",
                        "Water",
                        "FullScreen",
                    }
                };

                BuildPipeline.BuildAssetBundles("Assets/AssetBundles", new AssetBundleBuild[] {bun2,  bun }, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }



#endif
    }
}
