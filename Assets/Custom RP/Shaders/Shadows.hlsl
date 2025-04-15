#ifndef CUSTOM_SHADOWS_INCLUDE
#define CUSTOM_SHADOWS_INCLUDE

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

//定义了特殊的贴图的filtering
TEXTURE2D_SHADOW(_DirectionalShadowAtlas)
#define SHADOW_SAMPLER sampler_linear_clamp_compare //shadow只能使用这一种采样方式
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END
#endif