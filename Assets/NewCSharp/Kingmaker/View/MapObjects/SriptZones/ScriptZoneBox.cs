using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.View.MapObjects.SriptZones
{
	public  class ScriptZoneBox : Ex.Kingmaker.View.MapObjects.SriptZones.ScriptZoneBox 
	{
		public ScriptZoneBox () : base()
		{
        }
        Color LineColor = new Color(1f, 1f, 1f, 0.5f);
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var quat = gameObject ? gameObject.transform.rotation : Quaternion.identity;
            Color color = Handles.color;
            Handles.color = LineColor;
            if (Camera.current && Event.current.type == EventType.Repaint && Handles.ShouldRenderGizmos())
            {
                Handles.DrawWireCube(Center(), quat * GetBounds().size);
            }

            Handles.color = color;
        } 
#endif

    }
}
