using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using HarmonyLib;
using System.Linq;
using Owlcat.QA.Validation;
using System.Threading;
using System;
//using Owlcat.Runtime.Visual.RenderPipeline.Terrain;

namespace MyOwlcatModification
{
    public class test : ScriptableObject
    {
        [MenuItem("Examples/Terrain")]
        static void Test2()
        {
            Debug.Log("Trying test2");
            Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.Log("Null terrain");
                return;
            }
            // Get the pixel data from the texture
            var t = terrain.terrainData.alphamapTextures[0];
            Debug.Log($"test2 width and height are {t.width} and {t.height}");
            Color[] pixels = t.GetPixels();

            // Create a Vector4 array to store the converted pixel data
            Vector4[] pixelData = new Vector4[pixels.Length];

            // Convert each pixel into a Vector4 value
            for (int i = 0; i < pixels.Length; i++)
            {
                pixelData[i] = new Vector4(pixels[i].r, pixels[i].g, pixels[i].b, pixels[i].a);
            }

            Texture2DArray a = new Texture2DArray(t.width, t.height, 10, TextureFormat.RGBA32, true);
            a.SetPixels(pixels, 0);

            var m_MaterialPropertyBlock = new MaterialPropertyBlock();
            m_MaterialPropertyBlock.SetTexture(Shader.PropertyToID("_SplatArray"), a);
            terrain.SetSplatMaterialPropertyBlock(m_MaterialPropertyBlock);
        }

    }
}
