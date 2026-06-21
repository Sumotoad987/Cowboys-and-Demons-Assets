using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting
{
	[DisallowMultipleComponent]
	public class Highlighter : UpdateableBehaviour
	{
		private const float kDoublePI = (float)Math.PI * 2f;

		private bool m_IsRenderersDirty;

		private List<Renderer> m_Renderers = new List<Renderer>();

		private List<HighlighterBlockerHierarchy> m_Blockers = new List<HighlighterBlockerHierarchy>();

		[CanBeNull]
		private List<Renderer> m_ExtraRenderers;

		private Color m_ConstantColor = Color.white;

		private float m_TransitionTarget;

		private float m_TransitionTime;

		private float m_TransitionValue;

		private Color m_FlashingColorMin = Color.white;

		private Color m_FlashingColorMax = Color.white;

		private float m_FlashingFreq;

		private bool m_Flashing;

		private Color m_CurrentColor = Color.white;

		public bool IsOff => m_TransitionTarget == 0f;

		public Color CurrentColor => m_CurrentColor;

		protected override void OnEnabled()
		{
			m_IsRenderersDirty = true;
		}

		public void GetRenderers(List<Renderer> renderers)
		{
			UpdateRenderers();
			if (!(m_TransitionValue <= 0f) && !(m_CurrentColor.a <= 0f))
			{
				renderers.AddRange(m_Renderers);
			}
		}

		private void UpdateRenderers()
		{
			foreach (Renderer renderer2 in m_Renderers)
			{
				if (renderer2 == null)
				{
					m_IsRenderersDirty = true;
					break;
				}
			}
			if (!m_IsRenderersDirty)
			{
				return;
			}
			m_Renderers.Clear();
			m_Blockers.Clear();
			GetComponentsInChildren(m_Renderers);
			GetComponentsInChildren(m_Blockers);
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				Renderer renderer = m_Renderers[i];
				if (renderer == null)
				{
					m_Renderers.RemoveAt(i);
					i--;
					continue;
				}
				if (renderer.GetComponent<VisualEffect>() != null)
				{
					m_Renderers.RemoveAt(i);
					i--;
					continue;
				}
				if (renderer is ParticleSystemRenderer)
				{
					m_Renderers.RemoveAt(i);
					i--;
					continue;
				}
				Highlighter component = renderer.GetComponent<Highlighter>();
				if (component != null && component != this)
				{
					m_Renderers.RemoveAt(i);
					i--;
					continue;
				}
				if (renderer.GetComponent<HighlighterBlocker>() != null)
				{
					m_Renderers.RemoveAt(i);
					i--;
					continue;
				}
				//bool flag = false;
				foreach (HighlighterBlockerHierarchy blocker in m_Blockers)
				{
					if (renderer.transform.IsChildOf(blocker.transform))
					{
						m_Renderers.RemoveAt(i);
						i--;
						//flag = true;
						break;
					}
				}
			}
			if (m_ExtraRenderers != null)
			{
				m_Renderers.AddRange(m_ExtraRenderers);
			}
			m_IsRenderersDirty = false;
		}

		public void ConstantOn(Color color, float time = 0.25f)
		{
			m_ConstantColor = color;
			m_TransitionTime = ((time >= 0f) ? time : 0f);
			m_TransitionTarget = 1f;
		}

		public void ConstantOff(float time = 0.25f)
		{
			m_TransitionTime = ((time >= 0f) ? time : 0f);
			m_TransitionTarget = 0f;
		}

		public void ConstantOnImmediate(Color color)
		{
			m_ConstantColor = color;
			m_TransitionValue = (m_TransitionTarget = 1f);
		}

		public void ReinitMaterials()
		{
			m_IsRenderersDirty = true;
		}

		public void FlashingOn(Color color1, Color color2, float freq)
		{
			m_FlashingColorMin = color1;
			m_FlashingColorMax = color2;
			m_FlashingFreq = freq;
			m_Flashing = true;
		}

		public void FlashingOff()
		{
			m_Flashing = false;
		}

		public override void DoUpdate()
		{
			UpdateTransition();
		}

		private void UpdateTransition()
		{
			if (m_TransitionValue != m_TransitionTarget)
			{
				if (m_TransitionTime <= 0f)
				{
					m_TransitionValue = m_TransitionTarget;
					return;
				}
				float num = ((m_TransitionTarget > 0f) ? 1f : (-1f));
				m_TransitionValue = Mathf.Clamp01(m_TransitionValue + num * Time.unscaledDeltaTime / m_TransitionTime);
			}
		}

		public void UpdateColors()
		{
			if (m_Flashing)
			{
				m_CurrentColor = Color.Lerp(m_FlashingColorMin, m_FlashingColorMax, 0.5f * Mathf.Sin(Time.realtimeSinceStartup * m_FlashingFreq * ((float)Math.PI * 2f)) + 0.5f);
			}
			else if (m_TransitionValue > 0f)
			{
				m_CurrentColor = m_ConstantColor;
				m_CurrentColor.a *= m_TransitionValue;
			}
		}

		public void AddExtraRenderer(Renderer r)
		{
			m_ExtraRenderers = m_ExtraRenderers ?? new List<Renderer>();
			m_ExtraRenderers.Add(r);
			m_IsRenderersDirty = true;
		}

		public void RemoveExtraRenderer(Renderer r)
		{
			m_ExtraRenderers?.Remove(r);
			m_IsRenderersDirty = true;
		}
	}
}
