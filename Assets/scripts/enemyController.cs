﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// poly.google.com/view/56ym_pyVnel For the bear


public class enemyController : MonoBehaviour {

  public Transform player;

  public float walkSpeed = 2;
  public float runSpeed = 6;
  public float gravity = -12;
  public float jumpHeight = 2;
  public float dodgeSpeed = 16;
  public float hpRegenRate = 30;
  float framesUntilRegen;
  // TODO 
  // Lower than 350 attack rate seems to cause issues
  public float attackRate = 350;
  float framesUntilAttack;
  float resetLastSwingFrames = 2400;
  float framesUntilReset;
  public float attackAnimationFrames = 140;
  float attackAnimationFramesLeft = 0;
  // public float attack_2InactiveFrames = 46;
  // float inactiveFramesLeft;
  public float attack_2ActiveFrames = 22;
  float attack_2ActiveFramesLeft;
  public float leftOverAttackFrames = 15;

  public float speedOnAttack = .4f;

  public int hitPoints = 1;
  int maxHitPoints;

  int damageAmount = 2;

  Component[] enemyColliders;
 

  public float turnSpeed = .05f;

  public float interactDistance = 10;
  public float agrressiveDistance = 3;

  public bool DebugMode = true;

  int lastSwing = -1;
  bool alive = true;
  // bool canHit = true;

  Vector3 originalPosition;
  Quaternion originalRotation;

  int refreshRate;

  float velocityY;

  Animator enemyAnimator;
  CharacterController controller;


  private bool player_nearby = false;
  private bool isAttacking = false;
  bool activeFrames = false;

	// Use this for initialization
	void Start () {
		enemyAnimator = GetComponent<Animator>();
    controller =  GetComponent<CharacterController>();
    originalPosition = this.transform.position;
    originalRotation = this.transform.rotation;

    enemyColliders = GetComponentsInChildren<CapsuleCollider>();
    

    refreshRate = Screen.currentResolution.refreshRate;
    hpRegenRate = (hpRegenRate / 60f) * refreshRate;
    attackRate = (attackRate / 60f) * refreshRate;
    resetLastSwingFrames = (resetLastSwingFrames / 60f) * refreshRate;
    attackAnimationFrames = (attackAnimationFrames / 60f) * refreshRate;
    // attack_2InactiveFrames = (attack_2InactiveFrames / 60f) * refreshRate;
    attack_2ActiveFrames = (attack_2ActiveFrames / 60f) * refreshRate;
    leftOverAttackFrames = (leftOverAttackFrames / 60f) * refreshRate * -1;

    framesUntilRegen = hpRegenRate;
    framesUntilAttack = attackRate;
    framesUntilReset = resetLastSwingFrames;

    maxHitPoints = hitPoints;
	}
	
	// Update is called once per frame
	void Update () {

    if (alive){

      if(Vector3.Distance(player.position, this.transform.position) < interactDistance){

        player_nearby = true;

        Vector3 direction = player.position - this.transform.position;

         // Do not rotate on y axis ?if really close.
        direction.y = 0;
       
        // Check if can attack if this close
        if (direction.magnitude <= 2.5f){
          framesUntilAttack -= 1;
          attackPlayer();
        }

        // If he enemy can attack, do so. 
        if (isAttacking && attackAnimationFramesLeft > leftOverAttackFrames){
          attackAnimationFramesLeft -= 1;
          // inactiveFramesLeft -= 1;
          // TODO
          // Still move a little while attacking? Maybe add something to check for this later
          if (attackAnimationFramesLeft > attack_2ActiveFrames){
            Vector3 attackVelocity = transform.forward * speedOnAttack;
            controller.Move(attackVelocity * Time.deltaTime);
          }
        } else {
          isAttacking = false;
        }

        // Check for active frames.
        // Rotate towards player if not active frames or moving toward player, or prepping attack.
        // TODO
        // Make the prep and actual attack two different animations?
        if( isAttacking == true && (attackAnimationFramesLeft <= attack_2ActiveFrames) ){
          activeFrames = true;
          triggerColliders();
          // Debug.Log(attackAnimationFramesLeft);
        } else{
          this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), turnSpeed); //* Time.deltaTime);
          activeFrames = false;
          unTriggerColliders();
        }

        // Move towards the player if not within attack distance
        // only move when not right by player
        if (direction.magnitude >= 2f){
          if(direction.magnitude <= agrressiveDistance){
            velocityY += Time.deltaTime * gravity;
            Vector3 velocity = transform.forward * walkSpeed + Vector3.up * velocityY;
            controller.Move(velocity * Time.deltaTime);
          }
        }

      // not close enough to player for interaction.
      } else {
        player_nearby = false;
        regenerateHitPoints();
        if (Vector3.Distance(this.transform.position, originalPosition) >= .5f ){
          Move_To_originial_position();
        } else {
          this.transform.rotation = Quaternion.Slerp(this.transform.rotation, originalRotation, turnSpeed);
        }
      }



      // enemyAnimator.SetBool("player_nearby", player_nearby);
      enemyAnimator.SetBool("attack_2", isAttacking);
    
    // Ded bear
    } else {
      // no more chasing/moving
      // play death animation
      // give potential rewards
      this.tag = "dead_enemy";
      
    }
    // Un-comment if problems arise?

    // framesUntilReset -= 1;
    // if (framesUntilReset <= 0){
    //   lastSwing = -1;
    //   framesUntilReset = resetLastSwingFrames;
    // }
		
	}

  void Move_To_originial_position(){
    Vector3 direction = originalPosition - this.transform.position;
    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), turnSpeed);
    velocityY += Time.deltaTime * gravity;
    Vector3 velocity = transform.forward * walkSpeed + Vector3.up * velocityY;
    controller.Move(velocity * Time.deltaTime);
  }

  public void hitEnemy(int damage, int swingNum){
    // Calculate knockback, defenses, Crowd control, etc.

    // Only take damage if not yet hit by this swing.
    if (hitPoints > 0){
      if (swingNum != lastSwing){
        hitPoints = hitPoints - damage;
        lastSwing = swingNum;
      }
    }
    if (hitPoints <=0){
      alive = false;
    }
  }
    
  public int getHitPoints(){
    return hitPoints;
  }

  void regenerateHitPoints(){
    if (hitPoints < maxHitPoints){
      framesUntilRegen -= 1;
      if(framesUntilRegen <= 0){
        framesUntilRegen = hpRegenRate;
        hitPoints += 1; 
      }
    }
  }

  void attackPlayer(){
    if (framesUntilAttack <= 0 && attackAnimationFramesLeft <= 0){
      framesUntilAttack = attackRate;
      attackAnimationFramesLeft = attackAnimationFrames;
      // inactiveFramesLeft = attack_2InactiveFrames;
      isAttacking = true;
    } 
    //  else if (attackAnimationFramesLeft >= 0){
    //   isAttacking = true;
    // } else {
    //   isAttacking = false;
    // }
  }

  public bool checkForActiveFrames(){
    return activeFrames;
  }

  public int attackDamage(){
    return damageAmount;
  }

  public void triggerColliders(){
    foreach (CapsuleCollider col in enemyColliders){
      col.isTrigger = true;
    }
  }

  public void unTriggerColliders(){
    foreach (CapsuleCollider col in enemyColliders){
      col.isTrigger = false;
    }
  }

}