using System;
using UnityEngine;

public class HQHealth : MonoBehaviour
{
    public bool isPlayerHQ;
    public float gateMaxHealth = 200f;
    public float gateCurrentHealth;
    public float maxHealth = 1000f;
    public float currentHealth;
    public bool gateDestroyed;

    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        gateCurrentHealth = gateMaxHealth;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        gateCurrentHealth = Mathf.Clamp(gateCurrentHealth, 0f, gateMaxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(GetGatePercent(), GetHealthPercent());
    }

    public void TakeDamage(float amount)
    {
        // isPlayerHQ → enemy is attacking player → RED text
        // enemy HQ   → player is attacking enemy → GREEN text
        bool playerSideDamage = isPlayerHQ;

        if (!gateDestroyed)
        {
            gateCurrentHealth = Mathf.Max(0f, gateCurrentHealth - amount);
            if (gateCurrentHealth <= 0f)
            {
                gateDestroyed = true;
                gateCurrentHealth = 0f;
            }

            if (DamageTextManager.Instance != null)
                DamageTextManager.Instance.SpawnDamageText(transform.position, amount, playerSideDamage);

            OnHealthChanged?.Invoke(GetGatePercent(), GetHealthPercent());
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (DamageTextManager.Instance != null)
            DamageTextManager.Instance.SpawnDamageText(transform.position, amount, playerSideDamage);

        OnHealthChanged?.Invoke(GetGatePercent(), GetHealthPercent());

        if (currentHealth <= 0f)
        {
            if (isPlayerHQ)
                GameManager.Instance.EndGame(false);
            else
                GameManager.Instance.EndGame(true);
        }
    }

    public void RepairGate(float amount)
    {
        if (gateDestroyed)
            return;

        gateCurrentHealth = Mathf.Min(gateMaxHealth, gateCurrentHealth + amount);
        OnHealthChanged?.Invoke(GetGatePercent(), GetHealthPercent());
    }

    public bool IsGateAlive => !gateDestroyed;

    public float GetGatePercent()
    {
        return gateMaxHealth <= 0f ? 0f : Mathf.Clamp01(gateCurrentHealth / gateMaxHealth);
    }

    public float GetHealthPercent()
    {
        return maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
    }
}
