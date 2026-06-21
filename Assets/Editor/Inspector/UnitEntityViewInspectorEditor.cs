using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using Ex.Kingmaker.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using static UnityEngine.GUILayout;
using Kingmaker;
using Kingmaker.Utility;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(UnitEntityView), true)]
    public class UnitEntityViewInspectorEditor : UnityEditor.Editor
    {
        bool HandsFoldout = false;

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            UnitEntityView view = (target as UnitEntityView);
            if (Button("stop"))
                System.Diagnostics.Debugger.Break();
            if (view.CharacterAvatar != null)
            view.CharacterAvatar.PeacefulMode = GUILayout.Toggle(view.CharacterAvatar.PeacefulMode, "Peaceful mode");
            bool inCombat = false;
            if (view.AnimationManager != null)
            {
                inCombat = GUILayout.Toggle(view.AnimationManager.IsInCombat, "Is in combat");
                if (inCombat != view.AnimationManager.IsInCombat)
                {
                    view.AnimationManager.IsInCombat = inCombat;
                    view.HandsEquipment.InCombat = inCombat;
                    view.AnimationManager.Tick();
                    Kingmaker.Controllers.Units.UnitAnimationController.TickOnUnit(view.Data, null);
                    view.HandsEquipment.m_IsAnimatingWeaponsChange = true;
                    view.HandsEquipment.MatchWithCurrentCombatState();
                    view.HandsEquipment.m_IsAnimatingWeaponsChange = false;
                    view.AnimationManager.DoUpdate();
                }
                if (GUILayout.Button("Do"))
                {
                    try
                    {
                        view.AnimationManager.Tick();
                        Kingmaker.Controllers.Units.UnitAnimationController.TickOnUnit(view.Data, null);
                        view.AnimationManager.DoUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            UnitEntityData data = view.Data;
            if (data == null)
            {
                BeginHorizontal();
                FlexibleSpace();
                Label($"Unit data is null!");
                FlexibleSpace();
                EndHorizontal();

            }
            else if (data.Descriptor == null)
            {
                BeginHorizontal();
                FlexibleSpace();
                Label($"Unit descriptor is null!");
                FlexibleSpace();
                EndHorizontal();
            }
            else if (data.Descriptor.State == null)
            {
                BeginHorizontal();
                FlexibleSpace();
                Label($"Unit state is null!");
                FlexibleSpace();
                EndHorizontal();
            }
            else if (view.HandsEquipment != null && data?.Body != null)
            {


                if (HandsFoldout = EditorGUILayout.Foldout(HandsFoldout, "Hands"))
                {
                    var hands = view.HandsEquipment;
                    for (int i = 0; i < data.Body.HandsEquipmentSets.Count; i++)
                    {
                        bool isCurrent = data.Body.m_CurrentHandsEquipmentSetIndex == i;
                        bool isActive = GUILayout.Toggle(isCurrent, "Active set " + i);
                        if (isActive != isCurrent)
                        {
                            data.Body.m_CurrentHandsEquipmentSetIndex = i;
                            hands.ChangeEquipmentWithoutAnimation();
                            Kingmaker.Controllers.Units.UnitAnimationController.TickOnUnit(view.Data, null);
                            view.AnimationManager.DoUpdate();
                        }

                        EditorGUILayout.ObjectField("Primary Hand", data.Body.HandsEquipmentSets[i].PrimaryHand.MaybeItem?.Blueprint, typeof(Kingmaker.Blueprints.Items.BlueprintItem), false);
                        EditorGUILayout.ObjectField("Primary Hand", data.Body.HandsEquipmentSets[i].SecondaryHand.MaybeItem?.Blueprint, typeof(Kingmaker.Blueprints.Items.BlueprintItem), false);
                        GUILayout.Space(9);
                    }

            }


                var state = data.Descriptor.State;
                var oldSize = state.Size;
                state.Size = (Size)EditorGUILayout.EnumPopup("Size", state.Size);
                EditorGUILayout.Vector3Field("Original scale", view.m_OriginalScale);
                EditorGUILayout.FloatField("Previous frame scale", view.m_ScaleOnPrevFrame);
                EditorGUILayout.Toggle("DisableSizeScaling", view.DisableSizeScaling);
                if (view.m_CachedSizeScale == null)
                    EditorGUILayout.LabelField("Cached Size Scale is null");
                else
                    EditorGUILayout.FloatField("Cached Size Scale", view.m_CachedSizeScale.Value);
                //EditorGUILayout.FloatField("Cached Size Scale", view.CalculateSizeScale());
                EditorGUILayout.EnumPopup("Original Size", state.Size);
                var per = view.EntityData?.Progression?.CurrentScalePercent;
                if (per == null)
                    EditorGUILayout.LabelField("Cached Size Scale is null");
                else
                    EditorGUILayout.FloatField("Cached Size Scale", per.Value);
                if (oldSize != state.Size)
                    view.UpdateScaleImmediately();

            }
            DrawDefaultInspector();
        }

    }

}
