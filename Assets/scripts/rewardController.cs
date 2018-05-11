﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardController : MonoBehaviour {

  public Transform player;
  Animator rewardAnimator;
  int refreshRate;
  float framesUntilRelease = 0;
  public float animationFrames = 140;
  public float interactDistance;
  public string parameterName = "activate";
  public float leftOverFrames = 20;
  bool activated = false;
  bool rewardGenerated = false;

  public GameObject theReward;

  public string interactKey = "e";

	// Use this for initialization
	void Start () {
		rewardAnimator = GetComponent<Animator>();
    refreshRate = Screen.currentResolution.refreshRate;
    animationFrames = Mathf.Floor((animationFrames / 60f) * refreshRate);
    leftOverFrames = Mathf.Floor((leftOverFrames / 60f) * refreshRate) * -1;
	}
	
	// Update is called once per frame
	void Update () {

    if(activated){
      framesUntilRelease -= 1;
    }

    if(framesUntilRelease < 0 && rewardGenerated == false){
      Instantiate(theReward, this.transform.position, this.transform.rotation);
      rewardGenerated = true;
    }

    // TODO add puff of smoke after destorying
    if(framesUntilRelease < leftOverFrames){
      Destroy(this.gameObject);
    }

    if((Vector3.Distance(player.position, this.transform.position) <= interactDistance) && activated == false){
      if(Input.GetKeyDown(interactKey)) {
        rewardAnimator.SetBool(parameterName, true);
        framesUntilRelease = animationFrames;
        activated = true;
      }
    }

	}
}
