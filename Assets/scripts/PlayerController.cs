using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayerController : MonoBehaviour {

  // Movement parameters
  public float walkSpeed = 2;
  public float runSpeed = 6;
  public float gravity = -12;
  public float jumpHeight = 2;
  public float dodgeSpeed = 25;
  public float speedOnSwing = .4f;
  public float enemyLockOnMaxDist = 50f;
  public float turnSmoothTime = 0.08f;
  public float speedSmoothTime = 0.1f;

  // frames
  public float swing_timer = 40;
  public float stab_timer  = 50;
  public float swing2_timer = 80;
  public float swipe_timer = 50;
  public float swipe2_timer = 60;
  public float dodge_timer = 12;
  public float invincibility_timer = 20;

  public float comboTimer = .2f;

  int swing_count = 0;
  float swing_frames_left = 0;
  float dodge_frames_left = 0;
  float invincibility_frames_left = 0;

  // controls
  public string upKey = "w";
  public string downKey = "s";
  public string leftKey = "a";
  public string rightKey = "d";
  public string swingKey = "mouse 0";
  public string swipeKey = "q";
  public string lockOn = "r";
  public string lockOff = "f";

  // Resources
  public int playerHitPoints = 20;
  int playerMaxHitPoints;
  public float stamina = 10;
  float maxStamina;
  public float stamina_refresh_timer = 1;
  float stamina_refresh_frames_left;
  public float regenStaminaAmount = .06f;

  public float swing_StaminaUsage = 1;
  public float run_StaminaUsage = .04f;
  public float dodge_StaminaUsage = 1;

  //UI
  public Slider healthSlider;
  public Slider staminaSlider;

  public bool lockedCamera = false;

  player_Camera theCamera;

  int refreshRate;

  bool gotHit = false;
  int enemyDamage = 0;
  
  float turnSmoothVelocity;
  float speedSmoothVelocity;
  float currentSpeed;
  float checkSpeed;
  float velocityY;

  // Animations states
  // string swing_2hand = "Swing_2Hand";
  bool swing_test = false;
  bool stab_test = false;
  bool swing2_test = false;
  bool swipe_test = false;
  bool swipe2_test = false;
  bool sliding = false;


  GameObject[] nearbyEnemies = null;
  GameObject targetEnemy = null;

  GameObject equipedItem;

  float targetRotation;

  Vector2 input;
  Vector2 inputDir;

  Animator playerAnimator;
  Transform cameraT;
  CharacterController controller;
  check_sword_Collision weaponCollider;

  // Use this for initialization
  void Start() {
    playerAnimator = GetComponent<Animator>();
    theCamera = GetComponentInChildren<player_Camera>();
    cameraT = Camera.main.transform;
    controller =  GetComponent<CharacterController>();
    weaponCollider = GetComponentInChildren<check_sword_Collision>();
    // Debug.Log(weaponCollider);

    // input
    input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    inputDir = input.normalized;

    targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;

    maxStamina = stamina;

    syncFrameRates();
  }
  
  // Update is called once per frame
  // Check on memory?.
  void Update() {

    input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    inputDir = input.normalized;

    // Swing and shit
    // TODO
    // This probbaly needs some work for different swing sets.
    // Maybe create array of swings. Loop through that array
    // TODO Refact
    // should the weapon collider class access player, or should player access weapon collider?

    if (Input.GetKeyDown(swingKey) && stamina > 0){
      if (swing_count == 0){
        swing_test = true;
        swing_count += 1;
        swing_frames_left = swing_timer;
        weaponCollider.increaseSwingNum();
        useStamina(swing_StaminaUsage, swing_timer);
      } else if(swing_count == 1) {
        stab_test = true;
        swing_frames_left = stab_timer;
        swing_count += 1;
        weaponCollider.increaseSwingNum();
        useStamina(swing_StaminaUsage, stab_timer);
        // swing1_frames_left = 0;
      } else if(swing_count == 2) {
        swing2_test = true;
        swing_frames_left = swing2_timer;
        swing_count++;
        // Maybe put a delay on this one?
        weaponCollider.increaseSwingNum();
        useStamina(swing_StaminaUsage, swing2_timer);
      }
    } else if (Input.GetKeyDown(swipeKey) && stamina > 0){
      if (swing_count == 0){
        swipe_test = true;
        swing_count += 1;
        swing_frames_left = swipe_timer;
        weaponCollider.increaseSwingNum();
        useStamina(swing_StaminaUsage, swipe_timer);
      } else if(swing_count == 1 && swing_frames_left <= (swipe_timer * comboTimer)){
        swipe2_test = true;
        swing_count += 1;
        swing_frames_left = swipe2_timer;
        weaponCollider.increaseSwingNum();
        useStamina(swing_StaminaUsage, swipe_timer);
      }
    } else {
      if (swing_frames_left <= 0){
        // if (swing_count == 0){
        //   swing_test = false;
        //   swing_count = 0;
        // } else {
          swing_test = false;
          stab_test =  false;
          swing2_test = false;
          swipe_test = false;
          swipe2_test = false;
          swing_count = 0;
        // }
      }
    }

    // Get list of enemies, lock on to one
    // TODO
    // This will probably need some work, especially when dealing with moving around
    // Reset array after a certain number of frames have passed?
    if (Input.GetKeyDown(lockOn)){
      lockedCamera = true;
      if (nearbyEnemies == null || nearbyEnemies.Length == 0){
        nearbyEnemies = getEnemies();
        if (nearbyEnemies.Length == 0){
          lockedCamera = false;
          targetEnemy = null;
        } else {
          targetEnemy = nearbyEnemies[0];
        }
      } else if(Array.IndexOf(nearbyEnemies,targetEnemy) == (nearbyEnemies.Length - 1)) {
        targetEnemy = nearbyEnemies[0];
      } else {
        int currentIndex = Array.IndexOf(nearbyEnemies,targetEnemy);
        targetEnemy = nearbyEnemies[currentIndex + 1];
      }
      if (lockedCamera){
        // Debug.Log(targetEnemy);
        // theCamera.clampYaw();
      }
      
    }

    if (Input.GetKeyDown(lockOff)){
      lockedCamera = false;
      // theCamera.unsetRotation();
      // theCamera.unClampYaw();
    }

    // Add walking backwards instead of turning around

    bool running = (Input.GetKey(KeyCode.LeftShift) && stamina >= 0);
    // Tick stamina if running
    if (running){
      useStamina(run_StaminaUsage, 60f);
    }


    // if (Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d")){
    //   float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
    //   transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
    // }

    if (Input.GetKeyDown(KeyCode.Space)){
      Jump();
    }

    if (lockedCamera){
      if (targetEnemy != null){
        // Vector3 enemyDirection = targetEnemy.transform.position - transform.position;
        Vector3 enemyDirection = targetEnemy.transform.position - transform.position;
        // Debug.Log(enemyDirection);
        enemyDirection.y = 0;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(enemyDirection), 1f);
        
        // Create a custom rotation for the camera
        // theCamera.setRotation(Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(enemyDirection), 1f));

        // transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
      } else {
        lockedCamera = false;
      }
    }

    // Only rotate while moving
    if (movementInput() && lockedCamera == false ){
      targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
    }

    // Do not rotate while swinging, diiferent rotation for lockedCamera
    if (swing_frames_left <= 0 && lockedCamera == false){
      transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
    }

    float targetSpeed = ((running)?runSpeed:walkSpeed) * inputDir.magnitude;
    currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

    // Move slower if swinging. No rotations while swinging.
    if (swing_frames_left > 0){
      // Debug.Log(swing_frames_left);
      swing_frames_left -= 1;
      currentSpeed = 0;
      Vector3 swingVelocity = transform.forward * speedOnSwing;
      controller.Move(swingVelocity * Time.deltaTime);
      // Debug.Log(weaponCollider.enemyCollision());
    }

    // Dodge. Can't Dodge while swinging. Can make dodge queue up?

    if (Input.GetKeyDown(KeyCode.LeftAlt) && dodge_frames_left <= 0 && swing_frames_left <= 0){
      dodge_frames_left = dodge_timer;
      // swing_frames_left -= swing_frames_left * .8f;
      sliding = true;
      useStamina(dodge_StaminaUsage, dodge_timer);
    }

    if (dodge_frames_left >= 0) {
      dodge_frames_left -= 1;
      
      Vector3 rollVelocity = transform.forward * dodgeSpeed;
      if (lockedCamera){
        rollVelocity = getStepDirection() * dodgeSpeed;
      }
      
      controller.Move(rollVelocity * Time.deltaTime);
      // currentSpeed = currentSpeed / 2;
      currentSpeed = 0;
    } else {
      sliding = false;
    }

    if (lockedCamera){
      // currentSpeed = currentSpeed * .95f;
    }

    velocityY += Time.deltaTime * gravity;
    Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

    // If locked camera
    if(lockedCamera){
      Vector3 moveDir = getStepDirection();
      velocity = moveDir * currentSpeed + Vector3.up * velocityY;
    }

    controller.Move(velocity * Time.deltaTime);
    checkSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
    
    if (controller.isGrounded) {
      velocityY = 0;
    }

    float animationSpeedPercent = ((running)? checkSpeed/runSpeed : checkSpeed/walkSpeed * .5f);

    // set parameters for animation state machine

    // Blend tree
    playerAnimator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

    // States
    playerAnimator.SetBool("swing_test", swing_test);
    playerAnimator.SetBool("stab_test", stab_test);
    playerAnimator.SetBool("swing2_test", swing2_test);
    playerAnimator.SetBool("sliding", sliding);
    playerAnimator.SetBool("swipe_test", swipe_test);
    playerAnimator.SetBool("swipe2_test", swipe2_test);

    // Invincibility frames
    invincibility_frames_left -= 1;

    // Refresh stamina
    refreshStamina();

  }

  // Might have to queue up the damage.
  void OnTriggerEnter(Collider other) {
    // Debug.Log(other.gameObject);
    // Debug.Log(other.gameObject.tag);
    if (other.gameObject.tag == "enemy" || other.gameObject.tag == "enemy_col"){
      Debug.Log(other); // Change to a bool.
      // Get the enemy being struck
      enemyController enemy_controller = other.gameObject.GetComponentInParent<enemyController>();
      if (enemy_controller.checkForActiveFrames()){
        takeDamage(enemy_controller.attackDamage());
        // gotHit = true;
        // enemyDamage = enemy_controller.attackDamage();
        // enemy_controller.unTriggerColliders();
      }
    }
  }

  void Jump() {
    if (controller.isGrounded){
      float jumpVelocity = Mathf.Sqrt(-2*gravity*jumpHeight);
      velocityY = jumpVelocity;
      // possible animation cancel
      swing_frames_left = 0;
    }

  }

  void Dodge() {
    
  }

  bool movementInput(){
    // TODO: Change to specified keys;
    if (Input.GetKey(upKey) || Input.GetKey(downKey) || Input.GetKey(rightKey) || Input.GetKey(leftKey)){
      return true;
    } else {
      return false;
    }
  }

  // Get a list of enemies and find closest to player in direction of players rotation. Also at least within targetable bounds 
  //
  // TODO
  // Would only switch between two closest enemies right now
  // Change to make a sorted list/array of enemies from min to max distance
  // Only put enemies within a certain range into the list
  GameObject[] getEnemies(){
    // Players position
    Vector3 playerPos = transform.position;

    GameObject[] enemies;

    enemies = GameObject.FindGameObjectsWithTag("enemy");
    enemies.OrderBy(t=>((t.transform.position - playerPos).sqrMagnitude));

    return enemies;

  }

  Vector3 getStepDirection(){
    Vector3 theDirection = transform.forward;
    if (Input.GetKey(downKey)){
      theDirection = -transform.forward;
    } else if (Input.GetKey(rightKey)){
      theDirection = transform.right;
    } else if (Input.GetKey(leftKey)){
      theDirection = -transform.right;
    }
    return theDirection;
  }

  void takeDamage(int damage){
    // Calculate knockback, defenses, Crowd control, etc.

    // Only take damage if not yet hit by this swing.
    if (playerHitPoints > 0 && invincibility_frames_left <= 0){
      playerHitPoints = playerHitPoints - damage;
      invincibility_frames_left = invincibility_timer;
      healthSlider.value = playerHitPoints;
      // gotHit = false;
      // Debug.Log(playerHitPoints);
    }
    // if (hitPoints <=0){
    //   alive = false;
    // }
  }

  void syncFrameRates(){
    refreshRate = Screen.currentResolution.refreshRate;
    swing_timer = (swing_timer / 60f) * refreshRate;
    stab_timer  = (stab_timer / 60f) * refreshRate;
    swing2_timer = (swing2_timer / 60f) * refreshRate;
    dodge_timer  = (dodge_timer / 60f) * refreshRate;
    invincibility_timer = (dodge_timer / 60f) * refreshRate;
    stamina_refresh_timer = (stamina_refresh_timer / 60f) * refreshRate;
  }

  void useStamina(float amt, float actionFrames){
    if (stamina > 0){
      stamina -= amt;
      stamina_refresh_frames_left = stamina_refresh_timer + actionFrames;
    }
  }

  void refreshStamina(){
    if (stamina_refresh_frames_left <= 0){
      if (stamina < maxStamina){
      stamina_refresh_frames_left = stamina_refresh_timer;
      stamina += regenStaminaAmount;
      } else {
        stamina = maxStamina;
      }
    } else {
      stamina_refresh_frames_left -= 1;
    }
    staminaSlider.value = stamina;
  }

}
