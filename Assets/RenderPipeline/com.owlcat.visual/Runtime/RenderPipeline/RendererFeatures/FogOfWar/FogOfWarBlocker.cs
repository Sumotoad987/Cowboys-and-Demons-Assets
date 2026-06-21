using System.Collections.Generic;
using Owlcat.Runtime.Core.Math;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar
{
	public class FogOfWarBlocker : MonoBehaviour
	{
		private Matrix4x4 m_CachedLocalToWorld;

		private float m_CachedShadowOffset;

		private float m_CachedGlobalShadowOffset;

		private FogOfWarArea m_CachedFogOfWarArea;

		private NativeArray<Vector3> m_SegmentA_Index;

		private NativeArray<Vector3> m_SegmentB_Offset;

		private NativeArray<int> m_Indices;

		private Vector2[] m_Points;

		private List<Vector3> m_ConvexHull = new List<Vector3>();

		private bool m_Closed;

		public float ShadowFalloff;

		public Vector2 HeightMinMax;

		public Vector2 Center;

		public Vector2 Radius;

		public bool TwoSided;

		public static readonly HashSet<FogOfWarBlocker> All = new HashSet<FogOfWarBlocker>();

		private Transform m_Transform;

		public Vector2[] Points => m_Points;

		public NativeArray<Vector3> SegmentA_Index => m_SegmentA_Index;

		public NativeArray<Vector3> SegmentB_Offset => m_SegmentB_Offset;

		public NativeArray<int> Indices => m_Indices;

		public bool Closed => m_Closed;

		private void OnEnable()
		{
			All.Add(this);
			m_CachedLocalToWorld = base.transform.localToWorldMatrix;
			FogOfWarArea active = FogOfWarArea.Active;
			if (active != null)
			{
				m_CachedGlobalShadowOffset = active.ShadowFalloff;
			}
			m_CachedFogOfWarArea = active;
			Rebuild();
			LineOfSightGeometry.Instance.UpdateBlocker(this);
			m_Transform = base.transform;
		}

		private void OnDisable()
		{
			if (m_SegmentA_Index.IsCreated)
			{
				m_SegmentA_Index.Dispose();
			}
			if (m_SegmentB_Offset.IsCreated)
			{
				m_SegmentB_Offset.Dispose();
			}
			if (m_Indices.IsCreated)
			{
				m_Indices.Dispose();
			}
			All.Remove(this);
			LineOfSightGeometry.Instance.RemoveBlocker(this);
		}

		public void UpdateIfNecessary()
		{
			if (!base.enabled)
			{
				return;
			}
			if (!m_Transform)
			{
				OnEnable();
				return;
			}
			bool hasChanged = m_Transform.hasChanged;
			if (hasChanged || m_CachedFogOfWarArea != FogOfWarArea.Active)
			{
				Matrix4x4 localToWorldMatrix = m_Transform.localToWorldMatrix;
				hasChanged = hasChanged && localToWorldMatrix != m_CachedLocalToWorld;
				FogOfWarArea active = FogOfWarArea.Active;
				if (hasChanged || m_CachedShadowOffset != ShadowFalloff || (active != null && m_CachedGlobalShadowOffset != active.ShadowFalloff))
				{
					Rebuild();
				}
				m_Transform.hasChanged = false;
				if (hasChanged)
				{
					LineOfSightGeometry.Instance.UpdateBlocker(this);
				}
				m_CachedLocalToWorld = localToWorldMatrix;
			}
			m_CachedShadowOffset = ShadowFalloff;
			m_CachedFogOfWarArea = FogOfWarArea.Active;
			if (FogOfWarArea.Active != null)
			{
				m_CachedGlobalShadowOffset = FogOfWarArea.Active.ShadowFalloff;
			}
		}

		private void Rebuild()
		{
			BuildPoints();
			BuildMesh();
		}

		public void BuildPoints()
		{
			MeshFilter component = GetComponent<MeshFilter>();
			FogOfWarBlockerPolygon component2 = GetComponent<FogOfWarBlockerPolygon>();
			if ((bool)component2)
			{
				BuildPointFromPolygon(component2);
			}
			else if ((bool)component)
			{
				BuildPointFromMesh(component);
			}
		}

		private void BuildPointFromPolygon(FogOfWarBlockerPolygon poly)
		{
			Vector3[] transformedPoints = poly.TransformedPoints;
			Vector3 lhs = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 lhs2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			foreach (Vector3 rhs in transformedPoints)
			{
				lhs = Vector3.Min(lhs, rhs);
				lhs2 = Vector3.Max(lhs2, rhs);
			}
			float num = lhs2.x - lhs.x;
			float num2 = lhs2.z - lhs.z;
			Center = new Vector2(num / 2f + lhs.x, num2 / 2f + lhs.z);
			float num3 = Vector2.Distance(new Vector2(lhs.x, lhs.z), new Vector2(lhs2.x, lhs2.z)) / 2f;
			Radius = new Vector2(num3, num3 * num3);
			HeightMinMax = new Vector2(lhs.y - poly.Height, lhs2.y + poly.Height);
			int num4 = (TwoSided ? (transformedPoints.Length * 2 - 1) : transformedPoints.Length);
			m_Points = new Vector2[num4];
			for (int j = 0; j < transformedPoints.Length; j++)
			{
				Vector2 vector = new Vector2(transformedPoints[j].x, transformedPoints[j].z);
				m_Points[j] = vector;
				if (TwoSided && j > 0)
				{
					m_Points[m_Points.Length - j] = vector;
				}
			}
			m_Closed = poly.Closed || TwoSided;
		}

		private void BuildPointFromMesh(MeshFilter meshFilter)
		{
			Vector3[] vertices = meshFilter.sharedMesh.vertices;
			Vector3[] array = new Vector3[vertices.Length];
			Vector3 lhs = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 lhs2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vector = base.transform.TransformPoint(vertices[i]);
				lhs = Vector3.Min(lhs, vector);
				lhs2 = Vector3.Max(lhs2, vector);
				array[i] = vector;
			}
			float num = lhs2.x - lhs.x;
			float num2 = lhs2.z - lhs.z;
			Center = new Vector2(num / 2f + lhs.x, num2 / 2f + lhs.z);
			float num3 = Vector2.Distance(new Vector2(lhs.x, lhs.z), new Vector2(lhs2.x, lhs2.z)) / 2f;
			Radius = new Vector2(num3, num3 * num3);
			HeightMinMax = new Vector2(lhs.y, lhs2.y);
			Polygon.ConvexHullXZ(array, m_ConvexHull);
			m_Points = new Vector2[m_ConvexHull.Count];
			for (int j = 0; j < m_ConvexHull.Count; j++)
			{
				m_Points[j] = new Vector2(m_ConvexHull[j].x, m_ConvexHull[j].z);
			}
			m_Closed = true;
		}

		private void BuildMesh()
		{
			if (!m_SegmentA_Index.IsCreated || m_SegmentA_Index.Length != m_Points.Length * 4)
			{
				if (m_SegmentA_Index.IsCreated)
				{
					m_SegmentA_Index.Dispose();
				}
				m_SegmentA_Index = new NativeArray<Vector3>(m_Points.Length * 4, Allocator.Persistent);
				if (m_SegmentB_Offset.IsCreated)
				{
					m_SegmentB_Offset.Dispose();
				}
				m_SegmentB_Offset = new NativeArray<Vector3>(m_Points.Length * 4, Allocator.Persistent);
				if (m_Indices.IsCreated)
				{
					m_Indices.Dispose();
				}
				m_Indices = new NativeArray<int>(m_Points.Length * 6, Allocator.Persistent);
			}
			FogOfWarArea active = FogOfWarArea.Active;
			float z = ((ShadowFalloff > 0f) ? ShadowFalloff : ((active == null) ? 0f : active.ShadowFalloff));
			if (Points.Length >= 2)
			{
				int num = 0;
				Vector2 vector = Points[m_Closed ? (Points.Length - 1) : 0];
				for (int i = ((!m_Closed) ? 1 : 0); i < Points.Length; i++)
				{
					int num2 = i;
					Vector2 vector2 = Points[num2];
					m_SegmentA_Index[num * 4] = new Vector3(vector.x, vector.y, 0f);
					m_SegmentA_Index[num * 4 + 1] = new Vector3(vector.x, vector.y, 1f);
					m_SegmentA_Index[num * 4 + 2] = new Vector3(vector.x, vector.y, 2f);
					m_SegmentA_Index[num * 4 + 3] = new Vector3(vector.x, vector.y, 3f);
					m_SegmentB_Offset[num * 4] = new Vector3(vector2.x, vector2.y, z);
					m_SegmentB_Offset[num * 4 + 1] = new Vector3(vector2.x, vector2.y, z);
					m_SegmentB_Offset[num * 4 + 2] = new Vector3(vector2.x, vector2.y, z);
					m_SegmentB_Offset[num * 4 + 3] = new Vector3(vector2.x, vector2.y, z);
					m_Indices[num * 6] = num * 4;
					m_Indices[num * 6 + 1] = num * 4 + 1;
					m_Indices[num * 6 + 2] = num * 4 + 2;
					m_Indices[num * 6 + 3] = num * 4 + 1;
					m_Indices[num * 6 + 4] = num * 4 + 3;
					m_Indices[num * 6 + 5] = num * 4 + 2;
					num++;
					vector = vector2;
				}
			}
		}

		private Vector3 GetTransformedPoint3D(Vector2 localPointXZ)
		{
			return new Vector3(localPointXZ.x, base.transform.position.y, localPointXZ.y);
		}
	}
}
