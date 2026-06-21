using System;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	[Flags]
	public enum StencilRef
	{
		ReceiveDecals = 1,
		SpecialPostProcessFlag = 2,
		Distortion = 4,
		Reserve3 = 8,
		Reserve4 = 0x10,
		Reserve5 = 0x20,
		Reserve6 = 0x40,
		Reserve7 = 0x80
	}
}
