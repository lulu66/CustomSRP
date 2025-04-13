Shader "Custom RP/Unlit"
{
    Properties
    {
        //_Color("Discard",Color) = (0,0,0,1)
        _BaseColor("Color", Color) = (1,1,1,1)
        _BaseMap("Texture", 2D) = "white"{}
        [Toggle(_CLIPPING)]_Clipping("Alpha Clipping", float) = 0
        _Cutoff("Alpha Cutoff", Range(0.0,1.0)) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", float) = 1
        [Enum(Off, 0, On, 1)]_ZWrite("Z Write", float) = 1
    }
    SubShader
    {
        
        Pass
        {
            Blend [_SrcBlend][_DstBlend]

            ZWrite [_ZWrite]

            HLSLPROGRAM

            #pragma vertex UnlitPassVertex

            #pragma fragment UnlitPassFragment

            #pragma multi_compile_instancing

            #pragma shader_feature _CLIPPING

            #pragma target 3.5
            #include "UnlitPass.hlsl"

            ENDHLSL
        }
    }
}
