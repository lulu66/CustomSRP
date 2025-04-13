using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
public partial class CameraRenderer
{
	static ShaderTagId[] legacyShaderTagIds =
	{
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("Always"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
	};

	static Material errorMaterial;

	//�ú���ֻ�ڱ༭�����������ã����֮����Ч���������ط������øú��������д��partial������ֹ�������
	partial void DrawUnsupportedShaders();
	partial void DrawGizmos();
	partial void PrepareForSceneWindow();

	partial void PrepareBuffer();

#if UNITY_EDITOR

	string SampleName { get; set; }
	//editor�»����GC
	partial void PrepareBuffer()
	{
		//Profiler������Profiler�ж�λ��Ϣ�����Ĵ�����Դ
		Profiler.BeginSample("Editor Only");
		buffer.name = SampleName = camera.name;
		Profiler.EndSample();
	}
#else
	
	String SampleName => bufferName;
#endif

#if UNITY_EDITOR
	partial void PrepareForSceneWindow()
	{
		//ʹscene��ͼ��ʾUI
		if(camera.cameraType == CameraType.SceneView)
		{
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
	}


	partial void DrawGizmos()
	{
		if(Handles.ShouldRenderGizmos())
		{
			context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
			context.DrawGizmos(camera, GizmoSubset.PostImageEffects);

		}
	}
	partial void DrawUnsupportedShaders()
	{
		if (errorMaterial == null)
		{
			errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
		}
		//��֧�ֵ�shader pass���ͽ���error material��ʽ����
		var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
		{
			overrideMaterial = errorMaterial
		};

		var filterSettings = FilteringSettings.defaultValue;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
	}
#endif
}
