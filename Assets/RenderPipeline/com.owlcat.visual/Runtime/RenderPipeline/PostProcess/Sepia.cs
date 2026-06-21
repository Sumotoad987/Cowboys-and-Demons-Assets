using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	[VolumeComponentMenu("Post-processing/Sepia")]
	public class Sepia : VolumeComponent, IPostProcessComponent
	{
		public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

		public bool IsActive()
		{
			return Intensity.value > 0f;
		}

		public bool IsTileCompatible()
		{
			return false;
		}
	}
}
