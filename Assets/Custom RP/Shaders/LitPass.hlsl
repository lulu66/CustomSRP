#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Surface.hlsl"
#include "BRDF.hlsl"
#include "Light.hlsl"
#include "Lighting.hlsl"
#include "UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
//Ö§³ÖGPU instancing & SRP Batcher
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)


struct VertexInput
{
    float4 position : POSITION;
    float3 normalOS : NORMAL;
    float2 uv       : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD1;
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD2;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput LitPassVertex(VertexInput input)
{
    VertexOutput output;

    UNITY_SETUP_INSTANCE_ID(input);

    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float4 baseMap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.uv = input.uv * baseMap_ST.xy + baseMap_ST.zw;

    float3 positionWS = TransformObjectToWorld(input.position.xyz);
    output.positionCS = TransformWorldToHClip(positionWS);
    output.positionWS = positionWS;

    output.normalWS = TransformObjectToWorldDir(input.normalOS);

    return output;
}

float4 LitPassFragment(VertexOutput input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    float4 baseColor =  UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.color = baseColor.rgb * baseMap.rgb;
    surface.alpha = baseColor.a * baseMap.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
    surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);

    BRDF brdf;
    #if defined(_PREMULTIPLY_ALPHA)
    brdf = GetBRDF(surface, true);
    #else
    brdf = GetBRDF(surface);
    #endif

    float3 finalColor = GetLighting(surface, brdf);

    #if defined(_CLIPPING)
        clip(baseMap.r - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
    #endif
    return float4(finalColor,surface.alpha);

}

#endif
