using System;
using System.IO;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem
{
	[Serializable]
	public  class BlueprintJsonWrapper : Ex.Kingmaker.Blueprints.JsonSystem.BlueprintJsonWrapper 
	{
		public BlueprintJsonWrapper () : base()
		{
		
		}
		public BlueprintJsonWrapper (Ex.Kingmaker.Blueprints.SimpleBlueprint bp) : base(bp)
		{
		
		}

	}
}
