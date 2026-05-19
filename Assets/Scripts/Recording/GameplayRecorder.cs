using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class FrameData
{
    public float time;

    public Vector3 playerPosition;
    public Vector3 enemyPosition;

    public Vector3 playerVelocity;
    public Vector3 enemyVelocity;

    public float distance;

    public int playerHealth;
    public int enemyHealth;

    public bool playerBlocking;
    public bool enemyBlocking;

    public string action;
}

[System.Serializable]
public class GameplayData
{
    public List<FrameData> frames =
        new List<FrameData>();
}

public class GameplayRecorder : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform enemy;

    public PlayerHealth playerHealth;
    public EnemyHealth enemyHealth;

    private PlayerController playerController;
    private AdaptiveEnemyAI adaptiveEnemyAI;

    private Rigidbody playerRb;
    private Rigidbody enemyRb;

    private GameplayData gameplayData =
        new GameplayData();

    private float timer = 0f;

    private string savePath =
        @"C:\waheed\code\AdaptiveAITrainer\data\gameplay_data.json";

    void Start()
    {
        if (player != null)
        {
            playerController =
                player.GetComponent<PlayerController>();

            playerRb =
                player.GetComponent<Rigidbody>();
        }

        if (enemy != null)
        {
            adaptiveEnemyAI =
                enemy.GetComponent<AdaptiveEnemyAI>();

            enemyRb =
                enemy.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (playerHealth == null || enemyHealth == null)
            return;

        if (playerHealth.IsDead() || enemyHealth.IsDead())
            return;

        timer += Time.deltaTime;

        RecordFrame();
    }

    void RecordFrame()
    {
        FrameData frame = new FrameData();

        frame.time = timer;

        frame.playerPosition = player.position;
        frame.enemyPosition = enemy.position;

        frame.playerVelocity =
            playerRb != null ? playerRb.linearVelocity : Vector3.zero;

        frame.enemyVelocity =
            enemyRb != null ? enemyRb.linearVelocity : Vector3.zero;

        frame.distance =
            Vector3.Distance(player.position, enemy.position);

        frame.playerHealth =
            playerHealth.GetCurrentHealth();

        frame.enemyHealth =
            enemyHealth.GetCurrentHealth();

        frame.playerBlocking =
            playerController != null &&
            playerController.IsBlocking();

        frame.enemyBlocking =
            adaptiveEnemyAI != null &&
            adaptiveEnemyAI.IsBlocking();

        frame.action = GetPlayerAction();

        gameplayData.frames.Add(frame);
    }

    string GetPlayerAction()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            return "Block";

        if (Input.GetKey(KeyCode.J))
            return "Punch";

        if (Input.GetKey(KeyCode.K))
            return "Kick";

        if (Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0)
            return "Move";

        return "Idle";
    }

    public void SaveGameplay()
    {
        string json =
            JsonUtility.ToJson(gameplayData, true);

        File.WriteAllText(savePath, json);

        Debug.Log("Gameplay Saved: " + savePath);
    }

    public void ResetRecording()
    {
        gameplayData = new GameplayData();
        timer = 0f;
    }
}