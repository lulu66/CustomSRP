using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
	const string bufferName = "Shadows";
	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName,
	};

	ScriptableRenderContext context;

	CullingResults cullingResults;

	ShadowSettings shadowSettings;

	//最多支持4盏方向光产生阴影
	const int maxShadowedDirectionalLightCount = 4;

	ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

	int shadowedDirectionalLightCount = 0;

	static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
	{
		this.context = context;
		this.cullingResults = cullingResults;
		this.shadowSettings = shadowSettings;

		shadowedDirectionalLightCount = 0;
	}

	public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
	{
		if(shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f)
		{
			if(cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
			{
				shadowedDirectionalLights[shadowedDirectionalLightCount++] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
			}
		}
	}

	public void Render()
	{
		if(shadowedDirectionalLightCount > 0)
		{
			RenderDirectionalShadows();
		}
		else
		{
			buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
		}
	}

	public void Cleanup()
	{
		buffer.ReleaseTemporaryRT(dirShadowAtlasId);
		ExecuteBuffer();
	}
	void RenderDirectionalShadows()
	{
		int atlasSize = (int)shadowSettings.directional.atlasSize;
		//支持4盏方向光阴影，将shadowmap rt切分
		int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
		int tileSize = atlasSize / split;

		//RenderTextureFormat.Shadowmap : 具体的格式依赖于平台
		buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
		buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		buffer.ClearRenderTarget(true, false, Color.clear);

		buffer.BeginSample(bufferName);
		ExecuteBuffer();
		for(int i=0; i<shadowedDirectionalLightCount; i++)
		{
			RenderDirectionalShadows(i, split, tileSize);
		}
		buffer.EndSample(bufferName);
		ExecuteBuffer();

	}

	//对viewport做偏移，以便渲染多个方向光阴影
	void SetTileViewport(int index, int split, int tileSize)
	{
		Vector2 offset = new Vector2(index%split, index/split);
		buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
	}
	void RenderDirectionalShadows(int index, int split, int tileSize)
	{
		ShadowedDirectionalLight light = shadowedDirectionalLights[index];
		var shadowSetting = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
		cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);
		shadowSetting.splitData = splitData;
		SetTileViewport(index, split, tileSize);
		buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
		ExecuteBuffer();
		context.DrawShadows(ref shadowSetting); //只渲染含有shadowcaster pass的材质
	}
	void ExecuteBuffer()
	{
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	
	struct ShadowedDirectionalLight
	{
		public int visibleLightIndex;
	}
}
