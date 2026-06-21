using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.Controllers.Dialog;
using Kingmaker.Editor.Blueprints;
using Kingmaker.Editor.NodeEditor.Utility;
using Kingmaker.Editor.NodeEditor.Window;
using Owlcat.Editor.Core.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
using Kingmaker.Utility;
using static Kingmaker.Editor.NodeEditor.Nodes.UtilExtension;

namespace Kingmaker.Editor.NodeEditor.Nodes
{
    public abstract class EditorNode
    {
        private static int s_NextNodeId;
        public readonly Graph Graph;

        [CanBeNull]
        private EditorNode m_Parent;

        public bool FadeOut = false;

        [CanBeNull]
        public EditorNode Parent
        {
            get { return m_Parent; }
            set
            {
                m_Parent = value;
                if (value != null)
                    SetParentAsset(value.GetAsset());
            }
        }

        public IEnumerable<EditorNode> VirtualNodes
        {
            get { return VirtualChildren.Values; }
        }

        internal readonly Dictionary<ScriptableObject, EditorNode> VirtualChildren =
            new Dictionary<ScriptableObject, EditorNode>();

        private readonly int m_Id;

        public int GroupId
        {
            get { return Graph.GetGroupId(GetAsset()); }
            set
            {
                int current = GroupId;
                if (current > 0 && current != value)
                    Debug.LogWarningFormat("Node {0} belongs to multiple cue groups, bugs are likely.", GetAsset());
                Graph.SetGroupId(GetAsset(), value);
            }
        }

        public Vector2 Center;
        public Vector2 Size;

        protected EditorNode(Graph graph, Vector2 size)
        {
            m_Id = s_NextNodeId++;
            Graph = graph;
            Size = size;
        }

        public virtual EditorNode AddVirtualChild(EditorNode referencedNode)
        {
            EditorNode child = new VirtualNode(Graph, referencedNode);
            child.Parent = this;
            VirtualChildren[referencedNode.GetAsset()] = child;
            return child;
        }

        public void ClearVirtualChildren()
        {
            VirtualChildren.Clear();
        }

        public void Draw(CanvasView view)
        {
            Profiler.BeginSample("Schedule Window");
            try
            {
                if (GetAsset() == null)
                    return;
                var bp = GetBlueprint();
                //if (bp != null && BlueprintsDatabase.GetMetaById(bp.AssetGuid).ShadowDeleted)
                //    GUI.color = Colors.ShadowDeleted;
                //else
                    GUI.color = GetWindowColor();

                if (this == Graph.SelectedNode)
                    GUI.color = Colors.GetHighlighColor(GUI.color);

                if (FadeOut)
                    GUI.color = Colors.GetFadeColor(GUI.color);

                Rect r = new Rect(Center, Size);
                r.position -= Size / 2f;
                r = view.ToScreen(r);

                // check that node is visible on screen
                if (NodeEditorBase.DrawAllNodes || r.Overlaps(view.VisibleScreenArea))
                {
                    var oldSize = Size;
                    r = GUILayout.Window(m_Id, r, DrawWindow, GetName());
                    Size = r.size;
                    if (Size != oldSize)
                        Graph.Layout();
                }
            }
            finally
            {
                Profiler.EndSample();
            }

            foreach (EditorNode virualChild in VirtualChildren.Values)
            {
                virualChild.Draw(view);
            }
        }

        public void DrawDebug(CanvasView view)
        {
            int index = 0;
            Vector2 messagesStart = Center - Size / 2 + new Vector2(0f, 20f);
            foreach (var m in Kingmaker.Controllers.Dialog.DialogDebug.DebugMessages)
            {
                if (m.Blueprint != GetAsset() as BlueprintScriptableObject)
                    continue;

                Profiler.BeginSample("One Message");
                Rect messageRect = new Rect(messagesStart + new Vector2(0f, 16 * index), new Vector2(280f, 16f));
                messageRect = view.ToScreen(messageRect);
                index++;

                GUI.color = m.Color;
                var style = new GUIStyle(GUI.skin.button);
                style.alignment = TextAnchor.MiddleLeft;
                GUI.Button(messageRect, m.Message, style);
                Profiler.EndSample();
            }

            Profiler.BeginSample("Children");
            foreach (EditorNode virualChild in VirtualChildren.Values)
                virualChild.DrawDebug(view);
            Profiler.EndSample();
        }

        public virtual void DrawConnections(CanvasView view)
        {
            if (GetAsset() == null)
                return;
            foreach (EditorNode node in GetReferencedNodes())
                DrawFunctions.Connection(view, this, node, Colors.Connection);

            if (Graph.ShowRelations)
            {
                var assetPath = AssetDatabase.GetAssetPath(GetAsset());
                var children = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var child in children)
                {
                    if (child == GetAsset())
                        continue;
                    var so = new SerializedObject(child);
                    var p = so.GetIterator();
                    do
                    {
                        if (p.propertyType != SerializedPropertyType.ObjectReference)
                            continue;
                        var o = p.objectReferenceValue as ScriptableObject;
                        if (o == null)
                            continue;
                        if (!Graph.ContainsNode(o))
                            continue;
                        var node = Graph.GetNode(o);
                        DrawFunctions.Connection(view, this, 8, node, 8, Colors.ReferenceLink);
                    } while (p.Next(true));
                }
            }

            foreach (EditorNode virtualChild in VirtualChildren.Values)
                virtualChild.DrawConnections(view);
        }

        public virtual Color GetWindowColor()
        {
            return Colors.Default;
        }

        public bool MatchesFilter(string filter)
        {
            return GetName().ToLowerInvariant().Contains(filter)
                || GetText().ToLowerInvariant().Contains(filter);
        }

        public abstract string GetName();

        public virtual string GetText()
        {
            return "";
        }

        public abstract ScriptableObject GetAsset();

        public BlueprintScriptableObject GetBlueprint()
            => GetAsset() as BlueprintScriptableObject;

        public abstract ScriptableObject GetParentAsset();

        public abstract void SetParentAsset(ScriptableObject parent);

        public abstract SerializedObject GetSerializedObject();

        public virtual bool CanBeShared()
        {
            return true;
        }

        public void LoadParentNode()
        {
            var parentAsset = GetParentAsset();
            if (parentAsset == null)
                return;
            if (Graph.ContainsNode(parentAsset))
                Parent = Graph.GetNode(parentAsset);
        }

        private void DrawWindow(int id)
        {
            Profiler.BeginSample("Draw Window");
            try
            {
                if (FadeOut)
                {
                    GUI.color = Colors.GetFadeColor(GUI.color);
                    GUI.contentColor = Colors.GetFadeColor(GUI.contentColor);
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    Graph.SelectedNode = this;
                    Graph.Repaint();
                }

                if (Graph.SelectedNode == this && Event.current.type == EventType.MouseUp)
                {
                    Selection.activeObject = GetAsset();
                }

                NodeEditorBase.CurrentNode = this;

                Profiler.BeginSample("DrawComment()");
                DrawComment();
                Profiler.EndSample();

                Profiler.BeginSample("DrawContent()");
                using (new InfoBoxDisableScope())
                {
                    DrawContent();
                }
                Profiler.EndSample();

                Profiler.BeginSample("DrawMarkers()");
                DrawMarkers(Graph.ShowExtendedMarkers); //
                Profiler.EndSample();

                Profiler.BeginSample("DragAndDrop");
                DragAndDropController.Update(this);
                Profiler.EndSample();

                if (Event.current.type == EventType.MouseDown)
                {
                    // dropping focus from other windows
                    GUI.FocusControl("");
                    Graph.SelectedNode = this;
                    Graph.Repaint();
                    Event.current.Use();
                }

                if (Event.current.button != 2)
                    GUI.DragWindow();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void DrawMarkers(bool extended)
        {
            var markers = GetMarkers(extended).ToList();
            if (markers.Count <= 0)
                return;

            GUILayout.BeginHorizontal();
            int markersOnLine = 0;
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.focused = style.normal;
            style.active = style.normal;
            if (extended)
            {
                style.wordWrap = false;
                style.alignment = TextAnchor.UpperLeft;
            }

            foreach (string marker in markers)
            {
                GUILayout.Button(marker, style, GUILayout.ExpandWidth(false));
                markersOnLine++;
                if (extended || NodeEditorBase.SingleLineMarkers || markersOnLine >= 3)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    markersOnLine = 0;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawComment()
        {
            if (this is VirtualNode)
                return;

            var w = GetAsset() ;
            var blueprint = w;
            if (blueprint == null)
                return;

            if (!string.IsNullOrEmpty((blueprint as BlueprintScriptableObject)?.Comment))
            {
                GUI.color = Color.magenta;
                if (FadeOut)
                {
                    GUI.color = Colors.GetFadeColor(GUI.color);
                }

                GetSerializedObject().Update();
                var ww = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                var property = GetSerializedObject().FindProperty("Comment");
                if (property != null)
                {
                    property.stringValue = EditorGUILayout.TextArea(property.stringValue, ww);
                }

                GetSerializedObject().ApplyModifiedProperties();
                GUI.color = Color.white;
                if (FadeOut)
                {
                    GUI.color = Colors.GetFadeColor(GUI.color);
                }
            }
        }

        protected abstract void DrawContent();

        public virtual IEnumerable<string> GetMarkers(bool extended)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<SimpleBlueprint> GetAllReferencedAssets()
        {
            return GetAllReferencedAssetsInternal().Where(bp => bp != null);
        }

        protected virtual IEnumerable<SimpleBlueprint> GetAllReferencedAssetsInternal() { yield break; }

        public IEnumerable<EditorNode> GetReferencedNodes()
        {
            return GetAllReferencedAssets()
                .Select(GetReferencedNode);
        }

        public EditorNode GetReferencedNode(SimpleBlueprint asset)
        {
            if (asset == null)
            {
                PFLog.Default.Error($"NULL reference in node for {Kingmaker.Blueprints.BlueprintExtenstions.NameSafe(GetAsset())}");
            }
            EditorNode virtualChild;
            VirtualChildren.TryGetValue(asset, out virtualChild);
            if (virtualChild != null)
                return virtualChild;
            return Graph.GetNode(asset);
        }

        public virtual bool CanAddReference(Type type, SimpleBlueprint r = null)
        {
            GetSerializedObject().Update();
            return GetListProperty(type, r) != null;
        }

        [CanBeNull]
        protected abstract SerializedProperty GetListProperty(Type type, ScriptableObject r = null);

        public virtual void AddReferencedAsset(ScriptableObject asset)
        {
            var list = GetListProperty(asset.GetType(),  asset as SimpleBlueprint);
            if (list == null)
                return;

            GetSerializedObject().Update();
            list.arraySize++;
            SetReferencedAsset(list, list.arraySize - 1, asset);
            GetSerializedObject().ApplyModifiedProperties();

            UndoManager.Instance.RegisterUndo("", () => VirtualChildren.Remove(asset));
        }

        private void SetReferencedAsset(SerializedProperty list, int index, ScriptableObject asset)
        {
            PFLog.Default.Log($"{GetAsset().name}: Set RA at {index} to {asset.name}");
            if (list.propertyType == SerializedPropertyType.ObjectReference)
            {
                list.GetArrayElementAtIndex(index).objectReferenceValue = asset;
            }
            else if (list.propertyType == SerializedPropertyType.Generic && list.type.EndsWith("Reference"))
            {
                var elt = list.GetArrayElementAtIndex(index);
                BlueprintReferenceBaseSetPropertyValue(elt,  asset as BlueprintScriptableObject);
            }
        }

        private ScriptableObject GetReferencedAsset(SerializedProperty list, int index)
        {
            try
            {
                PFLog.Default.Log($"Tring to grab referenced asset at {list.propertyPath}[{index}]");
                if (list.propertyType == SerializedPropertyType.ObjectReference)
                {
                    return list.GetArrayElementAtIndex(index).objectReferenceValue as ScriptableObject;
                }
                else if (list.propertyType == SerializedPropertyType.Generic)
                {
                    var a = list.GetArrayElementAtIndex(index);
                    var b = BlueprintReferenceBaseGetPropertyValue(a);

                    return b;

                }
            }
            catch (Exception ex)
            {
                PFLog.Default.Exception(ex, $"when trying to get referenced asset at {list.propertyPath}[{index}]");
            }
            return null;
        }

        public virtual void RemoveReferencedAsset(ScriptableObject asset, bool move = false)
        {
            var list = GetListProperty(asset.GetType(), asset as SimpleBlueprint);
            if (list == null)
                return;

            VirtualChildren.Remove(asset);

            GetSerializedObject().Update();
            for (int i = list.arraySize - 1; i >= 0; i--)
                if (GetReferencedAsset(list, i) == asset)
                    list.DeleteArrayElementAtIndex(i);

            GetSerializedObject().ApplyModifiedProperties();
        }

        public void ReorderReferrencedAsset(ScriptableObject asset, int shift)
        {
            var list = GetListProperty(asset.GetType(), asset as SimpleBlueprint);
            if (list == null)
                return;

            GetSerializedObject().Update();

            int prevIndex = -1;
            for (int i = 0; i < list.arraySize; ++i)
                if (GetReferencedAsset(list, i) == asset)
                    prevIndex = i;

            if (prevIndex < 0)
                return;

            int newIndex = Math.Min(Math.Max(0, prevIndex + shift), list.arraySize - 1);
            if (newIndex == prevIndex)
                return;

            if (newIndex < prevIndex)
            {
                for (int i = prevIndex; i > newIndex; i--)
                    SetReferencedAsset(list, i, GetReferencedAsset(list, i - 1));
                //list.GetArrayElementAtIndex(i).objectReferenceValue = list.GetArrayElementAtIndex(i - 1).objectReferenceValue;
            }

            if (newIndex > prevIndex)
            {
                for (int i = prevIndex; i < newIndex; i++)
                    SetReferencedAsset(list, i, GetReferencedAsset(list, i + 1));
                //list.GetArrayElementAtIndex(i).objectReferenceValue = list.GetArrayElementAtIndex(i + 1).objectReferenceValue;
            }

            SetReferencedAsset(list, newIndex, asset);

            GetSerializedObject().ApplyModifiedProperties();
        }

        public virtual void RemoveReferencedAssets(Predicate<Object> predicate)
        {
            GetSerializedObject().Update();
            foreach (Type type in NodeEditorAssetType.AllTypes.Select(t => t.Template.GetType()))
            {
                var links = GetListProperty(type);
                if (links == null)
                    continue;

                for (int i = links.arraySize - 1; i >= 0; i--)
                    if (predicate(GetReferencedAsset(links, i)))
                        links.DeleteArrayElementAtIndex(i);
            }
            GetSerializedObject().ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public abstract class EditorNode<T> : EditorNode where T : SimpleBlueprint // todo: [bp] also element?
    {
        protected readonly T Asset;

        protected readonly SerializedObject SerializedObject;

        protected SerializedProperty FindProperty(string p)
            => SerializedObject.FindProperty(p);

        public override string GetName()
        {
            return Asset.name;
        }

        public override ScriptableObject GetAsset()
        {
            return Asset;
        }

        public override ScriptableObject GetParentAsset()
        {
            var p = FindProperty("ParentAsset");
            if (p == null || p.propertyType != SerializedPropertyType.String)
                return null;
            var bp = BlueprintsDatabase.LoadById<SimpleBlueprint>(Kingmaker.Blueprints.BlueprintGuid.Parse(p.stringValue));
            return bp;
        }

        public override void SetParentAsset(ScriptableObject parent)
        {
            using (GuiScopes.UpdateObject(SerializedObject))
            {
                var p = FindProperty("ParentAsset");
                if (p == null || p.propertyType != SerializedPropertyType.String)
                    return;
                Ex.Kingmaker.Blueprints.SimpleBlueprint bp = parent as Ex.Kingmaker.Blueprints.SimpleBlueprint;
                p.stringValue = bp != null ? bp.AssetGuid.ToString() : "";
            }
        }

        protected EditorNode(Graph graph, T asset, Vector2 size) : base(graph, size)
        {
            Asset = asset;
            SerializedObject = new SerializedObject(Asset);
        }

        public override SerializedObject GetSerializedObject()
        {
            return SerializedObject;
        }
    }
}