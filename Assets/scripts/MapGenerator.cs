using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour {

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

  // So we can choose which kind of map to draw
  public enum DrawMode {NoiseMap, ColorMap, Mesh};
  public DrawMode drawMode;

  // Kind of Perlin noise to draw
  public enum PerlinType {UnityPerlin, OtherPerlin, OtherPerlin_3D};
  public PerlinType perlinType;

  // How to distort the noise
  public enum NoiseType {None, Billowy, Ridged, Worley};
  public NoiseType noseType;

  [Range(0,20)] // Creates slider in unity
  public int octaves;
  [Range(0,2)]
  public float persistence;
  [Range(0,8)]
  public float lacunarity;

  public int seed;
  public Vector3 offset;

  // How tall the heights go
  public float meshHeightMultiplier;

  // Change how the curve is between points
  // Think of clever ways to make this random at different points on a single map
  public AnimationCurve meshHeightCurve;

  public TerrianType[] regions;

  public bool autoUpdate;

  public GameObject theDuck;

  // Threading stuff
  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


  public void Start(){
    MapData mapData = GenerateMapData();
    MapDisplay display = FindObjectOfType<MapDisplay>();
    MeshData myMap = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
    display.DrawMesh(myMap, TextureGenerator.TextureFromColorMap (mapData.colorMap, mapHeight, mapWidth));

      //////////////////////////////////// TODO put this somewhere else
      // test
      int minObjects = 25;
      int maxObjects = 26;
      int minDistance = 10;
      int maxDistance = 100;
      Vector3[] meshVerts = myMap.getVertices();
      int attempts = 30;
      //
      ///////////////////////////////////
      poissonDiscSampler.placeObjects(seed, minObjects, maxObjects, minDistance, maxDistance, theDuck, meshVerts, attempts);

  }


  public void DrawMapInEditor() {
    MapData mapData = GenerateMapData ();

    MapDisplay display = FindObjectOfType<MapDisplay> ();
    if (drawMode == DrawMode.NoiseMap) {
      display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
    } else if (drawMode == DrawMode.ColorMap) {
      display.DrawTexture (TextureGenerator.TextureFromColorMap (mapData.colorMap, mapHeight, mapWidth));
    } else if (drawMode == DrawMode.Mesh) {
      MeshData myMap = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
      display.DrawMesh(myMap, TextureGenerator.TextureFromColorMap (mapData.colorMap, mapHeight, mapWidth));
    }
  }

  public void RequestMapData(Action<MapData> callback) {
    ThreadStart threadStart = delegate {
      MapDataThread (callback);
    };

    new Thread (threadStart).Start ();
  }

  void MapDataThread(Action<MapData> callback) {
    MapData mapData = GenerateMapData ();
    lock (mapDataThreadInfoQueue) {
      mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
    }
  }

  public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
    ThreadStart threadStart = delegate {
      MeshDataThread (mapData, callback);
    };

    new Thread (threadStart).Start ();
  }

  void MeshDataThread(MapData mapData, Action<MeshData> callback) {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier, meshHeightCurve); // Add level of detail here
    lock (meshDataThreadInfoQueue) {
      meshDataThreadInfoQueue.Enqueue (new MapThreadInfo<MeshData> (callback, meshData));
    }
  }

  void Update() {
    if (mapDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
        MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
        threadInfo.callback (threadInfo.parameter);
      }
    }

    if (meshDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
        MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue ();
        threadInfo.callback (threadInfo.parameter);
      }
    }
  }

	public MapData GenerateMapData() {
    int thePerlinType;
    if (perlinType == PerlinType.UnityPerlin){
      thePerlinType = 0;
    } else if (perlinType == PerlinType.OtherPerlin){
      thePerlinType = 1;
    } else {
      thePerlinType = 2;
    }

		float [,] noiseMap = Noise.GenerateNoiseMap(mapWidth,mapHeight,seed,noiseScale,octaves,persistence,lacunarity,offset,thePerlinType);
    // float [,] noiseMap = Noise.theCatNoise(mapWidth,mapHeight,resolution,octaves,persistence, lacunarity, damping, strength);

    Color[] colorMap = new Color[mapWidth * mapWidth]; // why multiple width by height

    for (int y = 0; y < mapHeight; y++){
      for (int x = 0; x < mapWidth; x++){
        float currentHeight = noiseMap[x,y];

        for (int i = 0; i < regions.Length; i++){
          // Sort the colors
          // Array.Sort<TerrianType>(regions, (v,c) => v.height.CompareTo(c.height)); // sort before going through list
          regions.OrderBy(t=>t.height);
          
          if (currentHeight <= regions[i].height) {
            colorMap[y * mapWidth + x] = regions[i].color;
          }
        }
      }
    }

    return new MapData(noiseMap, colorMap);

	}

  void OnValidate(){
    if (mapWidth <= 1){
      mapWidth = 1;
    }
    if (mapHeight <= 1){
      mapHeight = 1;
    }
    if (lacunarity <= 0){
      lacunarity = 0;
    }
    if (octaves <=1 ){
      octaves = 1;
    }
  }
  struct MapThreadInfo<T> {
    public readonly Action<T> callback;
    public readonly T parameter;

    public MapThreadInfo (Action<T> callback, T parameter)
    {
      this.callback = callback;
      this.parameter = parameter;
    }
    
  }

}

[System.Serializable] // show it shows up in the inspector
public struct TerrianType{
  public string name;
  public float height;
  public Color color;
}

public struct MapData {
  public readonly float[,] heightMap;
  public readonly Color[] colorMap;

  public MapData (float[,] heightMap, Color[] colorMap)
  {
    this.heightMap = heightMap;
    this.colorMap = colorMap;
  }

}