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

	//该函数只在编辑器里面起作用，打包之后不生效，但其他地方会引用该函数，因此写成partial函数防止编译错误
	partial void DrawUnsupportedShaders();
	partial void DrawGizmos();
	partial void PrepareForSceneWindow();

	partial void PrepareBuffer();

#if UNITY_EDITOR

	string SampleName { get; set; }
	//editor下会产生GC
	partial void PrepareBuffer()
	{
		//Profiler用于在Profiler中定位信息产生的代码来源
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
		//使scene视图显示UI
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
		//不支持的shader pass类型将以error material方式绘制
		var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
		{
			overrideMaterial = errorMaterial
		};

		var filterSettings = FilteringSettings.defaultValue;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
	}
#endif
}
