using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	public abstract class ScriptableRendererFeature : ScriptableObject
	{
		public abstract string GetFeatureIdentifier();

		public abstract void Create();

		public abstract void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData);

		public abstract void DisableFeature();

		private void OnEnable()
		{
			Create();
		}

		private void OnValidate()
		{
			Create();
		}

		public virtual void Dispose()
		{
		}
	}
}
