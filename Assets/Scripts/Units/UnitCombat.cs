using UnityEngine;
using UnityEngine.AI;

public class UnitCombat : MonoBehaviour
{
    public float attackRange = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer;

    private void Start()
    {
        InvokeRepeating("TryAttack", 1f, attackCooldown);
    }

    private void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (hits.Length == 0)
            return;

        // Find the closest enemy collider
        Collider closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit;
            }
        }

        if (closest == null)
            return;

        EnemyHealth enemyHealth = closest.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
            return;

        // Flanking: direction from enemy to this unit, dotted against enemy's forward
        Transform enemyTransform = closest.transform;
        Vector3 directionFromEnemy = (transform.position - enemyTransform.position).normalized;
        float dot = Vector3.Dot(directionFromEnemy, enemyTransform.forward);

        float finalDamage = dot < 0.5f ? attackDamage * 1.5f : attackDamage;

        enemyHealth.TakeDamage(finalDamage);
    }
}
