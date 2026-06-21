using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public sealed class DepthOfFieldModeParameter : VolumeParameter<DepthOfFieldMode>
	{
		public DepthOfFieldModeParameter(DepthOfFieldMode value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
