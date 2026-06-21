// Problem 1: SHADOW_COORDS - undefined identifier.
// Why: Using SHADOWS_DEPTH without SPOT.
// The file AutoLight.cginc only takes into account the case where you use SHADOWS_DEPTH + SPOT (to enable SPOT just add a Spot Light in the scene).
// So, if your scene doesn't have a Spot Light, it will skip the SHADOW_COORDS definition and shows the error.
// Now, to workaround this you can:
// 1. Add a Spot Light to your scene
// 2. Use this CGINC to workaround this scase.  Also, you can copy this in your own shader.
#if defined (SHADOWS_DEPTH) && !defined (SPOT)
#       define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif


// Problem 2: _ShadowCoord - invalid subscript.
// Why: nor Shadow screen neighter Shadow Depth or Shadow Cube and trying to use _ShadowCoord attribute.
// The file AutoLight.cginc defines SHADOW_COORDS to empty when no one of these options are enabled (SHADOWS_SCREEN, SHADOWS_DEPTH and SHADOWS_CUBE),
// So, if you try to call "o._ShadowCoord = ..." it will break because _ShadowCoord isn't an attribute in your structure.
// To workaround this you can:
// 1. Check if one of those defines actually exists in any place where you have "o._ShadowCoord...".
// 2. Use the define SHADOWS_ENABLED from this file to perform the same check.
#if defined (SHADOWS_SCREEN) || defined (SHADOWS_DEPTH) || defined (SHADOWS_CUBE)
#    define SHADOWS_ENABLED
#endif

//#ifndef GLOBAL_SHADOWS_ON
//	#undef SHADOWS_SHADOWMASK
//#endif

#ifndef REFLECTIONS_ON
	#define _GLOSSYREFLECTIONS_OFF
#endif

//#if !defined(GLOBAL_REFLECTIONS_ON) && !defined(_GLOSSYREFLECTIONS_OFF)
//	#define _GLOSSYREFLECTIONS_OFF
//#endif

#ifndef USE_LIGHT_PROBE_PROXY_VOLUME
	#ifndef UNITY_LIGHT_PROBE_PROXY_VOLUME
		#define UNITY_LIGHT_PROBE_PROXY_VOLUME 0
	#endif
#endif

#include "UnityStandardCore.cginc"
#include "UnityMetaPass.cginc"

#define MAX_VISIBLE_LIGHTS 64
#define MAX_Z_SLICES_COUNT 64

CBUFFER_START(ClustersDraw)
	int _PunctualLightIndexMap[MAX_VISIBLE_LIGHTS]; // должно быть MAX_Z_SLICES_COUNT * 32, но компилятор не переживает массивы такого размера
	float4x4 _SliceMatrices[MAX_Z_SLICES_COUNT];
	int2 _ZBins[MAX_Z_SLICES_COUNT];
	float2 _HalfTexelSizeAtNearPlane;
	float2 _SliceZNearFar[MAX_Z_SLICES_COUNT];
	float4 _SliceHalfTexelSize[MAX_Z_SLICES_COUNT];
	float4 _LightPositionVSAndRange[MAX_VISIBLE_LIGHTS];
CBUFFER_END

#ifdef SHADER_API_METAL
	Texture2DArray<uint> _ClustersRT;
#else
	Texture2DArray _ClustersRT;
#endif

// *******COMMON***********
#ifndef UNITY_STANDARD_INPUT_INCLUDED
    sampler2D _MainTex;
    float4 _MainTex_ST;
    sampler2D _BumpMap;
    float _Metallic;
#endif
sampler2D _Parameters;
sampler2D _DissolveTex;
sampler2D _PetrificationTex;
sampler2D _ScreenSpaceDecalsTex;

CBUFFER_START(PFCommonBuffer)
float4 _DissolveTex_ST;
float4 _PetrificationTex_ST;
float4 _ScreenSpaceDecalsTex_TexelSize;
float4 _TintColor;
float _NdotLPower;
float _FogInfluence;
float _SpecularPower;
float _SpecularIntensity;
float _Roughness;
float _Emission;
float _Dissolve;
float _DissolveWidth;
float4 _DissolveColor;
float _DissolveColorScale;
float _DissolveCutout;
float _DissolveEmission;
float _Petrification;
float4 _PetrificationColor;
float _PetrificationColorScale;
float _PetrificationColorClamp;
float _PetrificationAlphaScale;
float _AlphaScale;
float _Cutout;
float _RimGlobalLighting;
float _RimLighting;
float4 _RimColor;
float4 _RimGlobalColor;
float _RimPower;
float _RimGlobalPower;
float _RimShadeNdotL;
float _RimGlobalShadeNdotL;
float _GroundColorPower;
float _StencilRef;
float _ScreenSpaceDecalsGlobalFlag;
float _UseNormalMapAtlas;
CBUFFER_END
// *******COMMON***********

// *******Fog Of War*******
sampler2D _FogOfWarMask;
float4 _FogOfWarMask_ST;
sampler2D _FogOfWarDissolveTex;
float4 _FogOfWarDissolveTex_ST;
// rgb = color, a = mask intensity 
float4 _FogOfWarColor;
float _FogOfWarGlobalFlag;
float _FogOfWarMaterialFlag;

#if defined(FOG_OF_WAR_ON) || defined(ALPHABLEND_ON) || defined(ALPHAPREMULTIPLY_ON)
	#define FOG_OF_WAR_COORDS(idx) float4 fogOfWarCoords : TEXCOORD##idx;
	#define TRANSFER_FOG_OF_WAR(o, worldXZ) o.fogOfWarCoords.xy = TRANSFORM_TEX(worldXZ, _FogOfWarMask);
	#define APPLY_FOG_OF_WAR(fogOfWarCoords, finalColor, uv) \
		finalColor = ApplyFogOfWar(finalColor, fogOfWarCoords, uv);
#else
	#if defined(PETRIFICATION_ON)
		#define FOG_OF_WAR_COORDS(idx) float4 fogOfWarCoords : TEXCOORD##idx;
	#else
		#define FOG_OF_WAR_COORDS(idx)
	#endif
	#define TRANSFER_FOG_OF_WAR(o, worldXZ)
	#define APPLY_FOG_OF_WAR(fogOfWarCoords, finalColor, uv)
#endif

float4 ApplyFogOfWar(float4 finalColor, float2 fogOfWarCoords, float2 uv)
{
	float enable = _FogOfWarGlobalFlag && _FogOfWarMaterialFlag;
	if (enable > 0)
	{
		float4 fogOfWar = tex2D(_FogOfWarMask, fogOfWarCoords);
		fogOfWar.g *= _FogOfWarColor.a;
		float mask = max(fogOfWar.r, fogOfWar.g) * (1 - fogOfWar.b);
		mask = saturate(mask);
    
		#if defined(FOG_OF_WAR_DISSOLVE_ON)
			float3 objPos = unity_ObjectToWorld._14_24_34;
			float2 objMaskUv = TRANSFORM_TEX(objPos.xz, _FogOfWarMask);
			float dissolve = 1 - tex2D(_FogOfWarMask, objMaskUv).r;
			float2 dissolveTexUv = TRANSFORM_TEX(uv, _FogOfWarDissolveTex);
			float dissolveMask = tex2D(_FogOfWarDissolveTex, dissolveTexUv).a;
			clip(dissolveMask - dissolve);
		#endif
		finalColor.rgb = lerp(_FogOfWarColor.rgb, finalColor.rgb, mask);
	}

	return finalColor;
}
// *******Fog Of War*******

// *******Light Indexed Lighting*******
sampler2D _LightIndexTex;
float4 _LightIndexTex_TexelSize;

#if defined(LIGHT_INDEX_RAMP_ON)
	sampler2D _LightIndexRamp;
	float4 _LightIndexRamp_TexelSize;
#endif

const float4 _LightIndexColor[MAX_VISIBLE_LIGHTS];
const float4 _LightIndexPosAndAtten[MAX_VISIBLE_LIGHTS];

float3 ShadeFourPointLights(
	float4 lightPosX, float4 lightPosY, float4 lightPosZ,
	float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
	float4 lightAttenSq,
	float4 invRangeSq,
	float3 pos, float3 normal,
	float3 reflected, float3 diffuseColor, float3 specularColor, float smoothness)
{
	// to light vectors
	float4 toLightX = lightPosX - pos.x;
	float4 toLightY = lightPosY - pos.y;
	float4 toLightZ = lightPosZ - pos.z;
	// squared lengths
	float4 lengthSq = 0;
	lengthSq += toLightX * toLightX;
	lengthSq += toLightY * toLightY;
	lengthSq += toLightZ * toLightZ;
	// don't produce NaNs if some vertex position overlaps with the light
	lengthSq = max(lengthSq, 0.000001);

	// NdotL
	float4 ndotl = 0;
	ndotl += toLightX * normal.x;
	ndotl += toLightY * normal.y;
	ndotl += toLightZ * normal.z;
	// correct NdotL
	float4 invLength = rsqrt(lengthSq);
	ndotl = max(float4(0, 0, 0, 0), ndotl * invLength);
	// attenuation
	float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);

	// !!!!!!!!! DON'T DELETE !!!!!!!!!
    // !!! attenuation correction from Engine
    /*static const float kConstantFac = 1.000f;
    static const float kQuadraticFac = 25.0f;
    // where the falloff down to zero should start
    static const float kToZeroFadeStart = 0.8f * 0.8f;
    float LightAttenuateNormalized(float distSqr)
    {
        // match the vertex lighting falloff
        float atten = 1 / (kConstantFac + CalculateLightQuadFac(1.0f) * distSqr);

        // ...but vertex one does not falloff to zero at light's range;
        // So force it to falloff to zero at the edges.
        if (distSqr >= kToZeroFadeStart)
        {
            if (distSqr > 1)
                atten = 0;
            else
                atten *= 1 - (distSqr - kToZeroFadeStart) / (1 - kToZeroFadeStart);
        }

        return atten;
    }*/

	float4 normalizedSqrDist = lengthSq * invRangeSq;
	float4 kToZeroFadeStart = 0.64f; // .8f * .8f
	float4 lerpFactor = normalizedSqrDist > kToZeroFadeStart;
	atten *= lerp(1, 1 - (normalizedSqrDist - kToZeroFadeStart) / (1 - kToZeroFadeStart), lerpFactor);
    atten *= normalizedSqrDist > 1 ? 0 : 1;

	float4 diff = ndotl * atten;
	// final color
	float3 col = 0;

#if defined(SPECULAR_ON) && !defined(VERTEX_LIGHTING_ON)
	float4 rdotl = 0;
	rdotl += toLightX * reflected.x;
	rdotl += toLightY * reflected.y;
	rdotl += toLightZ * reflected.z;
	// correct RdotL
	rdotl = max(float4(0, 0, 0, 0), rdotl * invLength);

	// Refactored BRDF3DirectSimplePF
	float oneMinusSmoothness = 1 - smoothness;
	float4 rdotl4 = Pow4(rdotl);
	half LUT_RANGE = 16.0;
	float4 specular = 0;
	specular.x = tex2D(unity_NHxRoughness, half2(rdotl4.x, oneMinusSmoothness)).UNITY_ATTEN_CHANNEL;
	specular.y = tex2D(unity_NHxRoughness, half2(rdotl4.y, oneMinusSmoothness)).UNITY_ATTEN_CHANNEL;
	specular.z = tex2D(unity_NHxRoughness, half2(rdotl4.z, oneMinusSmoothness)).UNITY_ATTEN_CHANNEL;
	specular.w = tex2D(unity_NHxRoughness, half2(rdotl4.w, oneMinusSmoothness)).UNITY_ATTEN_CHANNEL;
	specular *= LUT_RANGE;

	col += (diffuseColor + specular.x * specularColor) * lightColor0 * diff.x;
	col += (diffuseColor + specular.y * specularColor) * lightColor1 * diff.y;
	col += (diffuseColor + specular.z * specularColor) * lightColor2 * diff.z;
	col += (diffuseColor + specular.w * specularColor) * lightColor3 * diff.w;
#else
	col += lightColor0 * diff.x;
	col += lightColor1 * diff.y;
	col += lightColor2 * diff.z;
	col += lightColor3 * diff.w;
	col *= diffuseColor;
#endif

	return col;
}

void GetLightsInfo(float2 screenUv, out float4 posX, out float4 posY, out float4 posZ, out float3 color0, out float3 color1, out float3 color2, out float3 color3, out float4 atten, out float4 range)
{
	float4 index = tex2Dlod(_LightIndexTex, float4(screenUv, 0, 0));

	float4 p0 =	0;
	float4 p1 =	0;
	float4 p2 =	0;
	float4 p3 =	0;

	float4 c0 =	0;
	float4 c1 =	0;
	float4 c2 =	0;
	float4 c3 =	0;

	#if defined(LIGHT_INDEX_RAMP_ON)
		index.xyzw = index.xyzw * .25;
		index += float4(0, .25, .5, .75) + _LightIndexRamp_TexelSize.x * .5;
		p0 = tex2Dlod(_LightIndexRamp, float4(index.x, .75, 0, 0));
		c0 = tex2Dlod(_LightIndexRamp, float4(index.x, .25, 0, 0));

		p1 = tex2Dlod(_LightIndexRamp, float4(index.y, .75, 0, 0));
		c1 = tex2Dlod(_LightIndexRamp, float4(index.y, .25, 0, 0));

		p2 = tex2Dlod(_LightIndexRamp, float4(index.z, .75, 0, 0));
		c2 = tex2Dlod(_LightIndexRamp, float4(index.z, .25, 0, 0));

		p3 = tex2Dlod(_LightIndexRamp, float4(index.w, .75, 0, 0));
		c3 = tex2Dlod(_LightIndexRamp, float4(index.w, .25, 0, 0));
	#elif defined(VERTEX_LIGHTING_ON) || SHADER_TARGET > 30 // shader models less than 3.5 can't indexing arrays in pixel shader
		index.xyzw = index.xyzw * 255;
		index += float4(0, 256, 512, 768);
		index.w = min(index.w, 1022);

		p0 = _LightIndexPosAndAtten[index.x];
		p1 = _LightIndexPosAndAtten[index.y];
		p2 = _LightIndexPosAndAtten[index.z];
		p3 = _LightIndexPosAndAtten[index.w];

		c0 = _LightIndexColor[index.x];
		c1 = _LightIndexColor[index.y];
		c2 = _LightIndexColor[index.z];
		c3 = _LightIndexColor[index.w];
	#endif

	atten = float4(p0.w, p1.w, p2.w, p3.w);
	posX = float4(p0.x, p1.x, p2.x, p3.x);
	posY = float4(p0.y, p1.y, p2.y, p3.y);
	posZ = float4(p0.z, p1.z, p2.z, p3.z);
    range = float4(c0.w, c1.w, c2.w, c3.w);
	color0 = c0.rgb;
	color1 = c1.rgb;
	color2 = c2.rgb;
	color3 = c3.rgb;
}

float3 LightIndexLighting(float2 screenUv, float3 posWorld, float3 normalWorld, float3 reflected, float3 diffuseColor, float3 specularColor, float smoothness)
{
	float4 posX = 0;
	float4 posY = 0;
	float4 posZ = 0;
	float3 color0 = 0;
	float3 color1 = 0;
	float3 color2 = 0;
	float3 color3 = 0;
	float4 atten = 0;
	float4 range = 0;

	GetLightsInfo(screenUv, posX, posY, posZ, color0, color1, color2, color3, atten, range);

	return ShadeFourPointLights(posX, posY, posZ,
		color0, color1, color2, color3,
		atten,
        range,
		posWorld,
		normalWorld,
		reflected,
		diffuseColor,
		specularColor,
		smoothness);
}
// *******Light Indexed Lighting*******

// *******Vertex Animation*********
float2 Unpack(float input, int precision)
{
	float2 output = 0;
	output.x = floor(input / precision);
	output.y = input % precision;
	return output / (precision - 1);
}

float4 SmoothCurve( float4 x )
{   
	return x * x *( 3.0 - 2.0 * x );   
}

float4 TriangleWave( float4 x )
{   
	return abs( frac( x + 0.5 ) * 2.0 - 1.0 );   
}

float4 SmoothTriangleWave( float4 x )
{   
	return SmoothCurve( TriangleWave( x ) );   
}

sampler2D _FoliageMask;
float4 _FoliageMask_ST;

CBUFFER_START(VertexAnimBuffer)
float _ComplexAnimBranchScale;
float _ComplexAnimEdgeFlutterScale;
float _ComplexAnimPrimaryFactorScale;
float _ComplexAnimSecondaryFactorScale;
float _SimpleAnimInfluence;
float4 _Wind = float4(1, 1, 1, 1);
float _WindScaleForGrass = 1;
float _FoliageInteractionInfluence;
float _GlobalFoliageInteractionFlag;
float _GlobalFoliageInfluenceClamp;
CBUFFER_END

// Detail bending
inline float4 AnimateVertex(float4 pos, float3 normal, float4 vertexColor, float2 ObjectOffsetXZ, float4 wind)
{	
	// VERTEX_ANIMATION_SIMPLE_ON means that we drawing grass, which batched in AssetImportPostprocessor with object offset baking
	#if defined(VERTEX_ANIMATION_SIMPLE_ON)
		float3 offset = float3(ObjectOffsetXZ.x, 0, ObjectOffsetXZ.y);
		float maxGrassLength = .5f;
		float windLength = length(wind.xyz);
		float grassWindScale = maxGrassLength / windLength;
		wind.xyz *= windLength > maxGrassLength ? grassWindScale : 1;
	#else
		float3 offset = 0;
	#endif

	float4 objWorldPos = mul(unity_ObjectToWorld, float4(offset.xyz, 1));
	float fObjPhase = dot(objWorldPos.xyz, 1);
	float4 windWaves = (frac((_Time.yyyy + fObjPhase) * float4(1.975, 0.793, 0.375, 0.193) * wind.w) * 2.0 - 1.0);
	windWaves = SmoothTriangleWave(windWaves);
	wind.xyz += wind.xyz * windWaves.xyz;

	if (_GlobalFoliageInteractionFlag > 0)
	{
		#if defined(VERTEX_INTERACTION_ON)
			/*#if defined(VERTEX_ANIMATION_COMPLEX_ON)
				offset = float3(packedObjectOffset.x, 0, packedObjectOffset.y);
				objWorldPos = mul(unity_ObjectToWorld, float4(offset.xyz, 1));
			#endif*/
			float2 uv = TRANSFORM_TEX(objWorldPos.xz, _FoliageMask);
			float2 clampValue = _GlobalFoliageInfluenceClamp;
			float2 windMask = clamp(tex2Dlod(_FoliageMask, float4(uv, 0, 0)).rg, -clampValue, clampValue) * _FoliageInteractionInfluence;
			wind.xz += windMask;
		#endif
	}

	wind.xyz = mul((float3x3)unity_WorldToObject, wind.xyz);

	#if defined(VERTEX_ANIMATION_SIMPLE_ON)
		pos.xyz += _SimpleAnimInfluence * wind.xyz * vertexColor.a;
	#endif

	#if defined(VERTEX_ANIMATION_COMPLEX_ON)
		// animParams stored in color
		// animParams.x = branch phase
		// animParams.y = edge flutter factor
		// animParams.z = primary factor
		// animParams.w = secondary factor

		float4 animParams = 0;
		animParams.x = vertexColor.r;
		animParams.y = vertexColor.g;
		animParams.z = vertexColor.a;
		animParams.w = vertexColor.b;
		animParams *= float4(_ComplexAnimBranchScale, _ComplexAnimEdgeFlutterScale, _ComplexAnimPrimaryFactorScale, _ComplexAnimSecondaryFactorScale);

		float fDetailAmp = 0.1f;
		float fBranchAmp = 0.3f;
	
		// Phases (object, vertex, branch)
		float fBranchPhase = fObjPhase + animParams.x;
		float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);
	
		// x is used for edges; y is used for branches
		float2 vWavesIn = _Time.yy + float2(fVtxPhase, fBranchPhase );
	
		// 1.975, 0.793, 0.375, 0.193 are good frequencies
		float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);
	
		vWaves = SmoothTriangleWave( vWaves );
		float2 vWavesSum = vWaves.xz + vWaves.yw;

		// Edge (xz) and branch bending (y)
		float3 bend = animParams.y * fDetailAmp * normal.xyz;
		bend.y = animParams.w * fBranchAmp;
		pos.xyz += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w;

		// Primary bending
		// Displace position
		pos.xyz += animParams.z * wind.xyz;
	#endif
	
	return pos;
}
// *******Vertex Animation*********

// *******META***********
struct VertexInput_Meta
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

struct v2f_meta
{
	float2 uv		: TEXCOORD0;
	float4 pos		: SV_POSITION;
};

v2f_meta vert_meta(VertexInput_Meta v)
{
	v2f_meta o;
	o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	return o;
}

float4 frag_meta(v2f_meta i) : SV_Target
{
	UnityMetaInput o;
	UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );

	float3 albedo = tex2D(_MainTex, i.uv.xy).rgb;
    o.Albedo = lerp(_TintColor.rgb, albedo.rgb, _TintColor.a);
	o.Emission = 0;
	#if defined(EMISSION_ON)
		o.Emission = tex2D(_Parameters, i.uv.xy).g * _Emission * o.Albedo.rgb;
	#endif

	return UnityMetaFragment(o);
}
// *******META***********

//********************Position Reconstruction***********************
// Inverse projection matrix parts
float _TanHalfVerticalFov; // invProjection.11; 
float _TanHalfHorizontalFov; // invProjection.00;

// Camera basis in reconstruction space (world, shadow map, screen space)
float3 _CamBasisUp;
float3 _CamBasisSide;
float3 _CamBasisFront;

// postProjectiveSpacePosition in homogeneous projection space 
float3 CreateRay(float3 postProjectiveSpacePosition)
{
	float3 leftRight = _CamBasisSide * postProjectiveSpacePosition.x * _TanHalfHorizontalFov;
    #if UNITY_UV_STARTS_AT_TOP
	    postProjectiveSpacePosition.y = -postProjectiveSpacePosition.y;
    #endif
	float3 upDown = _CamBasisUp * postProjectiveSpacePosition.y * _TanHalfVerticalFov;
	float3 forward = _CamBasisFront;
	return (forward + leftRight + upDown);
}

float3 ReconstructPosition(float3 cameraRay, float3 cameraPos, float linearDepth)
{
    return cameraPos + cameraRay * linearDepth; 
}
//********************Position Reconstruction***********************

// *******COMMON***********
struct VertexInputData
{
    float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	#if defined(DYNAMICLIGHTMAP_ON)
		float2 uv2 : TEXCOORD2;
	#endif
	#if defined(VERTEX_ANIMATION_ON)
		float2 uv3 : TEXCOORD3;
		float4 color : COLOR0;
	#endif
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct FragmentInputData
{
    float4 pos : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 uv : TEXCOORD0;
	half4 ambientOrLightmapUV : TEXCOORD1;    // SH or Lightmap UV
    #if defined(LIGHT_INDEX_ON) && defined(VERTEX_LIGHTING_ON)
	    half3 vertexLighting : TEXCOORD2;
    #endif
	half4 tangentToWorldAndWorldPos[3] : TEXCOORD3;
	float3 posVS : TEXCOORD6;
	UNITY_SHADOW_COORDS(7)
	UNITY_FOG_COORDS(8)
	FOG_OF_WAR_COORDS(9)
    #if defined(NEED_POSITION_RECONSTRUCTION)
        float3 cameraRay : TEXCOORD10;
    #endif
	#if defined(NEED_SCREEN_POS)
		float4 screenPos : TEXCOORD11;
	#endif
	#if defined(VERTEX_ANIMATION_ON)
		float4 color : COLOR0;
	#endif
};

// *******Clustered Lighting***********
CBUFFER_START(ClusteredBuffer)
float4 _LightPosAndRange[MAX_VISIBLE_LIGHTS];
float4 _LightColorAndAtten[MAX_VISIBLE_LIGHTS];
float3 _Clusters;
float _GlobalClusterLightingEnabled;
CBUFFER_END

float LinearViewDepth(float3 posVS, float3 clipPos)
{
    float depth = 0;
    if (unity_OrthoParams.w < 1)
    {
        //float near = _ProjectionParams.y;
        //float far = _ProjectionParams.z;
		depth = (-posVS.z - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);
    }
    else
    {
        #if UNITY_REVERSED_Z
            depth = 1 - clipPos.z;
        #else
            depth = clipPos.z;
        #endif
    }

    return depth;
}

float3 EvalClusteredLighting(
	float3 lightPos,
	float3 lightColor,
	float invRangeSq,
	float lightAttenSq,
	float3 posWS,
	float3 normalWS,
	float3 reflected,
	float3 diffColor,
	float3 specColor,
	float smoothness)
{
	half3 lightDir = lightPos.xyz - posWS.xyz;
	half lengthSq = dot(lightDir, lightDir);
	lightDir = normalize(lightDir);
	// don't produce NaNs if some vertex position overlaps with the light
	lengthSq = max(lengthSq, 0.000001);
	half ndotl = saturate(dot(normalWS, lightDir));
	half atten = 1.0 / (1.0 + lengthSq * lightAttenSq);

	half normalizedSqrDist = lengthSq * invRangeSq;
	half kToZeroFadeStart = 0.64; // .8 * .8
	half lerpFactor = normalizedSqrDist > kToZeroFadeStart;
	atten *= lerp(1, 1 - (normalizedSqrDist - kToZeroFadeStart) / (1 - kToZeroFadeStart), lerpFactor);
	atten *= normalizedSqrDist > 1 ? 0 : 1;

	half3 diff = ndotl * atten;
	half3 col = 0;

	#if defined(SPECULAR_ON)
		half rdotl4 = Pow4(DotClamped(reflected, lightDir));
		half oneMinusSmoothness = 1 - smoothness;
		half LUT_RANGE = 16.0;
		//half specular = tex2D(unity_NHxRoughness, half2(rdotl4.x, oneMinusSmoothness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
		half specular = tex2Dlod(unity_NHxRoughness, half4(rdotl4.x, oneMinusSmoothness, 0, 0)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
		col = (diffColor + specColor * specular) * lightColor * diff;
	#else
		col = diffColor * lightColor * diff;
	#endif

	return col;
}

float3 ClusteredLighting(FragmentInputData IN, float3 posWS, float3 normalWS, float3 reflected, float3 diffuseColor, float3 specularColor, float smoothness)
{
	float depth = LinearViewDepth(IN.posVS.xyz, IN.pos.xyz);

	float2 screenUv = IN.pos.xy / _ScreenParams.xy;

	#if UNITY_UV_STARTS_AT_TOP
        screenUv.y = 1 - screenUv.y;
    #endif

	float depthSliceIndex = depth * _Clusters.z;
	int4 clustersUv = int4(screenUv.xy * _Clusters.xy, depthSliceIndex, 0);
	uint lightMask = asuint(_ClustersRT.Load(clustersUv));

	float3 result = 0;
	while (lightMask)
	{
		// Extract a light from the mask and disable that bit
		int mapIndex = firstbitlow(lightMask);
		lightMask &= ~(1 << mapIndex);

		// Get light index from index map
		int offset = _ZBins[depthSliceIndex].x;
		int lightIndex = _PunctualLightIndexMap[offset + mapIndex];

		float4 posAndRange = _LightPosAndRange[lightIndex];
		float4 colorAndAtten = _LightColorAndAtten[lightIndex];

		result += EvalClusteredLighting(
			posAndRange.xyz,
			colorAndAtten.xyz,
			posAndRange.w,
			colorAndAtten.w,
			posWS,
			normalWS,
			reflected,
			diffuseColor,
			specularColor,
			smoothness
		);
	}

	return result;
}
// *******Clustered Lighting***********

inline half4 VertexGIForwardPF(VertexInputData v, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}

FragmentInputData VertexCommon(VertexInputData v)
{
	UNITY_SETUP_INSTANCE_ID(v);
    FragmentInputData o = (FragmentInputData)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);

    #if defined(VERTEX_ANIMATION_ON)
		float4 vertexColor = v.color;
		#if defined(VERTEX_ANIMATION_SIMPLE_ON)
			float2 gradientAndWind = Unpack(v.color.a, 4096);
			vertexColor.a = gradientAndWind.y; // set alpha channel = wind influence 
		#endif
	    v.vertex = AnimateVertex(v.vertex, v.normal, vertexColor, v.uv3, _Wind);
		#if defined(VERTEX_ANIMATION_SIMPLE_ON)
			vertexColor.a = gradientAndWind.x; // set alpha channel = color gradient for using later in pixel shader
		#endif
        o.color = vertexColor;
	#endif

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);

	o.ambientOrLightmapUV = VertexGIForwardPF(v, posWorld, normalWorld);

	#if defined(LIGHT_INDEX_ON)
		#if defined(VERTEX_LIGHTING_ON)
			float4 screenPos = ComputeScreenPos(o.pos);
			o.vertexLighting = LightIndexLighting(screenPos.xy / screenPos.w, posWorld, normalWorld, 1, 1, 1, 1);
		#endif
	#endif

    o.pos = UnityObjectToClipPos(v.vertex);
	o.posVS = UnityWorldToViewPos(posWorld.xyz);
    o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

    #if defined(BUMP_ON)
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndWorldPos[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndWorldPos[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndWorldPos[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndWorldPos[0].xyz = 0;
        o.tangentToWorldAndWorldPos[1].xyz = 0;
        o.tangentToWorldAndWorldPos[2].xyz = normalWorld;
    #endif

    o.tangentToWorldAndWorldPos[0].w = posWorld.x;
    o.tangentToWorldAndWorldPos[1].w = posWorld.y;
    o.tangentToWorldAndWorldPos[2].w = posWorld.z;

    UNITY_TRANSFER_SHADOW(o, v.uv1);
    UNITY_TRANSFER_FOG(o, o.pos);
    TRANSFER_FOG_OF_WAR(o, posWorld.xz);

    #if defined(NEED_POSITION_RECONSTRUCTION)
        o.cameraRay = CreateRay(o.pos.xyz / o.pos.w);
    #endif

    #if defined(DISSOLVE_ON)
		o.uv.zw = TRANSFORM_TEX(v.uv, _DissolveTex);
	#endif

	#if defined(PETRIFICATION_ON)
		o.fogOfWarCoords.zw = TRANSFORM_TEX(v.uv, _PetrificationTex);
	#endif

    return o;
}

float4 CalculateAlbedo(FragmentInputData i, float2 screenPos, out float3 emissiveDecal)
{
	emissiveDecal = 0;

    // main tex
    float4 albedo = tex2D(_MainTex, i.uv.xy);

    #if defined(VERTEX_ANIMATION_ON) && defined(VERTEX_ANIMATION_SIMPLE_ON)
		albedo.rgb = lerp(i.color.rgb, albedo.rgb, pow(saturate(i.color.a), _GroundColorPower));
	#endif

    // color tint
	albedo.rgb = lerp(_TintColor.rgb, albedo.rgb, _TintColor.a);

    // screen space decals
    if (_ScreenSpaceDecalsGlobalFlag)
    {
        // decoding bool flags http://theinstructionlimit.com/encoding-boolean-flags-into-a-float-in-hlsl
	    //float stencilFlag = fmod(_StencilRef, 2) == 1;
		int stencilRef = (int)_StencilRef;
		float stencilFlag = stencilRef != 4;

	    float2 screenUv = screenPos.xy / _ScreenParams.xy;
        
        // decals color
	    float4 decals = tex2D(_ScreenSpaceDecalsTex, screenUv) * stencilFlag;

		float4 decalsSat = saturate(decals);
		albedo.rgb = lerp(albedo.rgb, decalsSat.rgb, decalsSat.a);
		
		emissiveDecal = max(0, decals.rgb - 1);

		// unpack two 8-bit gradient from 16-bit float
		/*float emissionGradient = floor(decals.a) / 255;
		float alphaGradient = frac(decals.a);*/

		//albedo.rgb = lerp(albedo.rgb, decals.rgb, alphaGradient * (1 - emissionGradient));

		//emissiveDecal = decals.rgb * emissionGradient * DECALS_EMISSION;
    }

    return albedo;
}

float4 ParametersMask(float2 uv)
{
    #if defined(SPECULAR_ON) || defined(EMISSION_ON) || defined(METALLNESS_ON)
        return tex2D(_Parameters, uv.xy);
    #else
        return float4(1, 1, 1, 1);
    #endif
}

inline half3 PreMultiplyAlphaPF(half3 diffColor, half alpha, half oneMinusReflectivity, out half outModifiedAlpha)
{
	#if defined(ALPHAPREMULTIPLY_ON)
		// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)

		// Transparency 'removes' from Diffuse component
		diffColor *= alpha;

		#if (SHADER_TARGET < 30)
			// SM2.0: instruction count limitation
			// Instead will sacrifice part of physically based transparency where amount Reflectivity is affecting Transparency
			// SM2.0: uses unmodified alpha
			outModifiedAlpha = alpha;
		#else
			// Reflectivity 'removes' from the rest of components, including Transparency
			// outAlpha = 1-(1-alpha)*(1-reflectivity) = 1-(oneMinusReflectivity - alpha*oneMinusReflectivity) =
			//          = 1-oneMinusReflectivity + alpha*oneMinusReflectivity
			outModifiedAlpha = 1-oneMinusReflectivity + alpha*oneMinusReflectivity;
		#endif
	#else
		outModifiedAlpha = alpha;
	#endif
	return diffColor;
}

float3 UnpackNormalFromAtlas(float4 tex)
{
	/*#if defined(ATLAS_NORMAL_MAP_ON)
		float3 result = tex.xyz * 2 - 1;
	#else
		float3 result = UnpackNormal(tex);
	#endif*/

	float3 result = 0;
	if (_UseNormalMapAtlas)
	{
		result = tex.xyz * 2 - 1;
	}
	else
	{
		result = UnpackNormal(tex);
	}

	return result;
}

half3 PerPixelWorldNormalPF(float2 uv, half4 tangentToWorld[3])
{
    #ifdef BUMP_ON
        half3 tangent = tangentToWorld[0].xyz;
        half3 binormal = tangentToWorld[1].xyz;
        half3 normal = tangentToWorld[2].xyz;

        #if UNITY_TANGENT_ORTHONORMALIZE
            normal = normalize(normal);

            // ortho-normalize Tangent
            tangent = normalize (tangent - normal * dot(tangent, normal));

            // recalculate Binormal
            half3 newB = cross(normal, tangent);
            binormal = newB * sign (dot (newB, binormal));
        #endif

        half3 normalTangent = UnpackNormalFromAtlas(tex2D(_BumpMap, uv));
        half3 normalWorld = normalize(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
    #else
        half3 normalWorld = normalize(tangentToWorld[2].xyz);
    #endif
    return normalWorld;
}

half3 BRDF3DirectSimplePF(half3 diffColor, half3 specColor, half smoothness, half rl)
{
    #if SPECULAR_ON
	    half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
						       // Lookup texture to save instructions
	    half specular = tex2D(unity_NHxRoughness, half2(Pow4(rl), 1 - smoothness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;

	    return diffColor + specular * specColor;
    #else
	    return diffColor;
    #endif
}

float4 RimLighting(float4 color, float ndotv, float ndotl)
{
	/*if (_RimGlobalLighting)
	{
		float rimGlobal = pow((1.0 - ndotv), _RimGlobalPower);
		if (_RimGlobalShadeNdotL > 0)
		{
			rimGlobal *= (1.0 - ndotl);
		}

		color.rgb += _RimGlobalColor.rgb * rimGlobal;
	}*/

	if (_RimLighting)
	{
		float rim = pow((1.0 - ndotv), _RimPower);
		if (_RimShadeNdotL > 0)
		{
			rim *= (1.0 - ndotl);
		}

		float3 rimColor = lerp(color.rgb, _RimColor.rgb, _RimColor.a);
		color.rgb = lerp(color.rgb, rimColor.rgb, rim);
	}

	return color;
}

float4 Dissolve(float4 albedo, float dissolveMask, out float3 emission)
{
    float dissolve = _Dissolve;
    float dissolveBorder = dissolve + _DissolveWidth;
    float dissolveValue = dissolveMask > dissolve ? 1 : 0;
    clip(dissolveMask - dissolve * _DissolveCutout);
    
    float borderNorm = saturate((dissolveMask - dissolve) / _DissolveWidth);

    float gradient = dissolveValue * borderNorm;
    gradient = gradient < .001 ? 1 : gradient;
    float4 color = _DissolveColorScale * _DissolveColor;
    emission = (1 - gradient) * color.rgb * _DissolveEmission;
    float3 result = lerp(color.rgb * (1 - _DissolveEmission), albedo.rgb, gradient);

    return float4(result, albedo.a);
}

float3 Petrification(float3 albedo, float4 petrificationMask)
{
    float intensity = dot(albedo.rgb, float3(0.299, 0.587, 0.114));
	float maskIntensity = petrificationMask.a;
	float lerpFactor = saturate((maskIntensity - (1 - _Petrification)) * _PetrificationAlphaScale);
	float3 petrificationColor = clamp(intensity * petrificationMask.rgb * _PetrificationColor.rgb * _PetrificationColorScale, 0, _PetrificationColorClamp);
	albedo.rgb = lerp(albedo.rgb, petrificationColor, lerpFactor);
    return albedo.rgb;
}

FragmentCommonData FragmentSetup(FragmentInputData i, half4 albedo, half4 parametersMask)
{
	half roughness = _Roughness;
	half metallic = 0;

	#if defined(SPECULAR_ON)
		roughness = max(.0001, parametersMask.r * _Roughness);
	#endif

	#if defined(METALLNESS_ON)
		metallic = parametersMask.b * _Metallic;
	#endif

	half oneMinusReflectivity;
	half3 specColor;
	half3 diffColor = DiffuseAndSpecularFromMetallic (albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.smoothness = 1 - sqrt(roughness);

	o.posWorld = half3(i.tangentToWorldAndWorldPos[0].w, i.tangentToWorldAndWorldPos[1].w, i.tangentToWorldAndWorldPos[2].w);
	o.normalWorld = PerPixelWorldNormalPF(i.uv, i.tangentToWorldAndWorldPos);
	o.eyeVec = normalize(o.posWorld.xyz - _WorldSpaceCameraPos.xyz);
	o.alpha = albedo.a;
	o.diffColor = PreMultiplyAlphaPF(o.diffColor, o.alpha, o.oneMinusReflectivity, /*out*/ o.alpha);

	return o;
}

inline UnityGI FragmentGIPF (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
	UnityGIInput d;
	d.light = light;
	d.worldPos = s.posWorld;
	d.worldViewDir = -s.eyeVec;
	d.atten = atten;
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

	d.probeHDR[0] = unity_SpecCube0_HDR;
	d.probeHDR[1] = unity_SpecCube1_HDR;
	#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
		d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
	#endif
	#ifdef UNITY_SPECCUBE_BOX_PROJECTION
		d.boxMax[0] = unity_SpecCube0_BoxMax;
		d.probePosition[0] = unity_SpecCube0_ProbePosition;
		d.boxMax[1] = unity_SpecCube1_BoxMax;
		d.boxMin[1] = unity_SpecCube1_BoxMin;
		d.probePosition[1] = unity_SpecCube1_ProbePosition;
	#endif

	UnityGI result = (UnityGI)0;
	#if defined(REFLECTIONS_ON)
	{
		Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
		// Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
		#if UNITY_STANDARD_SIMPLE
			g.reflUVW = s.reflUVW;
		#endif

		result = UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
	}
	#else
	{
		result = UnityGlobalIllumination (d, occlusion, s.normalWorld);
	}
	#endif

	return result;
}

//**********Tangent Space**************
half3 TransformToTangentSpace(half3 tangent, half3 binormal, half3 normal, half3 v)
{
	// Mali400 shader compiler prefers explicit dot product over using a half3x3 matrix
	return half3(dot(tangent, v), dot(binormal, v), dot(normal, v));
}

void TangentSpaceLightingInput(half3 normalWorld, half4 vTangent, half3 lightDirWorld, half3 eyeVecWorld, out half3 tangentSpaceLightDir, out half3 tangentSpaceEyeVec)
{
	half3 tangentWorld = UnityObjectToWorldDir(vTangent.xyz);
    half sign = half(vTangent.w) * half(unity_WorldTransformParams.w);
    half3 binormalWorld = cross(normalWorld, tangentWorld) * sign;
    tangentSpaceLightDir = TransformToTangentSpace(tangentWorld, binormalWorld, normalWorld, lightDirWorld);
    tangentSpaceEyeVec = normalize(TransformToTangentSpace(tangentWorld, binormalWorld, normalWorld, eyeVecWorld));
}