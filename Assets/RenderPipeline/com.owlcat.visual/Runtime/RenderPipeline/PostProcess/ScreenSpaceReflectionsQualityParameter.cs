using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public class ScreenSpaceReflectionsQualityParameter : VolumeParameter<ScreenSpaceReflectionsQuality>
	{
		public ScreenSpaceReflectionsQualityParameter(ScreenSpaceReflectionsQuality value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
