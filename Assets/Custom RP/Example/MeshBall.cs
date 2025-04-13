using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
	static int cutoffId = Shader.PropertyToID("_Cutoff");
	static int metallicId = Shader.PropertyToID("_Metallic");
	static int smoothnessId = Shader.PropertyToID("_Smoothness");


	Matrix4x4[] transformMatrix = new Matrix4x4[1023];
    Vector4[] baseColors = new Vector4[1023];
	float[] metallics = new float[1023];
	float[] smoothnesses = new float[1023];

    MaterialPropertyBlock block;

    [SerializeField]
    Mesh mesh = default;

    [SerializeField]
    Material material = default;


	[SerializeField]
	[Range(0, 1)]
	float alphaCutOff = 0.5f, metallic = 0f, smoothness = 0.5f;

	private void Awake()
	{
	}
	void Start()
    {

    }

    void Update()
    {
		if (block == null)
		{
			block = new MaterialPropertyBlock();
			for (int i = 0; i < transformMatrix.Length; i++)
			{
				transformMatrix[i] = Matrix4x4.TRS(UnityEngine.Random.insideUnitSphere * 10f, quaternion.identity, Vector3.one);
				baseColors[i] = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);

				metallics[i] = UnityEngine.Random.Range(0.1f,0.9f);
				smoothnesses[i] = UnityEngine.Random.Range(0.05f,0.95f);

			}
			block.SetVectorArray(baseColorId, baseColors);
			block.SetFloat(cutoffId, 0.5f);
			block.SetFloatArray(metallicId, metallics);
			block.SetFloatArray(smoothnessId, smoothnesses);

		}
		Graphics.DrawMeshInstanced(mesh, 0, material, transformMatrix, 1023,block);
	}
}
