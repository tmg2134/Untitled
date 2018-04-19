using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate_Terrain : MonoBehaviour 
{
  // Dimensions of the terrain
  public int depth  = 20;
  public int height = 256;
  public int width  = 256; 

  public float scale = 20f; // Used for Perlin noise algorithm
  // For random generatin each time in the CalculateHeight method
  public float offsetX = 100f;
  public float offsetY = 100f;

  void Start()
  {
    offsetX = Random.Range (0f, 9999f);
    offsetY = Random.Range (0f, 9999f);
  }

  void Update()
  {
    
    Terrain terrain = GetComponent<Terrain>();
    terrain.terrainData = GenerateTerrain (terrain.terrainData);

  }

  TerrainData GenerateTerrain (TerrainData terrainData)
  {
    terrainData.heightmapResolution = width + 1;

    terrainData.size = new Vector3 (width, depth, height);
    terrainData.SetHeights (0, 0, GenerateHeights ());
    return terrainData;
  }

  float[,] GenerateHeights () // function to create different heights for the terrain
  {
    float[,] heights = new float[width, height]; // Array widths and heights in the terrain.
    for (int x = 0; x < width; x++) 
    {
      for (int y = 0; y < height; y++)
      {
        heights [x, y] = CalculateHeight(x,y);
      }
    }
    return heights;
  }

  float CalculateHeight (int x, int y) // Function to create a height at a point? Unsure how this works.
  {
    float xCoordinate = (float)x / width * scale + offsetX;
    float yCoordinate = (float)y / height * scale + offsetY;

    return Mathf.PerlinNoise (xCoordinate, yCoordinate);
  }
}
