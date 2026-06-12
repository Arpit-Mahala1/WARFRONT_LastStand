using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StructureHealth))]
public class Turret : MonoBehaviour
{
    public float attackRange = 10f;
    public float fireRate = 1.2f;
    public float damage = 12f;
    public LayerMask enemyLayer;
    public bool isOperational = true;

    private StructureHealth structureHealth;
    private float lastShotTime;

    private void Awake()
    {
        structureHealth = GetComponent<StructureHealth>();
    }

    private void Update()
    {
        if (structureHealth == null || structureHealth.isDestroyed)
            return;

        if (structureHealth.currentHealth / structureHealth.maxHealth < 0.4f)
            isOperational = false;
        else
            isOperational = true;

        if (!isOperational)
            return;

        if (Time.time - lastShotTime < 1f / fireRate)
            return;

        Transform target = FindTargetInRange();
        if (target == null)
            return;

        Shoot(target);
        lastShotTime = Time.time;
    }

    private Transform FindTargetInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        Transform bestTarget = null;
        float nearest = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null)
                continue;

            float distance = Vector3.SqrMagnitude(transform.position - hit.transform.position);
            if (distance < nearest)
            {
                nearest = distance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private void Shoot(Transform target)
    {
        if (target == null)
            return;

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
