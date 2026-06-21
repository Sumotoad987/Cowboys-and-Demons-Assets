using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess.HBAO
{
	[Serializable]
	public sealed class BlurParameter : VolumeParameter<Blur>
	{
		public BlurParameter(Blur value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
