#ifndef SURFACE_INCLUDE
#define SURFACE_INCLUDE

struct Surface{
	float3 normal;
	float3 viewDirection;
	float3 color;
	float alpha;
	float metallic;
	float smoothness;
};
#endif