Shader "Custom RP/Lit"
{
    Properties
    {
        //_Color("Discard",Color) = (0,0,0,1)
        _BaseColor("Color", Color) = (1,1,1,1)
        _BaseMap("Texture", 2D) = "white"{}
        [HideInInspector][Toggle(_CLIPPING)]_Clipping("Alpha Clipping", float) = 0
        _Cutoff("Alpha Cutoff", Range(0.0,1.0)) = 0.5
        //PBR
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        [HideInInspector][Toggle(_PREMULTIPLY_ALPHA)]_PremultiplyAlpha("Premultiply Alpha", float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", float) = 1
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWrite("Z Write", float) = 1
    }
    SubShader
    {
        
        Pass
        {
            Tags {"LightMode" = "CustomLit"}

            Blend [_SrcBlend][_DstBlend]

            ZWrite [_ZWrite]

            HLSLPROGRAM

            #pragma vertex LitPassVertex

            #pragma fragment LitPassFragment

            #pragma multi_compile_instancing

            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLY_ALPHA

            #pragma target 3.5
            #include "LitPass.hlsl"

            ENDHLSL
        }
        pass
        {
            Tags{"LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM

            #pragma vertex ShadowCasterPassVertex

            #pragma fragment ShadowCasterPassFragment

            #pragma multi_compile_instancing
            
            #pragma shader_feature _CLIPPING

            #pragma target 3.5

            #include "ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "CustomShaderGUI_Lit"
}
