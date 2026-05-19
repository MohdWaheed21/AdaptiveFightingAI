using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    public float rotationSpeed = 8f;

    [Header("Combat")]
    public int punchDamage = 10;
    public int kickDamage = 15;
    public float punchDuration = 0.8f;
    public float kickDuration = 1.0f;
    public float hitReactionDuration = 0.6f;

    private Rigidbody rb;
    private Animator animator;

    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;

    private float nextAttackTime;

    private bool isAttacking = false;
    private bool isHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead())
        {
            StopMoving();
            return;
        }

        if (playerHealth != null && playerHealth.IsDead())
        {
            StopMoving();
            return;
        }

        if (isAttacking || isHit)
        {
            StopMoving();
            FacePlayer();
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        FacePlayer();

        if (distance > attackRange)
        {
            Move(direction.normalized);
        }
        else
        {
            StopMoving();
            Attack();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void Move(Vector3 direction)
    {
        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y,
            direction.z * moveSpeed
        );

        animator.SetFloat("Speed", 1f);
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);
    }

    void Attack()
    {
        if (Time.time < nextAttackTime)
            return;

        if (playerHealth == null || playerHealth.IsDead())
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (Random.value > 0.5f)
            StartCoroutine(PunchAttack());
        else
            StartCoroutine(KickAttack());
    }

    IEnumerator PunchAttack()
    {
        isAttacking = true;

        animator.SetTrigger("Punch");
        playerHealth.TakeDamage(punchDamage);

        yield return new WaitForSeconds(punchDuration);

        isAttacking = false;
    }

    IEnumerator KickAttack()
    {
        isAttacking = true;

        animator.SetTrigger("Kick");
        playerHealth.TakeDamage(kickDamage);

        yield return new WaitForSeconds(kickDuration);

        isAttacking = false;
    }

    public void TriggerHitReaction()
    {
        if (enemyHealth.IsDead()) return;

        StartCoroutine(HitReaction());
    }

    IEnumerator HitReaction()
    {
        isHit = true;

        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(hitReactionDuration);

        isHit = false;
    }
}