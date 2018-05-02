using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
  
	public Renderer textureRenderer;
  public MeshFilter meshFilter;
  public MeshRenderer meshRenderer;
  public MeshCollider meshCollider;


  // Original
	// public void DrawNoiseMap(float[,] noiseMap) {
  // int width = noiseMap.GetLength(NM_LENGTH);
  //   int height = noiseMap.GetLength(NM_WIDTH);

  //   Texture2D texture = new Texture2D (width, height);

  //   Color [] colorMap = new Color[width * height];
  //   for (int y = 0; y < height; y++){
  //     for (int x = 0; x < width; x++){
  //       colorMap[y * width + x]  = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
  //     }
  //   }
  //   texture.SetPixels(colorMap);
  //   texture.Apply();


  public void DrawTexture(Texture2D texture) {
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3(texture.width,1,texture.height);
	}

  public void DrawMesh(MeshData meshData, Texture2D texture){
    meshFilter.sharedMesh = meshData.createMesh();
    meshRenderer.sharedMaterial.mainTexture = texture;
    meshCollider.sharedMesh = null;
    meshCollider.sharedMesh = meshFilter.sharedMesh;
  }
}
