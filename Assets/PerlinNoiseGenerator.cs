using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseGenerator
{
    private Vector2 offset;

    public int seed = 0;
    public int octaves = 6;

    public float scale = 1750f;
    public float lacunarity = 1.7f;
    public float persistance = 2f;

    public PerlinNoiseGenerator()
    {
        seed = Random.Range(-999999, 999999);
    }

    public PerlinNoiseGenerator(int Seed)
    {
        seed = Seed;
    }

    public float[,] GenerateNoiseMap(bool constrainCircular, int mapWidth, int mapHeight)
    {
        float[,] perlinNoiseMap = GenerateNoiseMapMatrix(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset);

        if (constrainCircular)
        {
            float[,] squareGradient = GenerateSquareGradient(mapWidth, mapHeight);
            float[,] combinedMap = SubtractNoiseMaps(perlinNoiseMap, squareGradient);

            return combinedMap;
        }

        return perlinNoiseMap;
    }

    public float[,] GenerateNoiseMapMatrix(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    float subtractMin = float.MaxValue;
    float subtractMax = float.MinValue;
    private float[,] SubtractNoiseMaps(float[,] a, float[,] b)
    {   
        //returns a - b
        if(a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
        {
            Debug.Log("Noise maps to subtract must have the same amount of rows and columns.");
        }

        int xSize = a.GetLength(0);
        int ySize = a.GetLength(1);
        float[,] subtractedNoiseMap = new float[xSize, ySize];

        for(int x = 0; x < xSize; x++)
        {
            for(int y = 0; y < ySize; y++)
            {
                float s = a[x, y] - b[x, y];

                subtractedNoiseMap[x, y] = s;

                subtractMin = s < subtractMin ? s : subtractMin;
                subtractMax = s > subtractMax ? s : subtractMax;

            }
        }


        return subtractedNoiseMap;
    }

    private float[,] GenerateSquareGradient(int xSize, int ySize)
    {
        int halfWidth = xSize / 2;
        int halfHeight = ySize / 2;

        float[,] gradient = new float[xSize, ySize];

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                int x = i;
                int y = j;

                float colorValue;

                x = x > halfWidth ? xSize - x : x;
                y = y > halfHeight ? ySize - y : y;

                int smaller = x < y ? x : y;
                colorValue = smaller / (float)halfWidth;

                colorValue = (1 - colorValue);
                colorValue *= colorValue * colorValue;
                gradient[i, j] = colorValue;
            }
        }

        return gradient;
    }

    private Texture2D texture;
    public Sprite GenerateSpriteFromNoiseMap(float[,] noiseMap)
    {
        texture = new Texture2D(noiseMap.GetLength(0), noiseMap.GetLength(1));

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float sample = noiseMap[x, y];
                Color color = new Color(sample, sample, sample);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, noiseMap.GetLength(0), noiseMap.GetLength(1)), Vector2.one * 0.5f);
    }

}