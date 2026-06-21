using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	public sealed class FilmGrainLookupParameter : VolumeParameter<FilmGrainLookup>
	{
		public FilmGrainLookupParameter(FilmGrainLookup value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
