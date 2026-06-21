using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	public abstract class ScriptableRendererData : ScriptableObject
	{
		[SerializeField]
		private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

		[SerializeField]
		private List<ScriptableRendererFeature> m_ConsoleFeaturesOverride = new List<ScriptableRendererFeature>(10);

		internal bool IsInvalidated { get; set; }

		public List<ScriptableRendererFeature> rendererFeatures => m_RendererFeatures;

		protected abstract ScriptableRenderer Create();

		internal ScriptableRenderer InternalCreateRenderer()
		{
			IsInvalidated = false;
			return Create();
		}

		protected virtual void OnValidate()
		{
			IsInvalidated = true;
		}

		protected virtual void OnEnable()
		{
			IsInvalidated = true;
		}
	}
}
