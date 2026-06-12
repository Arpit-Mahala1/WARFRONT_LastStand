using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public PlayerSpawner playerSpawner;
    public GameObject airstrikePrefab;
    public GameObject smokeScreenPrefab;
    public GameObject artilleryShellPrefab;
    public Transform playerHQSpawnPoint;

    private Dictionary<AbilityType, float> cooldowns = new Dictionary<AbilityType, float>
    {
        { AbilityType.Airstrike, 45f },
        { AbilityType.ReinforcementDrop, 60f },
        { AbilityType.SmokeScreen, 30f },
        { AbilityType.ArtilleryBarrage, 90f }
    };

    private Dictionary<AbilityType, int> costs = new Dictionary<AbilityType, int>
    {
        { AbilityType.Airstrike, 5 },
        { AbilityType.ReinforcementDrop, 4 },
        { AbilityType.SmokeScreen, 2 },
        { AbilityType.ArtilleryBarrage, 8 }
    };

    private Dictionary<AbilityType, float> cooldownRemaining = new Dictionary<AbilityType, float>();

    private void Start()
    {
        foreach (var entry in cooldowns)
            cooldownRemaining[entry.Key] = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        foreach (var ability in new List<AbilityType>(cooldownRemaining.Keys))
        {
            if (cooldownRemaining[ability] <= 0f)
                continue;

            cooldownRemaining[ability] = Mathf.Max(0f, cooldownRemaining[ability] - Time.deltaTime);
            GameManager.Instance.RaiseAbilityCooldownUpdate(ability, cooldownRemaining[ability]);
        }
    }

    public bool TryUseAirstrike(Vector3 targetPosition)
    {
        if (!TrySpendAbility(AbilityType.Airstrike))
            return false;

        StartCoroutine(PerformAirstrike(targetPosition));
        return true;
    }

    public bool TryUseReinforcementDrop()
    {
        if (!TrySpendAbility(AbilityType.ReinforcementDrop))
            return false;

        if (playerSpawner == null)
            playerSpawner = FindObjectOfType<PlayerSpawner>();

        if (playerSpawner == null)
            return false;

        playerSpawner.SpawnReinforcements(3);
        return true;
    }

    public bool TryUseSmokeScreen(Vector3 targetPosition)
    {
        if (!TrySpendAbility(AbilityType.SmokeScreen))
            return false;

        if (smokeScreenPrefab != null)
            Instantiate(smokeScreenPrefab, targetPosition, Quaternion.identity);

        StartCooldown(AbilityType.SmokeScreen);
        return true;
    }

    public bool TryUseArtilleryBarrage(Vector3 lineStart, Vector3 lineEnd)
    {
        if (!TrySpendAbility(AbilityType.ArtilleryBarrage))
            return false;

        StartCoroutine(PerformArtilleryBarrage(lineStart, lineEnd));
        return true;
    }

    private IEnumerator PerformAirstrike(Vector3 position)
    {
        if (airstrikePrefab != null)
            Instantiate(airstrikePrefab, position, Quaternion.identity);

        yield return new WaitForSeconds(3f);

        Collider[] hits = Physics.OverlapSphere(position, 7f);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(100f);
                continue;
            }

            HQHealth hq = hit.GetComponent<HQHealth>();
            if (hq != null)
                hq.TakeDamage(100f);
        }

        StartCooldown(AbilityType.Airstrike);
    }

    private IEnumerator PerformArtilleryBarrage(Vector3 lineStart, Vector3 lineEnd)
    {
        int rounds = 10;
        for (int i = 0; i < rounds; i++)
        {
            Vector3 point = Vector3.Lerp(lineStart, lineEnd, i / (float)(rounds - 1));
            if (artilleryShellPrefab != null)
                Instantiate(artilleryShellPrefab, point, Quaternion.identity);

            Collider[] hits = Physics.OverlapSphere(point, 3f);
            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(80f);
                    continue;
                }

                HQHealth hq = hit.GetComponent<HQHealth>();
                if (hq != null)
                    hq.TakeDamage(80f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        StartCooldown(AbilityType.ArtilleryBarrage);
    }

    private bool TrySpendAbility(AbilityType type)
    {
        if (GameManager.Instance == null)
            return false;

        if (cooldownRemaining[type] > 0f)
            return false;

        int cost = costs[type];
        if (!GameManager.Instance.SpendCommandPoints(cost))
            return false;

        StartCooldown(type);
        return true;
    }

    private void StartCooldown(AbilityType type)
    {
        cooldownRemaining[type] = cooldowns[type];
        GameManager.Instance.StartAbilityCooldown(type, cooldowns[type]);
    }
}
