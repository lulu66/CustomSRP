using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class CustomShaderGUI_Lit : ShaderGUI
{

	MaterialEditor editor;
	Object[] materials;//可以一次性编辑多个材质球
	MaterialProperty[] properties;

	bool showPreset;
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		base.OnGUI(materialEditor, properties);
		editor = materialEditor;
		materials = materialEditor.targets;
		this.properties = properties;

		EditorGUILayout.Space();
		showPreset = EditorGUILayout.Foldout(showPreset, "Presets", true);
		if(showPreset)
		{
			OpaquePreset();
			ClipPreset();
			FadePreset();
			TransparentPreset();
		}
	}

	void SetProperty(string name, float value)
	{
		FindProperty(name,properties).floatValue = value;
	}

	void SetKeyword(string keyword, bool enabled)
	{
		if(enabled)
		{
			foreach(Material material in materials)
			{
				material.EnableKeyword(keyword);
			}
		}
		else
		{
			foreach (Material material in materials)
			{
				material.DisableKeyword(keyword);
			}

		}
	}

	//关键字开关
	void SetProperty(string name, string keyword, bool value)
	{
		SetProperty(name, value ? 1.0f : 0.0f);
		SetKeyword(keyword, value);
	}

	bool Clipping
	{
		set => SetProperty("_Clipping", "_CLIPPING", value);
	}

	bool PremultiplyAlpha
	{
		set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value);
	}

	BlendMode SrcBlend
	{
		set => SetProperty("_SrcBlend", (float)value);
	}

	BlendMode DstBlend
	{
		set => SetProperty("_DstBlend", (float)value);
	}
	bool ZWrite
	{
		set => SetProperty("_ZWrite",value?1.0f:0.0f);
	}

	RenderQueue RenderQueue
	{
		set
		{
			foreach(Material material in materials)
			{
				material.renderQueue = (int)value;
			}
		}
	}

	bool PresetButton(string name)
	{
		if(GUILayout.Button(name))
		{
			editor.RegisterPropertyChangeUndo(name);
			return true;
		}
		return false;
	}

	void OpaquePreset()
	{
		if(PresetButton("Opaque"))
		{
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.Geometry;
		}
	}

	void ClipPreset()
	{
		if (PresetButton("Clip"))
		{
			Clipping = true;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.AlphaTest;
		}
	}

	void FadePreset()
	{
		if (PresetButton("Fade"))
		{
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.SrcAlpha;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}

	void TransparentPreset()
	{
		if (PresetButton("Transparent"))
		{
			Clipping = false;
			PremultiplyAlpha = true;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}
}
