using System;
using Owlcat.Runtime.Core.Utils.EditorAttributes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging
{
	[Serializable]
	public class StencilDebug
	{
		public StencilDebugType StencilDebugType;

		[EnumFlags]
		public StencilRef Flags;

		[Range(0f, 255f)]
		public int Ref;
	}
}
