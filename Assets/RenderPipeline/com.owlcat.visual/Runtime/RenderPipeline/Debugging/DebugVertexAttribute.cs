using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1)]
	public enum DebugVertexAttribute
	{
		None,
		ColorRGB,
		ColorR,
		ColorG,
		ColorB,
		ColorAlpha
	}
}
