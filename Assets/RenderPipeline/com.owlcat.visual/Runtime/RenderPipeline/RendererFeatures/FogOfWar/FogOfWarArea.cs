using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar
{
	public class FogOfWarArea : MonoBehaviour
	{
		private static FogOfWarArea s_Active;

		private static HashSet<FogOfWarArea> s_All = new HashSet<FogOfWarArea>();

		private NativeArray<byte> m_Data;

		[CanBeNull]
		private IEnumerator m_DataRequest;

		private RenderTexture m_FogOfWarMapRT;

		[SerializeField]
		private Bounds m_Bounds = new Bounds(default(Vector3), new Vector3(30f, 30f, 30f));

		[SerializeField]
		private float m_ShadowFalloff = 0.15f;

		[SerializeField]
		private bool m_IsBlurEnabled = true;

		[SerializeField]
		private BorderSettings m_BorderSettings = new BorderSettings();

		[SerializeField]
		private Texture2D m_StaticMask;

		[SerializeField]
		private bool m_RevealOnStart;

		[SerializeField]
		private bool m_SetActiveOnEnable;

		[SerializeField]
		private bool m_ApplyShaderManually;

		[SerializeField]
		private bool m_IsCheatOffFog;

		public static FogOfWarArea Active
		{
			get
			{
				return s_Active;
			}
			set
			{
				if (!value && (object)value != null)
				{
					Debug.LogError("FogOfWarArea.set_Active: new value is destroyed object");
					value = null;
				}
				s_Active = value;
				if ((bool)s_Active)
				{
					LineOfSightGeometry.Instance.Init(s_Active.GetWorldBounds());
				}
			}
		}

		public static HashSet<FogOfWarArea> All => s_All;

		public Bounds Bounds
		{
			get
			{
				return m_Bounds;
			}
			set
			{
				m_Bounds = value;
			}
		}

		public float ShadowFalloff
		{
			get
			{
				return m_ShadowFalloff;
			}
			set
			{
				m_ShadowFalloff = value;
			}
		}

		public BorderSettings BorderSettings
		{
			get
			{
				return m_BorderSettings;
			}
			set
			{
				m_BorderSettings = value;
			}
		}

		public bool RevealOnStart
		{
			get
			{
				return m_RevealOnStart;
			}
			set
			{
				m_RevealOnStart = value;
			}
		}

		public bool ApplyShaderManually
		{
			get
			{
				return m_ApplyShaderManually;
			}
			set
			{
				m_ApplyShaderManually = value;
			}
		}

		public RenderTexture FogOfWarMapRT
		{
			get
			{
				if (m_FogOfWarMapRT == null)
				{
					FogOfWarFeature instance = FogOfWarFeature.Instance;
					if (instance != null)
					{
						Vector3 size = m_Bounds.size;
						int width = Mathf.Min((int)(instance.TextureDensity * size.x), 2048);
						int height = Mathf.Min((int)(instance.TextureDensity * size.z), 2048);
						m_FogOfWarMapRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
						m_FogOfWarMapRT.name = "FogOfWarMapRT_" + base.name;
						m_FogOfWarMapRT.wrapMode = TextureWrapMode.Clamp;
						m_FogOfWarMapRT.filterMode = FilterMode.Bilinear;
						RenderTexture active = RenderTexture.active;
						RenderTexture.active = m_FogOfWarMapRT;
						GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
						RenderTexture.active = active;
					}
				}
				return m_FogOfWarMapRT;
			}
		}

		public bool IsBlurEnabled
		{
			get
			{
				return m_IsBlurEnabled;
			}
			set
			{
				m_IsBlurEnabled = value;
			}
		}

		public Texture2D StaticMask
		{
			get
			{
				return m_StaticMask;
			}
			set
			{
				m_StaticMask = value;
			}
		}

		public bool IsCheatOffFog
		{
			get
			{
				return m_IsCheatOffFog;
			}
			set
			{
				m_IsCheatOffFog = value;
			}
		}

		public NativeArray<byte> GetData()
		{
			if (m_DataRequest != null)
			{
				Debug.LogError("FogOfWarArea.Data: loading data from GPU in progress");
			}
			return m_Data;
		}

		public Matrix4x4 CalculateProjMatrix(bool convertToGpu = true)
		{
			Matrix4x4 matrix4x = Matrix4x4.Ortho(0f - m_Bounds.extents.x, m_Bounds.extents.x, 0f - m_Bounds.extents.z, m_Bounds.extents.z, 0.1f, m_Bounds.size.y);
			if (convertToGpu)
			{
				matrix4x = GL.GetGPUProjectionMatrix(matrix4x, renderIntoTexture: true);
			}
			return matrix4x;
		}

		public Matrix4x4 CalculateViewMatrix()
		{
			Bounds worldBounds = GetWorldBounds();
			Matrix4x4 cameraWorld = Matrix4x4.TRS(worldBounds.center + Vector3.up * worldBounds.extents.y, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
			return WorldToCameraMatrix(in cameraWorld);
		}

		public static Matrix4x4 WorldToCameraMatrix(in Matrix4x4 cameraWorld)
		{
			Matrix4x4 inverse = cameraWorld.inverse;
			inverse.m20 *= -1f;
			inverse.m21 *= -1f;
			inverse.m22 *= -1f;
			inverse.m23 *= -1f;
			return inverse;
		}

		private void OnDestroy()
		{
			if (m_FogOfWarMapRT != null)
			{
				m_FogOfWarMapRT.Release();
				Object.DestroyImmediate(m_FogOfWarMapRT);
				m_FogOfWarMapRT = null;
			}
		}

		private void OnEnable()
		{
			if (m_SetActiveOnEnable)
			{
				Active = this;
			}
			s_All.Add(this);
        }


        private void OnDisable()
		{
			s_All.Remove(this);
		}

		public IEnumerator RequestDataCoroutine()
		{
			return m_DataRequest ?? (m_DataRequest = RequestDataCoroutineInternal());
		}

		private IEnumerator RequestDataCoroutineInternal()
		{
			GraphicsFormat format = GraphicsFormat.R8G8B8_SRGB;
			uint width = (uint)FogOfWarMapRT.width;
			uint height = (uint)FogOfWarMapRT.height;
			AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(FogOfWarMapRT, 0, format);
			while (!request.done)
			{
				yield return null;
			}
			m_DataRequest = null;
			if (request.hasError)
			{
				LogChannel.Default.Error("FogOfWarArea.RequestDataCoroutine: error occured while loading f-o-w data");
				yield break;
			}
			NativeArray<byte> data = request.GetData<byte>();
			m_Data = ImageConversion.EncodeNativeArrayToJPG(data, format, width, height, 0u, 50);
			data.Dispose();
		}

		public Bounds GetWorldBounds()
		{
			Bounds bounds = m_Bounds;
			bounds.center += base.transform.position;
			return bounds;
		}

		private void OnDrawGizmosSelected()
		{
			Color color = Gizmos.color;
			Gizmos.color = Color.magenta;
			Bounds worldBounds = GetWorldBounds();
			Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
			Gizmos.color = color;
		}

		internal Vector4 CalculateMaskST()
		{
			Bounds worldBounds = GetWorldBounds();
			return new Vector4(1f / worldBounds.size.x, 1f / worldBounds.size.z, (worldBounds.extents.x - worldBounds.center.x) / m_Bounds.size.x, (worldBounds.extents.z - worldBounds.center.z) / m_Bounds.size.z);
		}

		public void RestoreFogOfWarMask(Texture2D mask)
		{
			RenderTexture dest = (RenderTexture.active = FogOfWarMapRT);
			Graphics.Blit(mask, dest);
		}

		public void RestoreFogOfWarMask(byte[] colorsData)
		{
			RenderTexture fogOfWarMapRT = FogOfWarMapRT;
			Texture2D texture2D = new Texture2D(fogOfWarMapRT.width, fogOfWarMapRT.height, TextureFormat.RGB24, mipChain: false);
			texture2D.LoadImage(colorsData, markNonReadable: true);
			RestoreFogOfWarMask(texture2D);
			Object.DestroyImmediate(texture2D);
		}
	}
}
