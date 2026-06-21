using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	public class CameraChain
	{
		private Stack<List<Camera>> m_CameraQueuePool = new Stack<List<Camera>>();

		private Dictionary<CameraGroupDescriptor, List<Camera>> m_Chain = new Dictionary<CameraGroupDescriptor, List<Camera>>();

		private List<CameraGroupDescriptor> m_SortedGroups = new List<CameraGroupDescriptor>();

		private CameraChainDescriptor m_CameraChainDesc = new CameraChainDescriptor();

		private List<CameraHistoryInfo> m_HistoryCameraList = new List<CameraHistoryInfo>();

		public IEnumerable<CameraChainDescriptor> EnumerateCameras()
		{
			foreach (CameraGroupDescriptor sortedGroup in m_SortedGroups)
			{
				List<Camera> cameraQueue = m_Chain[sortedGroup];
				for (int i = 0; i < cameraQueue.Count; i++)
				{
					m_CameraChainDesc.IsFirst = i == 0;
					m_CameraChainDesc.IsLast = i == cameraQueue.Count - 1;
					m_CameraChainDesc.Camera = cameraQueue[i];
					yield return m_CameraChainDesc;
				}
			}
		}

		public void Update(Camera[] cameras)
		{
			bool flag = false;
			if (cameras.Length != m_HistoryCameraList.Count)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < cameras.Length; i++)
				{
					Camera camera = cameras[i];
					CameraHistoryInfo cameraHistoryInfo = m_HistoryCameraList[i];
					if (cameraHistoryInfo.UsedCamera != camera)
					{
						flag = true;
						break;
					}
					if (camera.depth != cameraHistoryInfo.Depth)
					{
						flag = true;
						break;
					}
					_ = camera.pixelRect.width;
					_ = camera.pixelRect.height;
					if (camera.pixelRect.width != cameraHistoryInfo.ViewportWidth || camera.pixelRect.height != cameraHistoryInfo.ViewportHeight)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			Reset();
			foreach (Camera camera2 in cameras)
			{
				OwlcatAdditionalCameraData component = camera2.GetComponent<OwlcatAdditionalCameraData>();
				CameraGroupDescriptor key = new CameraGroupDescriptor(camera2.targetTexture, component?.DepthTexture, camera2.pixelRect);
				if (!m_Chain.TryGetValue(key, out var value))
				{
					value = ClaimQueue();
					m_Chain.Add(key, value);
				}
				value.Add(camera2);
			}
			foreach (KeyValuePair<CameraGroupDescriptor, List<Camera>> item in m_Chain)
			{
				item.Value.Sort((Camera lhs, Camera rhs) => (int)(lhs.depth - rhs.depth));
			}
			m_SortedGroups.AddRange(m_Chain.Keys);
			m_SortedGroups.Sort((CameraGroupDescriptor lhs, CameraGroupDescriptor rhs) => (int)(rhs.Viewport.width * rhs.Viewport.height - lhs.Viewport.width * lhs.Viewport.height));
			foreach (Camera usedCamera in cameras)
			{
				m_HistoryCameraList.Add(new CameraHistoryInfo(usedCamera));
			}
		}

		private List<Camera> ClaimQueue()
		{
			if (m_CameraQueuePool.Count > 0)
			{
				return m_CameraQueuePool.Pop();
			}
			return new List<Camera>();
		}

		private void Reset()
		{
			foreach (KeyValuePair<CameraGroupDescriptor, List<Camera>> item in m_Chain)
			{
				item.Value.Clear();
				m_CameraQueuePool.Push(item.Value);
			}
			m_Chain.Clear();
			m_SortedGroups.Clear();
			m_HistoryCameraList.Clear();
		}
	}
}
