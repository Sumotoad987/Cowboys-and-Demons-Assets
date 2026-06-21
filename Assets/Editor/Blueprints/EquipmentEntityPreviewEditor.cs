using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using editor = UnityEditor;
using System;
using HarmonyLib;
using System.Reflection;
using Kingmaker.Utility;
using System.Linq;
using Ex.Kingmaker.Visual.CharacterSystem;

namespace MyOwlcatModification
{
    [editor.CustomEditor(typeof(EquipmentEntity), true)]
    //[editor.CanEditMultipleObjects]
    public class EquipmentEntityPreviewEditor : editor.Editor
    {
        private EquipmentEntityPreviewEditor.PreviewData previewData;
        private RenderTexture cachedTexture;
        private Vector2 m_PreviewDir;
        private Rect m_PreviewRect;
        //private editor.SerializedProperty m_Name;

        int PrimaryColor;
        int SecondaryColor;
        int SpecialPrimaryColor;
        int SpecialSecondaryColor;
        bool UseSpecialColors;
        RampColorPreset.IndexSet colorPreset = null;
        editor.GenericMenu colorPresetMenu = null;
        GUIContent noColorPresetContent = new GUIContent("No Color Presets");

        public void OnEnable()
        {
            m_PreviewDir = (editor.EditorSettings.defaultBehaviorMode == editor.EditorBehaviorMode.Mode2D) ? new Vector2(0f, 0f) : new Vector2(120f, -20f);
            //m_Name = serializedObject.FindProperty("m_Name");
        }
        internal void OnDisable()
        {
            previewData?.Dispose();
            if (cachedTexture != null)
                UnityEngine.Object.DestroyImmediate(cachedTexture);
            colorPreset = null;
            colorPresetMenu = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            bool flag = !editor.EditorUtility.IsPersistent(target);
            return !flag && HasStaticPreview();
        }
        private bool HasStaticPreview()
        {
            return true;
        }
        private EquipmentEntityPreviewEditor.PreviewData GetPreviewData()
        {
            if (previewData == null)
            {
                previewData = new EquipmentEntityPreviewEditor.PreviewData(ienum(target as EquipmentEntity));
                previewData.SetUpObjects();
            }
            if (!previewData.gameObjects.Any())
                ReloadPreviewInstances();            
            return previewData;

            IEnumerable<EquipmentEntity> ienum(EquipmentEntity EE)
            {
                yield return EE;
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            bool flag = !HasStaticPreview() || !editor.ShaderUtil.hardwareSupportsRectRenderTexture;
            Texture2D result;
            if (flag)
            {
                result = null;
            }
            else
            {
                editor.PreviewRenderUtility renderUtility = GetPreviewData().renderUtility;
                renderUtility.BeginStaticPreview(new Rect(0f, 0f, (float)width, (float)height));
                DoRenderPreview();
                result = renderUtility.EndStaticPreview();
            }
            return result;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            bool flag = !editor.ShaderUtil.hardwareSupportsRectRenderTexture;
            if (flag)
            {
                bool flag2 = Event.current.type == EventType.Repaint;
                if (flag2)
                {
                    editor.EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "Preview requires\nrender texture support");
                }
            }
            else
            {
                Vector2 vector = Drag2D(m_PreviewDir, r);
                bool flag3 = vector != m_PreviewDir;
                if (flag3)
                {
                    ClearPreviewCache();
                    m_PreviewDir = vector;
                }
                bool flag4 = Event.current.type != EventType.Repaint;
                if (!flag4)
                {
                    bool flag5 = this.m_PreviewRect != r;
                    if (flag5)
                    {
                        ClearPreviewCache();
                        m_PreviewRect = r;
                    }
                    var renderUtility = GetPreviewData().renderUtility;
                    if (!previewData?.EnsurePainting() ?? true)
                    {
                        ClearPreviewCache();
                    }
                    if (cachedTexture == null)
                    {
                        renderUtility.BeginPreview(r, background);
                        DoRenderPreview();
                        renderUtility.EndAndDrawPreview(r);
                        cachedTexture = new RenderTexture(PreviewData.PreviewRenderUtilityRenderTexture.Invoke(renderUtility));
                        RenderTexture active = RenderTexture.active;
                        Graphics.Blit(PreviewData.PreviewRenderUtilityRenderTexture.Invoke(renderUtility), (RenderTexture)cachedTexture);
                        RenderTexture.active = active;
                    }
                    GUI.DrawTexture(r, cachedTexture, ScaleMode.StretchToFill, false);
                }
            }
        }
        public override void OnPreviewSettings()
        {
            if (GUILayout.Button(UseSpecialColors ? "Myth" : "Norm"))
            {
                UseSpecialColors = !UseSpecialColors;
                if (previewData != null)
                {
                    previewData.SetColors(PrimaryColor, SecondaryColor, SpecialPrimaryColor, SpecialSecondaryColor, UseSpecialColors);
                }
            }

            string colorPresetName = colorPreset != null ? colorPreset.Name : "Color Preset";
            if (!(target is EquipmentEntity EE) || !EE.ColorPresets)
            {
                GUILayout.Button(noColorPresetContent);
                return;
            }
            if (colorPresetMenu == null)
            { 
                colorPresetMenu = new editor.GenericMenu();
                foreach (var preset in EE.ColorPresets.IndexPairs)            
                    colorPresetMenu.AddItem(new GUIContent(preset.Name), 
                        colorPreset == preset, 
                        f  => 
                        {
                            //GUIUtility.keyboardControl = GUIUtility.hotControl = -1;
                            RampColorPreset.IndexSet a = (RampColorPreset.IndexSet)f;
                            if (colorPreset != a && previewData != null)
                            {
                                previewData.SetColors(a.PrimaryIndex, a.SecondaryIndex, a.PrimaryIndex, a.SecondaryIndex, false);
                            }
                            colorPreset = a ; 
                        },
                        preset);
            }
            if (editor.EditorGUILayout.DropdownButton(new GUIContent() { text = colorPresetName }, FocusType.Passive))
                colorPresetMenu.ShowAsContext();
        }

        private void ClearPreviewCache()
        {
            cachedTexture = null;
        }
        private void DoRenderPreview()
        {
            EquipmentEntityPreviewEditor.PreviewData previewData = GetPreviewData();
            Bounds renderableBounds = previewData.renderableBounds;
            float num = Mathf.Max(renderableBounds.extents.magnitude, 0.0001f);
            float num2 = num * 3.8f;
            Quaternion quaternion = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
            Vector3 position = renderableBounds.center - quaternion * (Vector3.forward * num2);
            previewData.renderUtility.camera.transform.position = position;
            previewData.renderUtility.camera.transform.rotation = quaternion;
            previewData.renderUtility.camera.nearClipPlane = num2 - num * 1.1f;
            previewData.renderUtility.camera.farClipPlane = num2 + num * 1.1f;
            previewData.renderUtility.lights[0].intensity = 1.6f;
            previewData.renderUtility.lights[0].transform.rotation = quaternion * Quaternion.Euler(40f, 40f, 0f);
            previewData.renderUtility.lights[1].intensity = 1.6f;
            previewData.renderUtility.lights[1].transform.rotation = quaternion * Quaternion.Euler(340f, 218f, 177f);
            previewData.renderUtility.ambientColor = new Color(0.1f, 0.1f, 0.1f, 0f);
            previewData.renderUtility.Render(true, true);
        }

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    {
                        bool flag = position.Contains(current.mousePosition) && position.width > 50f;
                        if (flag)
                        {
                            GUIUtility.hotControl = controlID;
                            current.Use();
                            editor.EditorGUIUtility.SetWantsMouseJumping(1);
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        bool flag2 = GUIUtility.hotControl == controlID;
                        if (flag2)
                        {
                            GUIUtility.hotControl = 0;
                        }
                        editor.EditorGUIUtility.SetWantsMouseJumping(0);
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        bool flag3 = GUIUtility.hotControl == controlID;
                        if (flag3)
                        {
                            scrollPosition -= current.delta * (float)(current.shift ? 3 : 1) / Mathf.Min(position.width, position.height) * 140f;
                            current.Use();
                            GUI.changed = true;
                        }
                        break;
                    }
            }
            return scrollPosition;
        }

        internal class PreviewData : IDisposable
        {
            public IEnumerable<EquipmentEntity> EEs { get; protected set; }
            public Dictionary<Kingmaker.Visual.CharacterSystem.BodyPartType, Tuple<BodyPart, GameObject>> gameObjects =
                new Dictionary<Kingmaker.Visual.CharacterSystem.BodyPartType, Tuple<BodyPart, GameObject>>();
            public Dictionary<(int PrimaryColor, int SecondaryColor, bool UseSpecial), RenderTexture> TexturesCache = 
                new Dictionary<(int PrimaryColor, int SecondaryColor, bool UseSpecial), RenderTexture>();
            protected Material material { get; set; }

            protected int PrimaryColor;
            protected int SecondaryColor;
            protected int SpecialPrimaryColor;
            protected int SpecialSecondaryColor;
            protected bool UseSpecial;

            public bool NeedRepaint;

            public Bounds renderableBounds { get; protected set; }

            public PreviewData(IEnumerable<EquipmentEntity> targetObjects, RampColorPreset.IndexSet indexSet = null, bool useSpecial = false)
            {
                renderUtility = new editor.PreviewRenderUtility();
                renderUtility.camera.fieldOfView = 30f;
                if (indexSet != null)
                {
                    PrimaryColor = indexSet.PrimaryIndex;
                    SecondaryColor = indexSet.SecondaryIndex;
                    SpecialPrimaryColor = indexSet.PrimaryIndex;
                    SpecialSecondaryColor= indexSet.SecondaryIndex;
                    UseSpecial = useSpecial;
                }
                EEs = targetObjects.NotNull();
            }

            public virtual void SetUpObjects()
            {
                foreach (var gameObject in gameObjects.Values.Select(tuple => tuple.Item2))
                    DestroyImmediate(gameObject);
                gameObjects.Clear();
                foreach(var EE in EEs)
                    foreach (var tuple in ModAssetImporter.InstantiateAndColorBodyParts(PrimaryColor, SecondaryColor, SpecialPrimaryColor, SpecialSecondaryColor, EE))
                        gameObjects[tuple.Item1.Type] = tuple;
                if (gameObjects.Count < 1)
                    return;
                material = gameObjects.Values.First().Item2.GetComponentInChildren<Renderer>().sharedMaterial;
                var o = gameObjects.Values.Select(tuple => tuple.Item2);
                foreach (var gameObject in gameObjects.Values.Select(tuple => tuple.Item2))
                {
                    renderUtility.AddSingleGO(gameObject);
                }
                renderableBounds = GetRenderableBounds(o.SelectMany(ob => ob.GetComponentsInChildren<Renderer>()).NotNull());

            }

            public void SetColors (int primaryColor, 
            int secondaryColor, 
            int specialPrimaryColor, 
            int specialSecondaryColor,
            bool useSpecial)
            {
                PrimaryColor = primaryColor;
                SecondaryColor = secondaryColor; 
                SpecialPrimaryColor = specialPrimaryColor; 
                SpecialSecondaryColor = specialSecondaryColor; 
                UseSpecial= useSpecial;
                NeedRepaint = true;
            }

            public bool EnsurePainting()
            {
                if (!gameObjects.Any() || !NeedRepaint)
                    return true;
                if (TexturesCache.TryGetValue((PrimaryColor, SecondaryColor, UseSpecial), out var cachedTexture))
                {
                    material.mainTexture = cachedTexture;
                    NeedRepaint = false;
                    return false;
                }
                var sortedTextures = gameObjects.Select(go => 
                new Tuple<Kingmaker.Visual.CharacterSystem.BodyPartType, IEnumerable<CharacterTextureDescription>, Texture2D, Texture2D, Material>(
                    go.Key,
                    go.Value.Item1.Textures,
                    EvaluatePrimaryRamp(),
                    EvaluateSecondaryRamp(),
                    go.Value.Item2.GetComponentInChildren<Renderer>().sharedMaterial));

                var t = ModAssetImporter.buildTextureFromDescriptions(sortedTextures, Kingmaker.Visual.CharacterSystem.CharacterTextureChannel.Diffuse);
                material.mainTexture = t;
                TexturesCache.Add((PrimaryColor, SecondaryColor, UseSpecial), t);
                NeedRepaint = false;
                return false;

                Texture2D EvaluatePrimaryRamp()
                {
                    var EE = EEs.First();
                    Texture2D primaryRamp = null;
                    {
                        if (UseSpecial && SpecialPrimaryColor > -1 && SpecialPrimaryColor < EE.SpecialPrimaryRamps.Count)
                        {
                            primaryRamp = EE.SpecialPrimaryRamps[SpecialPrimaryColor];
                        }

                        else
                        if (PrimaryColor < EE.PrimaryRamps.Count)
                        {
                            primaryRamp = EE.PrimaryRamps[PrimaryColor];
                        }

                    }
                    return primaryRamp;
                }
                Texture2D EvaluateSecondaryRamp()
                {

                    var EE = EEs.First();
                    Texture2D secondaryRamp = null;
                    {
                        if (UseSpecial && SpecialSecondaryColor > -1 && SpecialSecondaryColor < EE.SpecialPrimaryRamps.Count)
                        {
                            secondaryRamp = EE.SpecialPrimaryRamps[SpecialSecondaryColor];
                        }

                        else
                        if (SecondaryColor < EE.PrimaryRamps.Count)
                        {
                            secondaryRamp = EE.PrimaryRamps[SecondaryColor];
                        }

                    }
                    return secondaryRamp;
                }

            }

            public void Dispose()
            {
                bool disposed = m_Disposed;
                if (!disposed)
                {
                    renderUtility.Cleanup();
                    UnityEngine.Object.DestroyImmediate(material);
                    foreach (var gameObject in gameObjects.Values.Select(tuple => tuple.Item2))
                        UnityEngine.Object.DestroyImmediate(gameObject);
                    EEs = null;
                    m_Disposed = true;
                }
            }

            public Bounds GetRenderableBounds (IEnumerable<Renderer> renderers)
            {
                if (!renderers.Any())
                    return new Bounds(Vector3.zero, Vector3.zero);

                float width_min = float.PositiveInfinity;
                float width_max = float.NegativeInfinity;

                float height_min = float.PositiveInfinity;
                float height_max = float.NegativeInfinity;

                float depth_min = float.PositiveInfinity;
                float depth_max = float.NegativeInfinity;


                foreach (var renderer in renderers)
                {
                    var center = renderer.bounds.center;
                    var extents = renderer.bounds.extents;
                    float left = center.x - extents.x;
                    float right = center.x + extents.x;
                    float bottom = center.y - extents.y;
                    float top = center.y + extents.y;
                    float back = center.z - extents.z;
                    float forward = center.z + extents.z;
                    if (left < width_min) width_min = left;
                    if (right > width_max) width_max = right;
                    if (bottom < height_min) height_min = bottom;
                    if (top > height_max) height_max = top;
                    if (back < depth_min) depth_min = back;
                    if (forward > depth_max) depth_max = forward;
                }

                Vector3 newExtends = new Vector3((width_max - width_min) / 2, (height_max - height_min) / 2, (depth_max - depth_min) / 2);

                return new Bounds(new Vector3(width_max - newExtends.x, height_max - newExtends.y, depth_max - newExtends.z), newExtends * 2);
            }

            private bool m_Disposed;
            public readonly editor.PreviewRenderUtility renderUtility;

            public static Func<UnityEngine.Object, GameObject> InstantiateForAnimatorPreview
            {
                get
                {
                    if (m_InstantiateForAnimatorPreview is null)
                        m_InstantiateForAnimatorPreview =
                            typeof(editor.EditorUtility)
                            .GetMethod("InstantiateForAnimatorPreview", BindingFlags.Static | BindingFlags.NonPublic)
                            .CreateDelegate(typeof(Func<UnityEngine.Object, GameObject>)) as Func<UnityEngine.Object, GameObject>;
                    return m_InstantiateForAnimatorPreview;
                }
            }
            static Func<UnityEngine.Object, GameObject> m_InstantiateForAnimatorPreview;

            public static Func<editor.PreviewRenderUtility, RenderTexture> PreviewRenderUtilityRenderTexture
            {
                get
                {
                    if (m_PreviewRenderUtilityRenderTexture is null)
                        m_PreviewRenderUtilityRenderTexture =
                            typeof(editor.PreviewRenderUtility)
                            .GetProperty("renderTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod
                            .CreateDelegate(typeof(Func<editor.PreviewRenderUtility, RenderTexture>)) as Func<editor.PreviewRenderUtility, RenderTexture>;
                    return m_PreviewRenderUtilityRenderTexture;
                }
            }

            public static Func<editor.PreviewRenderUtility, RenderTexture> m_PreviewRenderUtilityRenderTexture;
        }
    }
}
