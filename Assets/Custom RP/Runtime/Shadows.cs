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
	static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

	static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];
	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
	{
		this.context = context;
		this.cullingResults = cullingResults;
		this.shadowSettings = shadowSettings;

		shadowedDirectionalLightCount = 0;
	}

	public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
	{
		if(shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f)
		{
			if(cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
			{
				shadowedDirectionalLights[shadowedDirectionalLightCount] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };

				return new Vector2(light.shadowStrength, shadowedDirectionalLightCount++);
			}
		}
		return Vector2.zero;
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
		buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

		buffer.EndSample(bufferName);
		ExecuteBuffer();

	}

	//对viewport做偏移，以便渲染多个方向光阴影
	Vector2 SetTileViewport(int index, int split, int tileSize)
	{
		Vector2 offset = new Vector2(index%split, index/split);
		buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
		return offset;
	}
	void RenderDirectionalShadows(int index, int split, int tileSize)
	{
		ShadowedDirectionalLight light = shadowedDirectionalLights[index];
		var shadowSetting = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
		cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);
		shadowSetting.splitData = splitData;

		//需要计算atlas shadow的矩阵
		dirShadowMatrices[index] = ConvertToAtlasMatrix(projMatrix * viewMatrix, SetTileViewport(index, split, tileSize),split);

		buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);

		ExecuteBuffer();
		context.DrawShadows(ref shadowSetting); //只渲染含有shadowcaster pass的材质
	}

	Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
	{
		//判断是否反转z值
		if(SystemInfo.usesReversedZBuffer)
		{
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;

		}

		float scale = 1f / split;
		m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
		m.m20 = 0.5f * (m.m20 + m.m30);
		m.m21 = 0.5f * (m.m21 + m.m31);
		m.m22 = 0.5f * (m.m22 + m.m32);
		m.m23 = 0.5f * (m.m23 + m.m33);

		return m;
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
