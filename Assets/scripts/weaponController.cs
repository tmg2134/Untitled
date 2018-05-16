using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class weaponController : MonoBehaviour {
  public Transform player;
  bool activated = false;
  public string interactKey = "e";
  public float interactDistance = 3;
  GameObject item;
  GameObject checkWeapon;

	void Start () {
		player = GameObject.Find("Player").transform;
	}
	
	
	void Update () {

    if(activated){
      // Destroy(this.gameObject);
    }

    // TODO
    // Only able to pick-up items if player is idle

    if((Vector3.Distance(player.position, this.transform.position) <= interactDistance && activated == false)){ 
      if(Input.GetKeyDown(interactKey)) {
        activated = true;
        if(GameObject.Find("Hand_Hold_R").transform.childCount > 0){
          checkWeapon = GameObject.Find("Hand_Hold_R").transform.GetChild(0).gameObject;
        }
        if(checkWeapon != null && checkWeapon.tag == "weapon"){
          GameObject dropWeapon = Instantiate(checkWeapon, this.transform.position, checkWeapon.transform.rotation * Quaternion.Inverse(player.transform.rotation));
          // dropWeapon.GetComponentInChildren<Collider>().isTrigger = false;
          // dropWeapon.AddComponent<Rigidbody>().useGravity = true;
          // dropWeapon.GetComponent<Rigidbody>().useGravity = true;
          dropWeapon.AddComponent<weaponController>();
          Destroy(checkWeapon);
        }
        // 
        this.transform.SetParent(GameObject.Find("Hand_Hold_R").transform);
        this.transform.position = GameObject.Find("Hand_Hold_R").transform.position;
        this.transform.rotation = this.transform.rotation * player.transform.rotation;
        // TODO loop until weapon is found
        if(this.gameObject.GetComponentInChildren<check_sword_Collision>() == null){
          this.gameObject.GetComponentInChildren<Collider>().gameObject.AddComponent<check_sword_Collision>();
        }
        
        Destroy(this.gameObject.GetComponent<weaponController>());
      }
    }
	}
}
