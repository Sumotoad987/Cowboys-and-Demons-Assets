using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public class TracingMethodParameter : VolumeParameter<TracingMethod>
	{
		public TracingMethodParameter(TracingMethod value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
