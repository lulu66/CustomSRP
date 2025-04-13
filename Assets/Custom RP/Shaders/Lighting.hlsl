#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

float3 IncomingLight(Surface surface, Light light)
{
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	float nh = saturate(dot(surface.normal, h));
	float nh2 = nh * nh;
	float lh = saturate(dot(light.direction, h));
	float lh2 = lh * lh;
	float r2 = brdf.roughness * brdf.roughness;
	float d = nh2 * (r2 - 1.0) + 1.0001;
	float d2 = d * d;
	float normalization = brdf.roughness * 4.0 + 2.0;

	return r2 / (d2 * max(0.1,lh2) * normalization);
}

float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light)
{

	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf)
{
	float3 color = 0.0;

	for(int i=0; i<GetDirectionalLightCount(); i++)
	{
		color += GetLighting(surface, brdf, GetDirectionalLight(i));
	}
	return color;
}


#endif