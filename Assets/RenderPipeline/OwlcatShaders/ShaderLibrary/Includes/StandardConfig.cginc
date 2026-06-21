#include "PFCore.cginc"

FragmentInputData vert_base(VertexInputData v)
{
	FragmentInputData o = VertexCommon(v);

	return o;
}

half4 frag_base(FragmentInputData i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	float3 decalsEmission = 0;
	float4 albedo = CalculateAlbedo(i, i.pos.xy, decalsEmission);
	float4 parametersMask = ParametersMask(i.uv);

	albedo.a = saturate(albedo.a * _AlphaScale);

	#if defined(CUTOUT_ON)
		clip(albedo.a - _Cutout);
	#endif

	#if defined(EMISSION_ON)
		float3 emission = parametersMask.g * _Emission * albedo.rgb;
	#endif

	#if defined(PETRIFICATION_ON)
		float4 petrificationMask = tex2D(_PetrificationTex, i.fogOfWarCoords.zw);
		albedo.rgb = Petrification(albedo.rgb, petrificationMask);
	#endif

	#if defined(DISSOLVE_ON)
		float4 dissolveMask = tex2D(_DissolveTex, i.uv.zw);
		float3 dissolveEmission = 0;
		albedo = Dissolve(albedo, dissolveMask, dissolveEmission);
	#endif

	FragmentCommonData s = FragmentSetup(i, albedo, parametersMask);
	UnityLight mainLight = MainLight();

	UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

	half ndotl = saturate(dot(s.normalWorld, mainLight.dir));
	UnityGI gi = FragmentGIPF(s, 1, i.ambientOrLightmapUV, atten, mainLight);
	half3 attenuatedLightColor = gi.light.color * pow(ndotl, _NdotLPower);

	half grazingTerm = saturate(s.smoothness + (1 - s.oneMinusReflectivity));
	half ndotv = DotClamped(s.normalWorld, -s.eyeVec);
	half fresnelTerm = Pow4(1 - ndotv);
	half3 r = reflect(s.eyeVec, s.normalWorld);
	half rdotl = DotClamped(r, mainLight.dir);
	half4 finalColor = albedo;
	finalColor.rgb = BRDF3_Indirect(s.diffColor, s.specColor, gi.indirect, grazingTerm, fresnelTerm);
	finalColor.rgb += BRDF3DirectSimplePF(s.diffColor, s.specColor, s.smoothness, rdotl) * attenuatedLightColor;

	if (_GlobalClusterLightingEnabled > 0)
	{
		finalColor.rgb += ClusteredLighting(i, s.posWorld, s.normalWorld, r, s.diffColor, s.specColor, s.smoothness);
	}

	#if defined(EMISSION_ON)
		finalColor.rgb += emission;
	#endif

	#if defined(DISSOLVE_ON)
		finalColor.rgb += dissolveEmission;
	#endif

	finalColor.rgb += decalsEmission;

	// rim
	finalColor = RimLighting(finalColor, ndotv, ndotl);

	// apply fog
	float4 preFogColor = finalColor;
	UNITY_APPLY_FOG(i.fogCoord, finalColor);
	finalColor = lerp(preFogColor, finalColor, _FogInfluence);

	#if defined(ALPHABLEND_ON) || defined(ALPHAPREMULTIPLY_ON)
		_FogOfWarMaterialFlag = 1;
	#endif
	APPLY_FOG_OF_WAR(i.fogOfWarCoords, finalColor, i.uv.xy)

	return float4(finalColor.rgb, s.alpha);
}

FragmentInputData vert_add(VertexInputData v)
{
	FragmentInputData o = VertexCommon(v);

	return o;
}

float4 frag_add(FragmentInputData i) : SV_Target
{
	float3 decalsEmission = 0;
	float4 albedo = CalculateAlbedo(i, i.pos.xy, decalsEmission);
	float4 parametersMask = ParametersMask(i.uv);

	albedo.a = saturate(albedo.a * _AlphaScale);

	#if defined(CUTOUT_ON)
		clip(albedo.a - _Cutout);
	#endif

	#if defined(EMISSION_ON)
		half3 emission = parametersMask.g * _Emission * albedo.rgb;
	#endif

	#if defined(PETRIFICATION_ON)
		float4 petrificationMask = tex2D(_PetrificationTex, i.fogOfWarCoords.zw);
		albedo.rgb = Petrification(albedo.rgb, petrificationMask);
	#endif

	#if defined(DISSOLVE_ON)
		float4 dissolveMask = tex2D(_DissolveTex, i.uv.zw);
		float3 dissolveEmission = 0;
		albedo = Dissolve(albedo, dissolveMask, dissolveEmission);
	#endif

	FragmentCommonData s = FragmentSetup(i, albedo, parametersMask);
	half3 lightDir = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - s.posWorld.xyz, _WorldSpaceLightPos0.w));
	UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

	half ndotl = saturate(dot(s.normalWorld, lightDir));
	half3 r = reflect(s.eyeVec, s.normalWorld);
	half rdotl = DotClamped(r, lightDir);
				
	half3 attenuatedLightColor = _LightColor0.rgb * atten * pow(ndotl, _NdotLPower);

	half4 finalColor = albedo;
	finalColor.rgb = BRDF3DirectSimplePF(s.diffColor, s.specColor, s.smoothness, rdotl) * attenuatedLightColor;

	// apply fog
	float4 preFogColor = finalColor;
	UNITY_APPLY_FOG_COLOR(i.fogCoord, finalColor, fixed4(0, 0, 0, 0));
	finalColor = lerp(preFogColor, finalColor, _FogInfluence);

	#if defined(ALPHABLEND_ON) || defined(ALPHAPREMULTIPLY_ON)
		_FogOfWarMaterialFlag = 1;
	#endif
	APPLY_FOG_OF_WAR(i.fogOfWarCoords, finalColor, i.uv.xy)

	return finalColor;
}

struct appdata
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	#if defined(VERTEX_ANIMATION_ON)
		float2 uv3 : TEXCOORD3;
		float4 color : COLOR0;
	#endif
};

struct v2f
{
	V2F_SHADOW_CASTER;
	#if defined(CUTOUT_ON) || defined(FOG_OF_WAR_DISSOLVE_ON)
		float2 uv : TEXCOORD1;
	#endif
	#if defined(DISSOLVE_ON)
		float2 uv2 : TEXCOORD2;
	#endif
	FOG_OF_WAR_COORDS(3)
};

v2f vert_shadow_caster(appdata v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	#if defined(VERTEX_ANIMATION_ON)
		float4 vertexColor = v.color;
		#if defined(VERTEX_ANIMATION_SIMPLE_ON)
			float2 windAndGradient = Unpack(v.color.a, 4096);
			vertexColor.a = windAndGradient.y; // set alpha channel = wind influence 
		#endif
		v.vertex = AnimateVertex(v.vertex, v.normal, vertexColor, v.uv3, _Wind);
	#endif

	v2f o = (v2f)0;
	#if defined(CUTOUT_ON)
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	#endif

	#if defined(DISSOLVE_ON)
		o.uv2 = TRANSFORM_TEX(v.texcoord, _DissolveTex);
	#endif
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

	#if defined(FOG_OF_WAR_DISSOLVE_ON)
		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		TRANSFER_FOG_OF_WAR(o, worldPos.xz);
	#endif
	return o;
}

float4 frag_shadow_caster(v2f i) : SV_Target
{
	float4 albedo = 1;

	#if defined(CUTOUT_ON)
		albedo = tex2D(_MainTex, i.uv);
		clip(albedo.a - _Cutout);
	#endif

	#if defined(DISSOLVE_ON)
		float4 dissolveMask = tex2D(_DissolveTex, i.uv2);
		float3 dissolveEmission = 0;
		albedo = Dissolve(albedo, dissolveMask.r, dissolveEmission);
	#endif

	#if defined(FOG_OF_WAR_DISSOLVE_ON)
		float4 finalColor = float4(0,0,0,1);
		APPLY_FOG_OF_WAR(i.fogOfWarCoords, finalColor, i.uv)
	#endif

	SHADOW_CASTER_FRAGMENT(i)
}