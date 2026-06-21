using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public sealed class MotionBlurModeParameter : VolumeParameter<MotionBlurMode>
	{
		public MotionBlurModeParameter(MotionBlurMode value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
