using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess.HBAO
{
	[Serializable]
	public sealed class QualityParameter : VolumeParameter<Quality>
	{
		public QualityParameter(Quality value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
