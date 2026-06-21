using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[ImageEffectAllowedInSceneView]
	public class OwlcatAdditionalCameraData : MonoBehaviour
	{
		[SerializeField]
		private LayerMask m_VolumeLayerMask = 1;

		[SerializeField]
		private Transform m_VolumeTrigger;

		[SerializeField]
		private bool m_RenderPostProcessing;

		[SerializeField]
		private AntialiasingMode m_Antialiasing;

		[SerializeField]
		private AntialiasingQuality m_AntialiasingQuality = AntialiasingQuality.High;

		[SerializeField]
		private bool m_Dithering;

		[SerializeField]
		private bool m_AllowLighting = true;

		[SerializeField]
		private bool m_AllowDecals = true;

		[SerializeField]
		private bool m_AllowDistortion = true;

		[SerializeField]
		private bool m_AllowIndirectRendering = true;

		[SerializeField]
		private bool m_AllowFog = true;

		[SerializeField]
		private bool m_AllowVfxPreparation = true;

		[SerializeField]
		private RenderTexture m_DepthTexture;

		[SerializeField]
		private List<RendererFeatureFlag> m_RendererFeaturesFlags = new List<RendererFeatureFlag>();

		public LayerMask VolumeLayerMask
		{
			get
			{
				return m_VolumeLayerMask;
			}
			set
			{
				m_VolumeLayerMask = value;
			}
		}

		public Transform VolumeTrigger
		{
			get
			{
				return m_VolumeTrigger;
			}
			set
			{
				m_VolumeTrigger = value;
			}
		}

		public bool RenderPostProcessing
		{
			get
			{
				return m_RenderPostProcessing;
			}
			set
			{
				m_RenderPostProcessing = value;
			}
		}

		public AntialiasingMode Antialiasing
		{
			get
			{
				return m_Antialiasing;
			}
			set
			{
				m_Antialiasing = value;
			}
		}

		public AntialiasingQuality AntialiasingQuality
		{
			get
			{
				return m_AntialiasingQuality;
			}
			set
			{
				m_AntialiasingQuality = value;
			}
		}

		public bool Dithering
		{
			get
			{
				return m_Dithering;
			}
			set
			{
				m_Dithering = value;
			}
		}

		public bool AllowLighting
		{
			get
			{
				return m_AllowLighting;
			}
			set
			{
				m_AllowLighting = value;
			}
		}

		public bool AllowDecals
		{
			get
			{
				return m_AllowDecals;
			}
			set
			{
				m_AllowDecals = value;
			}
		}

		public bool AllowDistortion
		{
			get
			{
				return m_AllowDistortion;
			}
			set
			{
				m_AllowDistortion = value;
			}
		}

		public bool AllowIndirectRendering
		{
			get
			{
				return m_AllowIndirectRendering;
			}
			set
			{
				m_AllowIndirectRendering = value;
			}
		}

		public bool AllowFog
		{
			get
			{
				return m_AllowFog;
			}
			set
			{
				m_AllowFog = value;
			}
		}

		public bool AllowVfxPreparation
		{
			get
			{
				return m_AllowVfxPreparation;
			}
			set
			{
				m_AllowVfxPreparation = value;
			}
		}

		public RenderTexture DepthTexture
		{
			get
			{
				return m_DepthTexture;
			}
			set
			{
				m_DepthTexture = value;
			}
		}

		public void GetDisabledFeatures(HashSet<string> disabledFeatures)
		{
			foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
			{
				if (!rendererFeaturesFlag.Enabled && !disabledFeatures.Contains(rendererFeaturesFlag.FeatureIdentifier))
				{
					disabledFeatures.Add(rendererFeaturesFlag.FeatureIdentifier);
				}
			}
		}

		public void DisableFeature(ScriptableRendererFeature feature)
		{
			if (feature == null)
			{
				return;
			}
			bool flag = false;
			foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
			{
				if (rendererFeaturesFlag.FeatureIdentifier == feature.GetFeatureIdentifier())
				{
					rendererFeaturesFlag.Enabled = false;
					flag = true;
				}
			}
			if (!flag)
			{
				m_RendererFeaturesFlags.Add(new RendererFeatureFlag
				{
					FeatureIdentifier = feature.GetFeatureIdentifier(),
					Enabled = false
				});
			}
		}

		public void EnableFeature(ScriptableRendererFeature feature)
		{
			if (feature == null)
			{
				return;
			}
			foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
			{
				if (rendererFeaturesFlag.FeatureIdentifier == feature.GetFeatureIdentifier())
				{
					rendererFeaturesFlag.Enabled = true;
				}
			}
		}

		public void DisableAllFeatures()
		{
			OwlcatRenderPipelineAsset asset = OwlcatRenderPipeline.Asset;
			if (!(asset != null) || !(asset.ScriptableRendererData != null))
			{
				return;
			}
			foreach (ScriptableRendererFeature rendererFeature in asset.ScriptableRendererData.rendererFeatures)
			{
				DisableFeature(rendererFeature);
			}
		}
	}
}
