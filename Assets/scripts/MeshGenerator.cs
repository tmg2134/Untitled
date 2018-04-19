using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {

  public const int NM_WIDTH = 0;
  public const int NM_LENGTH = 1;

  public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve){
    int height = heightMap.GetLength(NM_LENGTH);
    int width = heightMap.GetLength(NM_WIDTH);
    int vertexIndex = 0;
    // set middle
    float topLeftX = (width-1) / -2f;
    float topLeftZ = (height-1) / 2f;
   
    MeshData meshData = new MeshData(width, height);

    // loop through the height map
    for (int i=0; i < height; i++){
      for (int j=0; j < width; j++){

        meshData.vertices [vertexIndex] = new Vector3(topLeftX + j, heightCurve.Evaluate(heightMap[i,j]) * heightMultiplier, topLeftZ - i);
        meshData.uvs[vertexIndex] = new Vector2(i/(float)height,j/(float)width);

        if (i < width -1 && j < height - 1){

          // creating triangle in array
          // Mesh's are made from triangle
          // Connect points on map to create a triangle
          meshData.addTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
          meshData.addTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1); 
        }

        vertexIndex++;
      }

    }
    return meshData;
  }
}

public class MeshData{
  public Vector3[] vertices;  
  public int[] triangles;
  public Vector2[] uvs;

  int triangleIndex;

  public MeshData(int meshWidth, int meshHeight){
    vertices = new Vector3[meshWidth * meshHeight];
    triangles = new int[(meshWidth - 1) * (meshHeight - 1)*6];
    // tell each vertex where it is in relation to the map
    uvs = new Vector2[meshWidth * meshHeight];
  }

  public void addTriangle(int a, int b, int c) {
    triangles[triangleIndex] = a;
    triangles[triangleIndex+1] = b; 
    triangles[triangleIndex+2] = c;
    triangleIndex += 3;
  }

  public Mesh createMesh(){
    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();
    return mesh;
  }
}