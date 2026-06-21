using System;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	[Serializable]
	[VolumeComponentMenu("Post-processing/Color Lookup")]
	public sealed class ColorLookup : VolumeComponent, IPostProcessComponent
	{
		[Tooltip("A custom 2D texture lookup table to apply.")]
		public TextureParameter texture = new TextureParameter(null);

		[Tooltip("How much of the lookup texture will contribute to the color grading effect.")]
		public ClampedFloatParameter contribution = new ClampedFloatParameter(1f, 0f, 1f);

		public bool IsActive()
		{
			if (contribution.value > 0f)
			{
				return ValidateLUT();
			}
			return false;
		}

		public bool IsTileCompatible()
		{
			return true;
		}

		public bool ValidateLUT()
		{
			OwlcatRenderPipelineAsset asset = OwlcatRenderPipeline.Asset;
			if (asset == null || texture.value == null)
			{
				return false;
			}
			int colorGradingLutSize = asset.PostProcessSettings.ColorGradingLutSize;
			if (texture.value.height != colorGradingLutSize)
			{
				return false;
			}
			bool flag = false;
			Texture value = texture.value;
			if (!(value is Texture2D texture2D))
			{
				if (value is RenderTexture renderTexture)
				{
					flag |= renderTexture.dimension == TextureDimension.Tex2D && renderTexture.width == colorGradingLutSize * colorGradingLutSize && !renderTexture.sRGB;
				}
			}
			else
			{
				flag |= texture2D.width == colorGradingLutSize * colorGradingLutSize && !GraphicsFormatUtility.IsSRGBFormat(texture2D.graphicsFormat);
			}
			return flag;
		}
	}
}
