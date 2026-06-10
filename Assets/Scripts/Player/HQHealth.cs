using UnityEngine;

public class HQHealth : MonoBehaviour
{
    public bool isPlayerHQ;
    public float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (isPlayerHQ)
            GameManager.Instance.DamagePlayerHQ(amount);
        else
            GameManager.Instance.DamageEnemyHQ(amount);
    }

    public float GetHealthPercent()
    {
        return Mathf.Clamp01(currentHealth / maxHealth);
    }
}
