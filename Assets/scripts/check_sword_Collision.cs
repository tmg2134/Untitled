using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class check_sword_Collision : MonoBehaviour {

  public float swordHeight;
  public int swordDamage = 1;
  public float minHitDist = 5f;
  bool weaponColliding = false;
  public bool debugMode = false;

  int swingNum = 0;

  Vector3 test;

  enemyController enemy_controller;

  void Update(){

  }

  void OnTriggerStay(Collider other) {
    // Debug.Log(other.gameObject); // Change to a bool.
    if (other.gameObject.tag == "enemy"){
      // Debug.Log(other.gameObject); // Change to a bool.
      weaponColliding = true;

      // Get the enemy being struck
      enemy_controller = other.gameObject.GetComponentInParent<enemyController>();
      enemy_controller.hitEnemy(swordDamage, swingNum);
      int hitPoints = enemy_controller.getHitPoints();
      Debug.Log(hitPoints);
    } else {
      weaponColliding = false;
    }
  }

  public bool enemyCollision(){
    return weaponColliding;
  }

  public void increaseSwingNum(){
    if (swingNum > 9000){
      swingNum = 0;
    } else {
      swingNum += 1;
    }
  }

} 
