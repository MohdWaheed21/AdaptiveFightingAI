using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Health Bars")]
    public Slider playerHealthBar;
    public Slider enemyHealthBar;

    [Header("Texts")]
    public TMP_Text roundText;
    public TMP_Text resultText;
    public TMP_Text adaptedPopup;

    [Header("References")]
    public PlayerHealth playerHealth;
    public EnemyHealth enemyHealth;

    private int currentRound = 1;

    void Start()
    {
        resultText.gameObject.SetActive(false);
        adaptedPopup.gameObject.SetActive(false);

        roundText.color = Color.white;

        adaptedPopup.color = Color.cyan;

        UpdateRoundText();
    }

    void Update()
    {
        UpdateHealthBars();
    }

    void UpdateHealthBars()
    {
        if (playerHealth != null)
        {
            playerHealthBar.maxValue = playerHealth.maxHealth;
            playerHealthBar.value = playerHealth.GetCurrentHealth();
        }

        if (enemyHealth != null)
        {
            enemyHealthBar.maxValue = enemyHealth.maxHealth;
            enemyHealthBar.value = enemyHealth.GetCurrentHealth();
        }
    }

    void UpdateRoundText()
    {
        roundText.text = "ROUND " + currentRound;
        roundText.color = Color.white;
    }

    public void ShowResult(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowResultRoutine(message));
    }

    IEnumerator ShowResultRoutine(string message)
    {
        resultText.text = message;

        if (message == "YOU WIN")
            resultText.color = Color.green;
        else
            resultText.color = Color.red;

        resultText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        resultText.gameObject.SetActive(false);
    }

    public void NextRound()
    {
        currentRound++;
        UpdateRoundText();
    }

    public void ShowAdaptedPopup()
    {
        StopCoroutine("AdaptPopupRoutine");
        StartCoroutine(AdaptPopupRoutine());
    }

    IEnumerator AdaptPopupRoutine()
    {
        adaptedPopup.text = "ENEMY ADAPTED!";
        adaptedPopup.color = Color.cyan;

        adaptedPopup.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        adaptedPopup.gameObject.SetActive(false);
    }
}