using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	internal struct CameraHistoryInfo
	{
		public float ViewportWidth;

		public float ViewportHeight;

		public float Depth;

		public Camera UsedCamera;

		public CameraHistoryInfo(Camera usedCamera)
		{
			UsedCamera = usedCamera;
			ViewportWidth = UsedCamera.pixelRect.width;
			ViewportHeight = UsedCamera.pixelRect.height;
			Depth = UsedCamera.depth;
		}
	}
}
