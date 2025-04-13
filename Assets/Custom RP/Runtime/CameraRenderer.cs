using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
	ScriptableRenderContext context;
	Camera camera;

	Lighting lighting = new Lighting();

	const string bufferName = "Camera Render";
	CommandBuffer buffer = new CommandBuffer();

	CullingResults cullingResults;

	//确定何种shader pass可以被渲染
	static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
	static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");
	public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
	{
		this.context = context;
		this.camera = camera;
		PrepareBuffer();
		PrepareForSceneWindow();
		if (!Cull(shadowSettings.maxDistance))
		{
			return;
		}
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
		lighting.Setup(context, cullingResults,shadowSettings);
		buffer.EndSample(SampleName);
		Setup();
		DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
		DrawUnsupportedShaders();
		DrawGizmos();
		lighting.Cleanup();
		Submit();
	}

	//使用该函数判断物体是否全部被裁剪掉，被裁剪掉就不做后续渲染
	bool Cull(float maxShadowDistance)
	{
		if(camera.TryGetCullingParameters(out ScriptableCullingParameters p))
		{
			//设置渲染阴影的最大距离
			p.shadowDistance = Mathf.Min(maxShadowDistance,camera.farClipPlane);
			//裁剪成功，可以访问裁剪结果
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}
	//将相机的属性传入context中，否则绘制的物体不会受camera的影响，该函数设置视图-投影矩阵.
	void Setup()
	{
		//setup camera和clear rendertarget顺序不能变，才能高效清理rt
		context.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		//如何清理rt需要照顾到多个相机的设置，first camera的clear flag肯定是color或者skybox，那么肯定要清理深度和颜色；最后一个camera是不用清理color的，但是要清理深度以便于绘制该相机的物品,综合需要下面的表达式
		buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags==CameraClearFlags.Color, flags == CameraClearFlags.Color?camera.backgroundColor.linear:Color.clear);
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
	}

	void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
	{
		//绘制几何体
		var sortingSettings = new SortingSettings(camera)
		{
			//front to back order
			criteria = SortingCriteria.CommonOpaque
		};
		//一些渲染的设置
		var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
		{
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing,
			
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);
		//all:所有渲染队列都被渲染
		//先绘制不透明物体
		var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
		//绘制天空球
		context.DrawSkybox(camera);

		//绘制半透明物体
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

	}


	//所有命令发送给context的命令都会被缓存，需要使用submit来执行这些工作
	void Submit()
	{
		buffer.EndSample(SampleName);
		ExecuteBuffer();
		context.Submit();
	}

	void ExecuteBuffer()
	{
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}
}
