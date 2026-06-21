using System;
using System.Collections.Generic;
using Core.Cheats;
using Owlcat.Runtime.Core.Utils.EditorAttributes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering.Details
{
	[ExecuteInEditMode]
	public class DetailsMesh : MonoBehaviour, IIndirectMesh
	{
		[SerializeField]
		private Mesh m_Mesh;

		[SerializeField]
		private List<Material> m_Materials = new List<Material>();

		[SerializeField]
		private DetailsData m_Data;

		[SerializeField]
		[MinMaxSlider(0.01f, 2f)]
		private Vector2 m_ScaleRange = Vector2.one;

		[SerializeField]
		[MinMaxSlider(-180f, 180f)]
		private Vector2 m_RotationRange = new Vector2(-180f, 180f);

		[SerializeField]
		[HideInInspector]
		private string m_Guid;

		private static readonly List<IndirectInstanceData> RuntimeInstanceData = new List<IndirectInstanceData>();

		//private bool m_IsDirty;

		private readonly Dictionary<Type, MonoBehaviour> m_CachedComponents = new Dictionary<Type, MonoBehaviour>();

		[Cheat]
		private static bool DisableFoliage { get; set; }

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

		public DetailsData Data
		{
			get
			{
				return m_Data;
			}
			set
			{
				m_Data = value;
			}
		}

		public bool IsDynamic => false;

		public int MaxDynamicInstances => 0;

		public string Guid => m_Guid;

		public Vector3 Position => base.transform.position;

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

		private void OnEnable()
		{
			if (string.IsNullOrEmpty(m_Guid))
			{
				m_Guid = System.Guid.NewGuid().ToString();
			}
			if (DisableFoliage)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				UnityEngine.Object.Destroy(Data);
				return;
			}
			for (int num = m_Materials.Count - 1; num >= 0; num--)
			{
				if (!m_Materials[num])
				{
					m_Materials.RemoveAt(num);
				}
			}
			IndirectRenderingSystem.Instance.RegisterMesh(this);
			//m_IsDirty = true;
			UpdateInstances();
		}

		private void OnDisable()
		{
			IndirectRenderingSystem.Instance.UnregisterMesh(this);
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

		public void UpdateInstances()
		{
			if (!(Data != null))
			{
				return;
			}
			RuntimeInstanceData.Capacity = Math.Max(Data.Instances.Count, RuntimeInstanceData.Capacity);
			if (Application.IsPlaying(this))
			{
				Data.SortInstancesByMortonCode();
			}
			foreach (DetailInstanceData instance in Data.Instances)
			{
				Matrix4x4 objectToWorld = Matrix4x4.TRS(instance.Position, Quaternion.Euler(0f, instance.Rotation, 0f), Vector3.one * instance.Scale);
				RuntimeInstanceData.Add(new IndirectInstanceData
				{
					objectToWorld = objectToWorld,
					worldToObject = objectToWorld.inverse,
					meshID = 0u,
					tintColor = instance.TintColor,
					shadowmask = instance.Shadowmask
				});
			}
			IndirectRenderingSystem.Instance.SetMeshInstances(this, RuntimeInstanceData);
			RuntimeInstanceData.Clear();
		}

		private void OnDrawGizmosSelected()
		{
		}
	}
}
