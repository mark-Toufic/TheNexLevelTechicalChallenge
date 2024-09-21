using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy AI Components")]
    public float moveSpeed = 3.5f;
    public float chaseRange = 10f;
    public float lockRange = 5f;
    public float attackRange = 2f;
    public float circleRadius = 3f;
    public float circleSpeed = 2f;
    public float idleTime = 2f;
    private Vector3 circleDirection = Vector3.right;

    public Transform player;
    private NavMeshAgent navMeshAgent;
    public Animator enemyAnim;
    public Enemy enemyHealth;
    public EnemyWeapon enemyWeapon;

    private bool isLockedOn = false;
    public bool isAlive = true;
    private bool isChasing = false;
    public bool isAttacking = false;
    public bool isControlled = false;

    private Vector3[] circlePoints;
    private int currentPointIndex = 0;
    private float timeAtCurrentPoint = 0f;
    private float timeToReachPoint = 3f;
    private float idleTimer = 0f;

    private Vector3 currentVelocity = Vector3.zero;
    public float velocitySmoothTime = 0.1f;

    [Header("PlayerControl Components")]
    public ParticleSystem controlEffect;
    private EnemyController currentControlledEnemy; // Track the currently controlled enemy
    private CinemachineFreeLook currentCamera; // Track the current active camera
    public Transform cameraTransform;
    public GameObject freeCam;
    public Transform enemyObj;
    public float rotationSpeed;
    private float velZ = 0.0f;
    private float velX = 0.0f;
    private float acceleration = 6.0f;
    private float deceleration = 2.0f;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;
        enemyAnim.SetFloat("Vel X", 0);
        enemyAnim.SetFloat("Vel Z", 0);
        PatrolOrIdle();
        enemyHealth = GetComponent<Enemy>();

        circlePoints = new Vector3[4];
        InitializeCirclePoints();
    }

    private void Update()
    {
        if (isControlled)
        {
            HandlePlayerInput();

            if (!controlEffect.isPlaying)
            {
                controlEffect.Play();
            }

            if (Input.GetKey(KeyCode.R))
            {
                RevertControl();
            }
        }
        else
        {
            PerformAI();
        }
    }

    public void RevertControl()
    {
        isControlled = false;
        navMeshAgent.isStopped = false;
        PlayerController playerController = FindObjectOfType<PlayerController>();
        playerController.RevertControl();
        playerController.lockOn = false;
        controlEffect.Stop();
    }

    private void HandlePlayerInput()
    {
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool backPressed = Input.GetKey(KeyCode.S);

        navMeshAgent.isStopped = true;

        if (!isAttacking)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            enemyAnim.SetBool("isLockedOn", true);
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

            if (moveDirection != Vector3.zero)
            {
                navMeshAgent.Move(moveDirection * moveSpeed * Time.deltaTime);
                RotateEnemy(moveDirection); // Rotate towards the movement direction
            }
            ChangeVelocity(forwardPressed, rightPressed, leftPressed, backPressed);
            lockOrResetVelocity(forwardPressed, rightPressed, leftPressed, backPressed);
            UpdateAnimationParameters(moveDirection);
        }
    
    }
    void ChangeVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed)
    {
        // Accelerate or decelerate based on input
        if (forwardPressed && velZ < 1)
            velZ += Time.deltaTime * acceleration;
        if (leftPressed && velX > -1)
            velX -= Time.deltaTime * acceleration;
        if (rightPressed && velX < 1)
            velX += Time.deltaTime * acceleration;
        if (backPressed && velZ > -1)
            velZ -= Time.deltaTime * acceleration;

        // Decelerate when not pressing movement keys
        if (!forwardPressed && !backPressed)
        {
            if (velZ > 0.0f) velZ -= Time.deltaTime * deceleration; // Decelerate forward
            if (velZ < 0.0f) velZ += Time.deltaTime * deceleration; // Decelerate backward
        }

        if (!leftPressed && !rightPressed)
        {
            if (velX > 0.0f) velX -= Time.deltaTime * deceleration; // Decelerate right
            if (velX < 0.0f) velX += Time.deltaTime * deceleration; // Decelerate left
        }

        // Set to zero if below a small threshold
        if (Mathf.Abs(velZ) < 0.01f) velZ = 0.0f;
        if (Mathf.Abs(velX) < 0.01f) velX = 0.0f;
    }

    void lockOrResetVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed)
    {
        // Ensure velocity is reset to zero if no movement keys are pressed
        if (!forwardPressed && !backPressed && Mathf.Abs(velZ) < 0.01f)
        {
            velZ = 0.0f;
        }

        if (!leftPressed && !rightPressed && Mathf.Abs(velX) < 0.01f)
        {
            velX = 0.0f;
        }
    }

    private void UpdateAnimationParameters(Vector3 moveDirection)
    {

        if (moveDirection != Vector3.zero)
        {
            enemyAnim.SetFloat("Vel X", velX);
            enemyAnim.SetFloat("Vel Z", velZ);
        }
        else
        {
            enemyAnim.SetFloat("Vel X", velX);
            enemyAnim.SetFloat("Vel Z", velZ);
        }
    }

    private void RotateEnemy(Vector3 moveDirection)
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            enemyObj.rotation = Quaternion.Slerp(enemyObj.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    private void PerformAI()
    {
        if (!isAlive) return;

        float distanceToTarget = Vector3.Distance(transform.position, player.position);


        // AI behavior logic based on distance
        if (distanceToTarget <= attackRange)
        {
            EnterAttackState();
            LookAtPlayer();
        }
        else if (distanceToTarget <= lockRange)
        {
            EnterLockedOnState();
            LookAtPlayer();
            CirclePlayer();
        }
        else if (distanceToTarget <= chaseRange)
        {
            EnterChaseState();
        }
        else
        {
            PatrolOrIdle();
            enemyHealth.RegenerateHealth();
        }

        if (isLockedOn && Random.value < 0.2f)
        {
            circleDirection = circleDirection == Vector3.right ? Vector3.left : Vector3.right;
            InitializeCirclePoints();
        }

        SmoothUpdateAnimationParameters(navMeshAgent.velocity);
    }

    private void SmoothUpdateAnimationParameters(Vector3 velocity)
    {
        float smoothedVelocityX = Mathf.SmoothDamp(enemyAnim.GetFloat("Vel X"), velocity.x, ref currentVelocity.x, velocitySmoothTime);
        float smoothedVelocityZ = Mathf.SmoothDamp(enemyAnim.GetFloat("Vel Z"), velocity.z, ref currentVelocity.z, velocitySmoothTime);

        enemyAnim.SetFloat("Vel X", smoothedVelocityX);
        enemyAnim.SetFloat("Vel Z", smoothedVelocityZ);
    }

    private void EnterAttackState()
    {
        if (isAttacking) return;

        isAttacking = true;
        enemyAnim.SetBool("isAttacking", true);

        if (Random.value > 0.5)
        {
            enemyAnim.SetBool("isLightAttack", true);
            enemyAnim.SetBool("isHeavyAttack", false);
            enemyWeapon.EnableTriggerBox();
        }
        else
        {
            enemyAnim.SetBool("isLightAttack", false);
            enemyAnim.SetBool("isHeavyAttack", true);
            enemyWeapon.EnableTriggerBox();
        }
        navMeshAgent.isStopped = true;
        StartCoroutine(CanAttack());
    }

    public void CancelAttack()
    {
        if (isAttacking)
        {
            isAttacking = false;
            enemyAnim.SetBool("isAttacking", false);
            enemyAnim.SetBool("isLightAttack", false);
            enemyAnim.SetBool("isHeavyAttack", false);
            navMeshAgent.isStopped = false;
            enemyWeapon.DisableTriggerBox();
        }
    }

    private IEnumerator CanAttack()
    {
        yield return new WaitForSeconds(4f);
        CancelAttack();
    }

    private void EnterLockedOnState()
    {
        if (!isLockedOn)
        {
            Debug.Log("Entering LockedOn State");
            isLockedOn = true;
            isChasing = false;
            enemyAnim.SetBool("isLockedOn", true);
            enemyAnim.SetBool("isWalking", false);
            navMeshAgent.isStopped = true;

            navMeshAgent.SetDestination(circlePoints[currentPointIndex]);
        }
    }

    private void CirclePlayer()
    {
        if (isLockedOn && !isAttacking)
        {
            timeAtCurrentPoint += Time.deltaTime;

            if (timeAtCurrentPoint >= timeToReachPoint)
            {
                if (idleTimer <= 0f)
                {
                    currentPointIndex = Random.Range(0, circlePoints.Length);
                    navMeshAgent.SetDestination(circlePoints[currentPointIndex]);
                    timeAtCurrentPoint = 0f;
                    idleTimer = idleTime;
                }
                else
                {
                    idleTimer -= Time.deltaTime;
                }
            }
            float distanceToCurrentPoint = Vector3.Distance(transform.position, circlePoints[currentPointIndex]);
            navMeshAgent.speed = Mathf.Lerp(circleSpeed * 0.5f, circleSpeed, distanceToCurrentPoint / circleRadius);

            navMeshAgent.isStopped = false;
        }
    }

    private void EnterChaseState()
    {
        if (!isChasing)
        {
            Debug.Log("Entering Chase State");
            isLockedOn = false;
            isChasing = true;
            enemyAnim.SetBool("isLockedOn", false);
            enemyAnim.SetBool("isWalking", true);
            navMeshAgent.isStopped = false;
        }
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.SetDestination(player.position); // Move towards the current target
    }

    private void PatrolOrIdle()
    {
        if (isChasing || isLockedOn)
        {
            Debug.Log("Patrolling or Idling");
            isChasing = false;
            isLockedOn = false;
            enemyAnim.SetBool("isLockedOn", false);
            enemyAnim.SetBool("isWalking", false);
            navMeshAgent.isStopped = true;
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        transform.rotation = targetRotation;
    }

    private void InitializeCirclePoints()
    {
        float angleStep = 360f / circlePoints.Length;
        float currentAngle = 0f;

        for (int i = 0; i < circlePoints.Length; i++)
        {
            float x = player.position.x + Mathf.Cos(currentAngle * Mathf.Deg2Rad) * circleRadius;
            float z = player.position.z + Mathf.Sin(currentAngle * Mathf.Deg2Rad) * circleRadius;
            circlePoints[i] = new Vector3(x, transform.position.y, z);

            currentAngle += angleStep;
        }
    }
}
