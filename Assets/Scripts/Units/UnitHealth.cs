using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    public float currentHealth;

    public float morale = 80f;
    public float maxMorale = 100f;
    public float moraleRecoverRate = 1f;
    public float moraleDropOnDamage = 10f;
    public float moraleDropOnAllyDeath = 5f;
    public float lowMoraleSpeedMultiplier = 0.7f;
    public float lowMoraleDamageMultiplier = 0.7f;
    public float retreatMoraleThreshold = 10f;
    public float recoverMoraleThreshold = 40f;

    public bool IsRetreating { get; private set; }
    public float DamageMultiplier => morale < 30f ? lowMoraleDamageMultiplier : 1f;
    public float SpeedMultiplier => morale < 30f ? lowMoraleSpeedMultiplier : 1f;

    public event Action OnDeath;
    public event Action<float> OnMoraleChanged;

    private float lastCombatTime;
    private float combatCooldown = 1.5f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (currentHealth <= 0f)
            return;

        if (Time.time - lastCombatTime > combatCooldown)
        {
            ChangeMorale(moraleRecoverRate * Time.deltaTime);
        }
    }

    public void InitializeHealth(float max)
    {
        maxHealth = max;
        currentHealth = max;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        lastCombatTime = Time.time;
        ChangeMorale(-moraleDropOnDamage);

        // Show RED damage text — enemy is dealing damage to player unit
        if (DamageTextManager.Instance != null)
            DamageTextManager.Instance.SpawnDamageText(transform.position, amount, true);

        if (currentHealth <= 0f)
            Die();
    }

    public void NotifyAllyDeath()
    {
        ChangeMorale(-moraleDropOnAllyDeath);
    }

    private void ChangeMorale(float amount)
    {
        float previous = morale;
        morale = Mathf.Clamp(morale + amount, 0f, maxMorale);
        if (morale <= retreatMoraleThreshold)
            IsRetreating = true;
        else if (IsRetreating && morale >= recoverMoraleThreshold)
            IsRetreating = false;

        if (!Mathf.Approximately(previous, morale))
            OnMoraleChanged?.Invoke(morale);
    }

    private void Die()
    {
        NotifyNearbyAllies();
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    private void NotifyNearbyAllies()
    {
        float radius = 5f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            UnitHealth ally = hit.GetComponent<UnitHealth>();
            if (ally != null)
                ally.NotifyAllyDeath();
        }
    }
}
