using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utils.EditorAttributes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering
{
	[ExecuteInEditMode]
	public class IndirectMeshComponent : MonoBehaviour, IIndirectMesh
	{
		[SerializeField]
		private Mesh m_Mesh;

		[SerializeField]
		private List<Material> m_Materials = new List<Material>();

		[SerializeField]
		[MinMaxSlider(0.01f, 2f)]
		private Vector2 m_ScaleRange = Vector2.one;

		[SerializeField]
		[MinMaxSlider(-180f, 180f)]
		private Vector2 m_RotationRange = new Vector2(-180f, 180f);

		private readonly Dictionary<Type, MonoBehaviour> m_CachedComponents = new Dictionary<Type, MonoBehaviour>();

		public Mesh Mesh
		{
			get
			{
				return m_Mesh;
			}
			set
			{
				m_Mesh = value;
			}
		}

		public Vector2 ScaleRange
		{
			get
			{
				return m_ScaleRange;
			}
			set
			{
				m_ScaleRange = value;
			}
		}

		public Vector2 RotationRange
		{
			get
			{
				return m_RotationRange;
			}
			set
			{
				m_RotationRange = value;
			}
		}

		public Vector3 Position => base.transform.position;

		public List<Material> Materials
		{
			get
			{
				return m_Materials;
			}
			set
			{
				m_Materials = value;
			}
		}

		public bool IsDynamic => false;

		public int MaxDynamicInstances => 0;

		private void OnEnable()
		{
			IndirectRenderingSystem.Instance.RegisterMesh(this);
		}

		private void OnDisable()
		{
			IndirectRenderingSystem.Instance.UnregisterMesh(this);
		}

		public void UpdateInstances()
		{
			throw new NotImplementedException();
		}

		public T GetUnityComponent<T>() where T : MonoBehaviour
		{
			if (m_CachedComponents.TryGetValue(typeof(T), out var value))
			{
				return value as T;
			}
			T component = GetComponent<T>();
			m_CachedComponents.Add(typeof(T), component);
			return component;
		}
	}
}
