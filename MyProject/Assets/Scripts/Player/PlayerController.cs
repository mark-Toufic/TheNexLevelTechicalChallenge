using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDragLocked;
    public float groundDragSprint;
    public float groundDrag;
    public float currentMaxVel;
    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    public Animator playerAnim;
    public ThirdPersonCam thirdPersonCam;
    public PlayerCombat combat;
    public PlayerHealth health;

    [Header("Mind Control Components")]
    public GameObject currentControlledEnemy;
    public GameObject spellPNG;
    public bool isControllingEnemy = false;
    [SerializeField] private Image controlUI = null;
    public float controlCooldown = 100.0f;
    public float maxControlCooldown = 100.0f;
    public float refillRate = 1;

    [Header("Aduio")]
    public AudioClip RunSound;
    public AudioClip rollSound;
    public AudioClip lockSound;
    private AudioSource audioSource;

    [Header("UI Components")]
    public Stamina staminaController;
    public bool isDead = false;

    [Header("Roll")]
    public bool isRolling = false;
    public float rollSpeed = 10.0f;

    [Header("Combat")]
    public bool lockOn;
    public bool isHit;
    public Transform lockOnTarget;
    public float lockOnRange = 10f;
    public LayerMask enemyLayer;
    private List<Transform> enemiesInRange = new List<Transform>();
    private int currentTargetIndex = -1;

    [Header("Animation")]
    private float velZ = 0.0f;
    private float velX = 0.0f;
    private float rollVelZ = 0.0f;
    private float rollVelX = 0.0f;
    private float acceleration = 2.0f;
    private float deceleration = 2.0f;
    private float maxWalkVel = 1.5f;
    private float maxRunVel = 2.5f;

    int VelocityXHash;
    int VelocityZHash;
    int RollVelocityXHash;
    int RollVelocityZHash;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        lockOn = false;
        isHit = false;
        thirdPersonCam = thirdPersonCam ?? GetComponent<ThirdPersonCam>();
        combat = combat ?? GetComponent<PlayerCombat>();
        staminaController = staminaController ?? GetComponent<Stamina>();
        controlCooldown = maxControlCooldown;
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = RunSound;
        VelocityZHash = Animator.StringToHash("Vel Z");
        VelocityXHash = Animator.StringToHash("Vel X");
        RollVelocityXHash = Animator.StringToHash("RollVel X");
        RollVelocityZHash = Animator.StringToHash("RollVel Z");

    }

    private void Update()
    {
        if (isDead) return;

        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool backPressed = Input.GetKey(KeyCode.S);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);
        bool rollPressed = Input.GetKey(KeyCode.Space);
        bool cycleNext = Input.GetKeyDown(KeyCode.Q);
        bool cyclePrevious = Input.GetKeyDown(KeyCode.E);



        if (Input.GetKeyDown(KeyCode.T) && controlCooldown >= 100)
        {
            TransferControl();
            controlCooldown = 0;
        }

        if (!isControllingEnemy)
        {
            if (controlCooldown < maxControlCooldown)
            {
                controlCooldown += refillRate * Time.deltaTime; // Refill over time
                if (controlCooldown > maxControlCooldown)
                    controlCooldown = maxControlCooldown; // Clamp to max value

                UpdateControlCooldown(); // Update UI
            }

            MyInput();
            if (staminaController.playerStamina <= 0)
            {
                runPressed = false;
                currentMaxVel = maxWalkVel;
            }

            else if (staminaController.playerStamina >= 30)
                currentMaxVel = runPressed ? maxRunVel : maxWalkVel;

            if (Input.GetKeyDown(KeyCode.F))
                LockOnToggle();

            if (cycleNext)
                CycleTargets(true);

            if (cyclePrevious)
                CycleTargets(false);


            if (runPressed && lockOn)
                staminaController.Sprinting();

            else if (!runPressed && lockOn)
                staminaController.isSprinting = false;


            if (lockOn)
                rb.drag = runPressed ? groundDragSprint : groundDragLocked;

            else
                rb.drag = groundDrag;


            // Continuously update enemies in range
            UpdateEnemiesInRange();

            // Handles changes in velocity
            ChangeVelocity(forwardPressed, rightPressed, leftPressed, backPressed, currentMaxVel);
            lockOrResetVelocity(forwardPressed, rightPressed, leftPressed, backPressed, runPressed, currentMaxVel);
            RollDirection(forwardPressed, rightPressed, leftPressed, backPressed, rollPressed);

            playerAnim.SetFloat(VelocityXHash, velX);
            playerAnim.SetFloat(VelocityZHash, velZ);
            playerAnim.SetFloat(RollVelocityXHash, rollVelX);
            playerAnim.SetFloat(RollVelocityZHash, rollVelZ);
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        bool isMoving = horizontalInput != 0f || verticalInput != 0f;
        playerAnim.SetBool("isWalking", isMoving && !lockOn); // Adjust animation state

        if (isRolling)
        {
            if (!audioSource.isPlaying || audioSource.clip != rollSound)
            {
                audioSource.clip = rollSound; // Set to roll sound
                audioSource.Play();
            }
            return; // Skip the rest of the logic if rolling
        }
        // Handle regular walk sound
        if (isMoving && !lockOn)
        {
            if (!audioSource.isPlaying || audioSource.clip != RunSound)
            {
                audioSource.pitch = 0.8f;
                audioSource.clip = RunSound; // Set to regular walk sound
                audioSource.Play();
            }
        }
        else if (isMoving && lockOn)
        {
            if (!audioSource.isPlaying || audioSource.clip != lockSound)
            {
                audioSource.pitch = 1;
                audioSource.clip = lockSound; // Set to walk-on sound
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // Stop the sound if not moving
            }
        }

        MovePlayer();
    }

    IEnumerator Roll()
    {
        isRolling = true;
        playerAnim.SetBool("isRolling", true);

        // Play the rolling sound
        audioSource.clip = rollSound; // Set to roll sound
        audioSource.pitch = 1.0f; // Set pitch to normal (adjust as needed)
        audioSource.Play(); // Play the rolling sound

        // Calculate the roll direction
        Vector3 rollDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rollDirection = rollDirection.normalized;

        // Duration of the roll
        float rollDuration = 0.5f;
        float elapsedTime = 0f;

        // Initial velocity
        Vector3 initialVelocity = rb.velocity;
        Vector3 targetVelocity = rollDirection * rollSpeed;

        // Smoothly interpolate the velocity
        while (elapsedTime < rollDuration)
        {
            float t = elapsedTime / rollDuration;
            rb.velocity = Vector3.Lerp(initialVelocity, targetVelocity, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = targetVelocity; // Ensure final velocity is correct

        yield return new WaitForSeconds(0.6f);
        playerAnim.SetBool("isRolling", false);
        isRolling = false;

        rollVelX = 0.0f;
        rollVelZ = 0.0f;
    }

    private void UpdateControlCooldown()
    {
        if (controlUI != null)
        {
            controlUI.fillAmount = controlCooldown / maxControlCooldown; 
        }
    }
    private void TransferControl()
    {
        if (currentControlledEnemy != null)
        {
            EnemyController enemyController = currentControlledEnemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.isControlled = true;
                this.enabled = false; // Disable player controls
                isControllingEnemy = true;
                enemyController.freeCam.SetActive(true);
                playerAnim.SetBool("isControlling", true);
            }
        }
    }

    public void RevertControl()
    {
        if (isControllingEnemy)
        {
            if (currentControlledEnemy != null)
            {
                EnemyController enemyController = currentControlledEnemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.isControlled = false;
                    this.enabled = true; // Enable player controls
                    enemyController.freeCam.SetActive(false);
                    isControllingEnemy = false;
                    playerAnim.SetBool("isControlling", false);
                }
            }
        }
    }

    public void MovePlayer()
    {
        if (!isRolling && !combat.isAttacking && !isHit)
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
    }

    private void UpdateEnemiesInRange()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);
        enemiesInRange.Clear();

        foreach (Collider collider in enemyColliders)
        {
            // Check if there's a clear line of sight to the enemy
            if (IsClearLineOfSight(collider.transform))
            {
                enemiesInRange.Add(collider.transform);
            }
        }

        if (enemiesInRange.Count > 0)
        {
            // Set the initial target to the closest one if no target is set
            if (lockOnTarget == null || !IsTargetInRange(lockOnTarget))
            {
                currentTargetIndex = GetClosestEnemyIndex();
                lockOnTarget = enemiesInRange[currentTargetIndex];
            }
        }
        else
        {
            // No enemies are in range, so we need to reset lockOn
            lockOn = false;
            playerAnim.SetBool("isLockedOn", false);
            lockOnTarget = null;
        }
    }

    private bool IsClearLineOfSight(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Perform a raycast to check for obstacles between the player and the target
        if (Physics.Raycast(transform.position, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, LayerMask.GetMask("Wall")))
        {
            if (hit.collider.transform != target) // Ensure the hit object is not the target itself
            {
                return false; // There is an obstacle in the way
            }
        }

        return true; // Clear line of sight
    }

    private int GetClosestEnemyIndex()
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, enemiesInRange[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private bool IsTargetInRange(Transform target)
    {
        if (target == null)
            return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= lockOnRange;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    public void CycleTargets(bool next)
    {
        if (!lockOn || enemiesInRange.Count == 0) return;

        if (next)
        {
            // Move to the next target in the list
            currentTargetIndex = (currentTargetIndex + 1) % enemiesInRange.Count;
        }
        else
        {
            // Move to the previous target in the list
            currentTargetIndex = (currentTargetIndex - 1 + enemiesInRange.Count) % enemiesInRange.Count;
        }
        lockOnTarget = enemiesInRange[currentTargetIndex];
        currentControlledEnemy = lockOnTarget.gameObject;
    }

    private void LockOnToggle()
    {
        if (lockOn)
        {
            lockOn = false;
            playerAnim.SetBool("isLockedOn", false);
            lockOnTarget = null;
            currentControlledEnemy = null;
        }
        else
        {
            LockOnTriggered();
        }
    }

    private void LockOnTriggered()
    {
        UpdateEnemiesInRange();

        if (enemiesInRange.Count > 0)
        {
            currentTargetIndex = 0;
            lockOnTarget = enemiesInRange[currentTargetIndex];
            currentControlledEnemy = lockOnTarget.gameObject;
            lockOn = true;
            playerAnim.SetBool("isLockedOn", true);
        }
        else
        {
            lockOn = false;
            playerAnim.SetBool("isLockedOn", false);
            lockOnTarget = null;
        }
    }

    // Handles acceleration and deceleration
    void ChangeVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed, float currentMaxVel)
    {
        // Accelerate or decelerate based on input
        if (forwardPressed && velZ < currentMaxVel)
            velZ += Time.deltaTime * acceleration;
        if (leftPressed && velX > -currentMaxVel)
            velX -= Time.deltaTime * acceleration;
        if (rightPressed && velX < currentMaxVel)
            velX += Time.deltaTime * acceleration;
        if (backPressed && velZ > -currentMaxVel)
            velZ -= Time.deltaTime * acceleration;

        // Decelerate when not pressing movement keys
        if (!forwardPressed && velZ > 0.0f)
            velZ -= Time.deltaTime * deceleration;
        if (!backPressed && velZ < 0.0f)
            velZ += Time.deltaTime * deceleration;
        if (!leftPressed && velX < 0.0f)
            velX += Time.deltaTime * deceleration;
        if (!rightPressed && velX > 0.0f)
            velX -= Time.deltaTime * deceleration;
    }

    void lockOrResetVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed, bool runPressed, float currentMaxVel)
    {
        if (lockOn)
        {
            //Declerate Idle from front back
            if (!forwardPressed && !backPressed && velZ != 0.0f && (velZ > 1.0f && velZ < 1.0f))
            {
                velZ = 0.0f;
            }

            //Declerate Idle fron left right
            if (!leftPressed && !rightPressed && velX != 0.0f && (velX > 1.0f && velX < 1.0f))
            {
                velX = 0.0f;
            }

            //Lock Sprint Front
            if (forwardPressed & runPressed && velZ > currentMaxVel)
            {
                velZ = currentMaxVel;
            }
            else if (forwardPressed && velZ > currentMaxVel)
            {
                velZ -= Time.deltaTime * deceleration;

                if (velZ > currentMaxVel && velZ < (currentMaxVel + 0.05f))
                    velZ = currentMaxVel;
            }
            else if (forwardPressed && velZ < currentMaxVel && velZ > (currentMaxVel - 0.05f))
            {
                velZ = currentMaxVel;
            }

            //Lock Sprint Back
            if (backPressed & runPressed && velZ < -currentMaxVel)
            {
                velZ = currentMaxVel;
            }
            else if (backPressed && velZ < -currentMaxVel)
            {
                velZ += Time.deltaTime * deceleration;

                if (velZ < -currentMaxVel && velZ > (-currentMaxVel - 0.05f))
                    velZ = -currentMaxVel;
            }
            else if (backPressed && velZ > -currentMaxVel && velZ < (-currentMaxVel + 0.05f))
            {
                velZ = -currentMaxVel;
            }

            //Lock Sprint Left
            if (leftPressed & runPressed && velX < -currentMaxVel)
            {
                velX = -currentMaxVel;
            }
            else if (leftPressed && velX < -currentMaxVel)
            {
                velX += Time.deltaTime * deceleration;

                if (velX > -currentMaxVel && velX > (-currentMaxVel - 0.05f))
                    velX = -currentMaxVel;

            }
            else if (leftPressed && velX > -currentMaxVel && velX < (-currentMaxVel + 0.05f))
            {
                velX = -currentMaxVel;
            }

            //Lock Sprint Right
            if (rightPressed & runPressed && velX > currentMaxVel)
            {
                velX = currentMaxVel;
            }
            else if (rightPressed && velX > currentMaxVel)
            {
                velX -= Time.deltaTime * deceleration;

                if (velX > currentMaxVel && velX < (currentMaxVel + 0.05f))
                    velX = currentMaxVel;
            }
            else if (rightPressed && velZ < currentMaxVel && velX > (currentMaxVel - 0.05f))
            {
                velX = currentMaxVel;
            }

        }
    }

    void RollDirection(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed, bool rollPressed)
    {
        if (staminaController.playerStamina <= staminaController.minStaminaForAction)
        {
            rollPressed = false;
        }

        if (lockOn && !combat.isAttacking && !isHit)
        {
            if (forwardPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (forwardPressed && rightPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                rollVelX = 0.05f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (forwardPressed && leftPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                rollVelX = -0.05f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (backPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = -1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (leftPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelX = -1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (rightPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelX = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
        }
        else if (!isHit)
        {
            if (forwardPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (leftPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (rightPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
            if (backPressed && rollPressed && !isRolling && !combat.isAttacking)
            {
                rollVelZ = 1.0f;
                StartCoroutine(Roll());
                staminaController.StaminaRoll();
            }
        }
    }
}