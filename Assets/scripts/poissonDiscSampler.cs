// Thomas Goodman
// May 1, 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

TODO

Might need a way to check if an object is already in that position or not

Might need a way to find a more exact spot on mesh 

*/

// Randomly distribute objects onto the map using poisson disc sampling

public class poissonDiscSampler : MonoBehaviour {

	// Use this for initialization
	public static void placeObjects(int seed, int minObjects, int maxObjects, int minDistance, GameObject thisObject, Vector3[] meshVerts, int attempts, int mapHeight, int mapWidth) {
		
    // Create tables for chance of monsters, loot, rewards

    // Loop vars
    int meshVertsLength = meshVerts.Length - 1;
    int chooseItem;
    float randAngle;
    float randRadius;
    bool foundPoint = false;
    int maxDistance = minDistance * 2;

    // Create seed for recreation
    System.Random prng = new System.Random(seed);
    int amountOfObjects = prng.Next(minObjects, maxObjects);

    // Keep track of points
    List<Vector3> activeList = new List<Vector3>();
    List<Vector3> placedItems = new List<Vector3>();

    // Where to place the first object
    int initialObjectPlacement = prng.Next(0, meshVertsLength);

    activeList.Add(meshVerts[initialObjectPlacement]);
    placedItems.Add(meshVerts[initialObjectPlacement]);

    while(activeList.Count > 0 && placedItems.Count < amountOfObjects){
      int k = 0;
      foundPoint = false;

      chooseItem = prng.Next(0, activeList.Count - 1);
      Vector3 startPoint = activeList[chooseItem];

      // Create a new vector with + magnitude of minDist up to magnitude of maxDist
      // TODO make it more efficient by randomly choosing a distance instead of finding possible options on mesh

      // Old: Create List of points on mesh that are within bounds of active point

      while((k < attempts) && foundPoint == false ){
        
        randRadius = prng.Next(minDistance, maxDistance);
        randAngle = (prng.Next(0,100) / 100f) * 360;
        
        // int nextRand = prng.Next(0, possiblePositions.Count -1);
        // Vector3 choosePointOnMesh = possiblePositions[nextRand];
        
        Vector3 choosePointOnMesh = createPointFromCenter(startPoint, randRadius, randAngle);

        // TODO add offsetx, offsetz
        if(checkDistance(choosePointOnMesh, minDistance, maxDistance, placedItems) && (checkInBounds(choosePointOnMesh.x, choosePointOnMesh.z,5,5, mapHeight, mapWidth))){
          activeList.Add(choosePointOnMesh);
          placedItems.Add(choosePointOnMesh);
          foundPoint = true;
        } else {
          k += 1;
        }

      }
      if(k >= attempts){
        activeList.RemoveAt(chooseItem);
      }
    }

    placeObjects(thisObject, placedItems);

	}

  // Check if the given point is within bounds of all objects to be placed
  private static bool checkDistance(Vector3 thePoint, float minDist, float maxDist, List<Vector3> placedItems){
    float theDist;
    for(int i=0; i < placedItems.Count; i++){
      Vector3 nextPoint = placedItems[i];
      nextPoint.y = 0;
      // theDist = Vector3.Distance(thePoint, nextPoint);
      theDist = (nextPoint - thePoint).magnitude;
      if (theDist <= minDist){
        return false;
      }
    }
    return true; 
  }

  // Instantiate objects at points from the given list of Vector3s
  // Places them slightly above the given y position
  private static void placeObjects(GameObject objectToPlace, List<Vector3> placedItems){
    for(int i=0; i < placedItems.Count; i++){
      Quaternion rotation = Quaternion.LookRotation(placedItems[i]);
      Vector3 newPos = placedItems[i];
      newPos.y = 14f;
      Instantiate(objectToPlace, newPos, rotation);
    } 
  }


  // Vector3 randomPoint = center + Random.insideUnitSphere * range;
  // .insideUnitySphere Returns a random point inside a sphere with radius 1
      // Vector3 pos = center + Random.insideUnitSphere * radius/2;
     // pos.y = center.y;

  // Return a Vector3 within an a radius from a starting point. Y pos will not change
  private static Vector3 createPointFromCenter(Vector3 center, float radius, float ang){
    Vector3 pos;
    pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
    pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
    pos.y = center.y;
    return pos;
 }

  // Check if the point is within the bounds of the given mesh
  private static bool checkInBounds(float x, float z, float offsetx, float offsetz, int mapHeight, int mapWidth){
    if (x >= ((mapWidth/2 * -1) + offsetx) && x <= ((mapHeight/2 * 1) - offsetx) && z >= ((mapWidth/2 * -1) + offsetz) && z <= ((mapWidth/2 * 1) - offsetz)){
      return true;
    } else {
      return false;
    }
  }


}
