using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_Camera : MonoBehaviour {

  public float mouseSensitivity = 2;
  public Transform target;
  public float dstFromTarget = 1;

  public float pitchMin = -40;
  public float pitchMax = 85;

  public float yawMin = -20;
  public float yawMax = 20;

  public float rotationSmoothTime = 0.0f;
  Vector3 rotationSmoothVelocity;
  Vector3 currentRotation;

  float yaw;
  float pitch;
  public float zedDistance = 1.5f;

  Quaternion customRotation;

  // bool yawClamp = false;
  bool customRot = false;

  public bool lockCursor;

  void Start() {
    if (lockCursor){
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }
  }

	void LateUpdate () {
		yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
    pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
    pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    // if (yawClamp){
    //   yaw = Mathf.Clamp(yaw, yawMin, yawMax);
    // }
    
    
    currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3 (pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
    if (customRot){
      transform.rotation = customRotation;
    } else {
      transform.eulerAngles = currentRotation;
    }
    // Vector3 targetRotation = new Vector3(pitch, yaw);
    // transform.eulerAngles = targetRotation;

    transform.position = target.position - transform.forward * dstFromTarget;

	}

  // public void clampYaw(){
  //   yawClamp = true;
  // }

  // public void unClampYaw(){
  //   yawClamp = false;
  // }

  public void setRotation(Quaternion thisRot){
    customRot = true;
    customRotation = thisRot;
  }

  public void unsetRotation(){
    customRot = false;
  }



}
