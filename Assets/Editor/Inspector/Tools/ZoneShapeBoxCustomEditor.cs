using Ex.Kingmaker.View.MapObjects.SriptZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace MyOwlcatModification.Assets.Editor.Inspector.Tools
{
    public abstract class SeamatosEditModeToolHackeroni
    {
        public virtual EditMode.SceneViewEditMode editMode { get; }
        public virtual Type editorType { get; }
        public IEnumerable<UnityEngine.Object> targets { get; }
        public UnityEngine.Object target { get; }
        public virtual void OnToolGUI(EditorWindow window) { }
    }

    //[EditorTool("Edit Bounds", typeof(ScriptZoneBox))]
    internal class ZoneShapeBoxCustomEditor : SeamatosEditModeToolHackeroni // UnityEditor.EditModeTool
    {
        public override EditMode.SceneViewEditMode editMode
            => EditMode.SceneViewEditMode.GridBox;
        public override Type editorType
            => typeof(ScriptZoneEditor);



        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle(); 
        internal static Color s_ColliderHandleColor = new Color(145f, 244f, 139f, 210f) / 255f;

        internal static Color s_ColliderHandleColorDisabled = new Color(84f, 200f, 77f, 140f) / 255f;

        internal static Color s_BoundingBoxHandleColor = new Color(255f, 255f, 255f, 150f) / 255f;

        protected void CopyColliderPropertiesToHandle(ScriptZoneBox collider)
        {
            boundsHandle.center = collider.Bounds.center;
            boundsHandle.size = collider.Bounds.size;
        }

        protected void CopyHandlePropertiesToCollider(ScriptZoneBox collider)
        {
            collider.Bounds.center = boundsHandle.center;
            collider.Bounds.size = boundsHandle.size;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (UnityEngine.Object @object in targets)
            {
                ScriptZoneBox t = (ScriptZoneBox)((object)@object);
                bool flag = Mathf.Approximately(t.transform.lossyScale.sqrMagnitude, 0f);
                if (!flag)
                {
                    using (new Handles.DrawingScope(Matrix4x4.TRS(t.transform.position, t.transform.rotation, Vector3.one)))
                    {
                        CopyColliderPropertiesToHandle(t);
                        boundsHandle.SetColor(t.enabled ? s_ColliderHandleColor : s_ColliderHandleColorDisabled);
                        EditorGUI.BeginChangeCheck();
                        boundsHandle.DrawHandle();
                        bool flag2 = EditorGUI.EndChangeCheck();
                        if (flag2)
                        {
                            Undo.RecordObject(t, string.Format("Modify {0}", ObjectNames.NicifyVariableName(base.target.GetType().Name)));
                            CopyHandlePropertiesToCollider(t);
                        }
                    }
                }

            }
        }
    }
}
