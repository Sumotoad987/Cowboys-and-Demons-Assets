using Owlcat.Runtime.Visual.RenderPipeline.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Dreamteck;
using static RootMotion.FinalIK.HitReactionVRIK;
using static UnityEngine.LightProbeProxyVolume;
using System.Linq;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(OwlcatTerrain))]
    public class OwlTerrainInspector : UnityEditor.Editor
    {
        private void OnEnable()
        {
            OwlcatTerrain terrain = target as OwlcatTerrain;
            if (terrain == null)
            {
                Debug.Log("null terrain");
                return;
            }
            var splatArray = terrain.DiffuseArray;
            if (splatArray == null)
            {
                Debug.Log("null array");
                return;
            }
            if (splatArray.depth == 0)
            {
                Debug.Log("empty array");
                return;
            }
            if (textures != null)
                return;

            var DiffuseArray = terrain.DiffuseArray;
            var rt = RenderTexture.active;
            textures = new RenderTexture[DiffuseArray.depth];
            for (int i = 0; i < textures.Length; i++)
            {
                var texture = new RenderTexture(DiffuseArray.width, DiffuseArray.height, 1);
                texture.name = i.ToString();
                texture.Create();
                RenderTexture.active = texture;
                Graphics.Blit(DiffuseArray, texture, i, 0);
                textures[i] = texture;
            }
            RenderTexture.active = rt;
        }

        private void OnDisable()
        {
            if (textures == null || textures.Length == 0)
                return;
            foreach (var t in textures)
            {
                t.Release();
                UnityEngine.Object.DestroyImmediate(t);
            }
            textures = null;
        }

        public override void OnInspectorGUI()
        {
            DoAtlases();
            base.OnInspectorGUI();
        }

        static GUIStyle style;

        RenderTexture[] textures= null;
        public void DoAtlases()
        {
            OwlcatTerrain terrain = target as OwlcatTerrain;
            if (terrain == null)
            {
                Debug.Log("null terrain");
                return;
            }
            var DiffuseArray = terrain.DiffuseArray;
            if (DiffuseArray == null)
            {
                Debug.Log("null array");
                return;
            }
            if (DiffuseArray.depth == 0)
            {
                Debug.Log("empty array");
                return;
            }
            if (style is null)
                style = new GUIStyle("GridList");
            int approxSize = 64;
            int selected = -1; 
            GUILayout.BeginVertical("box", new GUILayoutOption[]
            {
                GUILayout.MinHeight(approxSize)
            });
            int result = 0;
            //bool doubleClick = false;
           
           int num = (int)(EditorGUIUtility.currentViewWidth - 150f) / approxSize;
           int num2 = (int)Mathf.Ceil((DiffuseArray.depth + num - 1) / num);
           Rect aspectRect = GUILayoutUtility.GetAspectRect(num / num2);
           Event current = Event.current;

           // bool flag2 = current.type == EventType.MouseDown && current.clickCount == 2 && aspectRect.Contains(current.mousePosition);
           //if (flag2)
           //{
           //    //doubleClick = true;
           //    current.Use();
           //}
           result = GUI.SelectionGrid (aspectRect, Math.Min(selected, DiffuseArray.depth - 1), textures.Select(t => new GUIContent(t.name, t)).ToArray(), num, style);
           
            GUILayout.EndVertical();
        }
    }
}
