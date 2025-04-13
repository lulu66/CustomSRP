using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
	const string bufferName = "Lighting";
	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	CullingResults cullingResults;

	static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
	static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
	static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
	const int maxDirLightCount = 4;

	static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
	static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

	Shadows shadows = new Shadows();
	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
	{
		this.cullingResults = cullingResults;

		buffer.BeginSample(bufferName);
		shadows.Setup(context,cullingResults,shadowSettings);
		SetupLights();
		shadows.Render();
		buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	public void Cleanup()
	{
		shadows.Cleanup();
	}
	void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
	{
		dirLightColors[index] = visibleLight.finalColor;//已经包含了intensity
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
		shadows.ReserveDirectionalShadows(visibleLight.light, index);
	}

	void SetupLights()
	{
		//nativeArray可以高效地在c#托管代码和本地内存之间共享数据
		NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

		int dirLightCount = 0;
		for(int i=0; i<visibleLights.Length; i++)
		{
			var visibleLight = visibleLights[i];
			if(visibleLight.lightType != LightType.Directional)
			{
				continue;
			}
			SetupDirectionalLight(dirLightCount, ref visibleLight);
			dirLightCount++;
			if(dirLightCount>=maxDirLightCount)
			{
				break;
			}
		}
		buffer.SetGlobalInt(dirLightCountId, dirLightCount);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
	}
}
