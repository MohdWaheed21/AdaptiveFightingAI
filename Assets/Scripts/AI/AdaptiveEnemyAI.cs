using UnityEngine;
using Unity.InferenceEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class AdaptiveEnemyAI : MonoBehaviour
{
    [Header("AI Model")]
    public ModelAsset modelAsset;

    [Header("References")]
    public Transform player;
    public EnemyHealth enemyHealth;
    public PlayerHealth playerHealth;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float retreatSpeed = 3f;
    public float strafeSpeed = 2f;

    [Header("Combat")]
    public float attackRange = 2.5f;
    public int punchDamage = 8;
    public int kickDamage = 12;

    public float attackCooldown = 1.8f;
    public float thinkInterval = 0.25f;

    private Animator animator;
    private Rigidbody rb;

    private Model runtimeModel;
    private Worker worker;

    private bool isBusy = false;
    private bool isBlocking = false;

    private float nextAttackTime = 0f;
    private float nextThinkTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        LoadModel();
    }

    void LoadModel()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);

        Debug.Log("Adaptive AI model loaded.");
    }

    public void ReloadModel()
    {
        if (worker != null)
            worker.Dispose();

        LoadModel();
    }

    void Update()
    {
        if (enemyHealth == null || playerHealth == null)
            return;

        if (enemyHealth.IsDead() || playerHealth.IsDead())
        {
            StopMovement();
            animator.SetBool("Block", false);
            return;
        }

        if (isBusy)
            return;

        if (Time.time < nextThinkTime)
            return;

        nextThinkTime = Time.time + thinkInterval;

        ThinkAndAct();
    }

    void ThinkAndAct()
    {
        Vector3 playerPos = player.position;
        Vector3 enemyPos = transform.position;

        Vector3 playerVel = Vector3.zero;
        Vector3 enemyVel = rb.linearVelocity;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        if (playerRb != null)
            playerVel = playerRb.linearVelocity;

        float distance =
            Vector3.Distance(playerPos, enemyPos);

        float playerBlock =
            IsPlayerBlocking() ? 1f : 0f;

        float enemyBlock =
            isBlocking ? 1f : 0f;

        float[] inputData =
        {
            playerPos.x,
            playerPos.z,

            enemyPos.x,
            enemyPos.z,

            distance,

            playerHealth.GetCurrentHealth(),
            enemyHealth.GetCurrentHealth(),

            playerVel.x,
            playerVel.z,

            enemyVel.x,
            enemyVel.z,

            playerBlock,
            enemyBlock
        };

        using var inputTensor =
            new Tensor<float>(
                new TensorShape(1, 13),
                inputData
            );

        worker.Schedule(inputTensor);

        var output =
            worker.PeekOutput() as Tensor<float>;

        using var readable =
            output.ReadbackAndClone();

        int action = GetBestAction(readable);

        ExecuteAction(action);
    }

    int GetBestAction(Tensor<float> output)
    {
        float best = float.MinValue;
        int bestIndex = 0;

        for (int i = 0; i < 9; i++)
        {
            float value = output[0, i];

            if (value > best)
            {
                best = value;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    void ExecuteAction(int action)
    {
        switch (action)
        {
            case 0:
                Idle();
                break;

            case 1:
                Advance();
                break;

            case 2:
                if (CanAttack())
                    StartCoroutine(PunchAttack());
                break;

            case 3:
                if (CanAttack())
                    StartCoroutine(KickAttack());
                break;

            case 4:
                StartCoroutine(BlockRoutine(1f));
                break;

            case 5:
                Retreat();
                break;

            case 6:
                StrafeLeft();
                break;

            case 7:
                StrafeRight();
                break;

            case 8:
                AggressivePush();
                break;
        }
    }

    bool CanAttack()
    {
        if (Time.time < nextAttackTime)
            return false;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        return distance <= attackRange;
    }

    void Advance()
    {
        animator.SetBool("Block", false);

        Vector3 direction =
            (player.position - transform.position).normalized;

        direction.y = 0;

        RotateTo(direction);

        rb.linearVelocity = direction * moveSpeed;

        animator.SetFloat("Speed", 1);
    }

    void Retreat()
    {
        animator.SetBool("Block", false);

        Vector3 direction =
            (transform.position - player.position).normalized;

        direction.y = 0;

        RotateTo(direction);

        rb.linearVelocity = direction * retreatSpeed;

        animator.SetFloat("Speed", 1);
    }

    void StrafeLeft()
    {
        animator.SetBool("Block", false);

        FacePlayer();

        rb.linearVelocity =
            -transform.right * strafeSpeed;

        animator.SetFloat("Speed", 1);
    }

    void StrafeRight()
    {
        animator.SetBool("Block", false);

        FacePlayer();

        rb.linearVelocity =
            transform.right * strafeSpeed;

        animator.SetFloat("Speed", 1);
    }

    void AggressivePush()
    {
        Advance();
    }

    void Idle()
    {
        StopMovement();
        animator.SetBool("Block", false);
    }

    IEnumerator BlockRoutine(float duration)
    {
        if (isBlocking)
            yield break;

        isBusy = true;
        isBlocking = true;

        StopMovement();

        animator.SetBool("Block", true);

        yield return new WaitForSeconds(duration);

        animator.SetBool("Block", false);

        isBlocking = false;
        isBusy = false;
    }

    IEnumerator PunchAttack()
    {
        isBusy = true;
        nextAttackTime = Time.time + attackCooldown;

        StopMovement();
        FacePlayer();

        animator.SetTrigger("Punch");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPunch();

        playerHealth.TakeDamage(punchDamage);

        yield return new WaitForSeconds(1f);

        isBusy = false;
    }

    IEnumerator KickAttack()
    {
        isBusy = true;
        nextAttackTime = Time.time + attackCooldown;

        StopMovement();
        FacePlayer();

        animator.SetTrigger("Kick");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayKick();

        playerHealth.TakeDamage(kickDamage);

        yield return new WaitForSeconds(1.3f);

        isBusy = false;
    }

    void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);
    }

    void FacePlayer()
    {
        Vector3 direction =
            (player.position - transform.position).normalized;

        direction.y = 0;

        RotateTo(direction);
    }

    void RotateTo(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion target =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                target,
                8f * Time.deltaTime
            );
    }

    bool IsPlayerBlocking()
    {
        PlayerController controller =
            player.GetComponent<PlayerController>();

        if (controller == null)
            return false;

        return controller.IsBlocking();
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    void OnDestroy()
    {
        if (worker != null)
            worker.Dispose();
    }
}