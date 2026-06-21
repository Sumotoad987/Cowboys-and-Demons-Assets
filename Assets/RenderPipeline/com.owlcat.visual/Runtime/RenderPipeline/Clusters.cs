using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	[Serializable]
	public struct Clusters
	{
		[Range(8f, 64f)]
		public int TileSize;

		[Range(1f, 64f)]
		public int Slices;

		public static Clusters Default
		{
			get
			{
				Clusters result = default(Clusters);
				result.TileSize = 32;
				result.Slices = 4;
				return result;
			}
		}

		public int GetWidth(int screenWidth)
		{
			float num = (float)screenWidth / (float)TileSize;
			return (int)num + ((num % 1f > 0f) ? 1 : 0);
		}

		public int GetHeight(int screenHeight)
		{
			float num = (float)screenHeight / (float)TileSize;
			return (int)num + ((num % 1f > 0f) ? 1 : 0);
		}
	}
}
