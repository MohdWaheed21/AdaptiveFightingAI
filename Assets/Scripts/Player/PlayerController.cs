using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 3f;

    [Header("Camera")]
    public Transform cameraPivot;

    [Header("Combat")]
    public Transform enemy;
    public float attackRange = 2.5f;
    public int punchDamage = 10;
    public int kickDamage = 15;

    public float punchDuration = 0.8f;
    public float kickDuration = 1f;
    public float hitReactionDuration = 0.6f;

    private Rigidbody rb;
    private Animator animator;
    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;

    private Vector3 movement;

    private bool isAttacking = false;
    private bool isHit = false;
    private bool isBlocking = false;

    private float cameraPitch = 10f;
    private float targetYaw = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        if (enemy != null)
            enemyHealth = enemy.GetComponent<EnemyHealth>();

        targetYaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (playerHealth == null || playerHealth.IsDead())
        {
            movement = Vector3.zero;
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Block", false);
            return;
        }

        HandleMouseLook();
        HandleBlock();

        if (isAttacking || isHit || isBlocking)
        {
            movement = Vector3.zero;
            animator.SetFloat("Speed", 0f);
            return;
        }

        HandleMovement();
        HandleCombatInput();
    }

    void FixedUpdate()
    {
        if (playerHealth == null ||
            playerHealth.IsDead() ||
            isAttacking ||
            isHit ||
            isBlocking)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.MoveRotation(Quaternion.Euler(0f, targetYaw, 0f));

        Vector3 move = movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void HandleMouseLook()
    {
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        targetYaw += mouseX;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 60f);

        cameraPivot.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward =
            Quaternion.Euler(0, targetYaw, 0) * Vector3.forward;

        Vector3 right =
            Quaternion.Euler(0, targetYaw, 0) * Vector3.right;

        movement =
            (forward * moveZ + right * moveX).normalized;

        animator.SetFloat("Speed", movement.magnitude);
    }

    void HandleBlock()
    {
        isBlocking = Input.GetKey(KeyCode.LeftShift);

        animator.SetBool("Block", isBlocking);
    }

    void HandleCombatInput()
    {
        if (Input.GetKeyDown(KeyCode.J))
            StartCoroutine(PunchAttack());

        if (Input.GetKeyDown(KeyCode.K))
            StartCoroutine(KickAttack());
    }

    IEnumerator PunchAttack()
    {
        isAttacking = true;

        animator.SetTrigger("Punch");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPunch();

        TryAttack(punchDamage);

        yield return new WaitForSeconds(punchDuration);

        isAttacking = false;
    }

    IEnumerator KickAttack()
    {
        isAttacking = true;

        animator.SetTrigger("Kick");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayKick();

        TryAttack(kickDamage);

        yield return new WaitForSeconds(kickDuration);

        isAttacking = false;
    }

    void TryAttack(int damage)
    {
        if (enemy == null ||
            enemyHealth == null ||
            enemyHealth.IsDead())
            return;

        float distance =
            Vector3.Distance(transform.position, enemy.position);

        if (distance <= attackRange)
            enemyHealth.TakeDamage(damage);
    }

    public void TriggerHitReaction()
    {
        if (playerHealth == null ||
            playerHealth.IsDead() ||
            isBlocking)
            return;

        StartCoroutine(HitReaction());
    }

    IEnumerator HitReaction()
    {
        isHit = true;

        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(hitReactionDuration);

        isHit = false;
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }
}