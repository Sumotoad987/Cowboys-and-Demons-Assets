using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.JsonSystem.PropertyUtility;
using Ex.Kingmaker.DialogSystem.Blueprints;
using Ex.Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Editor.Utility;
using Kingmaker.GameModes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using Kingmaker.Utility;

namespace Kingmaker.Editor.NodeEditor.Window
{
	public class DialogEditor : NodeEditorBase
	{
		[CanBeNull]
		private BlueprintCue m_GameCue;

		[CanBeNull]
		private BlueprintDialog m_GameDialog;

		public DialogEditor()
		{
			titleContent = new GUIContent("Dialog Editor");
		}

		[MenuItem("Design/Dialog Editor", false, 2003)]
		public static void ShowWindow()
		{
			GetWindow<DialogEditor>().Show();
		}

        [BlueprintContextMenu("Open in Dialog Editor", BlueprintType = typeof(BlueprintDialog))]
        [BlueprintContextMenu("Open in Dialog Editor", BlueprintType = typeof(BlueprintCue))]
        [BlueprintContextMenu("Open in Dialog Editor", BlueprintType = typeof(BlueprintAnswer))]
        [BlueprintContextMenu("Open in Dialog Editor", BlueprintType = typeof(BlueprintCheck))]
		public static void OpenAssetInDialogEditor(SimpleBlueprint bp)
		{
			FocusAsset(null, bp);
		}

		public void Update()
		{
			bool focus = false;
			var newGameDialog = DialogDebug.Dialog;
			if (m_GameDialog != newGameDialog)
			{
				m_GameDialog = newGameDialog;
				focus = true;
			}

			if (Application.isPlaying)
			{
				var newGameCue = Game.Instance.DialogController.CurrentCue;
				if (m_GameCue != newGameCue)
				{
					m_GameCue = newGameCue;
					focus = true;
				}
			}

			if (focus)
			{
				FocusAsset(m_GameDialog, m_GameCue, false);
				Repaint();
			}
		}

        public static void FocusAsset([CanBeNull] BlueprintDialog dialog, SimpleBlueprint bp, bool focus = true)
        {
            bp = bp ?? dialog;
            if(!bp)
                return;

            if (dialog == null)
            {
                string assetPath = AssetDatabase.GetAssetPath(bp);
                string directory = Path.GetDirectoryName(assetPath);
                var dialogs = AssetDatabase.FindAssets("t:BlueprintDialog", new string[] { directory}).Select(guid => AssetDatabase.LoadAssetAtPath<BlueprintDialog>(guid));
                dialog = dialogs.FirstOrDefault();
            }

            if (dialog == null)
                return;

			var window = GetWindow<DialogEditor>("Dialog Editor", false);
			window.OpenAsset(dialog, bp, focus);
        }

		public static void FocusAsset([CanBeNull] BlueprintDialog dialog, UnityEngine.Object asset, bool focus = true)
		{
            //if(asset is BlueprintJsonWrapper bjw)
            //{
            //    FocusAsset(dialog, bjw.Data as BlueprintScriptableObject, focus);
            //    return;
            //}
            
			if (asset == null && dialog == null)
				return;

			if (asset == null)
				asset = dialog;

			if (dialog == null)
			{	
            }

            if (dialog == null)
				return;
            
            FocusAsset(dialog, dialog, focus);
		}

		protected override void ExtraHUDButtons()
        {
            if (GUILayout.Button("New Dialog", GUILayout.ExpandWidth(false)))
            {
				ShowNewDialogMode();
            }

            if (GUILayout.Button("SVG Export", GUILayout.ExpandWidth(false)))
			{
				if (Graph != null)
				{
					string fileName = EditorUtility.SaveFilePanel("Export dialogue to SVG", "", Graph.RootAsset.name + ".svg", "svg");
					EditorCoroutine.Start(SvgExportCoroutine(fileName, Graph.ShowExtendedMarkers));
				}
			}

			if (Graph != null)
			{
				if (Application.isPlaying && Game.Instance.IsModeActive(GameModeType.Dialog))
				{
					if (GUILayout.Button("Stop Debug Player"))
					{
						StopDebugPlayer();
					}
				}
				else
				{
					if (GUILayout.Button("Start Debug Player"))
					{
						StartDebugPlayer();
					}
				}
				if (Graph.ShowAllVirtualLinks)
				{
					if (GUILayout.Button("Hide Virtual"))
					{
						Graph.ShowAllVirtualLinks = false;
					}
				}
				else
				{
					if (GUILayout.Button("Show Virtual"))
					{
						Graph.ShowAllVirtualLinks = true;
					}
				}

				if (Graph.ShowRelations)
				{
					if (GUILayout.Button("Hide Relations"))
					{
						Graph.ShowRelations = false;
					}
				}
				else
				{
					if (GUILayout.Button("Show Relations"))
					{
						Graph.ShowRelations = true;
					}
				}
                if(GUILayout.Button("Check references"))
                {
                    var badRefs = Graph.SearchForOutsideReferences().ToList();
                    if (badRefs.Any())
                    {
                        var msg = "";
                        foreach (var badRef in badRefs)
                        {
                            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(badRef.Item2)).Substring(7);
                            msg += $"{badRef.Item1.name} references {badRef.Item2.name}\n\tin {path}\n";
                        }
                        EditorUtility.DisplayDialog("Bad references found", msg+"\nSee the UberConsole logs", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("No problem", "All references are correct", "OK");
                    }
                }
			}
			base.ExtraHUDButtons();
		}

		private void StartDebugPlayer()
		{
            var player = FindObjectOfType<DebugDialogPlayer>();
            if (!Application.isPlaying)
            {
                if (!player)
                {
                    const string scene = "5e148b44b3a962e409edbec776a57568";
                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(scene), OpenSceneMode.Single);
                    player = FindObjectOfType<DebugDialogPlayer>();
                }

                if (player)
                {
                    player.Dialog = (BlueprintDialog)RootAsset;
                    EditorApplication.isPlaying = true;
                }
            }
            else
            {
                if (player)
                {
                    player.Dialog = (BlueprintDialog)RootAsset;
                    player.StartDialog();
                }
            } 
        }

		private void StopDebugPlayer()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			var player = FindObjectOfType<DebugDialogPlayer>();
			if (player)
			{
				player.StopDialog();
			}
		}

		private void ShowNewDialogMode()
		{
			try
			{
				var path = EditorUtility.SaveFilePanel("Select a foler for new dialog", "Assets", "NewDialog", "");
				path = path.Substring(Environment.CurrentDirectory.Length+1);
				var directoryPath = Path.GetDirectoryName(path);

				if (!AssetDatabase.IsValidFolder(directoryPath))
				{
					Debug.Log($"Can't create a folder for new dialog, because it's not a valid folder. Selected path is: \n{path}");
					return;
				}

				var filePath = Path.GetFileName(path);
				if (filePath.IsNullOrEmpty())
				{
					Debug.Log("Can't create a folder for new dialog, because the name for new folder is new or empty");
					return;
				}

				var newFolder = AssetDatabase.CreateFolder(directoryPath, filePath);
				var blueprint = ScriptableObject.CreateInstance<DialogSystem.Blueprints.BlueprintDialog>();
				blueprint.name = string.Join("", filePath.Split(' ')) + "_dialog";
				var finalPath = Path.Combine(directoryPath, filePath, blueprint.name + ".jbp");
				;
				blueprint.AssetGuid = Kingmaker.Blueprints.BlueprintGuid.Parse(AssetDatabase.AssetPathToGUID(finalPath));
                new BlueprintJsonWrapper(blueprint).Save(finalPath);
				UnityEngine.Object.DestroyImmediate(blueprint);
				AssetDatabase.Refresh();
				var asset = AssetDatabase.LoadAssetAtPath<DialogSystem.Blueprints.BlueprintDialog>(finalPath);
                OpenAsset(asset);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
        }

		protected override Type GetOpenType()
		{
			return typeof(DialogSystem.Blueprints.BlueprintDialog);
		}
	}
}