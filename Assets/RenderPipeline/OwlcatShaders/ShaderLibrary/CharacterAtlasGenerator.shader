// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/CharacterAtlasGenerator"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			// No culling, blend or depth
			Cull Off ZWrite Off ZTest Always
			Fog{ Mode Off }
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ ALPHA_MASK_ON // означает, что нужно взять альфу из главной текстуры, одновременно значит, что запекается НЕ главная текстура, т.е. _MaskTex
			#pragma multi_compile __ NORMAL_MAP_ON // запекается текстура нормалей
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
			int _IsEmpty;
			float _Roughness;
			float _Emission;
			float _Metallic;
			sampler2D _MainTex;
			sampler2D _PreviousTex;
			sampler2D _AlphaMask;
			
			sampler2D _Mask;
			int _Shadow;

			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.xy = v.vertex.xy * _DstRect.zw + _DstRect.xy;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv * _SrcRect.zw + _SrcRect.xy);
				// _PreviousTex это ВЕСЬ атлас, нужно взять с него только текущий кусок.
				float4 prevCol = tex2D(_PreviousTex, i.uv * _DstRect.zw + _DstRect.xy);
				
				// Маска нужна в любом случае, но для диффуза мы НЕ включаем ALPHA_MASK_ON.
				float mask = tex2D(_AlphaMask, i.uv).a;

				#if defined(NORMAL_MAP_ON)
					if (_IsEmpty > 0)
					{
						col.xyz = float3(0, 0, 1);
					}
					else
					{
						col.xyz = UnpackNormal(col);
					}
					#if defined(ALPHA_MASK_ON)
						col.xyz = lerp(float3(0, 0, 1.), col.xyz, mask);
						col.a = mask;
					#endif
					col.xyz = col.xyz * .5 + .5;
				#else
					#if defined(ALPHA_MASK_ON)
						if (_IsEmpty > 0)
						{
							col.xyzw = float4(1., 0, 0, 0);
						}
                        // Бленд текстуры масок.
						col.r = ((col.r * _Roughness) * mask)       + ((1. - mask) * prevCol.r);
						col.g = (((col.g * _Emission) / 10) * mask) + ((1. - mask) * prevCol.g);
						col.b = ((col.b * _Metallic) * mask)        + ((1. - mask) * prevCol.b);
						col.a = (col.a * mask)                      + ((1. - mask) * prevCol.a);
						return col;
                    #endif
				#endif
                
                // Бленд диффуза и нормал мап.

                col.r = (col.r * mask) + ((1. - mask) * prevCol.r);
                col.g = (col.g * mask) + ((1. - mask) * prevCol.g);
                col.b = (col.b * mask) + ((1. - mask) * prevCol.b);
                col.a = (col.a * mask) + ((1. - mask) * prevCol.a);
                
				return col;
			}
			ENDCG
		}
	}
}
