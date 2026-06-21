using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes
{
	public class FogSettingsPass : ScriptableRenderPass
	{
		private const string kProfilerTag = "Fog settings";

		private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog settings");

		private static int FogColor = Shader.PropertyToID("unity_FogColor");

		private static int FogParams = Shader.PropertyToID("unity_FogParams");

		public override string Name => "Fog settings";

		public FogSettingsPass(RenderPassEvent evt)
		{
			base.RenderPassEvent = evt;
		}

		public void Setup()
		{
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("Fog settings");
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				if (!RenderSettings.fog)
				{
					RenderSettings.fogColor = Color.clear;
					RenderSettings.fogStartDistance = 100000f;
					RenderSettings.fogEndDistance = 200000f;
				}
			}
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
