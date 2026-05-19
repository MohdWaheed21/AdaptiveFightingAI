using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip punchSound;
    public AudioClip kickSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void PlayPunch()
    {
        if (punchSound != null)
            audioSource.PlayOneShot(punchSound);
    }

    public void PlayKick()
    {
        if (kickSound != null)
            audioSource.PlayOneShot(kickSound);
    }

    public void PlayHit()
    {
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }

    public void PlayDeath()
    {
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);
    }
}