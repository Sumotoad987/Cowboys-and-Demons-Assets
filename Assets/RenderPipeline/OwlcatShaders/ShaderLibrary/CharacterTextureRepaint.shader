Shader "Hidden/CharacterTextureRepaint"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Mask;
			sampler2D _Ramp;
			float _Specialmask;

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				
				col.rgb = LinearToGammaSpace(col.rgb) / 255;

				float gradient = dot(col.rgb, float3(0.299, 0.587, 0.114));

				float3 rampColor = tex2D(_Ramp, float2(gradient, .5f));

				float mask = 0;
				if(_Specialmask > 0)
				{
				    mask = tex2D(_Mask, i.uv).r;
				}
				
				else
				{
				    mask = tex2D(_Mask, i.uv).g;
				}
				return float4(rampColor.rgb, mask);
			}
			ENDCG
		}
	}
}
