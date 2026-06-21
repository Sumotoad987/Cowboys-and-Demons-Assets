using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.BundlesLoading;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utils.Locator;
using Owlcat.Runtime.UI.Utility;
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HarmonyLib;
using System.IO;
using DG.Tweening;
using System.Reflection;
using Diag = System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.SharedTypes;
using System.Diagnostics;
using static UnityEngine.Debug;
using Kingmaker.Visual.CharacterSystem;
//using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Settings;
using Kingmaker.Settings.LINQ;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Localization;
using System.Reflection.Emit;
using Kingmaker.UI.MVVM._VM.MainMenu;
using Kingmaker.Editor.Utility;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.UI.MVVM;
using Kingmaker.View;
using Ex.Kingmaker.Visual.Sound;
using Ex.Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Epic.OnlineServices;
using Ex.Kingmaker.Visual.Animation;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Class.LevelUp;
using System.Text.RegularExpressions;
using UnityEditor.UIElements;
using static Ex.LayoutRedirectElement;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Ex.Kingmaker.DLC;
using Mono.Cecil.Cil;
using Owlcat.QA.Validation;
using Kingmaker.Cheats;
using Owlcat.QA.Utility;
using Newtonsoft.Json.Linq;
using Owlcat.Runtime.Visual.RenderPipeline;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering.Details;
using Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering;
using UnityEditor.VersionControl;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using System.Runtime.InteropServices;

namespace MyOwlcatModification
{
    [HarmonyPatch]
    //[InitializeOnLoad]
    public class Launcher
    {
        static readonly Harmony harmony = new Harmony("UnityEditor_Mods");
        static readonly string[] tags = new[]
        {
            "SecondarySelection"
        };

        public Launcher()
        {
        }

        public static bool Launched
        {
            get
            {
                if (!LaunchedInternal)
                    DoLaunch();
                return LaunchedInternal;
            }
        }
        static bool LaunchedInternal;

        [MenuItem("Importers/Launch")]
        public static void Launch()
        {
            DoLaunch();
        }

        public static void DoLaunch()
        {


            try
            {
                var objects = AssetDatabase.LoadAllAssetsAtPath(Path.Combine("ProjectSettings", "TagManager.asset"));
                SerializedObject tagManager = new SerializedObject(objects[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                foreach (string tag in tags)
                {
                    bool found = false;
                    for (int i = 0; i < tagsProp.arraySize; i++)
                    {
                        SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                        if (t.stringValue.Equals(tag))
                        { found = true; break; }
                    }
                    if (found)
                        continue;
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = tag;
                    tagManager.ApplyModifiedProperties();
                }


            }
            catch (Exception e)
            {
                LogException(e);
            }

            if (LaunchedInternal) return;
            Harmony.DEBUG = true;
            harmony.PatchAll();

            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Initializing app paths", 0);
                ApplicationPaths.Init();
                Log($"Data Path is {ApplicationPaths.dataPath}");
                Log($"Persistent Path is {ApplicationPaths.persistentDataPath}");
                Log($"StartUp json path is {typeof(BuildModeUtility).GetField("s_JsonFile", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) ?? "DialogOptOutDecisionType NOT GET VALUE"}");

            }

            var originalScene = EditorSceneManager.GetActiveScene();
            string bm_name = "UI_LoadingScreen_Scene";
            try
            {
                EditorSceneManager.OpenScene($"Assets/{bm_name}.unity", OpenSceneMode.Additive);
                var basicMechanics = EditorSceneManager.GetSceneByName(bm_name);
                if (basicMechanics == null)
                {
                    Log("UI scene was null");
                    basicMechanics = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    basicMechanics.name = bm_name;
                }
                EditorSceneManager.SetActiveScene(basicMechanics);
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorSceneManager.SetActiveScene(originalScene);
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Configuring the logger", (float)0.1);
                LoggingConfiguration.Configure();
                Log(LoggingConfiguration.Configuration.LogFullPath);
                PFLog.Default.Log("test");

            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Overring bundles path to WrathPath", (float)0.2);
                const string WotrDirectoryKey = "wotr_directory";
                string jsonFilePath = typeof(BuildModeUtility).GetField("s_JsonFile", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as string;
                if (!File.Exists(jsonFilePath))
                {
                    var startUpJson = new StartupJson()
                    {
                        ForceLogging = true,
                        OverrideBundlesPath = WotrDirectoryKey
                    };
                    using var writer = new StreamWriter(jsonFilePath);
                    writer.WriteLine(JsonUtility.ToJson(startUpJson, true));
                }
                if (!BuildModeUtility.
                    Data?.
                    OverrideBundlesPath.
                    Contains(WotrDirectoryKey) ?? false)
                {
                    LogError("OverrideBundlesPath does not contain wotr_directory");
                    EditorUtility.ClearProgressBar();
                    return;
                }
                string WP = Path.Combine(EditorPrefs.GetString(WotrDirectoryKey), "Bundles");
                PFLog.Default.Log($"OverrideBundlesPath contains wotr_directory. Trying to substitute with '{WP}'");
                if (!string.IsNullOrEmpty(WP))
                {
                    BuildModeUtility.Data.OverrideBundlesPath = BuildModeUtility.Data.OverrideBundlesPath.Replace(WotrDirectoryKey, WP);
                    ApplicationPaths.streamingAssetsPath = Path.Combine(BuildModeUtility.Data.OverrideBundlesPath, "..", "Wrath_Data", "StreamingAssets");
                }
                else
                {
                    LogError("Project prefs doesn't contain wotr_directory path!");
                    EditorUtility.ClearProgressBar();
                    EditorSceneManager.SetActiveScene(originalScene);
                    return;
                }
                Log($"Name for bundles folder is {AssetBundleNames.GetBundlesFolder()}");
                Log($"StreamingAssets path is {ApplicationPaths.streamingAssetsPath}");
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorSceneManager.SetActiveScene(originalScene);
                return;
            }

            StartGameLoader starter = new StartGameLoader();
            PFLog.Default.Log("Created new StartGameLoader");
            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Preparing Type cache", (float)0.3);
                //starter.PrepareTypeCache(); 
                GuidClassBinder guidClassBinder = (GuidClassBinder)Json.Serializer.Binder;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    foreach (Type type in assembly.GetTypes())
                    {
                        try
                        {
                            TypeIdAttribute customAttribute = type.GetCustomAttribute<TypeIdAttribute>();
                            if (customAttribute != null)
                            {
                                guidClassBinder.AddToCache(type, customAttribute.GuidString);
                            }
                        }
                        catch (ReflectionTypeLoadException rtl)
                        {
                            PFLog.Default.Exception(rtl, $"ReflectionTypeLoadException when loading type {type.FullName}");
                            foreach (var e in rtl.LoaderExceptions)
                                LogException(e);

                        }
                        catch (MissingMethodException mme)
                        {
                            PFLog.Default.Exception(mme, $"MissingMethodException when loading type {type.FullName}");

                        }
                    }
                PFLog.Default.Log("Prepared Type Cache");
            }
            catch (Exception ex)
            {
                LogException(ex);
                if (!(ex is ReflectionTypeLoadException) || !(ex is MissingMethodException))
                {
                    EditorUtility.ClearProgressBar();
                    EditorSceneManager.SetActiveScene(originalScene);
                    return;
                }

            }

            EditorUtility.DisplayProgressBar("Launching minimal game services", "Ensuring Bundles Load Service", (float)0.4);
            PFLog.Default.Log("Trying to ensure BundlesLoadService");
            try
            {
                if (Services.GetInstance<BundlesLoadService>() == null)

                    Services.RegisterServiceInstance(new BundlesLoadService());
            }
            catch (Exception e)
            {
                LogException(e);
            }

            try
            {
                Game.EnsureServices();
                Services.RegisterDefaultServices();
            }
            catch (Exception e)
            {
                LogException(e);
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Requesting Common Bundles", Convert.ToSingle(0.5));
                PFLog.Default.Log("Running Common Bundles coroutine");
                IEnumerator routine = BundlesLoadService.Instance.RequestCommonBundlesCoroutine();
                while (routine.MoveNext()) { };
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Loading Direct References", Convert.ToSingle(0.6));
                PFLog.Default.Log("Loading Direct References");
                starter.LoadDirectReferencesList();
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorSceneManager.SetActiveScene(originalScene);
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Loading Pack", 0.7f);
                PFLog.Default.Log("Loading Pack");
                starter.LoadPackTOC();
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorSceneManager.SetActiveScene(originalScene);
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Loading imported blueprints", 0.7f);
                //PFLog.Default.Log("Loading Blueprints library");
                //PFLog.Default.Log($"There are {Directory.EnumerateFiles(Path.Combine("Assets", "Blueprints Library"), "*.jbp", SearchOption.AllDirectories).Count()} blueprints in the library");
                foreach (string pathOfPotentialBlueprint in Directory.EnumerateFiles(Path.Combine("Assets", "Blueprints Library"), "*.jbp", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(Path.Combine("Assets", "Modifications"), "*.jbp", SearchOption.AllDirectories)))
                {
                    //Log($"Trying to add blueprint from path {pathOfPotentialBlueprint}");
                    var blueprint = AssetDatabase.LoadAssetAtPath<Ex.Kingmaker.Blueprints.SimpleBlueprint>(pathOfPotentialBlueprint);
                    var importer = AssetImporter.GetAtPath(pathOfPotentialBlueprint) as BlueprintImporter;
                    if (importer != null && importer.wrapper != null && blueprint != null)
                        blueprint.AssetGuid = BlueprintGuid.Parse(importer.wrapper.AssetId);
                    //Log($"potential blueprint is null? {blueprint == null}. Has a blueprint importer? {importer != null}. Has a wrapper with ID? {importer?.wrapper == null}, ID is {importer?.wrapper?.AssetId ?? "null"}. AssetId is not empty now? {blueprint?.AssetGuid != BlueprintGuid.Empty}");
                    if (blueprint != null && blueprint.AssetGuid != BlueprintGuid.Empty)
                        ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(blueprint.AssetGuid, blueprint);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorSceneManager.SetActiveScene(originalScene);
                return;
            }

            try
            {

                LocalizationManager.Init();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Trying to fix TMP", 0.8f);
                TMP_Settings tmp = ScriptableObject.CreateInstance<TMP_Settings>();
                typeof(TMP_Settings).GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, tmp);
                typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, BlueprintRoot.Instance.UIRoot.DefaultTMPFontAsset);
                typeof(TMP_Settings).GetField("m_defaultSpriteAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, BlueprintRoot.Instance.UIRoot.DefaultTMPSriteAsset);
            }
            catch (Exception e)
            {
                LogException(e);
                EditorUtility.ClearProgressBar();
            }


            try
            {
                EditorUtility.DisplayProgressBar("Launching minimal game services", "Initializing the localization manager", 0.9f);
                Kingmaker.Localization.LocalizationManager.Init();
                SettingsRoot.Graphics.TexturesQuality.SetValueAndConfirm(QualityOption.High);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }


            EditorUtility.DisplayProgressBar("Launching minimal game services", "Finished loading. Trying to load basicMechanics bundle", Convert.ToSingle(1));
            LaunchedInternal = true;
            try
            {

                var rootUI_Config = RootUIContext.Instance.GetConfig(bm_name) ?? new GameObject(bm_name + "_UI_Config").AddComponent<RootUIConfig>();
                if (RootUIContext.Instance.CommonVM == null)
                {
                    RootUIContext.Instance.CommonVM = new Kingmaker.UI.MVVM._VM.Common.CommonVM();
                }
                RootUIContext.Instance.m_CommonView = rootUI_Config.TryCreateView(RootUIContext.Instance.CommonVM);
                RootUIContext.Instance.m_CommonView.hideFlags = HideFlags.DontSave;
                //EditorSceneManager.MoveGameObjectToScene(RootUIContext.Instance.m_CommonView.gameObject, originalScene);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            finally
            {
                EditorSceneManager.SetActiveScene(originalScene);
            }
            EditorUtility.ClearProgressBar();

        }



        [HarmonyPatch(typeof(BuildModeUtility), "JsonPath", MethodType.Getter)]
        [HarmonyPrefix]
        static bool PatchPath(ref string __result)
        {

            //if (!Application.isEditor) return true;
            __result = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            return false;
        }

        [HarmonyPatch(typeof(SoundFx), nameof(SoundFx.PlayWithDelay))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSoundFxPlayWithDelay(IEnumerable<CodeInstruction> instructions)
        {

            var _ins = instructions.ToList();
            int index = _ins.FindIndex(i => i.Calls(typeof(LoadingProcess).GetProperty(nameof(LoadingProcess.Instance)).GetMethod));
            if (index == -1)
                return instructions;
            _ins[index] = new CodeInstruction(OpCodes.Ldc_I4_1);
            _ins[index + 1] = new CodeInstruction(OpCodes.Nop);
            return _ins;
        }

        [HarmonyPatch(typeof(EntityViewBase), nameof(EntityViewBase.OnEnable))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchEntityViewBaseOnEnable(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var i in instructions)
                if (!i.Calls(typeof(Application).GetProperty(nameof(Application.isPlaying)).GetMethod))
                    yield return i;
                else yield return new CodeInstruction(OpCodes.Ldc_I4_1).MoveBlocksFrom(i).MoveLabelsFrom(i);
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.UI.MVVM._PCView.ChoseControllerMode.GamepadConnectedInKeyboardModeWindowView), "BindViewImplementation")]
        [HarmonyPrefix]
        static bool PatchGamepadConnectedInKeyboardModeWindowView()
        {
            return false;
        }

        [HarmonyPatch(typeof(ReflectionBasedSerializer), nameof(ReflectionBasedSerializer.CreateObject))]
        [HarmonyPrefix]
        static bool PatchCreateObject(ref object __result, Type type)
        {
            if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                __result = ScriptableObject.CreateInstance(type);
                return false;
            }
            else return true;
        }

        [HarmonyPatch(typeof(Kingmaker.Utility.Utils), nameof(Kingmaker.Utility.Utils.EditorSafeDestroy))]
        [HarmonyPrefix]
        static bool PatchEditorSafeDestroy(UnityEngine.Object obj)
        {
            if (Application.isEditor)
            {
                UnityEngine.Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(UnityObjectConverter), nameof(UnityObjectConverter.CanConvert))]
        [HarmonyPostfix]
        static void PatchUnityObjectConverterCanConvert(ref bool __result, Type objectType)
        {
            if (!__result) return;
            else
            {
                __result = !objectType.IsOrSubclassOf<Ex.Kingmaker.Blueprints.SimpleBlueprint>()
                    && !objectType.IsOrSubclassOf<Ex.Kingmaker.Blueprints.JsonSystem.BlueprintJsonWrapper>();
                //Log($"Type is {objectType}. Result is {r}");
            }
        }


        [HarmonyPatch(typeof(UnityObjectConverter), nameof(UnityObjectConverter.WriteJson))]
        [HarmonyPrefix]
        static bool PatchUnityObjectConverterWrite(JsonWriter writer, object value)
        {
            var obj = value as UnityEngine.Object;
            if (obj == null || !AssetDatabase.Contains(obj) || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId))
                return true;


            writer.WriteStartObject();
            writer.WritePropertyName("guid");
            writer.WriteValue(guid);
            writer.WritePropertyName("fileid");
            writer.WriteValue(localId);
            writer.WriteEndObject();

            return false;
        }
        [HarmonyPatch(typeof(UnityObjectConverter), nameof(UnityObjectConverter.ReadJson))]
        [HarmonyPrefix]
        static bool PatchUnityObjectConverterRead(JsonReader reader, Type objectType, ref object __result)
        {

            if (reader.TokenType == JsonToken.Null)
            {
                return false;
            }
            JObject jobject = JObject.Load(reader);
            string text = (string)jobject["guid"];
            long? num = (long?)jobject["fileid"];
            if (text == null || num == null)
            {
                return false;
            }


            var path = AssetDatabase.GUIDToAssetPath(text);

            // this is a super dumb way to load an asset by fileId, but Unity does not have anything better )-8
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path); 
            foreach (var asset in allAssets)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long fileId2) && fileId2 == num)
                {
                    __result = asset;
                    return false;
                }
            }

            if (UnityObjectConverter.AssetList)
            {
                UnityEngine.Object @object = UnityObjectConverter.AssetList.Get(text, num.Value);
                if (@object != null)
                {
                    __result = @object;
                    return false;
                }
            }
            Log($"ExtractBlueprintDirectReferences - failed to find asset with guid {text} and fileId {num?.ToString() ?? "null"}");

            return false;
        }

        [HarmonyPatch(typeof(MainMenuVM), nameof(MainMenuVM.GetIntroductoryText))]
        [HarmonyPrefix]
        static bool PatchIntroductoryYext(ref string __result)
        {
            __result = "Here should be Motivational Text";
            return false;
        }

        [HarmonyPatch(typeof(Owlcat.Runtime.UI.ConsoleTools.GamepadInput.GamePad), "UpdateConsoleType")]
        [HarmonyPrefix]
        static bool PatchUpdateConsoleType()
        {
            return false;
        }

        [HarmonyPatch(typeof(UniRx.MainThreadDispatcher), "Awake")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchThreadDispatcher(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            Log("Trying to transpile MainThreadDispatcher");

            int index = -1;
            index = instructions.FindIndex(instruction => instruction.Calls(typeof(UnityEngine.Object).GetMethod(nameof(UnityEngine.Object.DontDestroyOnLoad))));

            if (index == -1) { Log("Failed to find the corrext index!"); return instructions; }
            var _ins = instructions.ToList();

            Label label = gen.DefineLabel();
            _ins[index + 1].labels.Add(label);

            var n = new[]
            {
                new CodeInstruction(OpCodes.Call, typeof(Application).GetProperty(nameof(Application.isEditor)).GetMethod),
                new CodeInstruction(OpCodes.Brtrue_S, label)
            };
            _ins.InsertRange(index - 2, n);
            return _ins;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.MainMenu), nameof(MainMenu.WarmupDollRoomData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MainMenuWarmupDollRoomData(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
                if (instruction.Calls(typeof(Component).GetMethod(nameof(Component.GetComponentInChildren), new Type[] { typeof(bool) }).MakeGenericMethod(typeof(Ex.Kingmaker.UI.ServiceWindow.DollRoom))))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty(nameof(Component.transform)).GetMethod);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty(nameof(Transform.parent)).GetMethod);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return instruction;
                }
                else yield return instruction;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.UI.Common.Animations.SizeAnimator), nameof(Ex.Kingmaker.UI.Common.Animations.SizeAnimator.RectTransform), MethodType.Getter)]
        [HarmonyPostfix]
        static void PatchSizeAnimatorRectTransform(Ex.Kingmaker.UI.Common.Animations.SizeAnimator __instance, ref RectTransform __result)
        {
            if (__instance.m_RectTransform == null)
                __instance.m_RectTransform = __instance.GetComponent<RectTransform>();
            __result = __instance.m_RectTransform;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.MainMenu), "Start")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchMainMenuAwake(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            Log("Trying to transpile MainMenu");

            int index = -1;
            index = instructions.FindIndex(instruction => instruction.Calls(typeof(UnityEngine.SceneManagement.SceneManager).GetMethod(nameof(UnityEngine.SceneManagement.SceneManager.SetActiveScene))));


            if (index == -1) { Log("Failed to find the corrext index!"); return instructions; }
            var _ins = instructions.ToList();
            _ins.RemoveRange(index - 1, 4);
            return _ins;
        }


        [HarmonyPatch(typeof(Kingmaker.UI.MVVM.RootUIContext), nameof(Kingmaker.UI.MVVM.RootUIContext.InitializeUIKitDependencies))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchUIContextKitDependencies(IEnumerable<CodeInstruction> instructions)
        {
            Log("Trying to transpile RootUIContext");

            int index = -1;
            index = instructions.FindIndex(instruction => instruction.Calls(
                typeof(Ex.Kingmaker.EntitySystem.Persistence.LoadingProcess)
                .GetProperty(nameof(Ex.Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance))
                .GetMethod));

            if (index == -1) {
                Log("Failed to find the corrext index!");
                return instructions;
            }
            var _ins = instructions.ToList();
            _ins.RemoveRange(index, 4);
            return _ins;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.EntitySystem.Persistence.SaveScreenshotManager), nameof(Ex.Kingmaker.EntitySystem.Persistence.SaveScreenshotManager.Instance), MethodType.Getter)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchScreenshotManager(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            Log("Trying to transpile PatchScreenshotManager");

            int index = -1;
            index = instructions.FindIndex(instruction => instruction.Calls(typeof(UnityEngine.Object).GetMethod(nameof(UnityEngine.Object.DontDestroyOnLoad))));

            if (index == -1) {
                Log("Failed to find the corrext index!");
                return instructions;
            }
            var _ins = instructions.ToList();

            Label label = gen.DefineLabel();
            _ins[index + 1].labels.Add(label);

            var n = new[]
            {
                new CodeInstruction(OpCodes.Call, typeof(Application).GetProperty(nameof(Application.isEditor)).GetMethod),
                new CodeInstruction(OpCodes.Brtrue_S, label)
            };
            _ins.InsertRange(index - 2, n);
            return _ins;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.EntitySystem.Persistence.LoadingProcess), nameof(Ex.Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance), MethodType.Getter)]
        [HarmonyPrefix]
        static bool PatchLoadingProcessInstance()
        {
            return !Application.isEditor;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.UI.UICamera), nameof(Ex.Kingmaker.UI.UICamera.Claim))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchUICameraClaim(IEnumerable<CodeInstruction> instructions)
        {
            Log("Trying to transpile PatchUICameraClaim");

            int index = -1;
            index = instructions.FindIndex(instruction => instruction.Calls(typeof(Application).GetProperty(nameof(Application.isPlaying)).GetMethod));


            if (index == -1) { Log("Failed to find the corrext index!"); return instructions; }
            var _ins = instructions.ToList();
            _ins.RemoveRange(index, 2);
            return _ins;
        }

        [HarmonyPatch(typeof(RootUIContext), nameof(RootUIContext.GetConfig))]
        [HarmonyPrefix]
        static bool PatchUICContextGetCongig(string loadedUIScene, Ex.Kingmaker.UI.MVVM.RootUIConfig __result)
        {
            __result ??= EditorSceneManager.GetSceneByName(loadedUIScene).GetRootGameObjects().Select(go => go.GetComponent<RootUIConfig>()).FirstOrDefault();
            return false;
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.UI.BackgroundBlur), nameof(Ex.Kingmaker.UI.BackgroundBlur.OnEnable))]
        [HarmonyPrefix]
        static bool PatchBackgroundBlurOnEnable()
        {
            return Ex.Kingmaker.Visual.BackgroundBlurForUI.Instance != null;
        }


        //[HarmonyPatch(typeof(CharacterBonesSetup), nameof(CharacterBonesSetup.Instance), MethodType.Getter)]
        //[HarmonyPrefix]
        //static bool PatchCharacterBonesSetup(ref CharacterBonesSetup __result)
        //{
        //    if (CharacterBonesSetupInstance == null)
        //    {
        //        CharacterBonesSetupInstance = ScriptableObject.CreateInstance<CharacterBonesSetup>();
        //        CharacterBonesSetupInstance.KnownTransformNames = Extensions.KnownTransformNames;
        //    }
        //    __result = CharacterBonesSetupInstance;
        //    return false;
        //}

        [HarmonyPatch(typeof(Ex.Kingmaker.Visual.CharacterSystem.Character), nameof(Ex.Kingmaker.Visual.CharacterSystem.Character.UpdateCharacter))]
        //[HarmonyPrefix]
        static void CharacterTest(Ex.Kingmaker.Visual.CharacterSystem.Character __instance)
        {
            string[] ent = new[]
            {
               "e7c86166041c1e04a92276abdab68afa",
               "0f0330d28241f77419394399059025de",
           };
            Log("Trying!");
            if (__instance.Gender == Ex.Kingmaker.Visual.CharacterSystem.CharacterStudio.Gender.Male)
            {
                foreach (string e in ent)
                {
                    try
                    {
                        var link = new EquipmentEntityLink() { AssetId = e };
                        var asset = link.Load();
                        Log($"Tried to load ee {e}. Successful? {asset != null}");
                        if (asset != null)
                            __instance.m_EquipmentEntities.Add(asset);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.Visual.CharacterSystem.Character), nameof(Ex.Kingmaker.Visual.CharacterSystem.Character.IsLoadingProcess), MethodType.Getter)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CharacterTest(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Skip(6);
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.Visual.CharacterSystem.Character), nameof(Ex.Kingmaker.Visual.CharacterSystem.Character.BuildMesh))]
        [HarmonyPrefix]
        static void CharBuildMeshTest()
        {
            Log("CharBuildMeshTest");
        }



        [HarmonyPatch(typeof(BlueprintReferenceConverter), nameof(BlueprintReferenceConverter.WriteJson))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        static void PatchBlueprintReferenceConverterWriteJson(object value)
        {
            if (!(value is BlueprintReferenceBase blueprintReferenceBase))
                return;
            if (blueprintReferenceBase.Cached != null)
                blueprintReferenceBase.deserializedGuid = blueprintReferenceBase.Cached.AssetGuid;
        }

        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.name), MethodType.Getter)]
        [HarmonyPostfix]
        static void PatchBlueprintNameGetter(UnityEngine.Object __instance, ref string __result)
        {
            if (__instance is Ex.Kingmaker.Blueprints.SimpleBlueprint blueprint)
                if (blueprint is Ex.Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection)
                    __result = blueprint.name;
        }

        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.name), MethodType.Setter)]
        [HarmonyPrefix]
        static void PatchBlueprintNameSetter(UnityEngine.Object __instance, ref string value)
        {
            if (__instance is Ex.Kingmaker.Blueprints.SimpleBlueprint blueprint)
                value = blueprint.name;
        }

        [HarmonyPatch(typeof(SerializedProperty), nameof(SerializedProperty.editable), MethodType.Getter)]
        [HarmonyPostfix]
        static void PatchSerializedPropertyEditable(SerializedProperty __instance, ref bool __result)
        {
            if (__result is true) return;
            else __result =
                   __instance.serializedObject.targetObject is Ex.Kingmaker.Blueprints.SimpleBlueprint bp
                && UnityEditor.AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(bp)) is BlueprintImporter bpImporter
                && bpImporter != null
                && !bpImporter.wrapper.LoadedFromPath.IsNullOrEmpty();
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.Visual.Animation.IKController), nameof(IKController.SetupFbbik))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> IKController_SetupFbbik_TranspilerToChangeAddComponent(IEnumerable<CodeInstruction> instructions)
        {
            Log($"Trying to transpile IKController SetupFbbik");
            MethodInfo method = AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent), new Type[] { }, new Type[] { typeof(RootMotion.FinalIK.FullBodyBipedIK) });

            if (method is null) foreach (var inst in instructions) yield return inst;
            else foreach (var inst in instructions)
                {
                    if (!inst.Calls(method)) yield return inst;
                    else yield return CodeInstruction.Call(typeof(GameObject), nameof(GameObject.AddComponent), new Type[] { }, new Type[] { typeof(RootMotion.FinalIK.FullBodyBipedIKEx) });
                }
        }

        [HarmonyPatch(typeof(Ex.Kingmaker.Visual.Animation.IKController), nameof(IKController.SetupGrounder))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> IKController_SetupGrounder_TranspilerToChangeAddComponent(IEnumerable<CodeInstruction> instructions)
        {
            Log($"Trying to transpile IKController GrounderFBBIK");
            MethodInfo method = AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent), new Type[] { }, new Type[] { typeof(RootMotion.FinalIK.GrounderFBBIK) });

            if (method is null) foreach (var inst in instructions) yield return inst;
            else foreach (var inst in instructions)
                {
                    if (!inst.Calls(method)) yield return inst;
                    else yield return CodeInstruction.Call(typeof(GameObject), nameof(GameObject.AddComponent), new Type[] { }, new Type[] { typeof(RootMotion.FinalIK.GrounderFBBIKIKEx) });
                }
        }

        [HarmonyPatch(typeof(ReportingUtils), nameof(ReportingUtils.LoadAssignees))]
        [HarmonyPrefix]
        static bool PatchReportingUtilsLoadAssignees()
        {
            return false;
        }

        [HarmonyPatch(typeof(ToolbarCallback), "OnUpdate")]
        [HarmonyPrefix]
        static bool RemoveStupidToolbarCallbackUpdate()
        {
            return false;
        }


        [HarmonyPatch(typeof(Kingmaker.View.Equipment.UnitViewHandSlotData), nameof(Kingmaker.View.Equipment.UnitViewHandSlotData.DestroyModel))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DestroyModelPatch(IEnumerable<CodeInstruction> _instr)
        {
            foreach (var instruction in _instr)
                if (instruction.Calls(AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })))
                    yield return CodeInstruction.Call((UnityEngine.Object obj) => UnityEngine.Object.DestroyImmediate(obj));
                else
                    yield return instruction;
        }

        [HarmonyPatch(typeof(Kingmaker.View.Equipment.UnitViewHandSlotData), nameof(Kingmaker.View.Equipment.UnitViewHandSlotData.DestroySheathModel))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DestroySheathPatch(IEnumerable<CodeInstruction> _instr)
        {
            foreach (var instruction in _instr)
                if (instruction.Calls(AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })))
                    yield return CodeInstruction.Call((UnityEngine.Object obj) => UnityEngine.Object.DestroyImmediate(obj));
                else
                    yield return instruction;
        }

        [HarmonyPatch(typeof(Kingmaker.View.Equipment.UnitViewHandsEquipment), nameof(Kingmaker.View.Equipment.UnitViewHandsEquipment.UpdateBeltPrefabs))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DestroyModelUpdateBeltPrefabsPatch(IEnumerable<CodeInstruction> _instr)
        {
            foreach (var instruction in _instr)
                if (instruction.Calls(AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })))
                    yield return CodeInstruction.Call((UnityEngine.Object obj) => UnityEngine.Object.DestroyImmediate(obj));
                else
                    yield return instruction;
        }



        static readonly Dictionary<string, string> DisplayNamesDictionary = new Dictionary<string, string>();

        [HarmonyPatch(typeof(SerializedProperty), nameof(SerializedProperty.displayName), MethodType.Getter)]
        [HarmonyPrefix]
        static bool PatchSerializedPropertyDisplayName(SerializedProperty __instance, ref string __result)
        {
            if (__instance.serializedObject.targetObject as Ex.Kingmaker.Blueprints.BlueprintScriptableObject == null)
                return true;
            if (!Regex.IsMatch(__instance.propertyPath, @"^Components\.Array\.data\[\d+\]$"))
                return true;
            string name = __instance.FindPropertyRelative("name")?.stringValue;
            if (name.IsNullOrEmpty())
                return true;

            if (DisplayNamesDictionary.TryGetValue(name, out string cached))
            {
                __result = cached;
                return false;
            }


            int i = name.IndexOf('$', 2);
            string nameShort = name;
            if (i != -1)
                nameShort = nameShort.Substring(1, i - 1);
            var strings = Regex.Split(nameShort, @"(?<!^)(?=[A-Z])");
            char index = __instance.propertyPath.ElementAt(__instance.propertyPath.Length - 2);
            string resulted = $"{index}. {string.Join(" ", strings)}";
            DisplayNamesDictionary.Add(name, resulted);
            //var flag = __instance.FindPropertyRelative(nameof(Ex.Kingmaker.Blueprints.BlueprintComponent.m_Flags));
            //if (((flag?.enumValueIndex ?? -999) & (int)Ex.Kingmaker.Blueprints.BlueprintComponent.Flags.Disabled) > 0)
            //    resulted = $"(Disabled) {resulted}";
            __result = resulted;

            return false;
        }

        static string GiveLabelToArrayEntry(SerializedProperty property)
        {
            Log($"GiveLabelToArrayEntry - type is {property.type}");
            if (property.type != nameof(BlueprintComponent))
                return string.Empty;
            string name = property.FindPropertyRelative(nameof(BlueprintComponent.name))?.stringValue;
            if (name.IsNullOrEmpty())
                return "null name";
            else return name;
        }


        [HarmonyPatch(typeof(Ex.Kingmaker.UI.Common.Animations.FadeAnimator), nameof(Ex.Kingmaker.UI.Common.Animations.FadeAnimator.Initialize))]
        [HarmonyPostfix]
        static void PatchFadeAnimatorInitialize(Ex.Kingmaker.UI.Common.Animations.FadeAnimator __instance)
        {
            __instance.CanvasGroup.alpha = 1f;
        }

        [HarmonyPatch(typeof(DlcCondition), nameof(DlcCondition.IsFullfilled))]
        [HarmonyPrefix]
        static bool PatchDlcConditionIsFullfilled(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(GameObjectValidator), "GetAssetPath")]
        [HarmonyPrefix]
        static bool Validatortest3(GameObjectValidator __instance , GameObject objectToValidate, ref string __result)
        {
            string assetPath = AssetDatabase.GetAssetPath(objectToValidate);
            if (!string.IsNullOrEmpty(assetPath))
            {
                __result = assetPath;
                return false;
            }
            var scene = objectToValidate.scene;
            string path = scene.path;
            if (path == string.Empty)
                path = "Untitled scene";
            if (scene.IsValid() && !string.IsNullOrEmpty(path))
            {
                __result = path;
                return false;
            }
            GameObject correspondingObjectFromSource = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(objectToValidate);
            if (correspondingObjectFromSource != null)
            {
                __result = AssetDatabase.GetAssetPath(correspondingObjectFromSource);
                return false;
            }
            return false;
        }

        [HarmonyPatch(typeof(Kingmaker.Cheats.BlueprintList.Entry), nameof(Kingmaker.Cheats.BlueprintList.Entry.Type),MethodType.Getter)]
        [HarmonyPostfix]
        static void FixCheatDataToLookForBlueprintInThisAssembly(ref Type __result, Kingmaker.Cheats.BlueprintList.Entry __instance)
        {
            if (__result != null)
                return;
            __result = typeof(Kingmaker.Blueprints.BlueprintScriptableObject).Assembly.GetType(__instance.TypeFullName);
        }


        [HarmonyPatch(typeof(IKController), nameof(IKController.MountedCombatIk))]
        //[HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchIKControllerMountedCombatIk(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)

                if (instruction.operand is MethodInfo mInfo && mInfo.IsGenericMethod && mInfo.GetGenericArguments().Contains(typeof(Ex.Kingmaker.Visual.Mounts.MountOffsets)))
                    yield return new CodeInstruction(
                        opcode: OpCodes.Callvirt,
                        operand: typeof(GameObject).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(m => m.Name is nameof(GameObject.GetComponent) && m.IsGenericMethodDefinition).MakeGenericMethod(typeof(Kingmaker.Visual.Mounts.MountOffsets)));
                else yield return instruction;
            //var instr = __instructions.ToList();

            //var oldMethod = typeof(GameObject).GetMethod(nameof(GameObject.GetComponent), new Type[] { typeof(Ex.Kingmaker.Visual.Mounts.MountOffsets) });
            //var type = typeof(Kingmaker.Visual.Mounts.MountOffsets);

            //int index = -1;
            //index = instr.FindIndex(i => i.operand as MethodInfo == oldMethod);
            //Log("PatchIKControllerMountedCombatIk - index is " + index);
            //if (index == -1)
            //{
            //    LogError("PatchIKControllerMountedCombatIk transpiler failed to find index");
            //    return __instructions;
            //}

            //instr[index - 1].operand = type;
            //return instr;

        }

        [HarmonyPatch(typeof(PositionBasedDynamicsFeature), nameof(PositionBasedDynamicsFeature.AddRenderPasses))]
        [HarmonyPostfix]
        static void check(PositionBasedDynamicsFeature __instance, ScriptableRenderer renderer)
        {
            var a = PBD.IsEmpty;
            var b = PBD.GetGPUData();
            Simulation sim = AccessTools.DeclaredField(typeof(PBD), "s_Simulation").GetValue(null) as Simulation;

        }
        [HarmonyPatch(typeof(PBDGrassBody), "InitBody")]
        [HarmonyPostfix]
        static void check2(PBDGrassBody __instance)
        {
            var a = PBD.IsEmpty;
            var b = PBD.GetGPUData();
            Simulation sim = AccessTools.DeclaredField(typeof(PBD), "s_Simulation").GetValue(null) as Simulation;

        }
        //[MenuItem("Examples/Init")]
        static void d1o()
        {
            PBDGrassBody grass = Selection.activeGameObject.GetComponent<PBDGrassBody>();
            AccessTools.DeclaredMethod(typeof(PBDGrassBody), "InitBody").Invoke(grass, null);
        }

        [HarmonyPatch(typeof(ResourcesLibrary), nameof(ResourcesLibrary.HookForAssetDatabase))]
        [HarmonyPostfix]
        static void GetResourcesFromDatabaseIntoLibrary(ref bool __result, string assetId, Type type, out UnityEngine.Object @object)
        {
            @object = null;
            var path = AssetDatabase.GUIDToAssetPath(assetId);
            if (path.IsNullOrEmpty())
                return;
            @object = AssetDatabase.LoadAssetAtPath(path, type);
            __result = @object!= null;
        }



        //[HarmonyPatch(typeof(Ex.Kingmaker.Blueprints.BlueprintReferenceBase), nameof(BlueprintReferenceBase.OnAfterDeserialize))]
        //[HarmonyPrefix]
        //static void PatchBlueprintReferenceBaseOnAfterDeserialize()
        //{
        //    Log("PatchBlueprintReferenceBaseOnAfterDeserialize");
        //}
        //[HarmonyPatch(typeof(Ex.Kingmaker.Blueprints.BlueprintReferenceBase), nameof(BlueprintReferenceBase.OnBeforeSerialize))]
        //[HarmonyPrefix]
        //static void PatchBlueprintReferenceBaseOnBeforeSerialize()
        //{
        //    Log("PatchBlueprintReferenceBaseOnBeforeSerialize");
        //}

    }

}

