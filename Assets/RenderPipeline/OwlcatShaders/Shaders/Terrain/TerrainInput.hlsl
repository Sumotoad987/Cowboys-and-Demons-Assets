#ifndef OWLCAT_TERRAIN_INPUT_INCLUDED
#define OWLCAT_TERRAIN_INPUT_INCLUDED



#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"


#include "../../ShaderLibrary/Input.hlsl"
#include "../../ShaderLibrary/Core.hlsl"
#include "../../ShaderLibrary/SurfaceInput.hlsl"
#include "../../Terrain/TerrainLayerData.cs.hlsl"                                           

StructuredBuffer<TerrainLayerData> _TerrainLayerDatas;

TEXTURE2D_ARRAY(_SplatArray); SAMPLER(sampler_SplatArray);

TEXTURE2D_ARRAY(_DiffuseArray); SAMPLER(sampler_DiffuseArray);
TEXTURE2D_ARRAY(_NormalArray);  SAMPLER(sampler_NormalArray);
TEXTURE2D_ARRAY(_MasksArray);   SAMPLER(sampler_MasksArray);

TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
TEXTURE2D(_MetallicTex);    SAMPLER(sampler_MetallicTex);
       

CBUFFER_START(UnityPerMaterial)
float4 _TerrainLayerMasksScale[256];
float4 _TerrainLayerUvMatrix[256];
float4 _TerrainLayerParams[256];
float4 _MainTex_ST;
float4 _SplatArray_ST;
float4 _SplatArray_TexelSize;
float4 _DiffuseArray_TexelSize;
float4 _Color;
float _AlphaBlendFactor;
int _ControlTexturesCount;
float _TriplanarTightenFactor;
float4 _TerrainHeightmapRecipSize;
float _TerrainMaxHeight;    
sampler2D _Splat0;
sampler2D _Splat1;
sampler2D _Splat2;
sampler2D _Splat3;
sampler2D _Control;
sampler2D _Normal0;
sampler2D _Normal1;
sampler2D _Normal2;
sampler2D _Normal3;
sampler2D _Mask0;
sampler2D _Mask1;
sampler2D _Mask2;   
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;
float4 _Splat3_ST; 
CBUFFER_END

#endif //OWLCAT_TERRAIN_INPUT_INCLUDED
