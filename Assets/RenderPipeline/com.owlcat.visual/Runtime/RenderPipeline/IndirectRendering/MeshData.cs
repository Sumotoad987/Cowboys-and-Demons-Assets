using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, packingRules = PackingRules.Exact, needAccessors = false)]
	public struct MeshData
	{
		public Vector3 aabbMin;

		public Vector3 aabbMax;

		public Vector2 unused;
	}
}
