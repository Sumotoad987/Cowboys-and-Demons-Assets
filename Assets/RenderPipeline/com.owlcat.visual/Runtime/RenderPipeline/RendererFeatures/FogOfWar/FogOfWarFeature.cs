using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar
{
	[CreateAssetMenu(menuName = "Renderer Features/Fow Of War")]
	[ReloadGroup]
	public class FogOfWarFeature : ScriptableRendererFeature
	{
		[Serializable]
		[ReloadGroup]
		public sealed class ShaderResources
		{
			[SerializeField]
			[Reload("Hidden/Owlcat/FogOfWar", ReloadAttribute.Package.Builtin)]
			public Shader FogOfWarShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/ScreenSpaceFogOfWar", ReloadAttribute.Package.Builtin)]
			public Shader ScreenSpaceFogOfWarShader;

			[SerializeField]
			[Reload("Hidden/MobileBlur", ReloadAttribute.Package.Builtin)]
			public Shader BlurShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/Blit", ReloadAttribute.Package.Builtin)]
			public Shader BlitShader;
		}

		private static FogOfWarFeature s_Instance;

		public ShaderResources Shaders;

		[SerializeField]
		private Color m_Color = new Color(0f, 0f, 0f, 0.5f);

		[SerializeField]
		private float m_ShadowFalloff = 0.15f;

		[SerializeField]
		private float m_ShadowCullingHeightOffset = 1f;

		[SerializeField]
		private float m_ShadowCullingHeight = 1f;

		[SerializeField]
		private float m_RevealerInnerRadius = 1f;

		[SerializeField]
		private float m_RevealerOutterRadius = 11.7f;

		[SerializeField]
		private float m_BorderWidth = 6.35f;

		[SerializeField]
		private float m_BorderOffset = 3.22f;

		[SerializeField]
		private bool m_IsBlurEnabled = true;

		[SerializeField]
		[Range(1f, 4f)]
		private int m_BlurIterations = 2;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_BlurSize = 3f;

		[Range(0f, 2f)]
		private int m_BlurDownsample = 1;

		[SerializeField]
		private BlurType m_BlurType;

		[SerializeField]
		private float m_TextureDensity = 5f;

		[SerializeField]
		private bool m_ShowDebug;

		[SerializeField]
		[Range(128f, 4096f)]
		private int m_DebugSize = 256;

		private List<FogOfWarRevealer> m_Revealers = new List<FogOfWarRevealer>();

		private Mesh m_QuadMesh;

		private FogOfWarShadowmapPass m_ShadowmapPass;

		private FogOfWarSetupPass m_SetupPass;

		private FogOfWarPostProcessPass m_PostProcessPass;

		private FogOfWarDebugPass m_DebugPass;

		public Color Color => m_Color;

		public float ShadowFalloff => m_ShadowFalloff;

		public float ShadowCullingHeightOffset => m_ShadowCullingHeightOffset;

		public float ShadowCullingHeight => m_ShadowCullingHeight;

		public float RevealerInnerRadius => m_RevealerInnerRadius;

		public float RevealerOutterRadius => m_RevealerOutterRadius;

		public float BorderWidth => m_BorderWidth;

		public float BorderOffset => m_BorderOffset;

		public bool IsBlurEnabled => m_IsBlurEnabled;

		public int BlurIterations => m_BlurIterations;

		public float BlurSize => m_BlurSize;

		public int BlurDownsample => m_BlurDownsample;

		public BlurType BlurType => m_BlurType;

		public float TextureDensity => m_TextureDensity;

		public List<FogOfWarRevealer> Revealers => m_Revealers;

		public bool ShowDebug
		{
			get
			{
				return m_ShowDebug;
			}
			set
			{
				m_ShowDebug = value;
			}
		}

		public int DebugSize
		{
			get
			{
				return m_DebugSize;
			}
			set
			{
				m_DebugSize = value;
			}
		}

		public Mesh QuadMesh
		{
			get
			{
				if (m_QuadMesh == null)
				{
					CreateQuadMesh();
				}
				return m_QuadMesh;
			}
		}

		public bool IsScissorRectDisabled { get; set; }

		public static FogOfWarFeature Instance => s_Instance;

		public static bool IsActive
		{
			get
			{
				if (Instance != null && FogOfWarArea.Active != null)
				{
					return FogOfWarArea.Active.isActiveAndEnabled;
				}
				return false;
			}
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			CameraType cameraType = renderingData.CameraData.Camera.cameraType;
			if (cameraType != CameraType.Preview && cameraType != CameraType.Reflection)
			{
				FogOfWarArea active = FogOfWarArea.Active;
				m_ShadowmapPass.Setup(this, active);
				renderer.EnqueuePass(m_ShadowmapPass);
				m_SetupPass.Setup(this, active);
				renderer.EnqueuePass(m_SetupPass);
				if (renderer is ClusteredRenderer clusteredRenderer && active != null && !active.ApplyShaderManually)
				{
					m_PostProcessPass.Setup(clusteredRenderer.GetCurrentCameraColorTexture());
					renderer.EnqueuePass(m_PostProcessPass);
				}
				if (m_ShowDebug)
				{
					m_DebugPass.Setup(active);
					renderer.EnqueuePass(m_DebugPass);
				}
			}
		}

		public override void Create()
		{
			s_Instance = this;
			Material fowMaterial = CoreUtils.CreateEngineMaterial(Shaders.FogOfWarShader);
			Material material = CoreUtils.CreateEngineMaterial(Shaders.ScreenSpaceFogOfWarShader);
			Material blurMaterial = CoreUtils.CreateEngineMaterial(Shaders.BlurShader);
			Material blitMaterial = CoreUtils.CreateEngineMaterial(Shaders.BlitShader);
			m_ShadowmapPass = new FogOfWarShadowmapPass(RenderPassEvent.BeforeRendering, fowMaterial, blurMaterial);
			m_SetupPass = new FogOfWarSetupPass(RenderPassEvent.BeforeRendering);
			m_PostProcessPass = new FogOfWarPostProcessPass(RenderPassEvent.BeforeRenderingTransparents, material);
			m_DebugPass = new FogOfWarDebugPass(RenderPassEvent.AfterRendering, blitMaterial);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (m_QuadMesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_QuadMesh);
			}
		}

		private void CreateQuadMesh()
		{
			m_QuadMesh = new Mesh
			{
				name = "FOW_Quad"
			};
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(1f, 0f, 1f),
				new Vector3(1f, 0f, -1f),
				new Vector3(-1f, 0f, 1f),
				new Vector3(-1f, 0f, -1f)
			};
			Vector2[] uv = new Vector2[4]
			{
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
				new Vector2(0f, 1f),
				new Vector2(0f, 0f)
			};
			int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
			m_QuadMesh.vertices = vertices;
			m_QuadMesh.uv = uv;
			m_QuadMesh.triangles = triangles;
		}

		public override string GetFeatureIdentifier()
		{
			return "FogOfWarFeature";
		}

		public override void DisableFeature()
		{
			Shader.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
		}
	}
}
