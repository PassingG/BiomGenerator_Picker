using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
	public float seed;
	public float frequency;
	public float amplitude;
}

public class NoiseMapGeneration
{
	// Create maps based on depth and width
	public static float[] GeneratePerlinNoiseMap(float scale, int gridSize, float offsetX, float offsetY, Wave[] waves)
	{
		int dataLength = gridSize * gridSize;

		float[] noiseMap = new float[dataLength];

		for (int Index = 0; Index < dataLength; Index++)
		{
				int calculateX = (int)((float)Index % (float)gridSize);
				int calculateY = (int)Mathf.Floor((float)Index / (float)gridSize);

				// calculate sample indices based on the coordinates, the scale and the offset
				float sampleX = (calculateX + offsetX) / scale;
				float sampleY = (calculateY + offsetY) / scale;

				float noise = 0f;
				float normalization = 0f;
				foreach (Wave wave in waves)
				{
					// generate noise value using PerlinNoise for a given Wave
					noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleY * wave.frequency + wave.seed);
					normalization += wave.amplitude;
				}
				// normalize the noise value so that it is within 0 and 1
				noise /= normalization;

				noiseMap[Index] = noise;
		}

		return noiseMap;
	}

	public static float[,] GenerateUniformNoiseMap(int gridSize, float centerVertexY, float maxDistanceY, float offsetY)
	{
		float[,] noiseMap = new float[gridSize, gridSize];

		for (int yIndex = 0; yIndex < gridSize; yIndex++)
		{
			float sampleY = yIndex + offsetY;

			float noise = Mathf.Abs(sampleY - centerVertexY) / maxDistanceY;

			for (int xIndex = 0; xIndex < gridSize; xIndex++)
			{
				noiseMap[xIndex, yIndex] = noise;
			}
		}

		return noiseMap;
	}
}