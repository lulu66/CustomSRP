#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#define MIN_REFLECTIVITY 0.04

struct BRDF{
	float3 diffuse;
	float3 specular;
	float roughness;
};


float OneMinusReflectivity(float metallic)
{
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}


BRDF GetBRDF(Surface surface, bool multiplyAlpha = false)
{
	BRDF brdf;

	float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);

	if(multiplyAlpha)
	{
		brdf.diffuse = oneMinusReflectivity * surface.color * surface.alpha;
	}
	else
	{
		brdf.diffuse = oneMinusReflectivity * surface.color;
	}

	brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);

	float perceptualRoughness = PerceptualSmoothnessToRoughness(surface.smoothness);
	brdf.roughness = perceptualRoughness;

	return brdf;
}


#endif