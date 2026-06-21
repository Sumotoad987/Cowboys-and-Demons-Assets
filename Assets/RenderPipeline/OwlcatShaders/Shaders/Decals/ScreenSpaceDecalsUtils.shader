Shader "Hidden/ScreenSpaceDecalsUtils"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Name "GUI_DECALS_RESOLVE"

			// No culling or depth
			Cull Off ZWrite Off ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

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

			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}

		Pass
		{
			Name "NORMALS_RECONSTRUCTION"

			// No culling or depth
			Cull Off ZWrite Off ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ FETCH2x2 FETCH3x3
			
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

			sampler2D _MainTex;
			float4 _UVToView;
			sampler2D_float _CameraDepthTexture;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			inline float3 FetchViewPos(float2 uv)
			{
				#if ORTHOGRAPHIC_PROJECTION_ON
					float z = _ProjectionParams.y + SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv) * (_ProjectionParams.z - _ProjectionParams.y);
				#else
					float z = DECODE_EYEDEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
				#endif
				return float3((uv * _UVToView.xy + _UVToView.zw) * z, z);
			}

			inline float3 MinDiff(float3 P, float3 Pr, float3 Pl)
			{
				float3 V1 = Pr - P;
				float3 V2 = P - Pl;
				return (dot(V1, V1) < dot(V2, V2)) ? V1 : V2;
			}

			inline float3 MinDiff(float3 P, float3 Pr, float3 Pl, float3 defaultV)
			{
				float3 V1 = Pr - P;
				float3 V2 = P - Pl;
				float cosA = dot(normalize(V1), normalize(V2));
				if (cosA <= 0)
				{
					return defaultV;
				}
				return (dot(V1, V1) < dot(V2, V2)) ? V1 : V2;
			}

			float4 frag (v2f i) : SV_Target
			{
				float2 InvScreenParams = _ScreenParams.zw - 1.0;
				float3 N = 0;

				#if defined(FETCH3x3)
					float3 p00 = FetchViewPos(i.uv + float2(-InvScreenParams.x, InvScreenParams.y));
					float3 p01 = FetchViewPos(i.uv + float2(0, InvScreenParams.y));
					float3 p02 = FetchViewPos(i.uv + float2(InvScreenParams.x, InvScreenParams.y));

					float3 p10 = FetchViewPos(i.uv + float2(-InvScreenParams.x, 0));
					float3 p11 = FetchViewPos(i.uv);
					float3 p12 = FetchViewPos(i.uv + float2(InvScreenParams.x, 0));

					float3 p20 = FetchViewPos(i.uv + float2(-InvScreenParams.x, -InvScreenParams.y));
					float3 p21 = FetchViewPos(i.uv + float2(0, -InvScreenParams.y));
					float3 p22 = FetchViewPos(i.uv + float2(InvScreenParams.x, -InvScreenParams.y));

					float3 h0 = MinDiff(p01, p02, p00);
					float3 h1 = MinDiff(p11, p12, p10);
					float3 h2 = MinDiff(p21, p22, p20);

					float3 v0 = MinDiff(p10, p00, p20);
					float3 v1 = MinDiff(p11, p01, p21);
					float3 v2 = MinDiff(p12, p02, p22);

					float3 h = abs(h0.z) < abs(h1.z) ? h0 : h1;
					h = abs(h.z) < abs(h2.z) ? h : h2;

					float3 v = abs(v0.z) < abs(v1.z) ? v0 : v1;
					v = abs(v.z) < abs(v2.z) ? v : v2;

					N = normalize(cross(h, v));
				#elif defined(FETCH2x2)
					float3 P = FetchViewPos(i.uv);
					float3 Pr, Pl, Pt, Pb;
					Pr = FetchViewPos(i.uv + float2(InvScreenParams.x, 0));
					Pl = FetchViewPos(i.uv + float2(-InvScreenParams.x, 0));
					Pt = FetchViewPos(i.uv + float2(0, InvScreenParams.y));
					Pb = FetchViewPos(i.uv + float2(0, -InvScreenParams.y));
					N = normalize(cross(MinDiff(P, Pr, Pl, float3(1,0,0)), MinDiff(P, Pt, Pb, float3(0, -1, 0))));
				#else
					float3 P = FetchViewPos(i.uv);
					#if defined(UNITY_COMPILER_HLSL) && SHADER_TARGET >= 50
						N = normalize(cross(ddx_fine(P), ddy_fine(P)));
					#else
						N = normalize(cross(ddx(P), ddy(P)));
					#endif
				#endif

				N = float3(N.x, -N.y, N.z);
				N = mul((float3x3)unity_CameraToWorld, N);
				return float4(normalize(N) * .5 + .5, 1);
			}
			ENDCG
		}
	}
}
