using Owlcat.Runtime.Visual.RenderPipeline.Decals;
using Owlcat.Runtime.Visual.RenderPipeline.PostProcess;
using Owlcat.Runtime.Visual.RenderPipeline.Shadows;
using Owlcat.Runtime.Visual.RenderPipeline.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data
{
	public class OwlcatRenderPipelineAsset : RenderPipelineAsset
	{
		internal enum DefaultMaterialType
		{
			Standard,
			Decal,
			DecalFullScreen,
			Particle,
			Terrain,
			UnityBuiltinDefault,
			Skybox,
			UI
		}

		private Shader m_DefaultShader;

		private ScriptableRenderer m_Renderer;

		[SerializeField]
		private ScriptableRendererData m_RendererData;

		[SerializeField]
		private bool m_UseSRPBatcher = true;

		[SerializeField]
		private bool m_SupportsDynamicBatching = true;

		[SerializeField]
		private bool m_SupportsHDR;

		[SerializeField]
		private bool m_SupportsDistortion;

		[SerializeField]
		[Range(0.01f, 1f)]
		private float m_RenderScale = 1f;

		[SerializeField]
		private DecalSettings m_DecalSettings = new DecalSettings();

		[SerializeField]
		private ShadowSettings m_ShadowSettings = new ShadowSettings();

		[SerializeField]
		private PostProcessSettings m_PostProcessSettings = new PostProcessSettings();

		[SerializeField]
		private TerrainSettings m_TerrainSettings = new TerrainSettings();

		public ScriptableRendererData ScriptableRendererData => m_RendererData;

		public ScriptableRenderer ScriptableRenderer
		{
			get
			{
				if (m_RendererData == null)
				{
					Debug.LogError("Renderer is missing from the current Pipeline Asset.", this);
					return null;
				}
				if (m_RendererData.IsInvalidated || m_Renderer == null)
				{
					if (m_Renderer != null)
					{
						m_Renderer.Dispose();
					}
					m_Renderer = m_RendererData.InternalCreateRenderer();
				}
				return m_Renderer;
			}
		}

		public bool UseSRPBatcher
		{
			get
			{
				return m_UseSRPBatcher;
			}
			set
			{
				m_UseSRPBatcher = value;
			}
		}

		public bool SupportsDynamicBatching
		{
			get
			{
				return m_SupportsDynamicBatching;
			}
			set
			{
				m_SupportsDynamicBatching = value;
			}
		}

		public bool SupportsHDR
		{
			get
			{
				return m_SupportsHDR;
			}
			set
			{
				m_SupportsHDR = value;
			}
		}

		public bool SupportsDistortion
		{
			get
			{
				return m_SupportsDistortion;
			}
			set
			{
				m_SupportsDistortion = value;
			}
		}

		public float RenderScale
		{
			get
			{
				return m_RenderScale;
			}
			set
			{
				m_RenderScale = value;
			}
		}

		public DecalSettings DecalSettings => m_DecalSettings;

		public ShadowSettings ShadowSettings => m_ShadowSettings;

		public PostProcessSettings PostProcessSettings => m_PostProcessSettings;

		public TerrainSettings TerrainSettings => m_TerrainSettings;

		public override Shader defaultShader
		{
			get
			{
				if (m_DefaultShader == null)
				{
					m_DefaultShader = Shader.Find("Owlcat/Lit");
				}
				return m_DefaultShader;
			}
		}

		public override Material defaultMaterial => GetMaterial(DefaultMaterialType.Standard);

		public override Material defaultParticleMaterial => GetMaterial(DefaultMaterialType.Particle);

		public override Material defaultTerrainMaterial => GetMaterial(DefaultMaterialType.Terrain);

		public override Material defaultUIMaterial => GetMaterial(DefaultMaterialType.UI);

		public override Material defaultUIOverdrawMaterial => GetMaterial(DefaultMaterialType.UnityBuiltinDefault);

		public Material DefaultSkyboxMaterial => GetMaterial(DefaultMaterialType.Skybox);

		public override Shader terrainDetailLitShader => defaultShader;

		public override Shader terrainDetailGrassBillboardShader => defaultShader;

		public override Shader terrainDetailGrassShader => defaultShader;

		public ScriptableRendererData LoadBuiltinRendererData()
		{
			m_RendererData = null;
			return m_RendererData;
		}

		protected override UnityEngine.Rendering.RenderPipeline CreatePipeline()
		{
			if (m_RendererData == null)
			{
				LoadBuiltinRendererData();
			}
			if (m_RendererData == null)
			{
				return null;
			}
			m_Renderer = m_RendererData.InternalCreateRenderer();
			return new OwlcatRenderPipeline(this);
		}

		protected override void OnValidate()
		{
			DestroyRenderer();
			base.OnValidate();
		}

		private void DestroyRenderer()
		{
			if (m_Renderer != null)
			{
				m_Renderer.Dispose();
			}
		}

		protected override void OnDisable()
		{
			DestroyRenderer();
			base.OnDisable();
		}

		private Material GetMaterial(DefaultMaterialType materialType)
		{
			if (materialType == DefaultMaterialType.UI && m_RendererData != null)
			{
				ClusteredRendererData clusteredRendererData = m_RendererData as ClusteredRendererData;
				if (clusteredRendererData != null)
				{
					return clusteredRendererData.DefaultUIMaterial;
				}
			}
			return null;
		}

		public bool IsDefaultMaterial(Material material)
		{
			if (material == null)
			{
				return false;
			}
			if (material == default2DMaterial)
			{
				return true;
			}
			if (material == defaultLineMaterial)
			{
				return true;
			}
			if (material == defaultMaterial)
			{
				return true;
			}
			if (material == defaultParticleMaterial)
			{
				return true;
			}
			if (material == defaultTerrainMaterial)
			{
				return true;
			}
			if (material == GetDefaultDecalMaterial())
			{
				return true;
			}
			if (material == GetDefaultFullScreenDecalMaterial())
			{
				return true;
			}
			if (material == defaultUIETC1SupportedMaterial)
			{
				return true;
			}
			if (material == defaultUIMaterial)
			{
				return true;
			}
			if (material == defaultUIOverdrawMaterial)
			{
				return true;
			}
			if (material == DefaultSkyboxMaterial)
			{
				return true;
			}
			return false;
		}

		public Material GetDefaultDecalMaterial()
		{
			return GetMaterial(DefaultMaterialType.Decal);
		}

		public Material GetDefaultFullScreenDecalMaterial()
		{
			return GetMaterial(DefaultMaterialType.DecalFullScreen);
		}
	}
}
