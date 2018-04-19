using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TextureGenerator {

  public const int NM_WIDTH = 0;
  public const int NM_LENGTH = 1;
  public static Gradient coloring;


  public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height){
    Texture2D texture = new Texture2D(width, height);
    texture.filterMode = FilterMode.Point; // instead of bilinear(default)
    texture.wrapMode = TextureWrapMode.Clamp;
    // texture.SetPixel(height, width, coloring.Evaluate(sample));
    texture.SetPixels(colorMap);
    texture.Apply();
    return texture;
  }

  public static Texture2D TextureFromHeightMap(float[,] heightMap){
    int width = heightMap.GetLength(NM_LENGTH);
    int height = heightMap.GetLength(NM_WIDTH);

    Color [] colorMap = new Color[width * height];
    for (int y = 0; y < height; y++){
      for (int x = 0; x < width; x++){
        colorMap[y * width + x]  = Color.Lerp(Color.black, Color.white, heightMap[x,y]);
      }
    }
    return TextureFromColorMap(colorMap, width, height);
  }
}
