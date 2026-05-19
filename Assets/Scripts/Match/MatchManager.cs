using UnityEngine;
using System.Collections;
using System.Diagnostics;
using TMPro;

public class MatchManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject player;
    public GameObject enemy;

    [Header("Spawn Points")]
    public Transform playerSpawn;
    public Transform enemySpawn;

    [Header("UI")]
    public TMP_Text roundText;
    public TMP_Text resultText;
    public TMP_Text adaptiveText;

    [Header("Round Settings")]
    public float deathAnimationWait = 2.5f;
    public float roundResetDelay = 2f;
    public float resultTextDuration = 2f;

    [Header("Training")]
    public string pythonExe =
        @"C:\Users\mohmm\anaconda3\envs\adaptiveai\python.exe";

    public string trainerPath =
        @"C:\waheed\code\AdaptiveAITrainer\train.py";

    public string trainerWorkingDir =
        @"C:\waheed\code\AdaptiveAITrainer";

    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;

    private GameplayRecorder gameplayRecorder;
    private AdaptiveEnemyAI adaptiveEnemyAI;

    private bool roundEnded = false;

    private int roundNumber = 1;

    void Start()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = enemy.GetComponent<EnemyHealth>();

        gameplayRecorder = FindObjectOfType<GameplayRecorder>();
        adaptiveEnemyAI = enemy.GetComponent<AdaptiveEnemyAI>();

        UpdateRoundUI();

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (adaptiveText != null)
            adaptiveText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (roundEnded)
            return;

        if (playerHealth.IsDead())
        {
            ShowResult("YOU LOSE");
            StartCoroutine(EndRound());
        }
        else if (enemyHealth.IsDead())
        {
            ShowResult("YOU WIN");
            StartCoroutine(EndRound());
        }
    }

    void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true);
        }
    }

    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = "ROUND " + roundNumber;
        }
    }

    IEnumerator EndRound()
    {
        roundEnded = true;

        yield return new WaitForSeconds(deathAnimationWait);

        if (gameplayRecorder != null)
            gameplayRecorder.SaveGameplay();

        RunTraining();

        if (adaptiveText != null)
        {
            adaptiveText.text = "ENEMY ADAPTED!";
            adaptiveText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(roundResetDelay);

        if (adaptiveEnemyAI != null)
            adaptiveEnemyAI.ReloadModel();

        ResetRound();
    }

    void RunTraining()
    {
        Process process = new Process();

        process.StartInfo.FileName = pythonExe;
        process.StartInfo.Arguments = $"\"{trainerPath}\"";

        process.StartInfo.WorkingDirectory =
            trainerWorkingDir;

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        string output =
            process.StandardOutput.ReadToEnd();

        string error =
            process.StandardError.ReadToEnd();

        process.WaitForExit();

        UnityEngine.Debug.Log(output);

        if (!string.IsNullOrEmpty(error))
            UnityEngine.Debug.LogError(error);

        UnityEngine.Debug.Log("FAST adaptive training complete.");
    }

    void ResetRound()
    {
        roundNumber++;

        player.transform.position = playerSpawn.position;
        player.transform.rotation = playerSpawn.rotation;

        enemy.transform.position = enemySpawn.position;
        enemy.transform.rotation = enemySpawn.rotation;

        Rigidbody playerRb =
            player.GetComponent<Rigidbody>();

        Rigidbody enemyRb =
            enemy.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector3.zero;
            enemyRb.angularVelocity = Vector3.zero;
        }

        playerHealth.ResetHealth();
        enemyHealth.ResetHealth();

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (adaptiveText != null)
            adaptiveText.gameObject.SetActive(false);

        UpdateRoundUI();

        roundEnded = false;

        UnityEngine.Debug.Log(
            "FAST adaptive round " +
            roundNumber +
            " started."
        );
    }
}