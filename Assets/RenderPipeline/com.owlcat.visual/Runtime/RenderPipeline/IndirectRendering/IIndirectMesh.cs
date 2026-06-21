using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.IndirectRendering
{
	public interface IIndirectMesh
	{
		bool IsDynamic { get; }

		int MaxDynamicInstances { get; }

		Vector3 Position { get; }

		Mesh Mesh { get; set; }

		List<Material> Materials { get; set; }

		Vector2 ScaleRange { get; set; }

		Vector2 RotationRange { get; set; }

		void UpdateInstances();

		[CanBeNull]
		T GetUnityComponent<T>() where T : MonoBehaviour;
	}
}
