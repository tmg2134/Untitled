// Thomas Goodman
// May 1, 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Step 2. While the active list is not empty, choose a random index
from it (say i). Generate up to k points chosen uniformly from the
spherical annulus between radius r and 2r around xi. For each
point in turn, check if it is within distance r of existing samples
(using the background grid to only test nearby samples). If a point
is adequately far from existing samples, emit it as the next sample
and add it to the active list. If after k attempts no such point is
found, instead remove i from the active list.

*/

// Randomly distribute objects onto the map using poisson disc sampling

public class poissonDiscSampler : MonoBehaviour {

	// Use this for initialization
	public static void placeObjects(int seed, int minObjects, int maxObjects, int minDistance, int maxDistance, GameObject thisObject, Vector3[] meshVerts, int attempts) {
		
    // Create tables for chance of monsters, loot, rewards

    // Loop vars
    int meshVertsLength = meshVerts.Length - 1;
    int chooseItem;
    float randAngle;
    float randRadius;
    bool foundPoint = false;  

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
        if(checkDistance(choosePointOnMesh, minDistance, maxDistance, placedItems) && (checkInBounds(choosePointOnMesh.x, choosePointOnMesh.z,0,0))){
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
      theDist = Vector3.Distance(thePoint, placedItems[i]);
      if (theDist <= minDist || theDist >= maxDist){
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
      newPos.y = newPos.y + 0.5f;
      Instantiate(objectToPlace, newPos, rotation);
    } 
  }


  // Vector3 randomPoint = center + Random.insideUnitSphere * range;
  // .insideUnitySphere Returns a random point inside a sphere with radius 1

  // Return a Vector3 within an a radius from a starting point. Y pos will not change
  private static Vector3 createPointFromCenter(Vector3 center, float radius, float ang){
    Vector3 pos;
    pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
    pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
    pos.y = center.y;
    return pos;
 }

  // Check if the point is within the bounds of the given mesh
  // TODO swap 99.5 for another param
  private static bool checkInBounds(float x, float z, float offsetx, float offsetz){
    if (x > (-99.5f + offsetx) && x < (99.5 + offsetx) && z > (-99.5 + offsetz) && z < (99.5 + offsetz)){
      return true;
    } else {
      return false;
    }
  }


}
