using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 40f;
    public float currentHealth;
    private bool initialized;

    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (!initialized)
            currentHealth = maxHealth;
    }

    public void InitializeHealth(float max)
    {
        maxHealth = max;
        currentHealth = max;
        initialized = true;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= amount;

        // Show GREEN damage text — player is dealing damage to enemy
        if (DamageTextManager.Instance != null)
            DamageTextManager.Instance.SpawnDamageText(transform.position, amount, false);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
