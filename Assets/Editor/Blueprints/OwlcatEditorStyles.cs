using Owlcat.Editor.Core.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace Owlcat.Editor.Utility
{
    [Serializable]
    public class GUIStylePair
    {
        public GUIStyle Light;
        public GUIStyle Dark;

        public GUIStyle Style { get { return EditorGUIUtility.isProSkin ? Dark : Light; } }

        public static implicit operator GUIStyle(GUIStylePair p)
        {
            return p.Style;
        }
    }

    static class GUIStyleEx
    {
        public static void Draw(this GUIStyle s, Rect r)
        {
            if (Event.current.type == EventType.Repaint && s != null)
                s.Draw(r, false, false, false, false);
        }
        public static void Draw(this GUIStyle s, Rect r, Color c)
        {
            if (Event.current.type == EventType.Repaint && s != null)
            {
                using (GuiScopes.Color(c))
                {
                    s.Draw(r, false, false, false, false);
                }
            }
        }
    }

    //[CreateAssetMenu]
    public class OwlcatEditorStyles : ScriptableObject
    {
        private static OwlcatEditorStyles s_Instance;

        // generic
        [Header("Generic")]
        public Texture2D GridTexture;
        public Texture2D CrutchIcon;

        public Color SelectionColor = new Color(217f / 255, 187f / 255, 231f / 255);

        public GUIStyle BigCheckbox;
        public float BigCheckboxHeight = 32;

        public GUIStyle ColoredCheckbox;

        // state explorer
        [Header("State Explorer")]
        public GUIStyle NameToggle;
        public GUIStyle NameToggleSelected;
        public GUIStyle OpenButton;
        public GUIStyle OverrideButton;

        // blueprint editor
        [Header("Blueprint Editor")]
        public GUIStyle RevertButton;
        public GUIStyle SettingsButton;
        public GUIStyle RemoveButton;
        public GUIStyle Splitter;
        public GUIStyle ThinFrame;
        public Texture2D BlueprintIsDirtyIcon;
        public Texture2D BlueprintHasChangesIcon;
        public Texture2D NicolayIcon;

        // cutscene editor
        [Header("Cutscene Editor")]
        public GUIStylePair CommandBox;
        public GUIStylePair CommandConditionMarker;
        public GUIStyle TrackArrow;
        public GUIStyle TrackRepeatArrow;
        public GUIStylePair TrackBackground;
        public GUIStyle TrackRepeatMarker;
        public GUIStylePair TrackSelector;
        public GUIStylePair TrackCollapsed;
        public GUIStyle SelectionRect;
        public Texture2D GateTexture;
        public Texture2D SplitGateTexture;
        public Texture2D GateTextureFirst;
        public Texture2D GateTextureRandom;
        public GUIStyle GateFlagAnd;
        public GUIStyle GateFlagOr;
        public GUIStyle GateSignalOn;
        public GUIStyle GateSignalOff;
        public GUIStyle CommandTimeLabel;
        public GUIStyle ReactivateLinkUp;
        public GUIStyle ReactivateLinkUpRight;
        public GUIStyle ReactivateLinkUpLeft;
        public GUIStyle ReactivateLinkDown;
        public GUIStyle ReactivateLinkDownRight;
        public GUIStyle ReactivateLinkDownLeft;
        public GUIStyle ExtraSignal;
        public GUIStyle Comment;

        // workspace canvas
        [Header("Workspace Canvas")]
        public GUIStyle BlueprintItem;
        public GUIStyle PrefabItem;
        public GUIStyle ReferenceItem;
        public Texture2D BlueprintItemIcon;
        public Texture2D PrefabItemIcon;
        public Texture2D ReferenceItemIcon;
        public Texture2D SceneItemIcon;
        public Texture2D DeleteIcon;
        public Color WorkspaceTargetBackgroundColor
            => m_WorkspaceTargetBackgroundColor;
        [SerializeField]
        private Color m_WorkspaceTargetBackgroundColor =  new Color(0,0.6F,0.1F);

        // polygon editor
        [Header("Polygon Editor")]
        public GUIStyle PolygonPoint;

        [Header("Animation Manager")]
        public GUIStyle AnimationManagerTransitionButton;

        private GUIStyle m_RightAlignedLabel;

        [SerializeField]
        private Color m_ArrayZebraColor1 = new Color(0.79f, 0.78f, 0.78f);
        [SerializeField]
        private Color m_ArrayZebraColor2 = new Color(0.72f, 0.72f, 0.72f);
        [SerializeField]
        private Color m_ArrayZebraColorPro1 = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField]
        private Color m_ArrayZebraColorPro2 = new Color(0.14f, 0.14f, 0.14f);

        // sceneView gizmos
        [Header("SceneView Gizmos")]
        public GUIStyle SceneLabel;

        public Texture2D IconSpawner;
        public Texture2D IconSpawnerDelayed;
        public Texture2D IconSpawnerCompanion;

        [Header("Elements debug")]
        public Texture2D ActionIcon;
        public Texture2D ConditionIcon;
        public Texture2D EvaluatorIcon;

        public Color ArrayZebraColor1
            => EditorGUIUtility.isProSkin ? m_ArrayZebraColorPro1 : m_ArrayZebraColor1;

        public Color ArrayZebraColor2
            => EditorGUIUtility.isProSkin ? m_ArrayZebraColorPro2 : m_ArrayZebraColor2;

        

        public Color ArrayZebraColor(int idx)
        {
            return idx % 2 == 0 ? ArrayZebraColor1 : ArrayZebraColor2;
        }


        public static OwlcatEditorStyles Instance
        {
            get
            {
                return
                    s_Instance =
                        s_Instance ? s_Instance : EditorGUIUtility.Load("OwlcatEditorSkin.asset") as OwlcatEditorStyles;
            }
        }

        public GUIStyle RightAlignedLabel
        {
            get
            {
                m_RightAlignedLabel = m_RightAlignedLabel ?? new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
                return m_RightAlignedLabel;
            }
        }
    }
}