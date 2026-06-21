using System;
using System.Collections.Generic;
using System.Linq;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.Blueprints;
using Kingmaker.Editor.NodeEditor.Utility;
using Kingmaker.Editor.NodeEditor.Window;
using Kingmaker.ElementsSystem;
using Owlcat.Editor.Core.Utility;
using UnityEditor;
using UnityEngine;
using Kingmaker.ElementsSystemEx;
using static Ex.Kingmaker.Blueprints.BlueprintReferenceBaseEx;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
	public class CheckNode : EditorNode<BlueprintCheck>
	{
		private float m_SuccessY;
		private float m_FailY;

		public CheckNode(Graph graph, BlueprintCheck asset) : base(graph, asset, new Vector2(200, 80))
		{

		}

		public override Color GetWindowColor()
		{
			return Colors.CheckWindow;
		}

		public override string GetText()
		{
			return $"{Asset.Type} ({Asset.DC})";
		}

		protected override void DrawContent()
		{
			GUILayout.BeginHorizontal();
			using (GuiScopes.UpdateObject(SerializedObject))
			{
				EditorGUIUtility.fieldWidth = 150;
				EditorGUILayout.PropertyField(FindProperty("Type"), new GUIContent());
				EditorGUIUtility.fieldWidth = 20;
				EditorGUILayout.PropertyField(FindProperty("DC"), new GUIContent());
			}
			GUILayout.EndHorizontal();

			GUIStyle s = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight};
			GUILayout.Label("Success", s);
			m_SuccessY = GetCurrentY();
			GUILayout.Label("Fail", s);
			m_FailY = GetCurrentY();
		}

		public override IEnumerable<string> GetMarkers(bool extended)
		{
			if (Asset.ShowOnce)
				yield return "Once";
			if (Asset.Conditions.HasConditions)
				yield return ElementsDescription.Conditions(extended, Asset.Conditions);
		}

		public override void DrawConnections(CanvasView view)
		{
			if (Asset.Success != null)
				DrawFunctions.Connection(view, this, m_SuccessY, GetReferencedNode(Asset.Success), Colors.Connection);
			if (Asset.Fail != null)
				DrawFunctions.Connection(view, this, m_FailY, GetReferencedNode(Asset.Fail), Colors.Connection);

			foreach (var virtualChild in VirtualChildren.Values)
				virtualChild.DrawConnections(view);
		}

        protected override IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal()
		{
			return new [] {Asset.Success, Asset.Fail}.Where(a => a != null).Distinct();
		}
        
        
		public override bool CanAddReference(Type type, SimpleBlueprint r = null)
		{
			if (!typeof(DialogSystem.Blueprints.BlueprintCueBase).IsAssignableFrom(type))
				return false;
			return Asset.Success == null || Asset.Fail == null;
		}

		public override void AddReferencedAsset(ScriptableObject asset)
        {
            var bp = asset as BlueprintScriptableObject;
			SerializedObject.Update();
            if (Asset.Success == null)
            {
				SetPropertyValue(FindProperty("m_Success"), bp);
            }
            else if (Asset.Fail == null)
            {
                SetPropertyValue(FindProperty("m_Fail"), bp);
            }
            SerializedObject.ApplyModifiedProperties();
            
			UndoManager.Instance.RegisterUndo("", () => VirtualChildren.Remove(asset));
		}

		public override void RemoveReferencedAsset(ScriptableObject asset, bool move = false)
		{
			SerializedObject.Update();
            if (Asset.Success == asset as SimpleBlueprint)
            {
                SetPropertyValue(FindProperty("m_Success"), null);
			}

            if (Asset.Fail == asset as SimpleBlueprint)
            {
                SetPropertyValue(FindProperty("m_Fail"), null);
            }

            SerializedObject.ApplyModifiedProperties();

			VirtualChildren.Remove(asset);
		}

		protected override SerializedProperty GetListProperty(Type type, ScriptableObject r = null)
		{
			return null;
		}

		private static float GetCurrentY()
		{
			var r = GUILayoutUtility.GetLastRect();
			return r.y + r.height/2;
		}
	}
}