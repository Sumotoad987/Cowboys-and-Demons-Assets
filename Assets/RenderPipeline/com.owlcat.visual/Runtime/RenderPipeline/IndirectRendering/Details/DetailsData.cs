using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering.Details
{
	public class DetailsData : ScriptableObject
	{
		private int m_LastFrameId = int.MinValue;

		private bool m_AlreadySorted;

		[HideInInspector]
		public float InstancePerMeter = 1f;

		//[HideInInspector]
		public List<DetailInstanceData> Instances = new List<DetailInstanceData>();

		public const int DetailDataMaxInstances = 5000;

		private int ReduceInstanceCount
		{
			get
			{
				if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5 || Application.platform == RuntimePlatform.XboxOne)
				{
					if (Instances.Count <= 1000)
					{
						return 1;
					}
					return Mathf.Max(4, Mathf.CeilToInt(1f * (float)Instances.Count / 5000f));
				}
				return 1;
			}
		}

		public void SortInstancesByMortonCode()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("DetailsData sorted while edit mode!");
			}
			if (m_LastFrameId == FrameId.FrameCount || (!Application.isEditor && m_AlreadySorted))
			{
				return;
			}
			if (Instances.Count > 0)
			{
				Bounds bounds = new Bounds(Instances[0].Position, default(Vector3));
				for (int i = 0; i < Instances.Count; i++)
				{
					bounds.Encapsulate(Instances[i].Position);
				}
				for (int j = 0; j < Instances.Count; j++)
				{
					Instances[j].MortonCode = CalculateMortonCode(Instances[j].Position, bounds);
				}
				Instances.Sort(SortByMortonCode);
				if (ReduceInstanceCount > 1)
				{
					Instances = Instances.Where((DetailInstanceData instance, int index) => index % ReduceInstanceCount == 0).ToList();
				}
			}
			m_AlreadySorted = true;
			m_LastFrameId = FrameId.FrameCount;
		}

		private int SortByMortonCode(DetailInstanceData x, DetailInstanceData y)
		{
			if (x.MortonCode > y.MortonCode)
			{
				return -1;
			}
			return 1;
		}

		private uint CalculateMortonCode(float3 position, Bounds bounds)
		{
			float3 @float = (position - (float3)bounds.min) / bounds.size;
			return Morton3D(@float.x, @float.y, @float.z);
		}

		private uint ExpandBits(uint v)
		{
			v = (v * 65537) & 0xFF0000FFu;
			v = (v * 257) & 0xF00F00Fu;
			v = (v * 17) & 0xC30C30C3u;
			v = (v * 5) & 0x49249249u;
			return v;
		}

		private uint Morton3D(float x, float y, float z)
		{
			x = math.min(math.max(x * 1024f, 0f), 1023f);
			y = math.min(math.max(y * 1024f, 0f), 1023f);
			z = math.min(math.max(z * 1024f, 0f), 1023f);
			uint num = ExpandBits((uint)x);
			uint num2 = ExpandBits((uint)y);
			uint num3 = ExpandBits((uint)z);
			return num * 4 + num2 * 2 + num3;
		}
	}
}
