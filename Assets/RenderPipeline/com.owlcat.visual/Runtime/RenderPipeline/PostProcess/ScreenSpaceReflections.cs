using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	[VolumeComponentMenu("Post-processing/Screen Space Reflections")]
	public class ScreenSpaceReflections : VolumeComponent, IPostProcessComponent
	{
		public ScreenSpaceReflectionsQualityParameter Quality = new ScreenSpaceReflectionsQualityParameter(ScreenSpaceReflectionsQuality.None);

		public TracingMethodParameter TracingMethod = new TracingMethodParameter(Owlcat.Runtime.Visual.RenderPipeline.PostProcess.TracingMethod.HiZ);

		public ClampedIntParameter MaxRaySteps = new ClampedIntParameter(32, 0, 512);

		public ClampedFloatParameter MaxDistance = new ClampedFloatParameter(50f, 0f, 1000f);

		public ClampedIntParameter ScreenSpaceStepSize = new ClampedIntParameter(5, 1, 16);

		public ClampedFloatParameter MaxRoughness = new ClampedFloatParameter(1f, 0f, 1f);

		public ClampedFloatParameter RoughnessFadeStart = new ClampedFloatParameter(0f, 0f, 1f);

		public ClampedFloatParameter ObjectThickness = new ClampedFloatParameter(0.001f, 0.001f, 1f);

		public ClampedFloatParameter FresnelPower = new ClampedFloatParameter(2f, 1f, 4f);

		public ClampedFloatParameter ConfidenceScale = new ClampedFloatParameter(1f, 1f, 10f);

		public BoolParameter UseUpsamplePass = new BoolParameter(value: true);

		public bool IsActive()
		{
			return Quality.value != ScreenSpaceReflectionsQuality.None;
		}

		public bool IsTileCompatible()
		{
			return false;
		}
	}
}
