using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

  [Range(1, 3)]
  // public static int dimensions = 3;
  public static NoiseMethodType type;
  public const int MIN_RANDOM_RANGE = -100000;
  public const int MAX_RANDOM_RANGE = 100000;
  public static int[] perlinGradTable = { 
                                        245, 176,
                                        176, 245,
                                        79, 245,
                                        10, 176,
                                        10, 79,
                                        79, 10,
                                        176, 10,
                                        245, 79};

  public static int[] perm={151,160,137,91,90,15, 
    131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
    190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
    88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,
    77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
    102,143,54,65,25,63,161,255,216,80,73,209,76,132,187,208,89,18,169,200,196,
    135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,
    5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
    223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,
    129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,228,
    251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,
    49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
    138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,151};                                          

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector3 octaveOffsets, int thePerlinType){
		float [,] noiseMap = new float[mapWidth, mapHeight];

    //Create random seed for different maps each time



    if (scale <= 0){
      scale = 0.0001f;
    }

    // float centerMapHeight = mapHeight / 2f;
    // float centerMapWidth = mapWidth / 2f;

    float maxNoiseHeight = float.MinValue;
    float minNoiseHeight = float.MaxValue;

    float noiseHeight = 0;

    // Loop through range of the Map 
    for (int y=0; y < mapHeight; y++){
			for (int x=0; x < mapWidth; x++){

      // Layers of noise

      // IQ domain warp
        // Warp 1 normal Octaves
        // float domainWarpx = fBM(x,y, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        // float domainWarpy = fBM(x + 5.2f, y+ 1.3f, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);

        // Warp 1 with less octaves
        float domainWarpx = fBM(x,y, seed, 3, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        float domainWarpy = fBM(x + 5.2f, y+ 1.3f, seed, 3, lacunarity, scale, persistence, octaveOffsets, thePerlinType);

        // Warp 2
        float domainWarpxx = fBM(x + 4f*domainWarpx + 1.7f,y + 4f*domainWarpy + 9.2f, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        float domainWarpyy = fBM(x + 4f*domainWarpx + 8.3f,y + 4f*domainWarpy + 2.8f, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        // noiseHeight = fBM(x + 4f*domainWarpxx,y + 4f*domainWarpyy, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);

        // Warp 2 less octaves
        // float domainWarpxx = fBM(x + 4f*domainWarpx + 1.7f,y + 4f*domainWarpy + 9.2f, seed, 4, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        // float domainWarpyy = fBM(x + 4f*domainWarpx + 8.3f,y + 4f*domainWarpy + 2.8f, seed, 4, lacunarity, scale, persistence, octaveOffsets, thePerlinType);

        // Extra warp
        float LastWarp01 = fBM(x + 4f*domainWarpxx + 6.3f,y + 4f*domainWarpyy + 1.3f, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        float LastWarp02 = fBM(x + 4f*domainWarpxx + 2.3f,y + 4f*domainWarpyy + 8.1f, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);
        noiseHeight = fBM(x + 4f*LastWarp01,y + 4f*LastWarp02, seed, octaves, lacunarity, scale, persistence, octaveOffsets, thePerlinType);



        // Ridged
        // noiseHeight = 1f-Mathf.Abs(noiseHeight);

        // Og
        // noiseHeight = fBM(x,y, seed, octaves, lacunarity, scale, persistence, octaveOffsets);
        
        if (noiseHeight > maxNoiseHeight){
          maxNoiseHeight = noiseHeight;
        } else if (noiseHeight < minNoiseHeight){
          minNoiseHeight = noiseHeight;
        }
        noiseMap[x,y] = (noiseHeight);
      }
		}

    // Normalize noiseMap for no negative values 
    for (int y=0; y < mapHeight; y++){
      for (int x=0; x < mapWidth; x++){
        noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
      }
    }

    return noiseMap;
	}

  public static float fBM(float x, float y, int seed, int octaves, float lacunarity, float scale, float persistence, Vector3 octaveOffsets, int thePerlinType){
    float amplitude = 1f;
    float frequency = 1.95f;
    float noiseHeight = 0;

    System.Random prng = new System.Random(seed);
    Vector3[] offsets = new Vector3[octaves];

    for (int i = 0; i < octaves; i++){
      float offsetX = prng.Next(MIN_RANDOM_RANGE, MAX_RANDOM_RANGE) + octaveOffsets.x;
      float offsetY = prng.Next(MIN_RANDOM_RANGE, MAX_RANDOM_RANGE) + octaveOffsets.y;
      float offsetZ = prng.Next(MIN_RANDOM_RANGE, MAX_RANDOM_RANGE) + octaveOffsets.z;
      offsets[i] = new Vector3(offsetX, offsetY, offsetZ);
    }
    // int initialPerlin = thePerlinType;
    for (int i = 0; i < octaves; i++){
      //   Uncommment these for Original
      float sampleX = (x / scale * frequency + offsets[i].x);
      float sampleY = (y / scale * frequency + offsets[i].y);
      float sampleZ = (x / scale * frequency + offsets[i].z);

      // if (i < 3){
      //   thePerlinType = 0;
      // }else{
      //   thePerlinType = initialPerlin; 
      // }
      // The (* 2 - 1) changes to -1 to 1 instead of 0 1 for more interesting values? 
      //Might think about this more later.
      
      float perlinValue = 0.00001f;

      // Unity PerlinNoise
      if (thePerlinType == 0){
        perlinValue = (Mathf.PerlinNoise(sampleX,sampleY));
      }else if(thePerlinType == 1){
        perlinValue =  perlinNoise(sampleX, sampleY);
      }else{
        perlinValue = (perlinNoise(sampleX, sampleY, sampleZ) );
      }
      
      // Billowy
      // if (i > (i / 3)){
      //   perlinValue = Mathf.Abs(perlinValue);
      // }

      // Ridged
      // if (i > (i / 2)){
      //   perlinValue = 1f-Mathf.Abs(perlinValue);
      // }
      perlinValue = 1f-Mathf.Abs(perlinValue);

      // float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1; // Original
      // float perlinValue = 1.0f-Mathf.Abs(Mathf.PerlinNoise(sampleX, sampleY) * 2 -1); // Ridged

      //noiseMap[x,y] = perlinValue; Instead of setting directly perlinValue, Increase noiseHeight by the perlinValue of each octave.
      noiseHeight += perlinValue * amplitude;

      amplitude *= persistence; // Decreases each octave
      frequency *= lacunarity; // frequency increases each octave
      // scale += amplitude;
    }
    return noiseHeight;
  }

  // catlikecoding Noise
  public static float[,] theCatNoise (int mapWidth, int mapHeight, float resolution, int octaves, float persistence, float lacunarity, bool damping, float strength) {

    float frequency = 1f;
    Vector3 offset = new Vector3(1f,2f,3f);
    Vector3 rotation = new Vector3(45f,45f,0f);
    float [,] noiseMap = new float[mapWidth, mapHeight];

    Quaternion q = Quaternion.Euler(rotation);
    // Quaternion qInv = Quaternion.Inverse(q);
    Vector3 point00 = q * new Vector3(-0.5f, -0.5f) + offset;
    Vector3 point10 = q * new Vector3( 0.5f, -0.5f) + offset;
    Vector3 point01 = q * new Vector3(-0.5f, 0.5f) + offset;
    Vector3 point11 = q * new Vector3( 0.5f, 0.5f) + offset;

    // NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
    float stepSize = 1f / mapHeight;
    float amplitude = damping ? strength / frequency : strength;
    for (int v = 0, y = 0; y < mapWidth; y++) {
      Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
      Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);
      for (int x = 0; x < mapHeight; x++, v++) {
        Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
        NoiseSample sample = catNoise.Sum3D(point, frequency, octaves, lacunarity, persistence);
        sample = sample - 0.5f;
        sample *= amplitude;
        // sample = type == NoiseMethodType.Value ? (sample - 0.5f) : (sample * 0.5f);
        // if (coloringForStrength) {
        //   colors[v] = coloring.Evaluate(sample.value + 0.5f);
        //   sample *= amplitude;
        // }
        // else {
        //   sample *= amplitude;
        //   colors[v] = coloring.Evaluate(sample.value + 0.5f);
        // }
        //vertices[v].y = 1f-Mathf.Abs(sample.value);
        //sample.derivative = qInv * sample.derivative;
        // if (analyticalDerivatives) {
        //   normals[v] = new Vector3(-sample.derivative.x, 1f, -sample.derivative.y).normalized;
        // }
        noiseMap[y,x] = sample.value;
      }
    }
        
    // Ridged
    // sample.value = 1f-Mathf.Abs(sample.value);

    // sample.derivative = qInv * sample.derivative;
     return noiseMap;
  }

  // github.com/keijiro/PerlinNoise/blob/master/Assets/Perlin.cs

  public static float perlinNoise(float x, float y){
    var X = Mathf.FloorToInt(x) & 0xff;
    var Y = Mathf.FloorToInt(y) & 0xff;
    x -= Mathf.FloorToInt(x);
    y -= Mathf.FloorToInt(y);
    var u = Fade(x);
    var v = Fade(y);
    var A = (perm[X  ] + Y) & 0xff;
    var B = (perm[X + 1] + Y) & 0xff;
    return Lerp(v, Lerp(u, Grad(perm[A  ], x, y  ), 
                   Grad(perm[B  ], x-1, y  )),
                   Lerp(u, Grad(perm[A+ 1], x, y-1), 
                   Grad(perm[B + 1], x-1, y-1)));
  }

  public static float perlinNoise(float x, float y, float z)
  {
    var X = Mathf.FloorToInt(x) & 0xff;
    var Y = Mathf.FloorToInt(y) & 0xff;
    var Z = Mathf.FloorToInt(z) & 0xff;
    x -= Mathf.Floor(x);
    y -= Mathf.Floor(y);
    z -= Mathf.Floor(z);
    var u = Fade(x);
    var v = Fade(y);
    var w = Fade(z);
    var A  = (perm[X  ] + Y) & 0xff;
    var B  = (perm[X+1] + Y) & 0xff;
    var AA = (perm[A  ] + Z) & 0xff;
    var BA = (perm[B  ] + Z) & 0xff;
    var AB = (perm[A+1] + Z) & 0xff;
    var BB = (perm[B+1] + Z) & 0xff;
    return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA  ], x, y  , z  ), Grad(perm[BA  ], x-1, y  , z  )),
                           Lerp(u, Grad(perm[AB  ], x, y-1, z  ), Grad(perm[BB  ], x-1, y-1, z  ))),
                   Lerp(v, Lerp(u, Grad(perm[AA+1], x, y  , z-1), Grad(perm[BA+1], x-1, y  , z-1)),
                           Lerp(u, Grad(perm[AB+1], x, y-1, z-1), Grad(perm[BB+1], x-1, y-1, z-1))));
  } 

  // 6t5 - 15t4 + 10t3. It's derivative is 30t4 - 60t3 + 30t2
  private static float SmoothDerivative (float t) {
    return 30f * t * t * (t * (t - 2f) + 1f);
  }

  private static float Fade(float t)
  {
      return t * t * t * (t * (t * 6 - 15) + 10);
  }

  private static float Lerp(float t, float a, float b)
  {
      return a + t * (b - a);
  }

  private static float Grad(int hash, float x)
  {
      return (hash & 1) == 0 ? x : -x;
  }

  private static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }

  private static float Grad(int hash, float x, float y, float z)
  {
      var h = hash & 15;
      var u = h < 8 ? x : y;
      var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
      return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
  }
}
