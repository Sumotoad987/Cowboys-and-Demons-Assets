using Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes
{
	public class IndirectRenderingSystemSubmitPass : ScriptableRenderPass
	{
		public override string Name => "IndirectRenderingSystemSubmitPass";

		public override bool IsOncePerFrame => true;

		public IndirectRenderingSystemSubmitPass(RenderPassEvent evt)
		{
			base.RenderPassEvent = evt;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			IndirectRenderingSystem.Instance.Submit();
		}
	}
}
