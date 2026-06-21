using System;
using System.IO;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.Debugging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	[ReloadGroup]
	public class ClusteredRendererData : ScriptableRendererData
	{
		[Serializable]
		[ReloadGroup]
		public class ShaderResources
		{
			private const string kBasePath = "Assets/Code/Owlcat/";

			[SerializeField]
			[Reload("Hidden/Owlcat/CopyDepth", ReloadAttribute.Package.Builtin)]
			public Shader CopyDepthShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/CopyDepthSimple", ReloadAttribute.Package.Builtin)]
			public Shader CopyDepthFastShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/Shaders/Utils/DepthPyramid.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader DepthPyramidShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/Blit", ReloadAttribute.Package.Builtin)]
			public Shader BlitShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/FinalBlit", ReloadAttribute.Package.Builtin)]
			public Shader FinalBlitShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/Lighting/LightCulling.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader LightCullingShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/DeferredReflections", ReloadAttribute.Package.Builtin)]
			public Shader DeferredReflectionsShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/DeferredLighting", ReloadAttribute.Package.Builtin)]
			public Shader DeferredLightingShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/Lighting/DeferredLighting.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader DeferredLightingComputeShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/IndirectRendering/IndirectCulling.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader IndirectRenderingCullShader;

			[SerializeField]
			[Reload("Owlcat/Skybox/Procedural", ReloadAttribute.Package.Builtin)]
			public Shader SkyboxShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/ColorPyramid", ReloadAttribute.Package.Builtin)]
			public Shader ColorPyramidShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/ApplyDistortion", ReloadAttribute.Package.Builtin)]
			public Shader ApplyDistortionShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/Fog", ReloadAttribute.Package.Builtin)]
			public Shader FogShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/HBAO", ReloadAttribute.Package.Builtin)]
			public Shader HbaoShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/ScreenSpaceCloudShadows", ReloadAttribute.Package.Builtin)]
			public Shader ScreenSpaceCloudShadowsShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/ScreenSpaceReflections", ReloadAttribute.Package.Builtin)]
			public Shader ScreenSpaceReflectionsShaderPS;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/Shaders/PostProcessing/ScreenSpaceReflections/ScreenSpaceReflections.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader ScreenSpaceReflectionsShaderCS;

			[SerializeField]
			[Reload("Hidden/Owlcat/DBufferBlit", ReloadAttribute.Package.Builtin)]
			public Shader DBufferBlitShader;
		}

		public const int kMaxSliceCount = 64;

		public ShaderResources Shaders = new ShaderResources();

		public string ShadersBundlePath;

		[SerializeField]
		private PostProcessData m_PostProcessData;

		[SerializeField]
		private DebugData m_DebugData;

		[SerializeField]
		private TileSize m_TileSize = TileSize.Tile16;

		[SerializeField]
		private RenderPath m_RenderPath;

		[SerializeField]
		private bool m_UseComputeInDeferredPath;

		[SerializeField]
		private LayerMask m_OpaqueLayerMask = -1;

		[SerializeField]
		private LayerMask m_TransparentLayerMask = -1;

		[SerializeField]
		private StencilStateData m_DefaultStencilState;

		[SerializeField]
		private Material m_DefaultUIMaterial;

		public PostProcessData PostProcessData => m_PostProcessData;

		public DebugData DebugData => m_DebugData;

		public TileSize TileSize => m_TileSize;

		public RenderPath RenderPath
		{
			get
			{
				return m_RenderPath;
			}
			set
			{
				m_RenderPath = value;
			}
		}

		public bool UseComputeInDeferredPath
		{
			get
			{
				return m_UseComputeInDeferredPath;
			}
			set
			{
				m_UseComputeInDeferredPath = value;
			}
		}

		public LayerMask OpaqueLayerMask => m_OpaqueLayerMask;

		public LayerMask TransparentLayerMask => m_TransparentLayerMask;

		public StencilStateData DefaultStencilState => m_DefaultStencilState;

		public Material DefaultUIMaterial => m_DefaultUIMaterial;

		protected override ScriptableRenderer Create()
		{
			try
			{
				TryLoadMissingShadersFromBundle(Shaders, ShadersBundlePath);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			return new ClusteredRenderer(this);
		}

		private static void TryLoadMissingShadersFromBundle(ShaderResources resources, string assetBundlePath)
		{
			if (!string.IsNullOrEmpty(assetBundlePath) && File.Exists(assetBundlePath))
			{
				AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
				resources.CopyDepthShader = LoadShaderIfNull(resources.CopyDepthShader, "Hidden/Owlcat/CopyDepth", assetBundle);
				resources.CopyDepthFastShader = LoadShaderIfNull(resources.CopyDepthFastShader, "Hidden/Owlcat/CopyDepthSimple", assetBundle);
				resources.DepthPyramidShader = LoadComputeIfNull(resources.DepthPyramidShader, "DepthPyramid.compute", assetBundle);
				resources.BlitShader = LoadShaderIfNull(resources.BlitShader, "Hidden/Owlcat/Blit", assetBundle);
				resources.FinalBlitShader = LoadShaderIfNull(resources.FinalBlitShader, "Hidden/Owlcat/FinalBlit", assetBundle);
				resources.LightCullingShader = LoadComputeIfNull(resources.LightCullingShader, "LightCulling.compute", assetBundle);
				resources.DeferredReflectionsShader = LoadShaderIfNull(resources.DeferredReflectionsShader, "Hidden/Owlcat/DeferredReflections", assetBundle);
				resources.DeferredLightingShader = LoadShaderIfNull(resources.DeferredLightingShader, "Hidden/Owlcat/DeferredLighting", assetBundle);
				resources.DeferredLightingComputeShader = LoadComputeIfNull(resources.DeferredLightingComputeShader, "DeferredLighting.compute", assetBundle);
				resources.IndirectRenderingCullShader = LoadComputeIfNull(resources.IndirectRenderingCullShader, "IndirectCulling.compute", assetBundle);
				resources.SkyboxShader = LoadShaderIfNull(resources.SkyboxShader, "Owlcat/Skybox/Procedural", assetBundle);
				resources.ColorPyramidShader = LoadShaderIfNull(resources.ColorPyramidShader, "Hidden/Owlcat/ColorPyramid", assetBundle);
				resources.ApplyDistortionShader = LoadShaderIfNull(resources.ApplyDistortionShader, "Hidden/Owlcat/ApplyDistortion", assetBundle);
				resources.FogShader = LoadShaderIfNull(resources.FogShader, "Hidden/Owlcat/Fog", assetBundle);
				resources.HbaoShader = LoadShaderIfNull(resources.HbaoShader, "Hidden/Owlcat/HBAO", assetBundle);
				resources.ScreenSpaceCloudShadowsShader = LoadShaderIfNull(resources.ScreenSpaceCloudShadowsShader, "Hidden/Owlcat/ScreenSpaceCloudShadows", assetBundle);
				resources.ScreenSpaceReflectionsShaderPS = LoadShaderIfNull(resources.ScreenSpaceReflectionsShaderPS, "Hidden/Owlcat/ScreenSpaceReflections", assetBundle);
				resources.ScreenSpaceReflectionsShaderCS = LoadComputeIfNull(resources.ScreenSpaceReflectionsShaderCS, "ScreenSpaceReflections.compute", assetBundle);
				resources.DBufferBlitShader = LoadShaderIfNull(resources.DBufferBlitShader, "Hidden/Owlcat/DBufferBlit", assetBundle);
				assetBundle.Unload(unloadAllLoadedObjects: false);
			}
			static ComputeShader LoadComputeIfNull(ComputeShader shader, string name, AssetBundle shadersBundle)
            {
                if (shader != null)
                    return shader;
                else
                    return shadersBundle.LoadAsset<ComputeShader>(name);
			}
			static Shader LoadShaderIfNull(Shader shader, string name, AssetBundle shadersBundle)
			{
				if (shader != null)
                    return shader;
                else 
					return shadersBundle.LoadAsset<Shader>(name);
				 
			}
		}
	}
}
