using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {

  public const int NM_WIDTH = 0;
  public const int NM_LENGTH = 1;

  // note: float heightMultiplier, AnimationCurve heightCurve moved to MeshData

  // Generate the Mesh
  public static MeshData GenerateTerrainMesh(float[,] heightMap){
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

        // note: heightCurve.Evaluate(heightMap[i,j]) * heightMultiplier -> heightMap[i, j]
        // put heightCurve and multiplier in MeshData class to evaluate later

        meshData.vertices[vertexIndex] = new Vector3(topLeftX + j, heightMap[i,j], topLeftZ - i);
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

  private Mesh mesh;

  public MeshData(int meshWidth, int meshHeight){
    vertices = new Vector3[meshWidth * meshHeight];
    triangles = new int[(meshWidth - 1) * (meshHeight - 1)*6];
    // tell each vertex where it is in relation to the map
    uvs = new Vector2[meshWidth * meshHeight];
    mesh = new Mesh();
  }

  public void addTriangle(int a, int b, int c) {
    triangles[triangleIndex] = a;
    triangles[triangleIndex+1] = b; 
    triangles[triangleIndex+2] = c;
    triangleIndex += 3;
  }

  public Mesh createMesh(){
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();
    return mesh;
  }

  public Vector3[] getVertices(){
    return vertices;
  }

  // Raise up the mesh; Apply the height curve and multiplier
  public void raiseVerts(float heightMultiplier, AnimationCurve heightCurve, int height){
    for(int i=0; i < vertices.Length; i++){
      vertices[i].y = heightCurve.Evaluate(vertices[i].y) * heightMultiplier;
    }
    mesh.vertices = vertices;
    mesh.RecalculateNormals();
  }

}