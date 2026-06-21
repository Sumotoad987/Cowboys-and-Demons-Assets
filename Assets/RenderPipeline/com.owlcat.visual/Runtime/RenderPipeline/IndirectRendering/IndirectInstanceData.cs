using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, packingRules = PackingRules.Exact, needAccessors = false)]
	public struct IndirectInstanceData
	{
		public Matrix4x4 objectToWorld;

		public Matrix4x4 worldToObject;

		public uint meshID;

		public Vector3 tintColor;

		public Vector4 shadowmask;

		public uint hidden;

		public uint physicsDataIndex;

		public Vector2 unused;
	}
}
