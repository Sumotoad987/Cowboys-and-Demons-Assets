using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using editor = UnityEditor;
using Ex.Kingmaker.Visual.CharacterSystem;
using UnityEngine;
using Kingmaker.Blueprints;
using Kingmaker.Utility;
using static Kingmaker.Blueprints.Gender;
using static Kingmaker.Blueprints.Race;
using static UnityEngine.Object;
using Ex.Kingmaker.ResourceLinks;

namespace MyOwlcatModification.Assets.Editor.Blueprints
{
    [editor.CustomEditor(typeof(KingmakerEquipmentEntity), true)]
    //[CanEditMultipleObjects]
    internal class KingmakerEquipmentEntityPreviewEditor : BlueprintInspectorEditor
    {
        private Dictionary<(Gender SelectedGender, Race SelectedRace), Tuple<PreviewData, string>> previewDataInstances 
            = new Dictionary<(Gender SelectedGender, Race SelectedRace), Tuple<PreviewData, string>>();
        private Dictionary<PreviewData, RenderTexture> renderTextureCache
            = new Dictionary<PreviewData, RenderTexture>();
        private Vector2 m_PreviewDir;
        private Rect m_PreviewRect;
        //private editor.SerializedProperty m_Name;

        Gender selectedGender;
        Race selectedRace;

        editor.GenericMenu genderSelectionMenu;
        editor.GenericMenu raceSelectionMenu;


        public void OnEnable()
        {
            m_PreviewDir = (editor.EditorSettings.defaultBehaviorMode == editor.EditorBehaviorMode.Mode2D) ? new Vector2(0f, 0f) : new Vector2(120f, -20f);
            //m_Name = serializedObject.FindProperty("m_Name");
        }
        internal void OnDisable()
        {
            foreach (var Preview in previewDataInstances.Values.Select(v => v.Item1))
                Preview?.Dispose();
            foreach (var Texture in renderTextureCache.Values)
                DestroyImmediate(Texture);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            bool flag = !editor.EditorUtility.IsPersistent(target);
            return true 
                //&& HasStaticPreview()
                ;
        }
        private bool HasStaticPreview()
        {
            return true;
        }
        private Tuple<PreviewData,string> GetPreviewData()
        {
            if (previewDataInstances.TryGetValue((selectedGender, selectedRace), out var data))
                return data;

            var tar = target as KingmakerEquipmentEntity;
            EquipmentEntityLink[] EELinks = tar.GetLinks(selectedGender, selectedRace);
            string maybeErrorMessage = null;
            PreviewData maybePreviewData = null;
            var resources = ResourcesLibrary.s_LoadedResources;

            if (EELinks == null || !EELinks.Any(l => l.Exists()))
                maybeErrorMessage = "EELinks are empty";
            else
                maybePreviewData = new PreviewData(EELinks.Select(link => link.Load()), selectedRace, selectedGender);
            var result = new Tuple<PreviewData, string> (maybePreviewData, maybeErrorMessage);
            previewDataInstances[(selectedGender, selectedRace)] = result;
            if (maybePreviewData != null)
            {
                maybePreviewData.SetUpObjects();

                if (!maybePreviewData.gameObjects.Any())
                    ReloadPreviewInstances();
            }
            return result;
        }

        //public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        //{
        //    bool flag = 
        //        //!HasStaticPreview() || 
        //        !editor.ShaderUtil.hardwareSupportsRectRenderTexture;
        //    Texture2D result;
        //    if (flag)
        //    {
        //        result = null;
        //    }
        //    else
        //    {
        //        editor.PreviewRenderUtility renderUtility = GetPreviewData().renderUtility;
        //        renderUtility.BeginStaticPreview(new Rect(0f, 0f, (float)width, (float)height));
        //        DoRenderPreview();
        //        result = renderUtility.EndStaticPreview();
        //    }
        //    return result;
        //}

        private void ClearPreviewCache()
        {
            foreach (var t in renderTextureCache.Values)
                t?.Release();
            renderTextureCache.Clear();
        }
        private void DoRenderPreview(PreviewData previewData)
        {
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

        public override void OnPreviewSettings()
        {
            string genderName = genderSelectionMenu != null ? selectedGender.ToString() : "Gender";
            if (genderSelectionMenu == null)
            {
                genderSelectionMenu = new editor.GenericMenu();
                genderSelectionMenu.AddItem(new GUIContent(Male.ToString()),
                    false, //selectedGender is Male,
                    () => selectedGender = Male
                    ); 
                genderSelectionMenu.AddItem(new GUIContent(Female.ToString()),
                    false, //selectedGender is Female,
                    () => selectedGender = Female
                    );
            }
            if (editor.EditorGUILayout.DropdownButton(new GUIContent() { text = genderName }, FocusType.Passive))
                genderSelectionMenu.ShowAsContext();

            if (serializedObject.FindProperty(nameof(KingmakerEquipmentEntity.m_RaceDependent)).boolValue)
            {
                string raceName = raceSelectionMenu != null ? selectedRace.ToString() : "Race";
                if (raceSelectionMenu == null)
                {
                    raceSelectionMenu = new editor.GenericMenu();
                    var raceArray = serializedObject.FindProperty(nameof(KingmakerEquipmentEntity.m_RaceDependentArrays));
                    for (int i = 0; i < raceArray.arraySize; i++)
                    {
                        int r = i;
                        raceSelectionMenu.AddItem(new GUIContent(((Race)i).ToString()),
                            false, //(int)selectedRace == i,
                            race => selectedRace = (Race)race,   //(int)race,
                            (Race)r
                            );
                    }
                }
                if (editor.EditorGUILayout.DropdownButton(new GUIContent() { text = raceName }, FocusType.Passive))
                    raceSelectionMenu.ShowAsContext();

            }

        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!editor.ShaderUtil.hardwareSupportsRectRenderTexture && Event.current.type == EventType.Repaint)
            {
                editor.EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "Preview requires\nrender texture support");
                return;
            }
            var previewTuple = GetPreviewData();
            if (previewTuple.Item2 != null)
            {
                editor.EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "ERROR\n" + previewTuple.Item2);
                return;
            }
            Vector2 vector = EquipmentEntityPreviewEditor.Drag2D(m_PreviewDir, r);
            if (vector != m_PreviewDir)
            {
                ClearPreviewCache();
                m_PreviewDir = vector;
            }
            if (Event.current.type == EventType.Repaint)
            {
                if (m_PreviewRect != r)
                {
                    ClearPreviewCache();
                    m_PreviewRect = r;
                }
                var data = previewTuple.Item1;
                var renderUtility = data.renderUtility;
                if (!previewDataInstances.TryGetValue((selectedGender, selectedRace), out var preview) && preview.Item1 == null && !preview.Item1.EnsurePainting())
                {
                    if (renderTextureCache.ContainsKey(preview.Item1))
                    {
                        renderTextureCache[preview.Item1].Release();
                        renderTextureCache.Remove(preview.Item1);
                    }
                }
                if (!renderTextureCache.TryGetValue(data, out var cachedTex) || cachedTex != null)
                {
                    renderUtility.BeginPreview(r, background);
                    DoRenderPreview(data);
                    renderUtility.EndAndDrawPreview(r);
                    cachedTex = new RenderTexture(PreviewData.PreviewRenderUtilityRenderTexture.Invoke(renderUtility));
                    renderTextureCache[data] = cachedTex;
                    RenderTexture active = RenderTexture.active;
                    Graphics.Blit(PreviewData.PreviewRenderUtilityRenderTexture.Invoke(renderUtility), (RenderTexture)cachedTex);
                    RenderTexture.active = active;
                }
                GUI.DrawTexture(r, cachedTex, ScaleMode.StretchToFill, false);
            }
            
        }

        internal class PreviewData : EquipmentEntityPreviewEditor.PreviewData
        {
            public EquipmentEntity raceEE;
            private int raceEE_fatness = 0;
            public Dictionary<Kingmaker.Visual.CharacterSystem.BodyPartType, Tuple<BodyPart, GameObject>> bodyObjects =
                new Dictionary<Kingmaker.Visual.CharacterSystem.BodyPartType, Tuple<BodyPart, GameObject>>();
            Material bodyMaterial;
            readonly Color halfTransparent = new Color(1, 1, 1, 0.5f);

            public PreviewData (IEnumerable<EquipmentEntity> targetObjects, Race raceIndex, Gender genderIndex, RampColorPreset.IndexSet indexSet = null, bool useSpecial = false) 
                : base (targetObjects, indexSet, useSpecial)
            {
                var races = ResourcesLibrary.GetRoot()?.Progression?.m_CharacterRaces;
                if (races == null)
                {
                    Debug.LogError($"KingmakerEquipmentEntityPreviewEditor.PreviewData - failed to find races array in Root when constructing the data.");
                    return;
                }
                if (raceIndex < 0 || (int)raceIndex >= races.Length)
                {
                    Debug.LogError($"KingmakerEquipmentEntityPreviewEditor.PreviewData - failed to find the race under index {raceIndex} when constructing the data.");
                    return;
                }
                var race = races[(int)raceIndex]?.Get();
                if (race == null)
                {
                    Debug.LogError($"KingmakerEquipmentEntityPreviewEditor.PreviewData - failed to find to get the race blueprint {race?.AssetGuid.ToString() ?? "null"}" +
                        $"from under the index {raceIndex} when constructing the data.");
                    return;
                }

                var raceVisualPreset = race.m_Presets[raceEE_fatness]?.Get();
                if (race is null)
                {
                    Debug.LogError($"KingmakerEquipmentEntityPreviewEditor.PreviewData - failed to find to get the visual preset {race?.AssetGuid.ToString() ?? "null"}" +
                        $"from under the index {raceIndex} when constructing the data.");
                    return;
                }
                var skin = raceVisualPreset.m_Skin?.Get();
                var m_raceEE = genderIndex is Male ? skin.m_MaleArray[0].Load() : skin.m_FemaleArray[0].Load();
                raceEE = m_raceEE;
            }

            public override void SetUpObjects()
            {
                foreach (var gameObject in gameObjects.Values.Select(tuple => tuple.Item2))
                    DestroyImmediate(gameObject);
                foreach (var gameObject in bodyObjects.Values.Select(tuple => tuple.Item2))
                   DestroyImmediate(gameObject);
                gameObjects.Clear();
                bodyObjects.Clear();
                foreach (var tuple in ModAssetImporter.InstantiateAndColorBodyParts(PrimaryColor, SecondaryColor, SpecialPrimaryColor, SpecialSecondaryColor, raceEE).Where(tuple => tuple.Item2 != null))
                    bodyObjects[tuple.Item1.Type] = tuple;
                foreach (var EE in EEs)
                    foreach (var tuple in ModAssetImporter.InstantiateAndColorBodyParts(PrimaryColor, SecondaryColor, SpecialPrimaryColor, SpecialSecondaryColor, EE).Where(tuple => tuple.Item2 != null))
                        gameObjects[tuple.Item1.Type] = tuple;
                material = gameObjects.Values.First().Item2.GetComponentInChildren<Renderer>().sharedMaterial;
                material.EnableKeyword("_EMISSION");
                bodyMaterial = bodyObjects.Values.First().Item2.GetComponentInChildren<Renderer>().sharedMaterial;
                bodyMaterial.color = halfTransparent;
                var o = gameObjects.Values.Select(tuple => tuple.Item2);
                foreach (var gameObject in bodyObjects.Values.Select(tuple => tuple.Item2))
                {
                    renderUtility.AddSingleGO(gameObject);
                }
                foreach (var gameObject in gameObjects.Values.Select(tuple => tuple.Item2))
                {
                    renderUtility.AddSingleGO(gameObject);
                }
                renderableBounds = GetRenderableBounds(o.SelectMany(ob => ob.GetComponentsInChildren<Renderer>()).NotNull());

            }
        }
    }
}
