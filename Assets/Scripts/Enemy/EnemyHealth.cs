using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 40f;
    private float currentHealth;

    public event Action OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
