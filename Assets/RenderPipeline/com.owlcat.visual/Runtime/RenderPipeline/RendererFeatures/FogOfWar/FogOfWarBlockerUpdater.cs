using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar
{
	[RequireComponent(typeof(FogOfWarBlocker))]
	public class FogOfWarBlockerUpdater : MonoBehaviour
	{
		private class FogOfWarBlockerUpdateManager : MonoBehaviour
		{
			private static FogOfWarBlockerUpdateManager s_Instance;

			public static void Ensure()
			{
				if (s_Instance == null)
				{
					GameObject obj = new GameObject("FogOfWarBlockerUpdateManager");
					Object.DontDestroyOnLoad(obj);
					s_Instance = obj.AddComponent<FogOfWarBlockerUpdateManager>();
				}
			}

			private void Update()
			{
				foreach (FogOfWarBlocker updatableBlocker in UpdatableBlockers)
				{
					updatableBlocker.UpdateIfNecessary();
				}
			}
		}

		private static readonly List<FogOfWarBlocker> UpdatableBlockers = new List<FogOfWarBlocker>();

		private FogOfWarBlocker m_Blocker;

		private void Awake()
		{
			m_Blocker = (TryGetComponent<FogOfWarBlocker>(out var component) ? component : null);
			if (m_Blocker == null)
			{
				Debug.Log("FogOfWarBlockerUpdater.Awake: FogOfWarBlocker component is missing", this);
			}
		}

		private void OnEnable()
		{
			if (m_Blocker != null)
			{
				FogOfWarBlockerUpdateManager.Ensure();
				UpdatableBlockers.Add(m_Blocker);
			}
		}

		private void OnDisable()
		{
			UpdatableBlockers.Remove(m_Blocker);
		}
	}
}
