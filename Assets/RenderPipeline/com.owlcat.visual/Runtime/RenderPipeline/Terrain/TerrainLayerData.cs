using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Terrain
{
	[Serializable]
	[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, false, -1)]
	public struct TerrainLayerData
	{
		public Vector4 masksScale;

		public Vector4 uvMatrix;

		public float uvScale;

		public int diffuseTexIndex;

		public int normalTexIndex;

		public int masksTexIndex;
	}
}
