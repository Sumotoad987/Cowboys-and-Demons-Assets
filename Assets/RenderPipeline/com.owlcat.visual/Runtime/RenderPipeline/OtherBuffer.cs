using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline
{
	public static class OtherBuffer
	{
		public static int _ArgsBuffer = Shader.PropertyToID("_ArgsBuffer");

		public static int _IsVisibleBuffer = Shader.PropertyToID("_IsVisibleBuffer");
	}
}
