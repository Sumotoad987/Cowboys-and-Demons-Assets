Shader "Hidden/DecalBaker"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
				float4 screenPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float4x4 VIEW_PROJ;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Parameters;
			float _Metallness;
			float _Roughness;
			float _Emission;

			sampler2D _DecalMainTex;
			float4 _DecalMainTex_ST;
			sampler2D _DecalParameters;
			float _DecalMetallness;
			float _DecalRoughness;
			float _DecalEmission;

			float _BakeParameters;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(VIEW_PROJ, worldPos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float2 ssP = i.screenPos.xy / i.screenPos.w;
				ssP = TRANSFORM_TEX(ssP, _DecalMainTex);
				float4 decal = tex2D(_DecalMainTex, ssP);
				float4 result = decal;

				if (_BakeParameters > 0)
				{
					float4 parameters = tex2D(_Parameters, i.uv);
					parameters.rgb *= float3(_Roughness, _Emission, _Metallness);

					float4 decalParameters = tex2D(_DecalParameters, ssP);
					decalParameters.rgb *= float3(_DecalRoughness, _DecalEmission, _DecalMetallness);

					result.rgb = lerp(parameters.rgb, decalParameters.rgb, decal.a);
				}
				else
				{
					float4 albedo = tex2D(_MainTex, i.uv);
					result.rgb = lerp(albedo.rgb, decal.rgb, decal.a);
				}
				return result;
			}
			ENDCG
		}
	}
}
