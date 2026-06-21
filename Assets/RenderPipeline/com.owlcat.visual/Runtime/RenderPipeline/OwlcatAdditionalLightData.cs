using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Light))]
	public class OwlcatAdditionalLightData : MonoBehaviour
	{
		[Tooltip("Controls the usage of pipeline settings.")]
		[SerializeField]
		private bool m_UsePipelineSettings = true;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_InnerRadius;

		[Tooltip("Falloff type (realtime and mixed only)")]
		[SerializeField]
		private LightFalloffType m_FalloffType;

		[Tooltip("Snap spacular flare to inner radius")]
		[SerializeField]
		private bool m_SnapSpecularToInnerRadius;

		public bool UsePipelineSettings
		{
			get
			{
				return m_UsePipelineSettings;
			}
			set
			{
				m_UsePipelineSettings = value;
			}
		}

		public float InnerRadius
		{
			get
			{
				return m_InnerRadius;
			}
			set
			{
				m_InnerRadius = value;
			}
		}

		public LightFalloffType FalloffType
		{
			get
			{
				return m_FalloffType;
			}
			set
			{
				m_FalloffType = value;
			}
		}

		public bool SnapSperularToInnerRadius
		{
			get
			{
				return m_SnapSpecularToInnerRadius;
			}
			set
			{
				m_SnapSpecularToInnerRadius = value;
			}
		}
	}
}
