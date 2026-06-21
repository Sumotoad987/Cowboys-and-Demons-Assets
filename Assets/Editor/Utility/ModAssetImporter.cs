using Ex.Kingmaker.Blueprints.Items.Weapons;
using Ex.Kingmaker.UI.MVVM._PCView.MainMenu;
using Ex.Kingmaker.View.Equipment;
using Ex.Kingmaker.Visual.Animation.Kingmaker;
using Ex.Kingmaker.Visual.CharacterSystem;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.BundlesLoading;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Items;
using Kingmaker.Modding;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceManagement;
using Kingmaker.UI.MVVM;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.MaterialEffects;
//using Owlcat.Runtime.Visual.RenderPipeline.Terrain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Kingmaker.Visual.CharacterSystem.BodyPartType;
using static Kingmaker.Visual.CharacterSystem.CharacterTextureChannel;
//using Kingmaker.Visual.Animation.Kingmaker;
using oldCharSys = Kingmaker.Visual.CharacterSystem;

namespace MyOwlcatModification
{
    public class ModAssetImporter
    {
        public static bool Launched 
        {
            get
            {
                bool result = Launcher.Launched;
                if (!result)
                    Debug.LogWarning("Launcher has not run");
                return result;

            }
        }

        //[MenuItem("Bundles/Test Build")]
        public static void TestBuildBundle()
        {
            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            };
            var ab = new AssetBundleBuild()
            {
                assetBundleName = "ShaderTest",
                assetNames = new string[] { "Assets\\RenderPipeline\\OwlcatShaders\\Shaders\\Lit\\Lit.shader" }
            };
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, new AssetBundleBuild[] { ab }, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
        }



        //[MenuItem("Terrain/Test")]
        public static void TestTerrain()
        {
            Debug.Log("TestTerrain - start");
            Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.Log("TestTerrain - terrain is null");
                return;
            }
            var data = terrain.terrainData;
            if (data == null)
            {
                Debug.Log("TestTerrain - data is null");
                return;
            }
            Debug.Log($"TestTerrain - data is {terrain.name}");
            var layers = data.terrainLayers;
            if (layers == null)
            {
                Debug.Log("TestTerrain - layers is null");
                return;
            }
            Debug.Log($"TestTerrain - layers have {layers.Length} entries. First is null? {layers[0] == null}. Name is {((layers[0] != null) ? layers[0].name : "Null")}");
            ;
            Debug.Log($"TestTerrain - material has DiffuseArray property? {terrain.materialTemplate.HasProperty("_DiffuseArray")}");
            foreach (var prop in terrain.materialTemplate.GetTexturePropertyNames())
                Debug.Log($"Has texture property {prop}");
            terrain.materialTemplate.SetTexture("_MainTex", layers[0].diffuseTexture);
            Selection.activeObject = terrain.materialTemplate.GetTexture("_MainTex") ;
            var mat = new MaterialPropertyBlock();
            terrain.GetSplatMaterialPropertyBlock(mat);
            Debug.Log("TestTerrain - obtained the block");
            var tex = mat.GetTexture("_DiffuseArray");
            if (tex == null)
            {
                Debug.Log("TestTerrain - tex is null");
                return;
            }
            Debug.Log($"TestTerrain - tex is {tex.name}");
            Selection.activeObject = tex;


        }


        //[MenuItem("BundleTest/Test")]
        public static void BundleTest()
        {
            try
            {
                var ab = AssetBundle.GetAllLoadedAssetBundles();
                long index = 0x0170b2f;
                //string sword = "OHW_LongswordHolyIomedaeRadiance_Sword";
                string swordGuid = "7ffd2599aa5b3c34c8b34d1028eaa39a";
                foreach (var b in ab)
                    if (b.name.Contains("weapons.prefabs")) 
                        b.Unload(true);
                string filepath = "C:\\Users\\user\\Desktop/weapons.prefabs-decompressed";
                if (!File.Exists(filepath))
                {
                    Debug.Log("File does not exist");
                    return;
                }
                var bytes = File.ReadAllBytes(filepath);
                Debug.Log($"Read all bytes. Length is {bytes.Length}");
                if (bytes.Length < 0) return;
                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    using BinaryReader reader = new BinaryReader(fs);
                    fs.Position = index;
                    int nameLength = reader.ReadInt32();
                    nameLength += nameLength % 4;
                    Debug.Log($"name length is {nameLength}");
                    var name = reader.ReadChars(nameLength);
                    Debug.Log($"name is {new string(name)}");
                    int SubMeshCount = reader.ReadInt32();
                    Debug.Log($"SubMeshCount length is {SubMeshCount}");
                    for (int i = 0; i < SubMeshCount; i++)
                    {
                        reader.ReadUInt32();
                        Debug.Log($"Index Count is {reader.ReadUInt32()}");
                        reader.ReadInt32();
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                        Debug.Log($"Vertex Count is {reader.ReadUInt32()}");
                        Debug.Log($"AABB center 1 is {reader.ReadSingle()}, {reader.ReadSingle()}, {reader.ReadSingle()}");
                        Debug.Log($"AABB center 2 is {reader.ReadSingle()}, {reader.ReadSingle()}, {reader.ReadSingle()}");
                    }
                    reader.ReadBytes(37);
                    Debug.Log($"{reader.BaseStream.Position}");
                    bytes[(int)reader.BaseStream.Position] = 1;
                }
                var bundle = AssetBundle.LoadFromMemory(bytes);
                Debug.Log($"Tried to load the bundle. Is null? {bundle == null}");
                if (bundle == null) return;
                var swordObject = bundle.LoadAsset(swordGuid) as GameObject;
                if (swordObject == null)
                {
                    Debug.Log("failed to load the object");
                    return;
                }
                var meshFilter = swordObject.GetComponentInChildren<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.Log("failed to find meshFilter");
                    return;
                }
                Debug.Log($"Mesh is readable? {meshFilter.sharedMesh.isReadable}");
                var mesh = meshFilter.sharedMesh;
                var instance = GameObject.Instantiate(swordObject);
                var anotherMesh = UnityEngine.Object.Instantiate(mesh);

                Debug.Log(string.Join(", ", anotherMesh.GetVertexAttributes()));

                var layout = new VertexAttributeDescriptor[]
                {
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2),
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
                };
                var vertexCount = mesh.vertexCount;

                anotherMesh.Clear();
                anotherMesh.SetVertexBufferParams(vertexCount, layout);
                
                var buffer = new NativeArray<Extensions.MeshBuffer>(vertexCount, Allocator.Temp);


                
                for (int i = 0; i < vertexCount; i++)
                {
                    buffer[i] = new Extensions.MeshBuffer()
                    {
                        position = mesh.vertices[i],
                        normalX = mesh.normals[i].x,
                        normalY = mesh.normals[i].y,
                        tangent1 = mesh.tangents[i].x,
                        tangent2 = mesh.tangents[i].y,
                        tangent3 = mesh.tangents[i].z,
                        tangent4 = mesh.tangents[i].w,
                    };
                }

                anotherMesh.SetVertexBufferData(buffer, 0, 0, vertexCount);
                anotherMesh.RecalculateNormals();
                anotherMesh.RecalculateTangents();
                anotherMesh.RecalculateBounds();
                anotherMesh.UploadMeshData(false);
                AssetDatabase.CreateAsset(anotherMesh, "Assets/1.mesh");

                Selection.activeObject = anotherMesh;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }


        public static GameObject DoImportPrefab(string assetID)
        {
            if (!Launched) return null;
            Debug.Log($"Trying to get the object");
            try
            {
                var bundlesLoadService = BundlesLoadService.Instance;
                var DD = bundlesLoadService.m_DependencyData;
                string s = bundlesLoadService.GetBundleNameForAsset(assetID);
                Debug.Log($"Is there a name in localtion list? {bundlesLoadService.m_LocationList.GuidToBundle.Keys.Contains(assetID)}. Owlcat manager contains name? {OwlcatModificationsManager.Instance.GetBundleNameForAsset(assetID) != null}");
                List<string> bundles = new List<string>() { "" };
                if (DD?.BundleToDependencies?.TryGetValue(s, out bundles) ?? false)
                    Debug.Log($"Dependencies are {string.Join(", ", bundles)}");
                else Debug.Log("Failed to read dependencies");
                PrefabLink PL = Activator.CreateInstance<PrefabLink>();
                PL.AssetId = assetID;
                var model = PL.Load();
                Debug.Log($"prefab is null? {model == null}. {(model != null ? "prefab is " + model.name : "") }");
                //Debug.Log($"Loaded bundles are {String.Join(",", BundlesLoadService.Instance.m_Bundles)}");
                //Debug.Log($"There are {BundlesLoadService.Instance.m_Bundles.Count()} loaded bundles.");
                var go = UnityEngine.Object.Instantiate(model);
                //Debug.Log($"Mesh in prefab is null? {model.GetComponentInChildren<MeshFilter>()?.sharedMesh == null}");
                //Debug.Log($"Mesh in the instantiated object is null? {go.GetComponentInChildren<MeshFilter>()?.sharedMesh == null}");
                return go;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static UnitEntityView DoImportUnit(string guid, bool mounted = false, bool forceVisible = true)
        {
            if (!Launched)
                return null;
            try
            {
                if (!Guid.TryParse(guid, out Guid g))
                {
                    Debug.LogError($"Failed to parse guid {guid}!");
                    return null;
                }
                var unit = ResourcesLibrary.TryGetBlueprint<Ex.Kingmaker.Blueprints.BlueprintUnit>(new BlueprintGuid(g));
                if (unit == null)
                {
                    Debug.LogError("Failed to load the blueprint!");
                    return null;
                }
                if (unit.Prefab == null)
                {
                    Debug.LogError("Blueprint has no view link!");
                    return null;
                }
                //var prefab = unit.Prefab.LoadObject();
                var bundle = BundlesLoadService.Instance.RequestBundleForAsset(unit.Prefab.AssetId);
                if (bundle != null)
                    bundle.Unload(true);

                var view = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId, false)?.Object;
                if (view == null)
                {
                    Debug.LogError("Somehow there's no UnitEntityView on the loaded prefab!");
                    return null;
                }
                var strings = view.GetComponentInChildren<SkinnedMeshRenderer>()?.sharedMaterial.shaderKeywords;
                if (strings != null)
                    Debug.Log($"shader keywords in Unit are {string.Join(",", strings)}");
                else
                    Debug.Log($"Can't find renderer in Unit View!");

                view = GameObject.Instantiate(view.gameObject).GetComponent<UnitEntityView>();
                IKController ikc = null;
                if (mounted && view.GetComponentInChildren<Ex.Kingmaker.Visual.Animation.IKController>(true) == null)
                {
                    ikc = view.gameObject.AddComponent<Kingmaker.Visual.Animation.IKController>();
                    ikc.CharacterSystem = view.gameObject.GetComponent<Character>();
                    ikc.enabled = true;
                }

                var data = view.CreateEntityData(unit);
                data.AttachView(view);
                view.SetVisible(forceVisible, forceVisible);
                UnitAnimationManager animationManager = view.AnimationManager;
                if (mounted) ikc.CheckState();
                if (animationManager == null)
                {
                    Debug.LogError("Null animation manager");
                    return null;
                }
                animationManager.AnimationSet = UnityEngine.Object.Instantiate(animationManager.AnimationSet);
                animationManager.PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
                animationManager.IsInCombat = true;
                animationManager.CombatMicroIdle = Kingmaker.Visual.Animation.Kingmaker.CombatMicroIdle.Weapon;
                animationManager.Tick();
                animationManager.UpdateTime(0.1f);
                animationManager.LocoMotionHandle.Action.OnUpdate(animationManager.LocoMotionHandle, 0.01f);

                view.CharacterAvatar = view.GetComponentInChildren<Character>();
                if (view.CharacterAvatar)
                {
                    view.CharacterAvatar.AnimationSet = animationManager.AnimationSet;
                    view.UpdateAdditionalVisualSettings();
                    view.CharacterAvatar.OnUpdated += view.CharacterAvatarUpdated;
                    view.CharacterAvatar.Start();
                //view.CharacterAvatar.RemoveEquipmentEntities(view.CharacterAvatar.EquipmentEntities.Where(ee => ee?.BodyParts?.Any(bp => bp != null && (bp.Type & Hair) != 0) ?? false).ToArray());  
                //view.CharacterAvatar.AddEquipmentEntity(AssetDatabase.LoadAssetAtPath<EquipmentEntity>("Assets/EE_Test.asset"));
                }
                view.UpdateViewActive();
                if (view.CharacterAvatar)
                {
                    view.CharacterAvatar.PreventUpdate = new bool?(false);
                    view.CharacterAvatar.DoUpdate();
                }
                view.ForcePeacefulLook(false);
                animationManager.IsInCombat = true;
                view.HandsEquipment.InCombat = true;
                view.AnimationManager.DoUpdate();
                //Kingmaker.Controllers.Units.UnitAnimationController.TickOnUnit(view.Data, null);
                //view.HandsEquipment.m_IsAnimatingWeaponsChange = true;
                //view.HandsEquipment.MatchWithCurrentCombatState();
                //view.HandsEquipment.m_IsAnimatingWeaponsChange = false;

                return view;

                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public static bool DoImportEE(string assetID, int primaryRampIndex = 0, int secondaryRampIndex = 0, int primarySpecialRampIndex = -1, int secondarySpecialRampIndex = -1)
        {
            if (!Launched) return false;
            var EELink = new EquipmentEntityLink()
            {
                AssetId = assetID
            };


            var EE = EELink.Load();
            Debug.Log($"Loaded EE? {EE != null}." + (EE != null ? $"EE {EE.name}. Has Baked textures? {EE.BakedTextures != null}. Has body parts? {EE.BodyParts != null}" : ""));
            if (EE == null) return false;

            #region removed the copying

            //try
            //{
            //    Selection.activeObject = EE;
            //    var bodyPart = EE.BodyParts.First();
            //    EquipmentEntity EE1 = ScriptableObject.Instantiate(EE as EquipmentEntity);
            //    AssetDatabase.CreateAsset(EE1, $"Assets/EE/{EE.name}.asset");
            //}
            //catch(Exception e)
            //{
            //    Debug.LogException(e);
            //}
            //if (EE.BakedTextures != null)
            //{
            //    foreach (var tex in EE.BakedTextures.BakedTextures)
            //    {
            //        try
            //        {
            //            var Tex = tex.Load();
            //            if (Tex != null)
            //            {
            //                Debug.Log($"Here is texture {Tex.name}");
            //                //AssetDatabase.CreateAsset(Tex, tex.AssetId + ".tex");
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Debug.LogException(e);
            //        }
            //    }
            //foreach (var tex in EE.BakedTextures.ColorRamps)
            //    try
            //    {
            //        //AssetDatabase.CreateAsset(tex, tex.name + ".tex");
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.LogException(e);
            //    }
            //} 
            #endregion

            if (EE.ColorPresets?.IndexPairs?.ElementAt(0) is Ex.Kingmaker.Visual.CharacterSystem.RampColorPreset.IndexSet set)
            {
                primaryRampIndex = set.PrimaryIndex;
                secondaryRampIndex = set.SecondaryIndex;
            }

            var r = InstantiateAndColorBodyParts(primaryRampIndex, secondaryRampIndex, primarySpecialRampIndex, secondarySpecialRampIndex, EE);
            //var d = new Dictionary<Ex.Kingmaker.Visual.CharacterSystem.BodyPart, Ex.Kingmaker.Visual.CharacterSystem.EquipmentEntity>();
            //r.Select(t => t.Item1).ForEach(i => d[i] = EE);
            //TryBuildMesh(d, r.Select(a => a.Item2).First().GetComponentInChildren<Renderer>().sharedMaterial);
            Selection.activeObject = EE;
            return true;
        }

        public static Tuple<Ex.Kingmaker.Visual.CharacterSystem.BodyPart, GameObject>[] InstantiateAndColorBodyParts(int primaryRampIndex, int secondaryRampIndex, int primarySpecialRampIndex, int secondarySpecialRampIndex, EquipmentEntity EE)
        {
            var objects = EE.BodyParts.Select(p => new Tuple<Ex.Kingmaker.Visual.CharacterSystem.BodyPart, GameObject>(p, p?.RendererPrefab != null ? GameObject.Instantiate(p.RendererPrefab) : null))
                .Where(a => a.Item1 != null && a.Item2 != null)
                .ToArray();
            var sorted = new List<Tuple<oldCharSys.BodyPartType, IEnumerable<CharacterTextureDescription>, Texture2D, Texture2D, Material>>();
            try
            {
                var material = new Material(Shader.Find("Owlcat/Lit"));
                material.SetColor(StandardMaterialController._BaseColor, new Color(1, 1, 1, 1));
                material.SetFloat(StandardMaterialController._BaseColorBlending, 0);
                material.SetKeywordEnabled("ADDITIONAL_ALBEDO", false);
                material.SetKeywordEnabled("CUTOUT_ON", true);
                material.SetKeywordEnabled("DYNAMIC_FX", true);
                material.SetKeywordEnabled("RIM_LIGHTING_GLOBAL_SHADE_NDOTL", true);
                material.SetKeywordEnabled("_ALPHATEST_ON", true);
                material.SetKeywordEnabled("_ALPHATOMASK_OFF", true);
                material.SetKeywordEnabled("_DOUBLESIDED_ON", false);
                material.SetKeywordEnabled("_RIMLIGHTING_OFF", true);
                material.SetKeywordEnabled("_RIMSHADENDOTL_OFF", true);
                material.SetKeywordEnabled("_TRANSLUCENT", true);
                material.SetKeywordEnabled("_SPECIALPOSTPROCESSFLAG_ON", true);
                material.SetKeywordEnabled("_VERTEXINTERACTION_OFF", true);
                foreach (var bodyPart in objects.Where(p => p.Item2 != null))
                {
                    bodyPart.Item2.GetComponentInChildren<Renderer>().sharedMaterial = material;
                    Texture2D primaryRamp = null;
                    Texture2D secondaryRamp = null;
                    {
                        if (primarySpecialRampIndex > -1 && primarySpecialRampIndex < EE.SpecialPrimaryRamps.Count)
                        {
                            primaryRamp = EE.SpecialPrimaryRamps[primarySpecialRampIndex];
                        }
                        else
                        if (primaryRampIndex < EE.PrimaryRamps.Count)
                        {
                            primaryRamp = EE.PrimaryRamps[primaryRampIndex];
                        }
                        if (secondarySpecialRampIndex > -1 && secondarySpecialRampIndex < EE.SpecialSecondaryRamps.Count)
                        {
                            secondaryRamp = EE.SpecialPrimaryRamps[secondarySpecialRampIndex];
                        }
                        else
                        if (secondaryRampIndex < EE.SecondaryRamps.Count)
                        {
                            secondaryRamp = EE.SecondaryRamps[secondaryRampIndex];
                        }

                    }

                    sorted.Add(new Tuple<oldCharSys.BodyPartType, IEnumerable<CharacterTextureDescription>, Texture2D, Texture2D, Material>(bodyPart.Item1.Type, bodyPart.Item1.Textures, primaryRamp, secondaryRamp, material));
                }

                buildTextureFromDescriptions(sorted, Diffuse);
                buildTextureFromDescriptions(sorted, Normal);
                buildTextureFromDescriptions(sorted, Masks);

                //if (EE.OutfitParts?.Count > 0)
                //    Debug.Log($"Outfit parts are {string.Join(", ", EE.OutfitParts)}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            return objects;
        }

        public static RenderTexture m_TempLinearTexture;
        public static RenderTexture m_TempSRGBTexture;

        public static RenderTexture buildTextureFromDescriptions(
            IEnumerable<Tuple<oldCharSys.BodyPartType, IEnumerable<CharacterTextureDescription>, Texture2D, Texture2D, Material>> sortedTextures,
            oldCharSys.CharacterTextureChannel channel)
        {
            RenderTexture renderTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32, (channel == Diffuse) ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                anisoLevel = 1,
                useMipMap = true,
                autoGenerateMips = true,
                name = string.Format("{0}_WotR", channel)
            };
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            Color backgroundColor = new Color(0f, 0f, 0f, 0f);
            if (channel == Masks)
            {
                backgroundColor = new Color(0.85f, 0f, 0f, 0f);
            }
            if (channel == Normal)
            {
                backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
            }
            GL.Clear(false, true, backgroundColor);
            
            RenderTexture renderTexture2; 
            if (channel == Diffuse)
            {
                if (oldCharSys.CharacterAtlas.m_TempSRGBTexture != null && (oldCharSys.CharacterAtlas.m_TempSRGBTexture.width != 2048 || oldCharSys.CharacterAtlas.m_TempSRGBTexture.height != 2048))
                {
                    oldCharSys.CharacterAtlas.m_TempSRGBTexture.Release();
                    oldCharSys.CharacterAtlas.m_TempSRGBTexture = null;
                }
                if (oldCharSys.CharacterAtlas.m_TempSRGBTexture == null)
                {
                    oldCharSys.CharacterAtlas.m_TempSRGBTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                }
                oldCharSys.CharacterAtlas.m_TempSRGBTexture.DiscardContents();
                renderTexture2 = oldCharSys.CharacterAtlas.m_TempSRGBTexture;
            }
            else
            {
                if (oldCharSys.CharacterAtlas.m_TempLinearTexture != null && (oldCharSys.CharacterAtlas.m_TempLinearTexture.width != 2048 || oldCharSys.CharacterAtlas.m_TempLinearTexture.height != 2048))
                {
                    oldCharSys.CharacterAtlas.m_TempLinearTexture.Release();
                    oldCharSys.CharacterAtlas.m_TempLinearTexture = null;
                }
                if (oldCharSys.CharacterAtlas.m_TempLinearTexture == null)
                {
                    oldCharSys.CharacterAtlas.m_TempLinearTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                }
                oldCharSys.CharacterAtlas.m_TempLinearTexture.DiscardContents();
                renderTexture2 = oldCharSys.CharacterAtlas.m_TempLinearTexture;
            }
            renderTexture2.filterMode = FilterMode.Bilinear;
            renderTexture2.wrapMode = TextureWrapMode.Repeat;
            renderTexture2.anisoLevel = 1;
            if (!renderTexture2.useMipMap)
            {
                if (renderTexture2.IsCreated())
                {
                    renderTexture2.Release();
                }
                renderTexture2.useMipMap = true;
            }
            renderTexture2.autoGenerateMips = true;
            if (!renderTexture2.IsCreated())
            {
                renderTexture2.Create();
            }


            Material material2 = new Material(Shader.Find("Hidden/CharacterAtlasGenerator"));
            Material material3 = new Material(Shader.Find("Hidden/CharacterShadowBake"));
            Material material4 = new Material(Shader.Find("Hidden/CharacterDiffuseBake"));
            Material material5 = new Material(Shader.Find("Hidden/RoughnessLightenBlend"));
            material2.SetTexture(ShaderProps._PreviousTex, renderTexture2);
            oldCharSys.BodyPartType partType;
            IEnumerable<CharacterTextureDescription> descr;
            Texture2D primaryRamp;
            Texture2D secondaryRamp;
            Material material;
            foreach (var sTextures in sortedTextures)
            {
                partType = sTextures.Item1;
                descr = sTextures.Item2;
                primaryRamp = sTextures.Item3;
                secondaryRamp = sTextures.Item4;
                material = sTextures.Item5;
                RenderTexture rt = null;

                foreach (var characterTextureDescription in descr)
                {
                    var bytes = new byte[] { };
                    RenderTexture oldRT;
                    Texture2D tex;


                    if (channel == Diffuse)
                        characterTextureDescription.Repaint(ref rt, primaryRamp, secondaryRamp);

                    //if (rt != null)
                    //{
                    //    oldRT = RenderTexture.active;
                    //    RenderTexture.active = rt;
                    //    tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                    //    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    //    RenderTexture.active = oldRT;
                    //    bytes = ImageConversion.EncodeToJPG(tex);
                    //    File.WriteAllBytes("Assets/Test1.jpg", bytes);
                    //}

                    var mappedRect = oldCharSys.CharacterAtlas.MapRect(partType);
                    Vector4 rect = new Vector4(0, 0, 1f, 1f);
                    rect = new Vector4(mappedRect.x / 2048, mappedRect.y / 2048, mappedRect.width / 2048, mappedRect.height / 2048);

                    Vector4 value = new Vector4(0f, 0f, 1f, 1f);
                    material2.SetVector(ShaderProps._SrcRect, value);
                    material3.SetVector(ShaderProps._SrcRect, value);
                    material4.SetVector(ShaderProps._SrcRect, value);
                    material5.SetVector(ShaderProps._SrcRect, value);
                    material2.SetVector(ShaderProps._DstRect, rect);
                    material3.SetVector(ShaderProps._DstRect, rect);
                    material4.SetVector(ShaderProps._DstRect, rect);
                    material5.SetVector(ShaderProps._DstRect, rect);
                    material2.SetInt(ShaderProps._IsEmpty, characterTextureDescription.IsEmpty ? 1 : 0);
                    if (characterTextureDescription.Material != null)
                    {
                        material2.SetFloat(ShaderProps._Roughness, characterTextureDescription.Material.GetFloat(ShaderProps._Roughness));
                        material2.SetFloat(ShaderProps._Emission, characterTextureDescription.Material.GetFloat(ShaderProps._Emission));
                        material2.SetFloat(ShaderProps._Metallic, characterTextureDescription.Material.GetFloat(ShaderProps._Metallic));
                    }
                    else
                    {
                        material2.SetFloat(ShaderProps._Roughness, 1f);
                        material2.SetFloat(ShaderProps._Emission, 1f);
                        material2.SetFloat(ShaderProps._Metallic, 1f);
                    }
                    material2.SetTexture(ShaderProps._AlphaMask, characterTextureDescription.DiffuseTexture);
                    if (channel != Diffuse)
                    {
                        material2.EnableKeyword("ALPHA_MASK_ON");
                        if (channel == Normal)
                        {
                            material2.EnableKeyword("NORMAL_MAP_ON");
                            material.SetFloat(ShaderProps._UseNormalMapAtlas, 1f);
                        }
                    }
                    else
                    {
                        material2.DisableKeyword("ALPHA_MASK_ON");
                        material2.DisableKeyword("NORMAL_MAP_ON");
                        material.SetFloat(ShaderProps._UseNormalMapAtlas, 0f);
                    }
                    Texture source = rt as Texture ?? characterTextureDescription.ActiveTexture;
                    if (channel == Diffuse)
                    {
                        if (characterTextureDescription.UseShadowMask)
                        {
                            material3.SetTexture(ShaderProps._Mask, characterTextureDescription.RampShadowTexture);
                            Graphics.Blit(source, renderTexture, material3);

                            oldRT = RenderTexture.active;
                            RenderTexture.active = renderTexture;
                            tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                            RenderTexture.active = oldRT;
                            bytes = ImageConversion.EncodeToJPG(tex);
                            if (partType == Skirt && channel == Diffuse)
                            {
                                var blah = tex.GetPixel(200, 200);
                            }
                            //File.WriteAllBytes("Assets/Test2.jpg", bytes);
                            


                            if (!characterTextureDescription.ActiveTexture.name.EndsWith("_D"))
                            {
                                continue;
                            }
                            Graphics.Blit(source, renderTexture, material4);



                            //oldRT = RenderTexture.active;
                            //RenderTexture.active = renderTexture;
                            //tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                            //tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                            //RenderTexture.active = oldRT;
                            //bytes = ImageConversion.EncodeToJPG(tex);
                            //File.WriteAllBytes("Assets/Test3.jpg", bytes);


                            Graphics.Blit(renderTexture, renderTexture2);
                        }
                        source = rt as Texture ?? characterTextureDescription.ActiveTexture; 
                    }
                    else if (channel == Normal)
                    {
                        source = characterTextureDescription.NormalTexture;
                    }
                    else if (channel == Masks)
                    {
                        source = characterTextureDescription.MaskTexture;
                        if (characterTextureDescription.UseShadowMask && null != characterTextureDescription.RampShadowTexture)
                        {
                            RenderTexture temporary = RenderTexture.GetTemporary(characterTextureDescription.RampShadowTexture.width, characterTextureDescription.RampShadowTexture.height, 0, RenderTextureFormat.ARGB32);
                            Graphics.Blit(characterTextureDescription.RampShadowTexture, temporary);


                            //oldRT = RenderTexture.active;
                            //RenderTexture.active = temporary;
                            //tex = new Texture2D(temporary.width, temporary.height, TextureFormat.RGBA32, false);
                            //tex.ReadPixels(new Rect(0, 0, temporary.width, temporary.height), 0, 0);
                            //RenderTexture.active = oldRT;
                            //bytes = ImageConversion.EncodeToJPG(tex);
                            //File.WriteAllBytes("Assets/Test9.jpg", bytes);

                            //oldRT = RenderTexture.active;
                            //RenderTexture.active = renderTexture;
                            //tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                            //tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                            //RenderTexture.active = oldRT;
                            //bytes = ImageConversion.EncodeToJPG(tex);
                            //File.WriteAllBytes("Assets/Test10.jpg", bytes);

                            Graphics.Blit(temporary, renderTexture, material5);

                            //if (rt != null)
                            //{
                            //    oldRT = RenderTexture.active;
                            //    RenderTexture.active = rt;
                            //    tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                            //    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                            //    RenderTexture.active = oldRT;
                            //    bytes = ImageConversion.EncodeToJPG(tex);
                            //    File.WriteAllBytes("Assets/Test11.jpg", bytes);
                            //}

                            Graphics.Blit(renderTexture, renderTexture2);


                            //oldRT = RenderTexture.active;
                            //RenderTexture.active = renderTexture2;
                            //tex = new Texture2D(renderTexture2.width, renderTexture2.height, TextureFormat.RGBA32, false);
                            //tex.ReadPixels(new Rect(0, 0, renderTexture2.width, renderTexture2.height), 0, 0);
                            //RenderTexture.active = oldRT;
                            //bytes = ImageConversion.EncodeToJPG(tex);
                            //File.WriteAllBytes("Assets/Test12.jpg", bytes);

                            RenderTexture.ReleaseTemporary(temporary);
                        }
                    }
                    if (characterTextureDescription.DontBlendAlphaChannel || (channel == Masks && characterTextureDescription.MaskTexture != null && !HasAlphaChannel(characterTextureDescription.MaskTexture)))
                    {
                        material2.EnableKeyword("DONT_BLEND_ALPHA");
                    }
                    else
                    {
                        material2.DisableKeyword("DONT_BLEND_ALPHA");
                    }

                    //if (rt != null)
                    //{
                    //    oldRT = RenderTexture.active;
                    //    RenderTexture.active = rt;
                    //    tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                    //    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    //    RenderTexture.active = oldRT;
                    //    bytes = ImageConversion.EncodeToJPG(tex);
                    //    File.WriteAllBytes("Assets/Test5.jpg", bytes);
                    //}

                    //oldRT = RenderTexture.active;
                    //RenderTexture.active = renderTexture;
                    //tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                    //tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    //RenderTexture.active = oldRT;
                    //bytes = ImageConversion.EncodeToJPG(tex);
                    //    File.WriteAllBytes("Assets/Test6.jpg", bytes);




                    Graphics.Blit(source, renderTexture, material2);



                    //oldRT = RenderTexture.active;
                    //RenderTexture.active = renderTexture;
                    //tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                    //tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    //RenderTexture.active = oldRT;
                    //bytes = ImageConversion.EncodeToJPG(tex);
                    //File.WriteAllBytes("Assets/Test7.jpg", bytes);

                    Graphics.Blit(renderTexture, renderTexture2);


                    //oldRT = RenderTexture.active;
                    //RenderTexture.active = renderTexture2;
                    //tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                    //tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    //RenderTexture.active = oldRT;
                    //bytes = ImageConversion.EncodeToJPG(tex);
                    //File.WriteAllBytes("Assets/Test8.jpg", bytes);


                    rt?.DiscardContents();
                }
                rt?.Release();

            }

            var a2 = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var tex2 = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex2.ReadPixels(new Rect(200, 200, 1, 1), 0, 0);
            RenderTexture.active = a2;            
            var blah2 = tex2.GetPixel(0, 0);
            
            
            foreach (var mat in sortedTextures.Select(t => t.Item5) )
                switch (channel)
                {
                    case Diffuse:
                        mat.SetTexture(ShaderProps._BaseMap, renderTexture);
                        break;
                    case Normal:
                        mat.EnableKeyword("_NORMALMAP");
                        mat.SetTexture(ShaderProps._BumpMap, renderTexture);
                        break;
                    case Masks:
                        mat.EnableKeyword("_MASKSMAP");
                        mat.SetTexture(ShaderProps._MasksMap, renderTexture);
                        break;
                    default:
                        break;
                }

            RenderTexture.active = active;
            UnityEngine.Object.DestroyImmediate(material2);
            UnityEngine.Object.DestroyImmediate(material3);
            UnityEngine.Object.DestroyImmediate(material4);
            UnityEngine.Object.DestroyImmediate(material5);

            return renderTexture;
        }


        public static GameObject TryBuildMesh(Dictionary<Ex.Kingmaker.Visual.CharacterSystem.BodyPart, EquipmentEntity> geometryBodyParts, Material m_AtlasMaterial)
        {
            var gender = CharacterStudio.DetermineGender(geometryBodyParts.Keys.First(a => a.RendererPrefab != null).RendererPrefab);
            //var race = 0;
            GameObject pseudoAnimator = new GameObject("MergedStuff")
            {
                transform =
                    {
                        localPosition = default(Vector3),
                        localScale = Vector3.one,
                        localRotation = Quaternion.identity
                    }
            };
            GameObject temp = new GameObject("Rig")
            {
                transform =
                    {
                        localPosition = default(Vector3),
                        localScale = Vector3.one,
                        localRotation = Quaternion.identity
                    }
            };
            temp.transform.SetParent(pseudoAnimator.transform);
            List<Transform> list = new List<Transform>();
            List<BoneWeight> list2 = new List<BoneWeight>();
            List<Matrix4x4> list3 = new List<Matrix4x4>();
            Dictionary<string, Transform> dictionary = 
                //new Dictionary<string, Transform>();
                CacheHierarchy(pseudoAnimator, geometryBodyParts.Keys.Select(o => o.SkinnedRenderer?.rootBone).NotNull());
            if (m_AtlasMaterial != null)
            {
                List<CombineInstance> list4 = new List<CombineInstance>();
                List<Vector2> list5 = new List<Vector2>();
                SkinnedMeshRenderer skinnedMeshRenderer = new GameObject("Renderer_" + m_AtlasMaterial.name)
                {
                    transform =
                    {
                        parent = pseudoAnimator.transform,
                        localPosition = default(Vector3),
                        localScale = Vector3.one,
                        localRotation = Quaternion.identity
                    }
                }.AddComponent<SkinnedMeshRenderer>();
                Mesh mesh = new Mesh
                {
                    name = "Character"
                };
                mesh.Clear();
                foreach (var keyValuePair in geometryBodyParts)
                {
                    SkinnedMeshRenderer skinnedRenderer = keyValuePair.Key.SkinnedRenderer;
                    if (!(skinnedRenderer == null))
                    {
                        if (skinnedMeshRenderer.rootBone == null && skinnedRenderer.rootBone != null)
                        {
                            skinnedMeshRenderer.rootBone = dictionary[skinnedRenderer.rootBone.name];
                        }
                        int[] bonesMapping = new int[skinnedRenderer.sharedMesh.bindposes.Length];
                        EnsureBones(keyValuePair.Key, list, list3, bonesMapping, dictionary);
                        foreach (Vector2 item in skinnedRenderer.sharedMesh.uv)
                        {
                            list5.Add(item);
                        }
                        CombineInstance item2 = default(CombineInstance);
                        item2.mesh = skinnedRenderer.sharedMesh;
                        item2.transform = Matrix4x4.identity;
                        InsertBoneWeights(list2, bonesMapping, skinnedRenderer);
                        list4.Add(item2);
                    }
                }
                mesh.CombineMeshes(list4.ToArray());
                //mesh.bindposes = list3.ToArray();
                //mesh.boneWeights = list2.ToArray();
                mesh.uv = list5.ToArray();
                mesh.RecalculateBounds();
                mesh.UploadMeshData(false);
                skinnedMeshRenderer.sharedMesh = mesh;
                skinnedMeshRenderer.bones = list.ToArray();
                skinnedMeshRenderer.gameObject.layer = 9;
                skinnedMeshRenderer.sharedMaterial = m_AtlasMaterial;
                if (BuildModeUtility.Data.DisableShadowsOnDynamicObjects)
                {
                    skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                //this.m_Animator.Rebind();

                return skinnedMeshRenderer.gameObject;
            }
            else return null;

            Dictionary<string, Transform> CacheHierarchy(GameObject obj, IEnumerable<Transform> transforms)
            {
                foreach (var t in transforms)
                    t.SetParent(obj.transform, true);
                Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
                Stack<Transform> stack = new Stack<Transform>();
                stack.Push(obj.transform);
                while (stack.Count > 0)
                {
                    Transform transform = stack.Pop();
                    if (!dictionary.ContainsKey(transform.name))
                    {
                        dictionary.Add(transform.name, transform);
                    }
                    int childCount = transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        stack.Push(child);
                    }
                }
                return dictionary;
            }
            void EnsureBones(Ex.Kingmaker.Visual.CharacterSystem.BodyPart bodyPart, List<Transform> bones, List<Matrix4x4> bindposes, int[] bonesMapping, Dictionary<string, Transform> cachedBones)
            {
                Matrix4x4[] bindposes2 = bodyPart.SkinnedRenderer.sharedMesh.bindposes;
                Transform[] bones2 = bodyPart.SkinnedRenderer.bones;
                for (int i = 0; i < bones2.Length; i++)
                {
                    Transform transform = bones2[i];
                    int num = -1;
                    Transform item;
                    if (!cachedBones.TryGetValue(transform.name, out item))
                    {
                        return;
                    }
                    for (int j = 0; j < bones.Count; j++)
                    {
                        if (Character.CompareSkinningMatrices(bindposes[j], ref bindposes2[i]) && bones2[i].transform.name == bones[j].name)
                        {
                            num = j;
                            break;
                        }
                    }
                    if (num < 0)
                    {
                        num = bones.Count;
                        bones.Add(item);
                        bindposes.Add(bindposes2[i]);
                    }
                    bonesMapping[i] = num;
                }
            }
            void InsertBoneWeights(List<BoneWeight> boneWeights, int[] bonesMapping, SkinnedMeshRenderer renderer)
            {
                var arr = renderer.sharedMesh.boneWeights;
                for (int i =0; i < arr.Length; i++)
                {
                    var item = arr[i];
                    item.boneIndex0 = bonesMapping[item.boneIndex0];
                    item.boneIndex1 = bonesMapping[item.boneIndex1];
                    item.boneIndex2 = bonesMapping[item.boneIndex2];
                    item.boneIndex3 = bonesMapping[item.boneIndex3];
                    boneWeights.Add(item);
                }
            }

        }


        public static bool HasAlphaChannel(Texture2D tex)
        {
            TextureFormat format = tex.format;
            if (format <= TextureFormat.RGB9e5Float)
            {
                if (format != TextureFormat.RGB24)
                {
                    switch (format)
                    {
                        case TextureFormat.RGB565:
                        case TextureFormat.R16:
                        case TextureFormat.DXT1:
                        case TextureFormat.RHalf:
                        case TextureFormat.RGHalf:
                        case TextureFormat.RFloat:
                        case TextureFormat.RGFloat:
                            break;
                        case (TextureFormat)8:
                        case (TextureFormat)11:
                        case TextureFormat.DXT5:
                        case TextureFormat.RGBA4444:
                        case TextureFormat.BGRA32:
                        case TextureFormat.RGBAHalf:
                            return true;
                        default:
                            if (format != TextureFormat.RGB9e5Float)
                            {
                                return true;
                            }
                            break;
                    }
                }
            }
            else if (format - TextureFormat.BC4 > 2 && format - TextureFormat.RG16 > 1 && format - TextureFormat.RG32 > 1)
            {
                return true;
            }
            return false;
        }



        static ProjectileController projectileController = new ProjectileController()
        {
            m_ViewParent = GameObject.Find("__Projectiles__")?.transform ?? new GameObject("__Projectiles__").transform
        };
        //[MenuItem("Examples/TryProjectile")]
        public static void LaunchProjectile()
        {
            if (!Launched) return;

            try
            {
                var launcher = new TargetWrapper(new Vector3(0, 0, 0));
                var target = new TargetWrapper(new Vector3(15, 8, 0));
                var projectileBlueprint = ResourcesLibrary.TryGetBlueprint<Ex.Kingmaker.Blueprints.BlueprintProjectile>("467caea41e0c2144b9e78c31f49bb288");

                Projectile projectile = projectileController.CreateProjectile(launcher, target, projectileBlueprint);
                if (projectile == null)
                {
                    Debug.LogError("Failed to create a projectile!");
                    return;
                }
                projectileController.CreateView(projectile, launcher.IsUnit ? null : new Vector3?(launcher.Point));

                if (projectile?.View == null)
                {
                    Debug.LogError("Failed to create a projectile view!");
                    return;
                }
                foreach (var rv in projectile.View.GetComponentsInChildren<RayView>())
                {
                    rv.Awake();
                    Vector3 sourcePoint = rv.SourcePoint;
                    Vector3 targetPoint = rv.TargetPoint;
                    float fullDistance = rv.FullDistance;
                    Vector3 direction = targetPoint - sourcePoint;
                    UpdateVerices(rv, sourcePoint, fullDistance, direction);
                }
                projectileController.AddProjectile(projectile);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            static void UpdateVerices(RayView view, Vector3 source, float fullDistance, Vector3 direction)
            {
                view.m_VerticesCount = Mathf.Min(255, Mathf.CeilToInt(fullDistance / view.m_DistanceBetweenVertices) + 1);
                float num = fullDistance / (float)(view.m_VerticesCount - 1);
                float d = 0f;
                if (view.m_SnapToGround)
                {
                    Vector3 vector;
                    d = Math.Min(1f, RayView.DistanceToGround(source, out vector) ?? 1f);
                }
                float num2 = 0f;
                float progress = 0f;
                for (int i = 0; i < view.m_VerticesCount; i++)
                {
                    Vector3 b = TrajectoryCalculator.CalculateShift(view.m_Trajectory.Get(), direction, fullDistance, num2, progress, view.m_LifeTime, view.m_Projectile.InvertUpDirectionForTrajectories);
                    Vector3 vector2 = source + direction.normalized * num2;
                    if (view.m_SnapToGround && i > 0)
                    {
                        Vector3 a;
                        float? num3 = RayView.DistanceToGround(vector2, out a);
                        if (num3 != null)
                        {
                            vector2.y -= num3.Value;
                            vector2 += a * d;
                        }
                        else if (i > 0)
                        {
                            vector2.y = view.m_Vertices[i - 1].y;
                        }
                    }
                    view.m_Vertices[i] = vector2 + b;
                    num2 = Math.Min(fullDistance, num2 + num);
                    progress = num2 / fullDistance;
                }
                view.m_LineRenderer.positionCount = view.m_VerticesCount;
                view.m_LineRenderer.SetPositions(view.m_Vertices);
            }
        }

        public static void DoImportEquipmentEntity(string assetName)
        {
            AssetBundle ab = BundlesLoadService.Instance.RequestBundle("equipment");
            var o = ab.LoadAsset(assetName);
            Debug.Log($"Tried to load {assetName}. Asset is null? {o == null}. Type is {o.GetType()}");
            if (o.GetType() != typeof(GameObject)) return;
            var Renderers = (o as GameObject).GetComponentsInChildren<Renderer>(true);
            Debug.Log($"Found {Renderers.Length} renderers.");
            foreach (var r in Renderers)
            {
                if (r.sharedMaterials.Length > 0)
                    foreach (var m in r.sharedMaterials)
                        m.DuplicateTexture();
            }
            Selection.activeObject = GameObject.Instantiate(o);
        }
        public static GameObject DoImportWeapon(BlueprintItemWeapon bp)
        {
            if (bp is null)
            {
                Debug.LogError("Weapon blueprint is null! Can not import!");
                return null;
            }
            var visualParameters = bp.VisualParameters;
            if (visualParameters == null)
            {
                Debug.LogError($"Weapon blueprint {bp.name} has no visual parameters! Can not import!");
                return null;
            }
            GameObject weaponModel = visualParameters.Model;
            if (weaponModel == null)
            {
                Debug.LogError("Weapon has null model! Can not import!");
                return null;
            }
            var instance = GameObject.Instantiate(weaponModel);
            var weaponEquipLink = instance.GetComponentInChildren<WeaponEquipLinks>();
            if (weaponEquipLink == null)
                return instance;
            if (weaponEquipLink.SheathModel != null)
                GameObject.Instantiate(weaponEquipLink.SheathModel);
            if (weaponEquipLink.BeltModel != null)
                GameObject.Instantiate(weaponEquipLink.BeltModel);
            return instance;
        }

        [MenuItem("Examples/TryMainMenu")]
        public static void TryMainMenu()
        {

            if (!Launched)
                return;
            GameObject ass = null;
            MainMenuPCView c = null;
            try
            {
                ApplicationPaths.Init();
                RootUIContext.InitializeUIKitDependencies();
                var ab = AssetBundle.LoadFromFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Pathfinder Second Adventure\\Bundles/mainmenupcview.res");
                //Debug.Log($"There are {ab.GetAllAssetNames().Count()} assets in the bundle: {string.Join(", ", ab.GetAllAssetNames())}");
                try
                {
                    ass = GameObject.Instantiate(ab.LoadAllAssets()[0]) as GameObject;
                    c = ass.GetComponent<MainMenuPCView>();
                    c.DoInitialize();
                    c.SetupTexture(new Kingmaker.EntitySystem.Persistence.SaveInfo());
                }
                catch (Exception Ex)
                {
                    Debug.LogException(Ex);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception 1 is here");
                Debug.LogException(e);
            }
            try
            {
                //c?.Bind(new Kingmaker.UI.MVVM._VM.MainMenu.MainMenuVM());
            }
            catch (Exception e)
            {
                Debug.Log("Exception 2 is here");
                Debug.LogException(e);
            }

            //try
            //{
            //    if (room == null) return;
            //    room.CreateDefaultDolls();
            //    bool visible = true;
            //    room.gameObject.SetActive(visible);
            //    Debug.Log($"Object of doll room's parent object is {room.gameObject.transform.parent.gameObject}");
            //    if (visible)
            //    {
            //        room.m_DisabledLights = UnityEngine.Object.FindObjectsOfType<Light>().Except(room.GetComponentsInChildren<Light>()).ToArray<Light>();
            //        EventBus.Subscribe(room);
            //        room.DollRoomVisualSettingsOn();
            //    }
            //    room.m_DisabledLights.EmptyIfNull<Light>().ForEach(delegate (Light l)
            //    {
            //        if (l && l.enabled)
            //        {
            //            l.gameObject.SetActive(!room);
            //        }
            //    });
            //    if (!room)
            //    {
            //        room.m_DisabledLights = null;
            //        EventBus.Unsubscribe(room);
            //        if (room.m_OriginalAvatar != null)
            //        {
            //            room.m_OriginalAvatar.enabled = true;
            //        }
            //        room.m_OriginalAvatar = null;
            //        room.DollRoomVisualSettingsOff();
            //        room.Cleanup();
            //        room.SetFogOfWarEnabled(!visible);
            //        //PositionBasedDynamicsConfig.Instance.UpdateMode = (visible ? UpdateMode.UnscaledGameTime : UpdateMode.GameTime);
            //    }
            //}
            //catch(Exception ex)
            //{ Debug.LogException(ex); }

        }

        [MenuItem("Examples/TryDoll")]
        public static void TryDoll()
        {

            Kingmaker.UI.ServiceWindow.DollRoom room;
            //for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            //    foreach (var go in EditorSceneManager.GetSceneAt(i).GetRootGameObjects())
            //    {
            //        room = go.GetComponentInChildren<Kingmaker.UI.ServiceWindow.DollRoom>();
            //        if (room != null) break;
            //    }

            room = Selection.activeGameObject.GetComponent<Kingmaker.UI.ServiceWindow.DollRoom>();

            Debug.Log($"DollRoom is null? {room == null}");
            if (room != null)
            {
                var data = new Kingmaker.EntitySystem.Entities.UnitEntityData(Guid.NewGuid().ToString(), true, ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("397b090721c41044ea3220445300e1b8"));
                var unitEntityView = data.CreateView();
                unitEntityView.UniqueId = data.UniqueId;
                unitEntityView.DisableSizeScaling = true;
                unitEntityView.Blueprint = data.Blueprint;
                data.AttachToViewOnLoad(unitEntityView);
                Selection.activeGameObject = data.View.gameObject;
                room.CreateDefaultDolls();
                room.SetupInfo(data);
                var weapon = new Kingmaker.Items.ItemEntityWeapon(  ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("7d88fb635e054783aab93ee8af382866"));
                var weapon2 = new Kingmaker.Items.ItemEntityWeapon(ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("7d88fb635e054783aab93ee8af382866"));
                var hands = room.m_AvatarHands;
                room.m_Avatar.IsInDollRoom = true;
                var oldWeapon = hands.m_ActiveSet.MainHand.Slot.MaybeItem;
                hands.m_ActiveSet.MainHand.Slot.InsertItem(weapon);
                hands.m_ActiveSet.MainHand.Slot.HandsEquipmentSet.m_GripType = GripType.TwoHanded;
                room.HandleEquipmentSlotUpdated(hands.m_ActiveSet.MainHand.Slot, oldWeapon);
                oldWeapon = hands.m_ActiveSet.OffHand.Slot.MaybeItem;
                hands.m_ActiveSet.OffHand.Slot.InsertItem(weapon2);
                room.HandleEquipmentSlotUpdated(hands.m_ActiveSet.OffHand.Slot, oldWeapon);
                //hands.RedistributeSlots();
                hands.HandleEquipmentSetChanged(true);
            }
        }




        //[MenuItem("Examples/TryLoadScene")]
        public static void TryLoadScene()
        {
            if (!Launched) return;

            try
            {
                SceneReference scene = new SceneReference();
                scene.m_SceneName = "EmberQ1_BaphometsAltar_Light";
                //
                //Debug.Log($"Bundle is loaded? {bundle != null}");
                //if (bundle != null)
                //{
                //    string[] scenePaths = bundle.GetAllScenePaths();
                //    Debug.Log($"Scenes in the bundle are: {string.Join(",", scenePaths)}");
                //    if (scenePaths.Length > 0)
                //    {
                //        string FirstPath = Path.GetFileNameWithoutExtension(scenePaths[0]);
                //        Debug.Log($"Looking for the scene at path {FirstPath}");
                //        EditorSceneManager.OpenScene(FirstPath + ".unity");
                //        Debug.Log("Opened a scene");
                //    }

                //}
                var bundle = BundlesLoadService.Instance.RequestBundle(BundledSceneLoader.GetBundleName(scene.m_SceneName));
                Debug.Log($"Bundle is null? {bundle == null}. " + (bundle == null ? "" : $"Bundle contains following streamed scenes: {string.Join(",", bundle.GetAllScenePaths())}"));
                Scene sceneByName = SceneManager.GetSceneByName(scene.SceneName);
                Debug.Log("Scene has path? " + (sceneByName.path.IsNullOrEmpty() ? "No" : $"Yes, path is {sceneByName.path}"));
                bool flag = GameScenes.ScenesToHaveOwnPhysics.Any((string _sceneName) => _sceneName == scene.m_SceneName);
                try
                {
                    string path = bundle.GetAllScenePaths().FirstItem();
                    Debug.Log($"Trying to open the scene {path}. Bundle is not null? {bundle != null}.");
                    EditorSceneManager.
                        OpenScene(path);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                if (!Directory.Exists("Assets/Scenes"))
                    Directory.CreateDirectory("Assets/Scenes");
                EditorSceneManager.SaveScene(sceneByName, "Assets/Scenes" + scene.m_SceneName + ".unity");
                Debug.Log("Finished loading");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }

    public static class Extensions
    {
        public static void DuplicateTexture(this Material material)
        {
            try
            {
                var textures = material.GetAllTexturesFromMaterial().Select(t => t.name);
                Debug.Log($"Material {material.name} contains following {material.GetAllTexturesFromMaterial().Count()} textures: {string.Join(", ", textures)}");
                foreach (var text in textures)
                {

                    Texture2D source = material.GetTexture(text) as Texture2D;
                    if (source == null)
                    {
                        Debug.Log($"Failed to convert texture {text} into Texture2D");
                        continue;
                    }

                    Debug.Log($"Going for material {material.name}, texture is {source.name}.");
                    RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

                    Graphics.Blit(source, renderTex);
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = renderTex;
                    Texture2D readableText = new Texture2D(source.width, source.height);
                    readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                    readableText.Apply();
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(renderTex);
                    material.mainTexture = readableText;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        public static Texture2D DuplicateTexture(this Texture2D source)
        {
            try
            {

                if (source == null)

                    Debug.Log($"DuplicateTexture - source Texture2D is null");
                RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

                Graphics.Blit(source, renderTex);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTex;
                Texture2D readableText = new Texture2D(source.width, source.height);
                readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                readableText.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTex);
                return readableText;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct MeshBuffer
        {
            public Vector3 position;
            public float normalX, normalY;
            public float tangent1, tangent2, tangent3, tangent4;
        }
    }
}
