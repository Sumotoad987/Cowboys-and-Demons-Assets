using Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes
{
	public class IndirectiRenderingSystemCullingPass : ScriptableRenderPass
	{
		public override string Name => "IndirectRenderingSystemCullingPass";

		public IndirectiRenderingSystemCullingPass(RenderPassEvent evt)
		{
			base.RenderPassEvent = evt;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			IndirectRenderingSystem.Instance.Cull(ref context, ref renderingData);
		}
	}
}
