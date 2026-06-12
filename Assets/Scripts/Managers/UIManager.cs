using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text supplyText;
    public TMP_Text supplyRateText;
    public TMP_Text cpText;
    public TMP_Text matchTimerText;
    public TMP_Text waveText;
    public TMP_Text abilityCooldownText;
    public Slider playerHQSlider;
    public Slider enemyHQSlider;
    public HQHealth playerHQ;
    public HQHealth enemyHQ;
    public WaveManager waveManager;

    private void Awake()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (playerHQ == null || enemyHQ == null)
        {
            foreach (var hq in FindObjectsOfType<HQHealth>())
            {
                if (hq.isPlayerHQ)
                    playerHQ = hq;
                else
                    enemyHQ = hq;
            }
        }
    }

    private bool isSubscribedToGameManager;

    private void OnEnable()
    {
        SubscribeToGameManager();

        WaveManager.OnWaveStarted += OnWaveStarted;
        WaveManager.OnWaveTimerUpdated += OnWaveTimerUpdated;

        if (playerHQ != null)
            playerHQ.OnHealthChanged += OnPlayerHQHealthChanged;
        if (enemyHQ != null)
            enemyHQ.OnHealthChanged += OnEnemyHQHealthChanged;
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();

        WaveManager.OnWaveStarted -= OnWaveStarted;
        WaveManager.OnWaveTimerUpdated -= OnWaveTimerUpdated;

        if (playerHQ != null)
            playerHQ.OnHealthChanged -= OnPlayerHQHealthChanged;
        if (enemyHQ != null)
            enemyHQ.OnHealthChanged -= OnEnemyHQHealthChanged;
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null)
        {
            UpdateSupplyText(GameManager.Instance.supplyAmount);
            UpdateSupplyRateText(GameManager.Instance.CalculateCurrentSupplyRate());
            UpdateCPText(GameManager.Instance.commandPoints);
            UpdateMatchTimer(Mathf.Max(0f, GameManager.Instance.matchDuration - GameManager.Instance.elapsedTime));
        }

        if (waveManager != null)
            UpdateWaveText(waveManager.waveNumber, waveManager.timeUntilNextWave);

        if (playerHQ != null && playerHQSlider != null)
            playerHQSlider.value = playerHQ.GetHealthPercent();

        if (enemyHQ != null && enemyHQSlider != null)
            enemyHQSlider.value = enemyHQ.GetHealthPercent();
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribedToGameManager)
            return;

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnSupplyChanged += UpdateSupplyText;
        GameManager.Instance.OnSupplyRateChanged += UpdateSupplyRateText;
        GameManager.Instance.OnCommandPointsChanged += UpdateCPText;
        GameManager.Instance.OnMatchTimeUpdated += UpdateMatchTimer;
        GameManager.Instance.OnAbilityCooldownUpdated += UpdateAbilityCooldownText;
        isSubscribedToGameManager = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribedToGameManager || GameManager.Instance == null)
            return;

        GameManager.Instance.OnSupplyChanged -= UpdateSupplyText;
        GameManager.Instance.OnSupplyRateChanged -= UpdateSupplyRateText;
        GameManager.Instance.OnCommandPointsChanged -= UpdateCPText;
        GameManager.Instance.OnMatchTimeUpdated -= UpdateMatchTimer;
        GameManager.Instance.OnAbilityCooldownUpdated -= UpdateAbilityCooldownText;
        isSubscribedToGameManager = false;
    }

    private void UpdateSupplyText(int supply)
    {
        if (supplyText != null)
            supplyText.text = $"Supply: {supply}";
    }

    private void UpdateSupplyRateText(float rate)
    {
        if (supplyRateText != null)
            supplyRateText.text = $"+{rate:F0}/s";
    }

    private void UpdateCPText(int cp)
    {
        if (cpText != null)
            cpText.text = $"CP: {cp}/{GameManager.Instance.maxCommandPoints}";
    }

    private void UpdateMatchTimer(float timeRemaining)
    {
        if (matchTimerText == null)
            return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        matchTimerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateAbilityCooldownText(AbilityType type, float cooldown)
    {
        if (abilityCooldownText == null)
            return;

        abilityCooldownText.text = cooldown > 0f
            ? $"{type}: {cooldown:0.0}s"
            : $"{type}: Ready";
    }

    private void OnWaveStarted(int wave)
    {
        UpdateWaveText(wave, waveManager != null ? waveManager.timeUntilNextWave : 0f);
    }

    private void OnWaveTimerUpdated(float timeRemaining)
    {
        UpdateWaveText(waveManager != null ? waveManager.waveNumber : 0, timeRemaining);
    }

    private void UpdateHQSliders(float gatePercent, float healthPercent)
    {
        if (playerHQSlider != null && playerHQ != null)
            playerHQSlider.value = playerHQ.GetHealthPercent();

        if (enemyHQSlider != null && enemyHQ != null)
            enemyHQSlider.value = enemyHQ.GetHealthPercent();
    }

    private void OnPlayerHQHealthChanged(float gateValue, float healthValue)
    {
        if (playerHQSlider != null)
            playerHQSlider.value = playerHQ.GetHealthPercent();
    }

    private void OnEnemyHQHealthChanged(float gateValue, float healthValue)
    {
        if (enemyHQSlider != null)
            enemyHQSlider.value = enemyHQ.GetHealthPercent();
    }

    private void UpdateWaveText(int wave, float timeRemaining)
    {
        if (waveText == null)
            return;

        waveText.text = $"Wave {wave} — {Mathf.CeilToInt(timeRemaining)}s";
    }
}
