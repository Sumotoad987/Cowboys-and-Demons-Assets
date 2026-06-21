using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Utility;
using static Kingmaker.Utility.CodeTimer;
using Owlcat.Editor.Utility;
using Owlcat.Runtime.Core.Utils;
using UnityEditor;
using UnityEngine;
using System.Net;
using MyOwlcatModification.Editor.Utility;
using MyOwlcatModification.Assets.Editor.Utility;
using MyOwlcatModification;

namespace Kingmaker.Editor.Blueprints
{
	public class BlueprintPicker : KingmakerWindowBase
	{
		public class HierarchyEntry : IComparable<HierarchyEntry>
		{
			public HierarchyEntry Parent;
			public string Path;
			public string Name;
			public string DisplayName;
			public string AssetGuid;
			public bool IsShadowDeleted;
			public bool ContainsShadowDeletedBlueprints;
			public List<HierarchyEntry> Children;
			public bool Expanded = true;
			public bool Hidden;

            public SimpleBlueprint Asset
                => Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse(AssetGuid));

			public int CompareTo(HierarchyEntry other)
			{
				if (Children == null && other.Children != null)
				{
					return 1;
				}
				if (Children != null && other.Children == null)
				{
					return -1;
				}
				return string.Compare(AssetGuid, other.AssetGuid, StringComparison.Ordinal);
			}
		}

		public static GUIStyle SelectedAssetStyle;

		public static GUIStyle AssetStyle;

		public static GUIStyle FolderStyle;

		private bool m_ShowTimeOfDaySelection;
		private static TimeOfDay s_SelectedTimeOfDay = TimeOfDay.Day;

		private bool m_EnableSelectionOnClick = false;

		private HierarchyEntry SelectedAssetEntry
		{
			get { return m_SelectedAssetEntry; }
			set
			{
				var e = value;
				while (e != null)
				{
					e.Expanded = true;
					e = e.Parent;
				}
				m_SelectedAssetEntry = value;
			}
		}

		[MenuItem("Design/Blueprint picker #%B", false, 5000)]
		public static void ShowAssetPicker()
		{
			GetWindow<BlueprintPicker>();
		}

		public static void ShowAssetPicker<T>(
			[NotNull] Action<T> callback,
			[CanBeNull] T selectedAsset = null,
			[CanBeNull] Func<HierarchyEntry, bool> filter = null,
			bool enableSelectionOnClick = false) where T : SimpleBlueprint
		{
			ShowAssetPicker(typeof(T), null, e => callback?.Invoke((T)e), selectedAsset, filter, enableSelectionOnClick);
		}

		public static void ShowAssetPicker(
			[NotNull] Type assetType,
			[CanBeNull] FieldInfo fieldInfo,
			[NotNull] Action<SimpleBlueprint> callback,
			[CanBeNull] SimpleBlueprint selectedAsset = null,
			[CanBeNull] Func<HierarchyEntry, bool> filter = null,
			bool enableSelectionOnClick = false)
		{
			var w = CreateInstance<BlueprintPicker>();

			w.m_Filters.Clear();
			if (filter != null)
			{
				w.m_Filters.Add(filter);
			}
			if (fieldInfo != null)
			{
				if (fieldInfo.GetAttribute<Kingmaker.Blueprints.DependenciesFilterAttribute>() != null)
				{
					w.m_Filters.AddRange(DependenciesFilters);
				}

				var pathFilter = fieldInfo.GetAttribute<Kingmaker.Blueprints.AssetPathFilterAttribute>();
				if (pathFilter != null)
				{
					w.m_Filters.Add(he => pathFilter.Filters.Any(f => he.Path.Contains(f)));
				}
			}

			w.AssetType = assetType;
			w.m_EnableSelectionOnClick = enableSelectionOnClick;
			w.m_SelectionCallback = callback;

			w.UpdateAssetList();
			if (selectedAsset)
			{
				var assetGuid = selectedAsset.AssetGuid;
				w.SelectedAssetEntry =
					w.m_LoadedAssetsFlat.FirstOrDefault(e => Guid.Parse(e.AssetGuid) == assetGuid.m_Guid);
				w.m_MustScrollToSelected = true;
			}
			w.ShowAuxWindow();
			w.m_FocusNameFilter = true;
			w.Focus();
		}

		public static void ShowAssetPicker<T>(Action<SimpleBlueprint> callback, string[] paths, bool enableSelectionOnClick = false, SimpleBlueprint selectedAsset = null)
		{
			var w = CreateInstance<BlueprintPicker>();
			w.m_EnableSelectionOnClick = enableSelectionOnClick;
			w.AssetType = typeof(T);
			w.m_SelectionCallback = callback;
			if (paths != null)
			{
				w.m_Filters.Add(e => paths.Any(p => e.Path.StartsWith(p)));
			}
			w.UpdateAssetList();
			if (selectedAsset)
			{
				var assetGuid = selectedAsset.AssetGuid;
				w.SelectedAssetEntry =
					w.m_LoadedAssetsFlat.FirstOrDefault(e => Guid.Parse(e.AssetGuid) == assetGuid.m_Guid);
				w.m_MustScrollToSelected = true;
			}
			w.ShowAuxWindow();
			w.m_FocusNameFilter = true;
			w.Focus();
		}

		public static void ShowAreaPicker(Action<SimpleBlueprint, TimeOfDay> callback)
		{
			var w = CreateInstance<BlueprintPicker>();
			w.AssetType = typeof(BlueprintArea);
			w.m_SelectionCallback = o => callback.Invoke(o, s_SelectedTimeOfDay);
			w.UpdateAssetList();
			w.ShowAuxWindow();
			w.m_FocusNameFilter = true;
			w.m_ShowTimeOfDaySelection = true;
			w.Focus();
		}

		public static void ShowAreaPartPicker(Action<SimpleBlueprint, TimeOfDay> callback)
		{
			var w = CreateInstance<BlueprintPicker>();
			w.AssetType = typeof(BlueprintAreaPart);
			w.m_SelectionCallback = o => callback.Invoke(o, s_SelectedTimeOfDay);
			w.UpdateAssetList();
			w.ShowAuxWindow();
			w.m_FocusNameFilter = true;
			w.m_ShowTimeOfDaySelection = true;
			w.Focus();
		}

        public static void ShowObjectField(
            SimpleBlueprint currentValue, Action<SimpleBlueprint> pickCallback,
			GUIContent label, Type assetType, Type fieldType = null,
			Func<HierarchyEntry, bool> filter = null, bool enableSelectionOnClick = false,
			string setControlName = null)
		{
			var rect = EditorGUILayout.GetControlRect(true);
			ShowObjectField(rect, currentValue, pickCallback, label, assetType, fieldType, filter, enableSelectionOnClick, setControlName);
		}

		public static void ShowObjectField(
			Rect position, SimpleBlueprint currentValue, Action<SimpleBlueprint> pickCallback,
			GUIContent label, Type assetType, Type fieldType = null,
			Func<HierarchyEntry, bool> filter = null, bool enableSelectionOnClick = false,
			string setControlName = null)
		{
			EditorGUI.BeginChangeCheck();
			var buttonPos = position;
			buttonPos.xMin = buttonPos.xMax - EditorGUIUtility.singleLineHeight;
			var requesterWindow = focusedWindow;

			string controlName = setControlName ?? ("ObjField" + label.text);
			var e = Event.current;
			bool showHotKey =
				e.type == EventType.KeyDown &&
				(e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);

			bool deleteHotKey =
				e.type == EventType.KeyDown &&
				e.keyCode == KeyCode.Delete;
            
			GUI.SetNextControlName(controlName);
            var obj = BlueprintEditorUtility.ObjectField(
                position,
                label,
                currentValue,
                fieldType ?? assetType);

            showHotKey = GUI.GetNameOfFocusedControl() == controlName && showHotKey;
            deleteHotKey = GUI.GetNameOfFocusedControl() == controlName && deleteHotKey;

			if (deleteHotKey)
			{
				pickCallback(null);
			}
			else if (showHotKey || GUI.Button(buttonPos, "", GUIStyle.none))
			{
				// invisible button overrides object picker
				ShowAssetPicker(
					assetType,
					null,
					o =>
					{
                        //                  if (o?.GetMeta()?.ShadowDeleted ?? false)
                        //					
                        //                  {
                        //                      EditorUtility.DisplayDialog("Oops",
                        //                          $"The blueprint {o.NameSafe()} is shadow-deleted and cannot be used.", "OK");
                        //                  }
                        //                  else
                        //                  {
                        //                  }
                        pickCallback(o);

                        if (requesterWindow != null)
						{
							requesterWindow.Repaint();
							if (!s_PreviewSelection)
							{
								requesterWindow.Focus();
							}
						}
					},
					currentValue,
					filter,
					enableSelectionOnClick
				);
				GUI.changed = false; // prevent extra callback on selecting button
			}

			if (EditorGUI.EndChangeCheck())
			{
				pickCallback(obj);
			}
		}


		[SerializeField]
		private Type m_AssetType;

		private Type AssetType
		{
			get { return m_AssetType; }
			set {
				m_AssetType = value;

			}
		}

		[SerializeField]
		private bool m_IsFlatMode;

		[SerializeField]
		private string m_NameFilter;
        [SerializeField]
        private string m_TypeNameFilter = "";

        private List<HierarchyEntry> m_LoadedAssets;
		public List<HierarchyEntry> LoadedAssets => m_LoadedAssets;
		private readonly List<HierarchyEntry> m_LoadedAssetsFlat = new List<HierarchyEntry>();
		private Vector2 m_Scroll;

		private static readonly Type[] PossibleTypeFilters;
        private static string[] s_ContextMenusFirstLevel;

		private readonly HashSet<string> m_FoldedPaths = new HashSet<string>();

		private static bool s_PreviewSelection;

		private Action<SimpleBlueprint> m_SelectionCallback;

		private List<Func<HierarchyEntry, bool>> m_Filters = new List<Func<HierarchyEntry, bool>>();

		public static List<Func<HierarchyEntry, bool>> DependenciesFilters = new List<Func<HierarchyEntry, bool>>();

		private bool m_FocusNameFilter;
        private bool m_FocusTypeNameFilter;

        private HierarchyEntry m_SelectedAssetEntry;

		private bool m_MustScrollToSelected;

		static BlueprintPicker()
		{
			var blueprintTypes = BlueprintTypesCache.Blueprints.Select(t => t.Type);
				//TypeCache.GetTypesDerivedFrom(typeof(SimpleBlueprint)).Where(t => !t.IsAbstract);
			PossibleTypeFilters =
				blueprintTypes
					.OrderBy(n => n.Name)
					.ToArray();
		}

		private static void InitGUIStyles()
		{
            try
            {
                SelectedAssetStyle = new GUIStyle(EditorStyles.label);
                SelectedAssetStyle.richText = true;
                SelectedAssetStyle.normal = SelectedAssetStyle.focused;
                SelectedAssetStyle.active = SelectedAssetStyle.focused;
                SelectedAssetStyle.onNormal = SelectedAssetStyle.focused;
                SelectedAssetStyle.onActive = SelectedAssetStyle.focused;

                AssetStyle = new GUIStyle(EditorStyles.label);
                AssetStyle.richText = true;
                AssetStyle.active = AssetStyle.normal;
                AssetStyle.focused = AssetStyle.normal;
                AssetStyle.onActive = AssetStyle.normal;
                AssetStyle.onFocused = AssetStyle.normal;

                FolderStyle = new GUIStyle(EditorStyles.foldout);
                FolderStyle.focused = FolderStyle.normal;
                FolderStyle.active = FolderStyle.normal;
                FolderStyle.hover = FolderStyle.normal;
                FolderStyle.onFocused = FolderStyle.normal;
                FolderStyle.onActive = FolderStyle.normal;
                FolderStyle.onHover = FolderStyle.normal;
            }catch(NullReferenceException)
            {
				// this just happens sometimes because apparently EditorStyles does not get initialized immediately
            }
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			titleContent = new GUIContent("Assets");

			FillContextMenuData();

            if (AssetType != null)
            {
                UpdateAssetList();
            }

            var folded = EditorPrefs.GetString("KM_BP_Folded_Paths", "");
			m_FoldedPaths.UnionWith(folded.Split('\n'));

			if (SelectedAssetStyle == null)
			{
				InitGUIStyles();
			}
		}

		private static void FillContextMenuData()
		{
			if (s_ContextMenusFirstLevel == null)
			{
				// find all types that have context menus attached
				// todo: this does not actually work: we need also menus for PARENT classes,
				// and also methods defined with [ContextMenu] attribute do not appear here
				var allMenus = EditorGUIUtility.SerializeMainMenuToString()
					.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				s_ContextMenusFirstLevel = allMenus
					.SkipWhile(m => m != "CONTEXT") // find CONTEXT menu
					.Skip(1) // skip it
					.TakeWhile(m => m.StartsWith("    ")) // take until next top-level menu
					.Where(m => !m.StartsWith("     ")) // 5 spaces means at least third-level, we don't want those
					.Select(m => m.TrimStart(' ')) // cut spaces from start
					.ToArray();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			EditorPrefs.SetString("KM_BP_Folded_Paths", string.Join("\n", m_FoldedPaths.ToArray()));
		}

		protected override void OnGUI()
		{
			base.OnGUI();
			HandleKeyDown();

			// header (filters)
			using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(false)))
			{
				GUILayout.Space(1);
				using (new EditorGUILayout.HorizontalScope())
				{
					// disable changing type if this is a pop-up picker
					using (new EditorGUI.DisabledGroupScope(m_SelectionCallback != null))
					{
						GUILayout.Label("Asset type: "+(AssetType?.Name), GUILayout.ExpandWidth(false));

						if (GUILayout.Button("", OwlcatEditorStyles.Instance.OpenButton, GUILayout.ExpandWidth(false)))
						{
							var m = new GenericMenu();
							foreach (var filter in PossibleTypeFilters.Where(t =>t.Name.Contains(m_TypeNameFilter)))
							{
								m.AddItem(
									new GUIContent(filter.Name),
									filter == AssetType,
									f =>
									{
										GUIUtility.keyboardControl = GUIUtility.hotControl = 0;
										AssetType = (Type)f;
										UpdateAssetList();
									},
									filter);
							}
							m.ShowAsContext();
						}
					}
                    ///


                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Type name filter:", GUILayout.ExpandWidth(false));

                        EditorGUI.BeginChangeCheck();
                        GUI.SetNextControlName("TypeNameFilter");
                        m_TypeNameFilter = EditorGUILayout.TextField(m_TypeNameFilter ?? "", GUILayout.MaxWidth(200));
                        if (m_FocusTypeNameFilter)
                        {
                            EditorGUI.FocusTextInControl("TypeNameFilter");
                            m_FocusTypeNameFilter = false;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            UpdateFilter();
                        }
                    }

                    ///
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Name filter:", GUILayout.ExpandWidth(false));

                    EditorGUI.BeginChangeCheck();
                    GUI.SetNextControlName("NameFilter");
                    m_NameFilter = EditorGUILayout.TextField(m_NameFilter ?? "", GUILayout.MaxWidth(200));
                    if (m_FocusNameFilter)
                    {
                        EditorGUI.FocusTextInControl("NameFilter");
                        m_FocusNameFilter = false;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateFilter();
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(
						    m_IsFlatMode ? "Flat mode" : "Folders mode",
						    GUILayout.Width(90),
						    GUILayout.Height(18)))
					{
						m_IsFlatMode = !m_IsFlatMode;
					}
				}
				using (new EditorGUILayout.HorizontalScope())
				{
				}

				if (m_ShowTimeOfDaySelection)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Time of day:", GUILayout.ExpandWidth(false));
						s_SelectedTimeOfDay = (TimeOfDay)EditorGUILayout.EnumPopup(s_SelectedTimeOfDay, GUILayout.MaxWidth(200));
					}
				}
			}
			// asset list
			using (var scope = new EditorGUILayout.ScrollViewScope(m_Scroll, GUI.skin.box, GUILayout.ExpandHeight(true)))
			{
				m_Scroll = scope.scrollPosition;
                m_CurrentLine = 0;
                m_CurrentIndent = 0;
                using (ProfileScope.New("List root"))
                {
                    DisplayList(m_IsFlatMode ? m_LoadedAssetsFlat : m_LoadedAssets);
                }

                GUILayoutUtility.GetRect(0, 0, m_CurrentLine, m_CurrentLine); // layout rect to make scroll work
            }

			if (m_MustScrollToSelected)
			{
				Repaint();
			}
		}

        private float m_CurrentLine;
        private float m_CurrentIndent;
        
		private void HandleKeyDown()
		{
			// special keyboard control when windown is in picker mode
			if (m_SelectionCallback == null || Event.current.type != EventType.KeyDown)
			{
				return;
			}

			switch (Event.current.keyCode)
			{
				case KeyCode.KeypadEnter:
				case KeyCode.Return:
					if (SelectedAssetEntry != null)
					{
						s_PreviewSelection = false;
						m_SelectionCallback(SelectedAssetEntry.Asset);
						Close();
					}
					break;
				case KeyCode.Escape:
					Close();
					return;
				case KeyCode.DownArrow:
					{
						SelectedAssetEntry =
							m_LoadedAssetsFlat
								.SkipWhile(e => e != SelectedAssetEntry)
								.Skip(1)
								.FirstOrDefault(e => !e.Hidden) ?? SelectedAssetEntry;
						m_MustScrollToSelected = true;
						if (SelectedAssetEntry != null && m_EnableSelectionOnClick)
						{
							s_PreviewSelection = true;
							m_SelectionCallback(SelectedAssetEntry.Asset);
						}
						Repaint();
					}
					break;
				case KeyCode.UpArrow:
					{
						SelectedAssetEntry =
							m_LoadedAssetsFlat
								.Reverse<HierarchyEntry>()
								.SkipWhile(e => e != SelectedAssetEntry)
								.Skip(1)
								.FirstOrDefault(e => !e.Hidden) ?? SelectedAssetEntry;
						m_MustScrollToSelected = true;
						if (SelectedAssetEntry != null && m_EnableSelectionOnClick)
						{
							s_PreviewSelection = true;
							m_SelectionCallback(SelectedAssetEntry.Asset);
						}
						Repaint();
					}
					break;
			}
		}

		private void DisplayList(List<HierarchyEntry> list)
		{
			if (list == null)
			{
				return;
			}
			foreach (var entry in list)
			{
				if (entry.Hidden)
				{
					continue;
				}

				if (entry.Children == null)
				{
					// this is a leaf node
                    using (ProfileScope.New("List leaf gui"))
					DisplayAsset(entry);
				}
				else
				{
					// this is a folder
                    Rect rect = new Rect(m_CurrentIndent, m_CurrentLine, position.width - m_CurrentIndent, EditorGUIUtility.singleLineHeight);
                    m_CurrentLine += rect.height;
                        
                    var ex = EditorGUI.Foldout(rect, entry.Expanded, entry.Name, true, FolderStyle);
					// store folded status
					if (ex != entry.Expanded)
					{
						if (ex)
						{
							m_FoldedPaths.Remove(entry.AssetGuid);
						}
						else
						{
							m_FoldedPaths.Add(entry.AssetGuid);
						}
						entry.Expanded = ex;
					}
					// show contents
					// ReSharper disable once AssignmentInConditionalExpression
					if (entry.Expanded)
					{
                        {
                            m_CurrentIndent += 10;
							DisplayList(entry.Children);
                            m_CurrentIndent -= 10;
                        }
					}
				}
			}
		}

        private void DisplayAsset(HierarchyEntry entry)
        {
            Rect rect = new Rect(m_CurrentIndent, m_CurrentLine, position.width - m_CurrentIndent, EditorGUIUtility.singleLineHeight);
            m_CurrentLine += rect.height;
            rect.xMin += 16;

            if (rect.yMax < m_Scroll.y)
                return;
            if (rect.yMin - m_Scroll.y > position.height)
                return;
            
            using (ProfileScope.New("Asset label"))
            {

                var selected = Selection.assetGUIDs.Contains(entry.AssetGuid) || entry == SelectedAssetEntry;
                if (Event.current.type == EventType.Repaint)
                {
                    if (m_MustScrollToSelected && selected)
                    {
                        m_Scroll.y = rect.yMin - (position.height - 50) / 2;
                        m_MustScrollToSelected = false;
                    }
                }

                if (rect.Contains(Event.current.mousePosition))
                {
                    // click & doubleclick
                    if (m_SelectionCallback != null)
                    {
                        if (Event.current.type == EventType.MouseDown &&
                            (m_EnableSelectionOnClick || Event.current.clickCount > 1))
                        {
                            s_PreviewSelection = Event.current.clickCount <= 1;
                            m_SelectionCallback(entry.Asset);
                            if (m_EnableSelectionOnClick)
                            {
                                SelectedAssetEntry = entry;
                            }

                            if (Event.current.clickCount > 1)
                            {
                                Close();
                            }

                            Event.current.Use();
                        }
                    }

                    // drag
                    if (m_SelectionCallback == null && Event.current.type == EventType.MouseDrag)
                    {
                        DragAndDrop.PrepareStartDrag();
                        if (!selected)
                        {
                            DragAndDrop.objectReferences = new[] {entry.Asset};
                        }
                        else
                        {
                            DragAndDrop.objectReferences = Selection.objects;
                        }

                        DragAndDrop.StartDrag("AssetPicker");
                        Event.current.Use();
                    }

                    // context menu
                    if (Event.current.type == EventType.ContextClick)
                    {
                        Vector2 mousePos = Event.current.mousePosition;
                        var typeName = entry.Asset.GetType().Name;
                        DisplayAssetContextMenu(typeName, mousePos);
                        Event.current.Use();
                    }

                    // prevent button from eating RMB
                    if (Event.current.isMouse && Event.current.button != 0)
                    {
                        return;
                    }
                }

                var style = selected ? SelectedAssetStyle : AssetStyle;
                if (GUI.Button(rect, entry.DisplayName, style))
                {
                    if (m_SelectionCallback == null)
                    {

                        //// normal asset selection
                        //if (Event.current.control)
                        //{
                        //	// ctrl-click: add/remove to selection
                        //	Selection.objects = selected
                        //		? Selection.objects.Where(o => o != entry.Asset).ToArray()
                        //		: Selection.objects.Concat(new[] { entry.Asset }).ToArray();
                        //}
                        //// todo: shift-click
                        //else if (!Event.current.shift)
                        //{
                        //	// simple click - select asset
                        Selection.activeObject = entry.Asset;
                        //}
                    }
                    else
                    {
                        SelectedAssetEntry = entry;
                    }
                }
            }

        }

        public static void DisplayAssetContextMenu(string typeName, Vector2 mousePos)
		{
			FillContextMenuData();
			if (s_ContextMenusFirstLevel.Contains(typeName))
			{
				EditorUtility.DisplayPopupMenu(
					new Rect(mousePos.x, mousePos.y, 0, 0),
					"CONTEXT/" + typeName + "/",
					null);
			}
		}

		private HierarchyEntry m_FirstVisibleEntry = null;

		private void UpdateFilter()
		{
			m_FirstVisibleEntry = null;
			UpdateFilterRecursive(m_LoadedAssets);
			if (m_SelectionCallback != null)
			{
				if (SelectedAssetEntry == null || SelectedAssetEntry.Hidden)
				{
					SelectedAssetEntry = m_FirstVisibleEntry;
				}
				m_MustScrollToSelected = true;
			}
		}

		private bool UpdateFilterRecursive(List<HierarchyEntry> list)
		{
			if (list == null)
			{
				return false;
			}

			var res = false;
			foreach (var entry in list)
			{
				if (entry.Children != null)
				{
					entry.Hidden = !UpdateFilterRecursive(entry.Children);
				}
				else
				{
					entry.Hidden = !MatchesName(entry, m_NameFilter);

					if (!entry.Hidden && m_FirstVisibleEntry == null)
					{
						m_FirstVisibleEntry = entry;
					}
				}

				res |= !entry.Hidden;
			}
			return res;
		}

		private bool MatchesName(HierarchyEntry entry, string nameFilter)
		{
			if (string.IsNullOrEmpty(nameFilter))
			{
				return true;
			}

			foreach (var filter in nameFilter.ToLowerInvariant().Split())
			{
				if (!entry.Path.ToLowerInvariant().Contains(filter))
				{
					return false;
				}
			}

			return true;
		}

		private void UpdateAssetList()
		{
			if (!Launcher.Launched)
				return;
			m_LoadedAssets = new List<HierarchyEntry>();
			m_LoadedAssetsFlat.Clear();

			if (Kingmaker.Blueprints.ResourcesLibrary.BlueprintsCache.m_PackSerializer is null)
				Kingmaker.Blueprints.ResourcesLibrary.BlueprintsCache.Init();

			var bpsUnfiltered = AssetDatabase.FindAssets((AssetType != null ? $"t:{AssetType.Name}" : "t:SimpleBlueprint"), new string[] { "Assets/Blueprints Library", "Assets/Modifications" });
			var bps = bpsUnfiltered
				.Select(id => AssetDatabase.GUIDToAssetPath(id))
				.ToArray();


			var existingBlueprints = bps
                .Select(address => new Tuple<string, string>(GetGuid(address), address)).ToArray();
            
			IEnumerable<Ex.Kingmaker.Cheats.BlueprintList.Entry> entries = Cheats.Utilities.GetAllBlueprints().Entries;

            if (AssetType != null)
                entries = entries.Where(a => a.Type.IsSubclassOf( AssetType) || a.Type == AssetType);
            IEnumerable<Tuple<string, string>> ids = entries
				.Select(entry => new Tuple<string, string>(entry.Guid, "BaseGame" + Path.DirectorySeparatorChar + entry.Path))
				.Where(id => !existingBlueprints.Any(bp => bp.Item1 == id.Item1))
				.Concat(existingBlueprints);			

            foreach (var pair in ids)
			{
				AddAssetInfo(pair.Item1, pair.Item2, false, false);
			}

			FilterAssets(ref m_LoadedAssets);
			m_LoadedAssets.ForEach(SortChildren);

			m_LoadedAssetsFlat.Clear();
			BuildFlatAssets(m_LoadedAssets);

			UpdateFilter();
			if (m_SelectionCallback != null)
			{
				SelectedAssetEntry = m_LoadedAssetsFlat.FirstOrDefault();
			}


			string GetGuid(string address)
			{
				var importer = AssetImporter.GetAtPath(address) as BlueprintImporter;
				if (importer == null || importer.wrapper == null)
					return string.Empty;
				else
				return importer.wrapper.AssetId;

            }
		}

		private void AddAssetInfo(string guid, string path, bool isShadowDeleted, bool ContainsShadowDeletedBlueprints)
		{
			var folders = path.Split('/','\\');
			var list = m_LoadedAssets;
			HierarchyEntry parent = null;
			for (int ii = 1; ii < folders.Length - 1; ii++) // 1 to skip Blueprints folder, -1 to skip asset name
			{
				var folder = folders[ii];
				var p = parent;
				parent = list.SingleOrDefault(e => e.Name == folder && e.Children != null);
				if (parent == null)
				{
					parent = new HierarchyEntry
					{
						Parent = p,
						Name = folder,
						Path = string.Join("/", folders, 0, ii + 1),
					};
					list.Add(parent);
					parent.Expanded = !m_FoldedPaths.Contains(parent.Path);
				}
				list = parent.Children ??= new List<HierarchyEntry>();
			}
			var hierarchyEntry = new HierarchyEntry
			{
				Parent = parent,
				Name = Path.GetFileNameWithoutExtension(path),
				Path = path,
				AssetGuid = guid,
				IsShadowDeleted = isShadowDeleted,
				ContainsShadowDeletedBlueprints = ContainsShadowDeletedBlueprints,
				Children = null
			};
			hierarchyEntry.DisplayName = hierarchyEntry.IsShadowDeleted
				? $"<color=#ff0000ff>{hierarchyEntry.Name}</color>"
				: hierarchyEntry.ContainsShadowDeletedBlueprints 
					? $"<color=#ffa500ff>{hierarchyEntry.Name}</color>" 
					: hierarchyEntry.Name;
			list.Add(hierarchyEntry);
		}

		private void SortChildren(HierarchyEntry entry)
		{
			if (entry.Children == null)
			{
				return;
			}

			entry.Children.Sort();
			foreach (var child in entry.Children)
			{
				SortChildren(child);
			}
		}

		private void FilterAssets(ref List<HierarchyEntry> entries)
		{
			bool prefabType = typeof(MonoBehaviour).IsAssignableFrom(AssetType);
			if (!prefabType
				&& m_Filters.Count <= 0)
			{
				return;
			}

			if (entries == null)
			{
				return;
			}

			entries.ForEach(e => FilterAssets(ref e.Children));

			entries = entries
				.Where(FitsFilters)
				.ToList();
		}

		private bool FitsFilters(HierarchyEntry he)
		{
			if (he.Children != null && he.Children.Count > 0)
				return true;
			foreach (var filter in m_Filters)
			{
				if (!filter(he))
					return false;
			}

			return true;
		}

		private void BuildFlatAssets(List<HierarchyEntry> entries)
		{
			foreach (var e in entries)
			{
				if (e.Children != null)
				{
					BuildFlatAssets(e.Children);
				}
				else
				{
					m_LoadedAssetsFlat.Add(e);
				}
			}
		}
	}

}