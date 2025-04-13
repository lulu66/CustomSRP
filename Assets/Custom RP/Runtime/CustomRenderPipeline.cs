using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
	CameraRenderer renderer = new CameraRenderer();

	bool useDynamicBatching;
	bool useGPUInstancing;
	ShadowSettings shadowSettings;

	public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
	{
		this.useDynamicBatching = useDynamicBatching;
		this.useGPUInstancing = useGPUInstancing;
		GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
		GraphicsSettings.lightsUseLinearIntensity = true;
		this.shadowSettings = shadowSettings;
	}
	//��ں�����Unityÿ֡��Ҫ����Render����
	//ScriptableRenderContext�ṩ��ԭ�����������
	//Unity���Ը�����Camera��˳����Ⱦ��ЩCameras
	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		for(int i=0; i<cameras.Length; i++)
		{
			renderer.Render(context, cameras[i], useDynamicBatching, useGPUInstancing, shadowSettings);
		}
	}


}
