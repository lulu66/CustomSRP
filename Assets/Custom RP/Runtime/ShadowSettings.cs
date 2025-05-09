using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
	public enum MapSize
	{
		_256 = 256,
		_512 = 512,
		_1024 = 1024,
		_2048 = 2048,
		_4096 = 4096,
		_8192 = 8192

	}
	[Min(0)]
	public float maxDistance = 100f;//��Ӱ��������

	public Directional directional = new Directional { atlasSize = MapSize._1024};

	[System.Serializable]
	public struct Directional
	{
		public MapSize atlasSize;
	}
}
