using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IslandGeneratorScript : MonoBehaviour
{
    [SerializeField]
    private Vector2Int mapSize;

    [SerializeField]
    private int terrainMapNoiseSeed, moistureMapNoiseSeed;

    [SerializeField]
    private Texture2D IslandGraph;

    private PerlinNoiseGenerator terrainNoiseGenerator, moistureNoiseGenreator;

#if UNITY_EDITOR
    void OnValidate()
    {
        InitializeMap();
    }
#endif

    private void Start()
    {
        InitializeMap();
    }

    private void InitializeMap()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        terrainMapNoiseSeed = Random.Range(-99999, 99999);
        moistureMapNoiseSeed = Random.Range(-99999, 99999);

        terrainNoiseGenerator = new PerlinNoiseGenerator(terrainMapNoiseSeed);
        moistureNoiseGenreator = new PerlinNoiseGenerator(moistureMapNoiseSeed);
        moistureNoiseGenreator.octaves = 10;
        moistureNoiseGenreator.lacunarity = 1.2f;
        moistureNoiseGenreator.persistance = 0.3f;
        moistureNoiseGenreator.scale = 75;

        float[,] terrainNoiseMap = terrainNoiseGenerator.GenerateNoiseMap(true, mapSize.x, mapSize.y);
        float[,] moistureNoiseMap = moistureNoiseGenreator.GenerateNoiseMap(false, mapSize.x, mapSize.y);

        sr.sprite = GenerateIslandTerrain(terrainNoiseMap, moistureNoiseMap);
        //sr.sprite = GenerateSpriteFromNoiseMap(terrainNoiseMap);
    }

    private float islandMax = float.MinValue;
    private float islandMin = float.MaxValue;
    private Sprite GenerateIslandTerrain(float[,] terrainNoiseMap, float[,] moistureNoiseMap)
    {
        if (terrainNoiseMap.GetLength(0) != moistureNoiseMap.GetLength(0) || terrainNoiseMap.GetLength(1) != moistureNoiseMap.GetLength(1))
        {
            Debug.Log("Noise map sizes don't match.");
            return null;
        }

        int xSize = terrainNoiseMap.GetLength(0);
        int ySize = terrainNoiseMap.GetLength(1);

        Texture2D terrainTexture = new Texture2D(xSize, ySize);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                float sample = terrainNoiseMap[x, y];

                islandMin = sample < islandMin ? sample : islandMin;
                islandMax = sample > islandMax ? sample : islandMax;

            }
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                terrainNoiseMap[x, y] = Mathf.InverseLerp(islandMin, islandMax, terrainNoiseMap[x, y]);

                int islandGraphX = Mathf.RoundToInt(moistureNoiseMap[x, y] * IslandGraph.width);
                int islandGraphY = Mathf.RoundToInt(terrainNoiseMap[x, y] * IslandGraph.height);

                terrainTexture.SetPixel(x, y, IslandGraph.GetPixel(islandGraphX, islandGraphY));

            }

        }

        terrainTexture.filterMode = FilterMode.Point;
        terrainTexture.Apply();
        return Sprite.Create(terrainTexture, new Rect(0, 0, xSize, ySize), Vector2.one * 0.5f, 24);
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
