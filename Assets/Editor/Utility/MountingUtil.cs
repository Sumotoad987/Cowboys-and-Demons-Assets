using Ex.Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Mounts;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Kingmaker.Enums;
using Ex.Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine.Playables;
using Kingmaker.Controllers.Units;

namespace MyOwlcatModification
{
    static class Mounting
    {
        const string RiderBlueprintForQuickSetUp = "5f2733a1f13a0874388ffb78bd216546";
        const string MountBlueprintForQuickSetUp = "9e8727d008bec6e47842ba13df87d939";
        static Size? RiderSizeForQuickSetUp = null; //Size.Medium;
        static Size? MountSizeForQuickSetUp = null; //Size.Large;
        static UnitEntityView mount;
        static UnitEntityView rider;

        static RaceMountOffsetsConfig m_Config;
        static RaceMountOffsetsConfig Config { get { if (m_Config == null) m_Config = GenerateConfig(); return m_Config; } }
        static RaceMountOffsetsConfig GenerateConfig()
        {
            var c = ScriptableObject.CreateInstance<RaceMountOffsetsConfig>();
            c.offsets = new RaceMountOffsetsConfig.MountOffsetData[]
              {
                    new RaceMountOffsetsConfig.MountOffsetData ()
                    {
                      Races = new List<Ex.Kingmaker.Blueprints.BlueprintRaceReference>()
                      {
                        new Ex.Kingmaker.Blueprints.BlueprintRaceReference(){deserializedGuid = BlueprintGuid.Parse("b7f02ba92b363064fb873963bec275ee") } //Aasimar race which I used for testing
                      },
                      RootPosition = new Vector3(-0.8f, -0.9f, 0),
                      RootBattlePosition = new Vector3(0, 0, 0f),
                      PelvisPosition = new Vector3(0, -0.15f, 0),
                      HandsPosition = new Vector3(-0.35f, 0.1f, 0),
                      PelvisRotation = new Vector4(0, 0, 0, 1),
                      RightFootPosition = new Vector3(0, 0.6f, -0.2f),
                      RightFootRotation = new Vector4(-0.7037f, -0.1517f, -0.6364f, -0.2771f),
                      LeftFootPosition = new Vector3(0, 0.6f, 0.2f),
                      LeftFootRotation = new Vector4(0.3729f, -0.6323f, 0.0635f, -0.6761f),
                      RightKneePosition = new Vector3(-0.3f, -0.2f, -0.2f),
                      LeftKneePosition = new Vector3(-0.3f, -0.2f, 0.2f),
                    }
              };
            return c;
        }

        [MenuItem("Mount/Mount-Dismount")]
        public static void Mount()
        {
            var riderPart = rider.Data.Ensure<UnitPartRider>();
            if (riderPart.SaddledUnit != null)
            {
                AssetDatabase.Refresh();
                riderPart.Dismount(true);
                rider.IkController.DoUpdate();
            }
            rider.transform.localRotation = mount.transform.localRotation;
            riderPart.Mount(mount.Data, true);
            try
            {
                rider.IkController.DoUpdate();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        static void DoMountConfig(UnitEntityView __instance)
        {

            var mountTr = __instance.transform.Find(t => t.gameObject.name.EndsWith("_RIG"));
            if (mountTr == null)
            {
                Debug.LogError($"DoMountConfig - did not find Rig");
                return;
            }

            var TorsoTr = mountTr.transform.FindRecursive("UpperTorso");
            if (TorsoTr == null)
            {
                Debug.LogError($"DoMountConfig - did not find torso");
                return;
            }
            GameObject Torso = TorsoTr.gameObject;

            var root = Torso.transform.Find("RootMountPosition")?.gameObject;
            if (root != null)
            {
                Debug.LogWarning($"DoMountConfig - mount already has RootMountPosition");
                return;
            }                
            root = new GameObject("RootMountPosition");
            root.transform.SetParent(Torso.transform);
            root.transform.localEulerAngles = new Vector3(-6.75f, 26.73f, -0.54f);


            var root_combat = new GameObject("RootMountPositionBattle");
            root_combat.transform.SetParent(root.transform);
            var saddle_pelvis = new GameObject("Saddle_Pelvis");
            saddle_pelvis.transform.SetParent(root.transform);

            var saddle_leftfoot = new GameObject("Saddle_LeftFoot");
            saddle_leftfoot.transform.SetParent(root.transform);
            var saddle_rightfoot = new GameObject("Saddle_RightFoot");
            saddle_rightfoot.transform.SetParent(root.transform);
            var saddle_rightknee = new GameObject("Saddle_RIghtKnee");
            saddle_rightknee.transform.SetParent(root.transform);
            var saddle_leftknee = new GameObject("Saddle_LeftKnee");
            saddle_leftknee.transform.SetParent(root.transform);
            var saddle_hands = new GameObject("Saddle_Hands");
            saddle_hands.transform.SetParent(root.transform);

            if (__instance.TryGetComponent<MountOffsets>(out var component)) 
                return;
            component = __instance.gameObject.AddComponent<MountOffsets>();
            
            component.Root = root.transform;
            component.RootBattle = root_combat.transform;
            component.PelvisIkTarget = saddle_pelvis.transform;
            component.RightFootIkTarget = saddle_rightfoot.transform;
            component.LeftFootIkTarget = saddle_leftfoot.transform;
            component.RightKneeIkTarget = saddle_rightknee.transform;
            component.LeftKneeIkTarget = saddle_leftknee.transform;
            component.Hands = saddle_hands.transform;

            //var test = AssetDatabase.LoadAssetAtPath<RaceMountOffsetsConfig>("Assets/TestConfig.asset");
            //if (test != null)
            //{
            //    component.LargeOffsetsConfig = test;
            //    return;
            //}
            //component.LargeOffsetsConfig = Config;
            //AssetDatabase.CreateAsset(component.LargeOffsetsConfig, "Assets/TestConfig.asset");
        }

        [MenuItem("Mount/Try Weapon")]
        public static void TryWeapon()
        {
            try
            {
                if (rider == null)
                    QuickSetUp();
                if (ContextData<UnitEntityData.DoNotCreateItems>.Current)
                    UnityEngine.Debug.Log("Item creation is forbidden");
                var bp = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("a3081b5219943ad4b9d88a5569a70146");
                var body = rider.Data.Body;
                var r = rider;
                r.CharacterAvatar.IsInDollRoom= true;
                body.TryInsertItem(bp, body.Hands.First());
                var animationManager = rider.AnimationManager;
                animationManager.IsInCombat = true;
                rider.Data.CombatState.m_InCombat = true;
                animationManager.PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
                animationManager.CombatMicroIdle = Kingmaker.Visual.Animation.Kingmaker.CombatMicroIdle.Weapon;
                animationManager.ActiveMainHandWeaponStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
                animationManager.LocoMotionHandle.Action.OnUpdate(animationManager.LocoMotionHandle, 0.01f);
                r.ForcePeacefulLook(false);
                UnitAnimationController.TickOnUnit(r.Data, null);
                animationManager.Tick();
                animationManager.UpdateTime(0.1f);
                r.HandsEquipment.StartCombatChangeAnimation();
            }
            catch (Exception ex)
            {
                PFLog.Default.Exception(ex);
            }

        }


        [MenuItem("Mount/Quick SetUp")]
        public static void QuickSetUp()
        {
            if (!ModAssetImporter.Launched)
                return;
            rider = ModAssetImporter.DoImportUnit(RiderBlueprintForQuickSetUp, true);
            rider.SetVisible(true, true);
            //rider.GetComponentInChildren<SkinnedMeshRenderer>().enabled= true;
            if (RiderSizeForQuickSetUp != null)
            rider.Data.State.Size = RiderSizeForQuickSetUp.Value;
            rider.UpdateScaleImmediately();
            mount = ModAssetImporter.DoImportUnit(MountBlueprintForQuickSetUp);
            mount.SetVisible(true, true);
            //mount.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            DoMountConfig(mount);
            if (MountSizeForQuickSetUp != null)
            {
                mount.Data.State.Size = MountSizeForQuickSetUp.Value;
                mount.UpdateScaleImmediately();
            }

        }

        [MenuItem("Mount/Set Rider")]
        public static void SetRider()
        {
            var part = rider?.Data?.Get<UnitPartRider>();
            if (part != null && part.SaddledUnit != null)
                part.DismountForce();
            rider = Selection.activeGameObject?.GetComponentInChildren<UnitEntityView>();
        }

        [MenuItem("Mount/Set Mount")]
        public static void SetMount()
        {
            var part = rider?.Data?.Get<UnitPartRider>();
            if (part != null && part.SaddledUnit != null)
                part.DismountForce();
            rider = Selection.activeGameObject?.GetComponentInChildren<UnitEntityView>();
            //Config.offsets[0].Races.Add(rider.Blueprint.Race.ToReference<Kingmaker.Blueprints.BlueprintRaceReference>());
            DoMountConfig(mount);
        }

        [MenuItem("Mount/Dump TExture")]
        public static void DumpTexture()
        {
            if (rider == null)
                QuickSetUp();
            var cutout = rider.GetComponentInChildren<SkinnedMeshRenderer>();
            var mainTex = cutout.material.mainTexture;
            Debug.Log($"width is {mainTex.width}. Height is {mainTex.height}");

        }


        //[MenuItem("Mount/Check Animator")]
        public static void CheckAnimator()
        {
            var go = Selection.activeGameObject;
            Debug.Log($"active go is {go?.name ?? "null"}");
            if (go == null) return;
            var animator = go.GetComponent<Animator>();
            Debug.Log($"animator is null? {animator == null}.");
            if (animator == null) return;
            Debug.Log($"Animator has {animator.layerCount} layers");
            var a = animator.runtimeAnimatorController as AnimatorController;
            Debug.Log($"AnimatorController a is null? {a == null}.");
            if (a == null) goto skip;
            Debug.Log($"AnimatorController a has {a.layers?.Length ?? 0} layers");

            var newc = new AnimatorController();
            foreach (var layer in a.layers)
                newc.AddLayer(layer);
            if (newc.layers.Length == 0)
                newc.AddLayer("default");
            foreach (var clip in a.animationClips)
                newc.AddMotion(clip);
            animator.runtimeAnimatorController = newc;
            AssetDatabase.CreateAsset(newc, "Assets/NewController.asset");

            var manager = animator.GetComponent<UnitAnimationManager>()?.ActionSet;
            foreach (var action in manager.OfType<UnitAnimationActionLocoMotion>())
            {
                action.AnimatorController = newc;
            }
            ; skip:
            ;
        }


    }
}
