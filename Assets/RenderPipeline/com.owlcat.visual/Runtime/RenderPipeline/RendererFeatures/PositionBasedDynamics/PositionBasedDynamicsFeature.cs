using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Debugging;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics
{
	[CreateAssetMenu(menuName = "Renderer Features/Position Based Dynamics")]
	[ReloadGroup]
	public class PositionBasedDynamicsFeature : ScriptableRendererFeature
	{
		[Serializable]
		[ReloadGroup]
		public class ShaderResources
		{
			private const string kBasePath = "Assets/Code/Owlcat/";

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDSingleDispatchSimulator.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDSingleDispatchSimulatorShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDCollision.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDCollision;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDForceVolume.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDForceVolume;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDSkinning.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDSkinningShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDMesh.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDMeshShader;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDBodyAabb.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader PBDBodyAabbShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/PBDParticlesDebug", ReloadAttribute.Package.Builtin)]
			public Shader PBDParticlesDebugShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/PBDConstraintsDebug", ReloadAttribute.Package.Builtin)]
			public Shader PBDConstraintDebugShader;

			[SerializeField]
			[Reload("Hidden/Owlcat/PBDDebug", ReloadAttribute.Package.Builtin)]
			public Shader PBDDebug;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/GPUParallelSort/RadixSort.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader RadixSortCS;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/GPUHashtable/LinearProbingHashtable.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader HashtableCS;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/OptimizedSpatialHashingBroadphase.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader SpatialHashingCS;

			[SerializeField]
			[Reload("Assets/Code/Owlcat/Runtime/Visual/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDCameraCulling.compute", ReloadAttribute.Package.Builtin)]
			public ComputeShader CameraCullingCS;
		}

		[Serializable]
		public class PBDDebugSettings
		{
			public bool Enabled;

			public float ParticleSize = 0.1f;

			public Color ParticleColor = Color.red;

			public Color ConstraintColor = Color.yellow;

			public bool ShowNormals;

			public Color NormalsColor = Color.blue;

			public bool ShowCollidersAabb;

			public bool ShowCollidersGrid;

			public bool ShowForceVolumesAabb;

			public bool ShowForceVolumesGrid;
		}

		[SerializeField]
		private ShaderResources m_Shaders;

		[SerializeField]
		private PBDDebugSettings m_DebugSettings;

		private PositionBasedDynamicsConfig m_Config;

		//private Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes.DebugPass m_DebugPass;

		private SingleDispatchSimulationPass m_SingleDispatchSimulationPass;

		private GPUBroadphaseBase m_Broadphase;

		private GPUDebugSoA m_DebugSoA;

		public PBDDebugSettings DebugSettings => m_DebugSettings;

		internal GPUBroadphaseBase Broadphase => m_Broadphase;

		internal GPUDebugSoA DebugSoA => m_DebugSoA;

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			CameraType cameraType = renderingData.CameraData.Camera.cameraType;
			if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
			{
				DisableFeature();
				return;
			}
			if (!Application.isPlaying)
			{
				DisableFeature();
				return;
			}
			if (m_Config == null)
			{
				m_Config = PositionBasedDynamicsConfig.Instance;
			}
			if (!m_Config.GPU || PBD.IsEmpty)
			{
				DisableFeature();
				return;
			}
			if (PBD.GetGPUData() == null)
			{
				DisableFeature();
				return;
			}
			if (m_Broadphase == null || (m_Broadphase.Type != PBD.BroadphaseSettings.Type && PBD.BroadphaseSettings.Type != BroadphaseType.MultilevelGrid))
			{
				ResetBroadphase();
			}
			ClusteredRenderer clusteredRenderer = renderer as ClusteredRenderer;
			m_SingleDispatchSimulationPass.Setup(m_Broadphase, m_Config.SimulationIterations, m_Config.ConstraintIterations, m_Config.Decay);
			renderer.EnqueuePass(m_SingleDispatchSimulationPass);
			//if (m_DebugSettings.Enabled && !PBD.IsSceneInitialization)
			//{
			//	if (m_DebugSoA == null)
			//	{
			//		m_DebugSoA = new GPUDebugSoA();
			//	}
			//	m_DebugPass.Setup(this, clusteredRenderer.GetCurrentCameraFinalColorTexture(ref renderingData));
			//	renderer.EnqueuePass(m_DebugPass);
			//}
		}

		private void ResetBroadphase()
		{
			if (m_Broadphase != null)
			{
				m_Broadphase.Dispose();
			}
			switch (PBD.BroadphaseSettings.Type)
			{
			case BroadphaseType.SimpleGrid:
				m_Broadphase = new GPUSimpleGridBroadphase(PBD.BroadphaseSettings.SimpleGridSettings, m_Shaders.PBDCollision, m_Shaders.PBDForceVolume);
				break;
			case BroadphaseType.MultilevelGrid:
				Debug.LogWarning("Multilevel grid is not implemented on GPU. Switch to OptimizedSpatialHashing broadphase");
				m_Broadphase = new GPUOptimizedSpatialHashingBroadphase(PBD.BroadphaseSettings.OptimizedSpatialHashingSettings, m_Shaders.SpatialHashingCS);
				break;
			case BroadphaseType.OptimizedSpatialHashing:
				m_Broadphase = new GPUOptimizedSpatialHashingBroadphase(PBD.BroadphaseSettings.OptimizedSpatialHashingSettings, m_Shaders.SpatialHashingCS);
				break;
			}
		}

		public override void Create()
		{
			m_SingleDispatchSimulationPass = new SingleDispatchSimulationPass((RenderPassEvent)1, m_Shaders.PBDSingleDispatchSimulatorShader, m_Shaders.PBDCollision, m_Shaders.PBDForceVolume, m_Shaders.PBDSkinningShader, m_Shaders.PBDMeshShader, m_Shaders.PBDBodyAabbShader, m_Shaders.CameraCullingCS);

			if (m_Shaders.PBDDebug == null)
				return;
			Material particlesMaterial = CoreUtils.CreateEngineMaterial(m_Shaders.PBDParticlesDebugShader);
			Material constraintsMaterial = CoreUtils.CreateEngineMaterial(m_Shaders.PBDConstraintDebugShader);
			Material debugMaterial = CoreUtils.CreateEngineMaterial(m_Shaders.PBDDebug);
			//m_DebugPass = new RendererFeatures.PositionBasedDynamics.Passes.DebugPass(RenderPassEvent.AfterRendering, particlesMaterial, constraintsMaterial, debugMaterial);
		}

		public override void DisableFeature()
		{
			Shader.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 0f);
		}

		public override string GetFeatureIdentifier()
		{
			return "Position Based Dynamics Feature";
		}

		public override void Dispose()
		{
			if (m_DebugSoA != null)
			{
				m_DebugSoA.Dispose();
			}
			if (m_Broadphase != null)
			{
				m_Broadphase.Dispose();
			}
		}
	}
}
