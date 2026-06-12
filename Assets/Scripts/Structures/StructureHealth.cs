using System;
using UnityEngine;

public class StructureHealth : MonoBehaviour
{
    public float maxHealth = 120f;
    public float currentHealth;
    public bool isDestroyed;
    public event Action OnDestroyed;
    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDestroyed)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        // Show RED damage text — enemies are damaging player structures
        if (DamageTextManager.Instance != null)
            DamageTextManager.Instance.SpawnDamageText(transform.position, amount, true);

        if (currentHealth <= 0f)
            DestroyStructure();
    }

    public void Repair(float amount)
    {
        if (isDestroyed)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void DestroyStructure()
    {
        isDestroyed = true;
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
