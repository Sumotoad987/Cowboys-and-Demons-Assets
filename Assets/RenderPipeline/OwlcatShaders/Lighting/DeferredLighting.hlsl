#ifndef OWLCAT_DEFERRED_LIGHTING_INCLUDED
#define OWLCAT_DEFERRED_LIGHTING_INCLUDED

#if !defined(SHADER_API_PS4) && !defined(SHADER_API_PS5)
        // https://jira.owlcat.local/browse/WH-91743
        // TAA fights with shadow bias. So we have to fix camera ray somehow for jittering
        //#define DEFERRED_INTERPOLATED_POSITION_RECONSTRUCTION
#endif

#define SUPPORT_FOG_OF_WAR
// we use shared lighting function for the forward and for the deferred lighting
// so turn off reflections here and calculate it in later passes (DeferredReflections.shader)
#define _ENVIRONMENTREFLECTIONS_OFF
#define _LIGHT_LAYERS

#include "../../OwlcatShaders/ShaderLibrary/Core.hlsl"
#include "../../OwlcatShaders/ShaderLibrary/Lighting.hlsl"
#include "../../OwlcatShaders\ShaderLibrary/GBufferUtils.hlsl"

float4 CalculateDeferredLighting(uint2 positionSS, float3 cameraRay)
{
        InputData inputData;
        SurfaceData surfaceData;
        uint materialFeatures;
        uint renderingLayers;
        DecodeGBuffer(
                positionSS,
                cameraRay,
                inputData,
                surfaceData,
                materialFeatures);

        //#ifdef SUPPORT_FOG_OF_WAR
        //float fowFactor = GetFogOfWarFactor(inputData.positionWS);
        //if (fowFactor <= 0)
        //{
        //        return float4(_FogOfWarColor.rgb, 0);
        //}
        //#endif

        float4 color = 0;
        //if (!HasFlag(materialFeatures, kMaterialFlagLightingOff))
        //{
        //        // Если делать это здесь, будут однопиксельные артифакты из-за перепадов глубины/нормалей
        //        // Поэтому я перенес в GBufferUtils.hsls
        //        //surfaceData.smoothness = IsotropicNDFFiltering(inputData.normalWS, surfaceData.smoothness);

        //        bool receiveShadowsOff = HasFlag(materialFeatures, kMaterialFlagReceiveShadowsOff);
        //        bool specularHighlightsOff = HasFlag(materialFeatures, kMaterialFlagSpecularHighlightsOff);
        //        bool isTranslucent = HasFlag(materialFeatures, kMaterialFlagTranslucent);
        //        bool shadowMaskEnabled = HasFlag(materialFeatures, kMaterialFlagShadowmask);

        //        #if defined(DEBUG_DISPLAY)
        //        // In deferred path we calculate mipmap color in GBuffer pass, so disable _DebugMipMap to avoid wrong mipmap recalculating
        //        _DebugMipMap = 0;
        //        #endif

        //        color += WaaaghFragmentPBR(
        //                inputData,
        //                surfaceData,
        //                receiveShadowsOff,
        //                shadowMaskEnabled,
        //                specularHighlightsOff,
        //                isTranslucent,
        //                renderingLayers);
        //}

        FinalColorOutput(color);

        return color;
}

#endif //OWLCAT_DEFERRED_LIGHTING_INCLUDED
