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

	//ȷ������shader pass���Ա���Ⱦ
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

	//ʹ�øú����ж������Ƿ�ȫ�����ü��������ü����Ͳ���������Ⱦ
	bool Cull(float maxShadowDistance)
	{
		if(camera.TryGetCullingParameters(out ScriptableCullingParameters p))
		{
			//������Ⱦ��Ӱ��������
			p.shadowDistance = Mathf.Min(maxShadowDistance,camera.farClipPlane);
			//�ü��ɹ������Է��ʲü����
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}
	//����������Դ���context�У�������Ƶ����岻����camera��Ӱ�죬�ú���������ͼ-ͶӰ����.
	void Setup()
	{
		//setup camera��clear rendertarget˳���ܱ䣬���ܸ�Ч����rt
		context.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		//�������rt��Ҫ�չ˵������������ã�first camera��clear flag�϶���color����skybox����ô�϶�Ҫ������Ⱥ���ɫ�����һ��camera�ǲ�������color�ģ�����Ҫ��������Ա��ڻ��Ƹ��������Ʒ,�ۺ���Ҫ����ı��ʽ
		buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags==CameraClearFlags.Color, flags == CameraClearFlags.Color?camera.backgroundColor.linear:Color.clear);
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
	}

	void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
	{
		//���Ƽ�����
		var sortingSettings = new SortingSettings(camera)
		{
			//front to back order
			criteria = SortingCriteria.CommonOpaque
		};
		//һЩ��Ⱦ������
		var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
		{
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing,
			
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);
		//all:������Ⱦ���ж�����Ⱦ
		//�Ȼ��Ʋ�͸������
		var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
		//���������
		context.DrawSkybox(camera);

		//���ư�͸������
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

	}


	//��������͸�context������ᱻ���棬��Ҫʹ��submit��ִ����Щ����
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
