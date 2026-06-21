Shader "Hidden/CharacterDiffuseBake"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Lighting Off

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			
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
			
            float4 _SrcRect;
			float4 _DstRect;

			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.xy = v.vertex.xy * _DstRect.zw + _DstRect.xy;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			float4 frag (v2f i) : SV_Target
			{
			    float4 col;
                col = tex2D(_MainTex, i.uv * _SrcRect.zw + _SrcRect.xy);

				return float4(col);
			}
			ENDCG
		}
	}
}