using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitCombat : MonoBehaviour
{
    public float attackRange = 6f;
    public float detectionRange = 10f;
    public float attackDamage = 15f;
    public float attackCooldown = 1f;
    public float rotationSpeed = 10f;
    public bool isRanged = true;
    public LayerMask enemyLayer;
    public LayerMask obstacleMask;

    private UnitMovement movement;
    private UnitHealth sourceHealth;
    private Transform currentTarget;
    private EnemyHealth currentEnemyHealth;
    private HQHealth currentHQHealth;
    private float lastAttackTime;

    private void Awake()
    {
        movement = GetComponent<UnitMovement>();
        sourceHealth = GetComponent<UnitHealth>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(ScanForTargets), 0.2f, 0.25f);
        InvokeRepeating(nameof(TryAttack), 0.3f, 0.1f);
    }

    private void ScanForTargets()
    {
        if (sourceHealth != null && sourceHealth.IsRetreating)
        {
            movement?.MoveToPosition(GameManager.Instance.playerHQ.transform.position);
            return;
        }

        if (currentTarget == null || (currentEnemyHealth == null && currentHQHealth == null))
            AcquireNearestEnemy();

        if (currentTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            movement?.StopMoving();
            FaceTarget();
            return;
        }

        movement?.MoveToPosition(currentTarget.position);
    }

    private void AcquireNearestEnemy()
    {
        if (currentEnemyHealth != null)
            currentEnemyHealth.OnDeath -= OnTargetDeath;

        currentTarget = null;
        currentEnemyHealth = null;
        currentHQHealth = null;

        LayerMask searchMask = enemyLayer.value != 0 ? enemyLayer : ~0;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, searchMask);
        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            HQHealth hqHealth = hit.GetComponent<HQHealth>();
            if (enemyHealth == null && hqHealth == null)
                continue;

            if (hqHealth != null && hqHealth.IsGateAlive && Vector3.Distance(transform.position, hit.transform.position) <= detectionRange)
            {
                currentTarget = hit.transform;
                currentHQHealth = hqHealth;
                return;
            }

            float distance = Vector3.SqrMagnitude(transform.position - hit.transform.position);
            if (distance < bestScore)
            {
                bestScore = distance;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget == null)
            return;

        currentTarget = bestTarget;
        currentEnemyHealth = currentTarget.GetComponent<EnemyHealth>();
        currentHQHealth = currentTarget.GetComponent<HQHealth>();

        if (currentEnemyHealth != null)
            currentEnemyHealth.OnDeath += OnTargetDeath;
    }

    private void TryAttack()
    {
        if (currentTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance > attackRange)
            return;

        if (isRanged && !HasLineOfSight(currentTarget))
            return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        float finalDamage = attackDamage;
        if (sourceHealth != null)
            finalDamage *= sourceHealth.DamageMultiplier;

        finalDamage = ApplyFlankingBonus(currentTarget, finalDamage);

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.TakeDamage(finalDamage);
        }
        else if (currentHQHealth != null)
        {
            currentHQHealth.TakeDamage(finalDamage);
        }

        lastAttackTime = Time.time;
    }

    private float ApplyFlankingBonus(Transform target, float baseDamage)
    {
        if (target == null)
            return baseDamage;

        Vector3 incoming = (transform.position - target.position).normalized;
        float dot = Vector3.Dot(incoming, target.forward);

        if (dot < -0.5f)
            return baseDamage * 2f;

        if (dot < 0.2f)
            return baseDamage * 1.5f;

        return baseDamage;
    }

    private bool HasLineOfSight(Transform target)
    {
        if (target == null)
            return false;

        if (SmokeScreen.ActiveScreens != null)
        {
            foreach (SmokeScreen smoke in SmokeScreen.ActiveScreens)
            {
                if (smoke != null && smoke.ContainsPoint(target.position))
                    return false;
            }
        }

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 direction = target.position + Vector3.up * 1f - origin;
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, attackRange, obstacleMask))
        {
            return hit.transform == target;
        }

        return true;
    }

    private void FaceTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnTargetDeath()
    {
        if (currentEnemyHealth != null)
            currentEnemyHealth.OnDeath -= OnTargetDeath;

        currentTarget = null;
        currentEnemyHealth = null;
        currentHQHealth = null;
    }
}
