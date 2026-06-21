using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Debugging
{
	public class GPUDebugSoA : GPUSoABase
	{
		public ComputeBuffer DrawParticlesArgsBuffer;

		public ComputeBuffer DebugParticleIndicesBuffer;

		public ComputeBuffer DrawDistanceConstraintsArgsBuffer;

		public ComputeBuffer DebugDistanceConstraintsIndicesBuffer;

		public ComputeBuffer DrawNormalsArgsBuffer;

		public ComputeBuffer DrawCollidersGridArgsBuffer;

		public ComputeBuffer DrawCollidersAabbArgsBuffer;

		public ComputeBuffer DrawForceVolumeAabbArgsBuffer;

		public ComputeBuffer DrawForceVolumeGridArgsBuffer;

		public ComputeBuffer DrawBodiesAabbArgsBuffer;

		public override string Name => "GPUDebugSoA";

		protected override IEnumerable<KeyValuePair<int, ComputeBuffer>> EnumerateBuffers()
		{
			yield return new KeyValuePair<int, ComputeBuffer>(0, DrawParticlesArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(1, DebugParticleIndicesBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(2, DrawDistanceConstraintsArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(3, DebugDistanceConstraintsIndicesBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(4, DrawNormalsArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(5, DrawCollidersGridArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(6, DrawCollidersAabbArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(7, DrawForceVolumeAabbArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(8, DrawForceVolumeGridArgsBuffer);
			yield return new KeyValuePair<int, ComputeBuffer>(9, DrawBodiesAabbArgsBuffer);
		}
	}
}
