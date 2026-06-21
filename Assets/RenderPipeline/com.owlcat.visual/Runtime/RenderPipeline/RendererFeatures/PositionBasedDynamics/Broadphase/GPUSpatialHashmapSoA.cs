using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Utils;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase
{
	public class GPUSpatialHashmapSoA : GPUSoABase
	{
		private static int _SpatialHashmapKeysBuffer = Shader.PropertyToID("_SpatialHashmapKeysBuffer");

		private static int _SpatialHashmapValuesBuffer = Shader.PropertyToID("_SpatialHashmapValuesBuffer");

		public ComputeBuffer KeysBuffer;

		public ComputeBuffer ValuesBuffer;

		public override string Name => "GPUSpatialHashmapSoA";

		public GPUSpatialHashmapSoA(int size)
			: base(size)
		{
		}

		protected override IEnumerable<KeyValuePair<int, ComputeBuffer>> EnumerateBuffers()
		{
			yield return new KeyValuePair<int, ComputeBuffer>(_SpatialHashmapKeysBuffer, KeysBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(_SpatialHashmapValuesBuffer, ValuesBuffer);
		}

		public override void Resize(int newSize)
		{
			base.Resize(newSize);
			KeysBuffer = ComputeBufferUtils.SetSize(KeysBuffer, typeof(uint), newSize, "PBD.SpatialHashmapKeysBuffer");
			ValuesBuffer = ComputeBufferUtils.SetSize(ValuesBuffer, typeof(uint), newSize, "PBD.SpatialHashmapValuesBuffer");
		}
	}
}
