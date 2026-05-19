using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead = false;

    private Animator animator;
    private PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (playerController != null &&
            playerController.IsBlocking())
        {
            damage = Mathf.RoundToInt(damage * 0.3f);
        }

        currentHealth -= damage;

        Debug.Log("Player Health: " + currentHealth);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHit();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (playerController != null)
            playerController.TriggerHitReaction();
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeath();

        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Punch");
        animator.ResetTrigger("Kick");

        animator.SetBool("Block", false);
        animator.SetFloat("Speed", 0f);

        animator.Play("Death", 0, 0f);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        animator.Rebind();
        animator.Update(0f);
    }
}