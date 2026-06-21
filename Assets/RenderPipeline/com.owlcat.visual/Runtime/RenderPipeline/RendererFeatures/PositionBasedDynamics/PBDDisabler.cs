using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics
{
	public class PBDDisabler : IDisposable
	{
		private bool initValue;

		public PBDDisabler()
		{
			initValue = PositionBasedDynamicsConfig.Instance.GPU;
			PositionBasedDynamicsConfig.Instance.GPU = false;
		}

		public void Dispose()
		{
			PositionBasedDynamicsConfig.Instance.GPU = initValue;
		}
	}
}
