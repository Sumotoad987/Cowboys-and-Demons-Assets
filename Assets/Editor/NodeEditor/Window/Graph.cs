using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Ex.Kingmaker.Blueprints.Quests;
using Ex.Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Editor.NodeEditor.Nodes.Quest;
using Kingmaker.Editor.NodeEditor.Nodes.Encyclopedia;
using UnityEditor;
using UnityEngine;
using Kingmaker.Utility;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class Graph
	{
		private EditorWindow m_EditorWindow;

		public ScriptableObject RootAsset;

		private readonly Dictionary<ScriptableObject, EditorNode> m_Nodes = new Dictionary<ScriptableObject, EditorNode>();

		public List<EditorNode> LayoutOrderNodes;

		private readonly Dictionary<ScriptableObject, int> m_Groups = new Dictionary<ScriptableObject, int>();

		public IEnumerable<EditorNode> Nodes
		{
			get { return m_Nodes.Values; }
		}

		public EditorNode Root;

		public EditorNode SelectedNode;

		private bool m_ReloadScheduled;
		private bool m_LayoutScheduled;

		public int NextAssetId = 1;

		public bool ShowAllVirtualLinks = false;

		public bool ShowRelations = false;

		public bool ShowExtendedMarkers = false;

		private static Regex s_AssetIdRegex = new Regex(".*_(\\d+).asset");

		public Graph(EditorWindow editorWindow, ScriptableObject rootAsset)
		{
			m_EditorWindow = editorWindow;
			RootAsset = rootAsset;
			DoReloadGraph();
		}

		public void Update()
		{
			if (m_ReloadScheduled)
			{
				m_ReloadScheduled = false;
				DoReloadGraph();
			}
			if (m_LayoutScheduled)
			{
				m_LayoutScheduled = false;
				var layout = new GraphLayout(this);
				layout.DoLayout();
				LayoutOrderNodes = layout.VisitOrder;
				m_EditorWindow.Repaint();
			}
		}

		public void Repaint()
		{
			m_EditorWindow.Repaint();
		}

		public void ReloadGraph()
		{
			m_ReloadScheduled = true;
		}

		public void Layout()
		{
			m_LayoutScheduled = true;
		}

		public bool IsLayoutCompleted()
		{
			return !m_LayoutScheduled && !m_ReloadScheduled;
		}

		private void DoReloadGraph()
		{
			NextAssetId = 1;
			m_Nodes.Clear();

			Root = AddNode(RootAsset);

			//if (m_EditorWindow is DialogEditor)
			//{             
			//	var Ids = new List<Kingmaker.Blueprints.BlueprintGuid>();
   //             DialogNode.SearchDialogRecursive(RootAsset as SimpleBlueprint, ref Ids);
   //             foreach (var p in Ids)
   //             {
	  //              //bool shadowDeleted = BlueprintsDatabase.GetMetaById(p.Guid).ShadowDeleted;
	  //              //if(shadowDeleted)
		 //              // continue;
   //                 var bp = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(p);
   //                 if (bp != null)
   //                     AddNode(bp);
   //             }
			//}

			ClearNullReferenceLinks();
			m_LayoutScheduled = true;
		}

		public void ResetGroups()
		{
			m_Groups.Clear();
			Nodes.Select(n => n as ObjectiveNode)
				.Where(n => n != null)
				.ForEach(n => n.AddendumsGroup = 0);
		}

		public int GetGroupId(ScriptableObject asset)
		{
			int result;
			if (m_Groups.TryGetValue(asset, out result))
				return result;
			return -1;
		}

		public void SetGroupId(ScriptableObject asset, int id)
		{
			m_Groups[asset] = id;
		}

		public EditorNode GetNode(ScriptableObject asset)
		{
            if(!m_Nodes.TryGetValue(asset, out var node))
            {
                PFLog.Default.Error($"Cannot find node for {asset}");
            }
			return node;
		}

		public EditorNode AddNode(ScriptableObject asset)
		{
			//asset = asset.ReloadFromInstanceID();
			if (m_Nodes.ContainsKey(asset))
				return m_Nodes[asset];

			var node = CreateNode(asset);
			if (node == null)
				return null;

			m_Nodes[asset] = node;
			m_Groups[asset] = -1;
			foreach (var child in node.GetAllReferencedAssets())
				AddNode(child);

			string path = AssetDatabase.GetAssetPath(asset);
			var match = s_AssetIdRegex.Match(path);
			if (match.Success)
			{
				int id = int.Parse(match.Groups[1].Value);
				NextAssetId = Math.Max(NextAssetId, id + 1);
			}

			m_LayoutScheduled = true;
			return node;
		}

		public void CheckForNewNodes()
		{
			var nodes = m_Nodes.Values.ToList();
			foreach (var node in nodes)
				foreach (var child in node.GetAllReferencedAssets())
					AddNode(child);

			m_LayoutScheduled = true;
		}

        public bool ContainsNode(ScriptableObject asset)
        {
            return m_Nodes.ContainsKey(asset);
        }

        public bool ContainsNode(SimpleBlueprint asset)
        {
            return m_Nodes.ContainsKey(asset);
        }
        
        private void ClearNullReferenceLinks()
		{
			m_Nodes.Values.ForEach(node => node.RemoveReferencedAssets(o => o == null));
		}

		[CanBeNull]
		private EditorNode CreateNode(object asset)
		{
			// inspector node editor
			if (m_EditorWindow is BlueprintNodeEditor)
			{
                // todo: [bp] restore inspector nodes
		//		if (asset is ScriptableObject)
		//			return new InspectorEditorNode(this, (ScriptableObject)asset);
				return null;
			}

			// dialog nodes
			if (asset is BlueprintDialog)
				return new DialogNode(this, (BlueprintDialog)asset);
			if (asset is BlueprintCue)
				return new CueNode(this, (BlueprintCue)asset);
			if (asset is BlueprintCueSequence)
				return new CueSequenceNode(this, (BlueprintCueSequence)asset);
			if (asset is BlueprintCheck)
				return new CheckNode(this, (BlueprintCheck)asset);
			if (asset is BlueprintBookPage)
				return new BookPageNode(this, (BlueprintBookPage)asset);
			if (asset is BlueprintAnswer)
				return new AnswerNode(this, (BlueprintAnswer)asset);
			if (asset is BlueprintAnswersList)
				return new AnswersListNode(this, (BlueprintAnswersList)asset);
			if (asset is BlueprintSequenceExit)
				return new CueSequenceExitNode(this, (BlueprintSequenceExit)asset);

			// quest nodes
			if (asset is BlueprintQuest)
				return new QuestNode(this, (BlueprintQuest)asset);
			if (asset is BlueprintQuestObjective)
				return new ObjectiveNode(this, (BlueprintQuestObjective)asset);

            // encyclopedia nodes
            if (asset is BlueprintEncyclopediaChapter)
                return new EncyclopediaChapterNode(this, (BlueprintEncyclopediaChapter)asset);
            if (asset is BlueprintEncyclopediaPage)
                return new EncyclopediaPageNode(this, (BlueprintEncyclopediaPage)asset);

            if(asset is SimpleBlueprint bp)
            {
                switch (bp)
                {
                    case BlueprintAnswer a:
                        return new AnswerNode(this, a);
					case BlueprintDialog d:
                        return new DialogNode(this, d);
                    case BlueprintCue c:
                        return new CueNode(this, c);
                    case BlueprintCueSequence a:
                        return new CueSequenceNode(this, a);
                    case BlueprintCheck a:
                        return new CheckNode(this, a);
                    case BlueprintBookPage a:
                        return new BookPageNode(this, a);
                    case BlueprintAnswersList a:
                        return new AnswersListNode(this, a);
                    case BlueprintSequenceExit a:
                        return new CueSequenceExitNode(this, a);
                    case BlueprintQuest a:
                        return new QuestNode(this, a);
					case BlueprintQuestObjective a:
                        return new ObjectiveNode(this, a);
                    case BlueprintEncyclopediaChapter a:
                        return new EncyclopediaChapterNode(this, a);
                    case BlueprintEncyclopediaPage a:
                        return new EncyclopediaPageNode(this, a);
                }
            }
            
            return null;
		}

        public IEnumerable<(ScriptableObject, ScriptableObject)> SearchForOutsideReferences()
        {
            var basePath = GetNodePath(Root);
			var b = Nodes.ToList();
            foreach (var node in Nodes)
            {
				if (GetNodePath(node) == basePath)
				{
					var c = node.GetAllReferencedAssets().ToList();
                    foreach (var a in c)
                    {
                        var refpath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(a));
                        if (refpath != basePath)
                        {
                            PFLog.Default.Error($"Node {node.GetAsset().name} references {a.name} in {refpath}",
                                node.GetAsset());
                            yield return (node.GetAsset(), a);
                        }
                    }

				}
            }
        }

        private string GetNodePath(EditorNode graphRoot)
        {
            return Path.GetDirectoryName(AssetDatabase.GetAssetPath(graphRoot.GetAsset()));
        }
    }
}