using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MyOwlcatModification.Editor.Utility
{
	public static class BlueprintTypesCache
	{
		public class Entry
		{
			public readonly Type Type;
			public readonly string Name;
			public readonly string NameLowerInvariant;
			public readonly Guid Guid;

			public Entry(Type type, string name, Guid guid)
			{
				Type = type;
				Name = name;
				NameLowerInvariant = name.ToLowerInvariant();
				Guid = guid;
			}
		}

		private static AdvancedDropdownItem s_cachedAdvancedDropdownComponentItems;
        private static AdvancedDropdownItem s_cachedAdvancedDropdownActionItems;
        private static List<Entry> s_TypeCache;
		private static List<Entry> s_Blueprints;

		public static List<Entry> Blueprints
		{
			get
			{

                PrepareTypeCache();
                return s_Blueprints;
            }
		}

		public static IEnumerable<Entry> Types {
			get 
			{
				PrepareTypeCache();
				return s_TypeCache;
			}
		}

		public static AdvancedDropdownItem CachedAdvancedDropdownComponentItems
		{
			get
            {
                PrepareTypeCache();
                return s_cachedAdvancedDropdownComponentItems;
            }
        }
        public static AdvancedDropdownItem CachedAdvancedDropdownActionItems
        {
            get
            {
                PrepareTypeCache();
                return s_cachedAdvancedDropdownActionItems;
            }
        }

        private static void PrepareTypeCache()
		{
			if (s_TypeCache != null)
			{
				return;
			}

			s_TypeCache = new List<Entry>();
			s_Blueprints = new List<Entry>();
			s_cachedAdvancedDropdownComponentItems = new AdvancedDropdownItem("Root");
            s_cachedAdvancedDropdownActionItems = new AdvancedDropdownItem("Root");
            var newAssembly = Assembly.GetAssembly(typeof(SimpleBlueprint));
			var oldAssembly = Assembly.GetAssembly(typeof(Ex.Kingmaker.Blueprints.BlueprintComponent));
            foreach (var type in newAssembly.GetTypes().Concat(oldAssembly.GetTypes()))
			{
				if (!GuidClassBinder.IsIdentifiedType(type))
				{
					continue;
				}

				var typeId = type.GetCustomAttribute<TypeIdAttribute>();
				if (typeId != null)
				{
					if ( Guid.Parse(typeId.GuidString) == default)
                        UnityEngine.Debug.LogError($"failed to parse guid {typeId.GuidString} on the TypeIdAttribute of the type {type.Name}");
                    s_TypeCache.Add(new Entry(type, type.Name, typeId.Guid));

					if (type.IsAbstract)
						return;
					if (type.IsOrSubclassOf<Ex.Kingmaker.Blueprints.BlueprintComponent>())
						type.TryAddToDropdownPrototype(s_cachedAdvancedDropdownComponentItems);
					else if (type.IsOrSubclassOf<Kingmaker.ElementsSystem.GameAction>())
                        type.TryAddToDropdownPrototype(s_cachedAdvancedDropdownActionItems);
                    else if (type.IsOrSubclassOf<Ex.Kingmaker.Blueprints.SimpleBlueprint>())
                        s_Blueprints.Add(new Entry(type, type.Name, typeId.Guid));

                }
			}
		}

		private static void TryAddToDropdownPrototype(this Type type, AdvancedDropdownItem dropdownItem)
		{
			var theItem = new DropdownItemTyped(type);

            string Namespace = type.Namespace;
			if (Namespace.IsNullOrEmpty())
			{
                dropdownItem.AddChild(theItem);
				return;
			}

            var subspaces = Namespace.Split('.');
			AdvancedDropdownItem item = dropdownItem;

            for (int i = 0; i < subspaces.Length; i++)
			{
				if (!item.children.TryFind(item => item.name == subspaces[i], out var newItem))
				{
                    newItem = new AdvancedDropdownItem(subspaces[i]);
					item.AddChild(newItem);
                }
				item = newItem;
			}
			item.AddChild(theItem);
		}
	}
}