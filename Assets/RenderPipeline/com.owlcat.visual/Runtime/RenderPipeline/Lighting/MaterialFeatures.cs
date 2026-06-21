using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1)]
	public enum MaterialFeatures
	{
		Shadowmask = 1,
		LightingEnabled = 2,
		Translucent = 4,
		Reflections = 8,
		WrapDiffuse = 0x10
	}
}
