using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public class TextureCurveParameter : VolumeParameter<TextureCurve>
	{
		public TextureCurveParameter(TextureCurve value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
