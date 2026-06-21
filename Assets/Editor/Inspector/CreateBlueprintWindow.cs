using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility;
using MyOwlcatModification.Editor.Utility;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyOwlcatModification.Editor.Inspector
{
	public class CreateBlueprintWindow : EditorWindow
	{

		private string m_SearchString = "";
		private Vector2 m_ScrollPosition;
		private string m_Path;
		
		public static void ShowWindow()
		{
			var window = GetWindow<CreateBlueprintWindow>();
			window.m_Path = TryGetSelectedPath();
			window.Show();
		}
		
		[CanBeNull]
		private static string TryGetSelectedPath()
		{
			foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
			{
				string path = AssetDatabase.GetAssetPath(obj);
				if (!string.IsNullOrEmpty(path) && File.Exists(path)) 
				{
					return Path.GetDirectoryName(path);
				}
			}
			
			try
			{
				var tryGetActiveFolderPathMethod = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath",
					BindingFlags.Static | BindingFlags.NonPublic);

				var args = new object[] {null};
				bool found = (bool)tryGetActiveFolderPathMethod.Invoke(null, args);
				if (found)
				{
					return (string)args[0];
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return null;
		}

		private void OnLostFocus()
		{
			Close();
		}

		static GUIContent s_TitleContent= new GUIContent("Create Blueprint");
        private void OnEnable()
		{
			titleContent = s_TitleContent;

        }

		private void OnGUI()
		{
			if (string.IsNullOrEmpty(m_Path))
			{
				GUILayout.Label("Can'blueprintType detect path for new asset!", EditorStyles.boldLabel);
				ShowSelectFolderButton();
				return;
			}
			
			EditorGUILayout.LabelField($"Target folder: {m_Path}");
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Type: ", GUILayout.Width(50));
				
				GUI.SetNextControlName("SearchTextField");
				m_SearchString = EditorGUILayout.TextField(m_SearchString);
				GUI.FocusControl("SearchTextField");
			}
			
			EditorGUILayout.LabelField("enter at least 3 characters");

			if (m_SearchString.Length < 3)
			{
				return;
			}

			Type typeToInstantiate = null;
			using (var scroll = new EditorGUILayout.ScrollViewScope(
				m_ScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar))
			{
				m_ScrollPosition = scroll.scrollPosition;
				
				using (new EditorGUILayout.VerticalScope())
				{
					string[] words = m_SearchString.ToLowerInvariant().Split(' ');
					
					foreach (var t in BlueprintTypesCache.Blueprints)
					{
						if (!words.All(t.NameLowerInvariant.Contains))
						{
							continue;
						}

						if (GUILayout.Button(t.Name))
						{
							typeToInstantiate = t.Type;
						}
					}
				}
			}

			if (typeToInstantiate != null)
            {
				if (!typeToInstantiate.IsOrSubclassOf<Ex.Kingmaker.Blueprints.SimpleBlueprint>())
				{
					Debug.Log($"Something went off");
					return;
				}
				try
				{
					if (!Launcher.Launched)
					{
						Debug.LogWarning("Cancelled creation of new blueprint, because services are not launched");
						return;
					}

					string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(m_Path, $"New {typeToInstantiate.Name}.jbp"));
					var prototype = (Ex.Kingmaker.Blueprints.SimpleBlueprint)ScriptableObject.CreateInstance(typeToInstantiate);
					var wrapper = new Ex.Kingmaker.Blueprints.JsonSystem.BlueprintJsonWrapper(prototype);
					var json = JsonConvert.SerializeObject(wrapper, serializerSettings);
					UnityEngine.Object.DestroyImmediate(prototype);
					ProjectWindowUtil.StartNameEditingIfProjectWindowExists
					(
						0,
						ScriptableObject.CreateInstance<DoCreateBlueprint>(),
						$"New {typeToInstantiate.Name}.jbp",
						EditorGUIUtility.FindTexture(typeToInstantiate.Name),
                        json
                    );
					Close();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					throw;
				}
			}
			else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				Close();
			}
		}


		private void ShowSelectFolderButton()
		{
			if (GUILayout.Button("Select folder"))
			{
				m_Path = EditorUtility.OpenFolderPanel("Select destination folder", m_Path, "");
				if (m_Path != null && m_Path.StartsWith(Application.dataPath))
				{
					m_Path = "Assets" + m_Path.Substring(Application.dataPath.Length);
				}
				else
				{
					m_Path = null;
				}
			}
		}

        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new FieldsContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            Formatting = Formatting.Indented,
            Binder = new GuidClassBinder()
        };

        class DoCreateBlueprint : EndNameEditAction 
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
				try
				{

					if (!pathName.EndsWith(".jbp"))
					{
						Debug.LogWarning("Chosen name for the blueprint does not end with \".jbp\" extension. It will be added now.");
						pathName += ".jbp";
					}
					File.WriteAllText(pathName, resourceFile);

					AssetDatabase.Refresh();
					string metaContent = File.ReadAllText(pathName + ".meta");
					var metaGuidRegex = new Regex("guid: ([^\n]*)\n");
					var m = metaGuidRegex.Match(metaContent);
					string guid = m.Groups[1].ToString();

					string blueprintContent = File.ReadAllText(pathName);
					blueprintContent = blueprintContent.Replace("\"AssetId\": \"\"", $"\"AssetId\": \"{guid}\"");
					File.WriteAllText(pathName, blueprintContent);
                    AssetDatabase.Refresh();

                    ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadMainAssetAtPath(pathName));
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					throw;
				}
            }
        }

    }
}