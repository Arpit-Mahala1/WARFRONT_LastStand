using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AbilityType
{
    Airstrike,
    ReinforcementDrop,
    SmokeScreen,
    ArtilleryBarrage
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Resources")]
    public int initialSupply = 150;
    public int supplyAmount;
    public int commandPoints;
    public int maxCommandPoints = 10;
    public float cpRegenPerSecond = 1f;

    [Header("Match")]
    public float matchDuration = 1200f;
    public float elapsedTime;
    public bool gameIsOver;
    public HQHealth playerHQ;
    public HQHealth enemyHQ;
    public List<CaptureZone> captureZones = new List<CaptureZone>();

    [Header("Supply Rates")]
    public int flagSupplyRate = 8;
    public int commandPostSupplyRate = 20;

    [Header("Engineer Limits")]
    public int maxEngineers = 2;
    public int activeEngineers;

    public static bool PlayerWon;

    public event Action<int> OnSupplyChanged;
    public event Action<int> OnCommandPointsChanged;
    public event Action<float> OnSupplyRateChanged;
    public event Action<float> OnMatchTimeUpdated;
    public event Action<bool> OnGameOver;
    public event Action<AbilityType, float> OnAbilityCooldownUpdated;
    public event Action<int, int> OnControlPointUpdate;

    private Dictionary<AbilityType, float> abilityCooldownRemaining = new Dictionary<AbilityType, float>();
    private float supplyAccumulator;
    private float cpAccumulator;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        supplyAmount = initialSupply;
        commandPoints = 0;
        elapsedTime = 0f;
        gameIsOver = false;

        InitializeCache();
        UpdateSupplyUI();
        UpdateCommandPointUI();
        UpdateMatchTimeUI();
        UpdateAbilityCooldowns();
    }

    private void Update()
    {
        if (gameIsOver)
            return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= matchDuration)
        {
            ResolveMatchOutcome();
            return;
        }

        if (supplyAmount < 20 && CalculateCurrentSupplyRate() <= 0f)
        {
            if (FindObjectsOfType<UnitHealth>().Length == 0)
            {
                EndGame(false);
                return;
            }
        }

        cpAccumulator += cpRegenPerSecond * Time.deltaTime;
        if (cpAccumulator >= 1f)
        {
            int gained = Mathf.FloorToInt(cpAccumulator);
            cpAccumulator -= gained;
            AddCommandPoints(gained);
        }

        float supplyRate = CalculateCurrentSupplyRate();
        supplyAccumulator += supplyRate * Time.deltaTime;
        if (supplyAccumulator >= 1f)
        {
            int gained = Mathf.FloorToInt(supplyAccumulator);
            supplyAccumulator -= gained;
            AddSupply(gained);
        }

        UpdateSupplyRateUI(supplyRate);
        UpdateMatchTimeUI();
        TickAbilityCooldowns(Time.deltaTime);
    }

    private void InitializeCache()
    {
        if (captureZones == null || captureZones.Count == 0)
        {
            captureZones = new List<CaptureZone>(FindObjectsOfType<CaptureZone>());
        }

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

        foreach (AbilityType ability in Enum.GetValues(typeof(AbilityType)))
        {
            if (!abilityCooldownRemaining.ContainsKey(ability))
                abilityCooldownRemaining.Add(ability, 0f);
        }
    }

    public void AddSupply(int amount)
    {
        supplyAmount = Mathf.Max(0, supplyAmount + amount);
        UpdateSupplyUI();
    }

    public bool SpendSupply(int amount)
    {
        if (amount <= 0)
            return true;

        if (supplyAmount < amount)
            return false;

        supplyAmount -= amount;
        UpdateSupplyUI();
        return true;
    }

    public void AddCommandPoints(int amount)
    {
        if (amount <= 0)
            return;

        commandPoints = Mathf.Min(maxCommandPoints, commandPoints + amount);
        UpdateCommandPointUI();
    }

    public bool SpendCommandPoints(int amount)
    {
        if (commandPoints < amount)
            return false;

        commandPoints -= amount;
        UpdateCommandPointUI();
        return true;
    }

    public bool CanUseAbility(AbilityType type, int cost)
    {
        return abilityCooldownRemaining[type] <= 0f && commandPoints >= cost;
    }

    public void StartAbilityCooldown(AbilityType type, float cooldown)
    {
        abilityCooldownRemaining[type] = cooldown;
        OnAbilityCooldownUpdated?.Invoke(type, cooldown);
    }

    private void TickAbilityCooldowns(float deltaTime)
    {
        bool changed = false;
        foreach (var key in new List<AbilityType>(abilityCooldownRemaining.Keys))
        {
            if (abilityCooldownRemaining[key] <= 0f)
                continue;

            abilityCooldownRemaining[key] = Mathf.Max(0f, abilityCooldownRemaining[key] - deltaTime);
            OnAbilityCooldownUpdated?.Invoke(key, abilityCooldownRemaining[key]);
            changed = true;
        }

        if (changed)
            UpdateAbilityCooldowns();
    }

    public float GetAbilityCooldown(AbilityType type)
    {
        return abilityCooldownRemaining.ContainsKey(type) ? abilityCooldownRemaining[type] : 0f;
    }

    public int GetControlledPlayerFlagCount()
    {
        if (captureZones == null)
            return 0;

        int count = 0;
        foreach (var zone in captureZones)
            if (zone.currentState == CaptureState.Player)
                count++;
        return count;
    }

    public int GetControlledEnemyFlagCount()
    {
        if (captureZones == null)
            return 0;

        int count = 0;
        foreach (var zone in captureZones)
            if (zone.currentState == CaptureState.Enemy)
                count++;
        return count;
    }

    public float CalculateCurrentSupplyRate()
    {
        float rate = 0f;
        foreach (var zone in captureZones)
        {
            if (zone.currentState == CaptureState.Player)
                rate += zone.pointType == PointType.CommandPost ? commandPostSupplyRate : flagSupplyRate;
        }

        return rate;
    }

    public void RegisterEngineerCount(int amount)
    {
        activeEngineers = Mathf.Clamp(activeEngineers + amount, 0, maxEngineers);
    }

    public static float CurrentTimeTaken;
    public static int CurrentWavesSurvived;

    public void EndGame(bool playerWon)
    {
        if (gameIsOver)
            return;

        gameIsOver = true;
        PlayerWon = playerWon;

        CurrentTimeTaken = elapsedTime;
        var waveManager = FindObjectOfType<WaveManager>();
        CurrentWavesSurvived = waveManager != null ? waveManager.waveNumber : 0;

        if (playerWon)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
            if (CurrentTimeTaken < bestTime)
                PlayerPrefs.SetFloat("BestTime", CurrentTimeTaken);
        }
        else
        {
            int bestWaves = PlayerPrefs.GetInt("BestWaves", 0);
            if (CurrentWavesSurvived > bestWaves)
                PlayerPrefs.SetInt("BestWaves", CurrentWavesSurvived);
        }
        PlayerPrefs.Save();

        OnGameOver?.Invoke(playerWon);
        SceneManager.LoadScene("GameOver");
    }

    private void ResolveMatchOutcome()
    {
        if (playerHQ == null || enemyHQ == null)
        {
            EndGame(true);
            return;
        }

        int playerFlags = GetControlledPlayerFlagCount();
        int enemyFlags = GetControlledEnemyFlagCount();

        if (playerFlags > enemyFlags)
        {
            EndGame(true);
            return;
        }

        if (enemyFlags > playerFlags)
        {
            EndGame(false);
            return;
        }

        int playerUnits = FindObjectsOfType<UnitHealth>().Length;
        int enemyUnits = FindObjectsOfType<EnemyHealth>().Length;
        EndGame(playerUnits >= enemyUnits);
    }

    private void UpdateSupplyUI()
    {
        OnSupplyChanged?.Invoke(supplyAmount);
    }

    private void UpdateCommandPointUI()
    {
        OnCommandPointsChanged?.Invoke(commandPoints);
    }

    public void RaiseAbilityCooldownUpdate(AbilityType ability, float remaining)
    {
        OnAbilityCooldownUpdated?.Invoke(ability, remaining);
    }

    private void UpdateSupplyRateUI(float rate)
    {
        OnSupplyRateChanged?.Invoke(rate);
    }

    private void UpdateMatchTimeUI()
    {
        OnMatchTimeUpdated?.Invoke(Mathf.Max(0f, matchDuration - elapsedTime));
    }

    private void UpdateAbilityCooldowns()
    {
        foreach (var entry in abilityCooldownRemaining)
        {
            OnAbilityCooldownUpdated?.Invoke(entry.Key, entry.Value);
        }
    }
}
