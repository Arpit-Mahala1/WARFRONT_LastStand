using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    public UnitType enemyType = UnitType.BasicSoldier;
    public float detectionRange = 12f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;
    public GameObject[] patrolWaypoints;
    public LayerMask playerLayer;
    public float chaseSpeed = 4.5f;
    public float patrolSpeed = 2.5f;
    public float waypointReachDistance = 0.5f;
    public float attackRotationSpeed = 10f;

    private int waypointIndex;
    private Transform target;
    private float lastAttackTime;
    private NavMeshAgent agent;
    private Vector3 currentDestination;

    // FIX 3: Track whether we've set a patrol destination so we don't
    // re-issue SetDestination every tick, and can detect "arrived"
    private bool hasPatrolDestination = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = UnityEngine.Random.Range(10, 90);
        agent.updateRotation = true;
    }

    private void Start()
    {
        ApplyEnemyTypeStats();

        // FIX 1: Randomize starting waypoint index so enemies don't all
        // head to waypoints[0] simultaneously
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            waypointIndex = Random.Range(0, patrolWaypoints.Length);
        else
            waypointIndex = 0;

        InvokeRepeating(nameof(UpdateState), 0.1f, 0.25f);
    }

    private void ApplyEnemyTypeStats()
    {
        switch (enemyType)
        {
            case UnitType.HeavyGunner:
                attackDamage = 18f;
                attackRange = 2.2f;
                attackCooldown = 1.5f;
                chaseSpeed = 3.2f;
                patrolSpeed = 1.8f;
                break;
            case UnitType.Marksman:
                attackDamage = 20f;
                attackRange = 10f;
                attackCooldown = 1.8f;
                chaseSpeed = 2.5f;
                patrolSpeed = 2f;
                break;
            case UnitType.Sapper:
                attackDamage = 8f;
                attackRange = 1.8f;
                attackCooldown = 1.3f;
                chaseSpeed = 3.5f;
                patrolSpeed = 2.2f;
                break;
            default:
                attackDamage = 12f;
                attackRange = 2f;
                attackCooldown = 1.2f;
                chaseSpeed = 4.5f;
                patrolSpeed = 2.5f;
                break;
        }

        agent.stoppingDistance = attackRange * 0.75f;
    }

    public void SetWaypoints(GameObject[] waypoints)
    {
        patrolWaypoints = waypoints;
        // FIX 1: Also randomize here when waypoints are assigned at runtime
        waypointIndex = (patrolWaypoints != null && patrolWaypoints.Length > 0)
            ? Random.Range(0, patrolWaypoints.Length)
            : 0;
        // FIX 3: Force a fresh destination when waypoints change
        hasPatrolDestination = false;
    }

    private void UpdateState()
    {
        if (target == null)
            FindNearestTarget();

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
        }
    }

    private void FindNearestTarget()
    {
        LayerMask searchMask = playerLayer.value != 0 ? playerLayer : ~0;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, searchMask);
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            UnitHealth unitHealth = hit.GetComponent<UnitHealth>();
            HQHealth hqHealth = hit.GetComponent<HQHealth>();
            if (unitHealth == null && hqHealth == null)
                continue;

            if (hqHealth != null && hqHealth.IsGateAlive)
            {
                closest = hit.transform;
                break;
            }

            float distance = Vector3.SqrMagnitude(transform.position - hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit.transform;
            }
        }

        if (closest != null)
        {
            target = closest;
            currentState = State.Chase;
        }
    }

    private void HandlePatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f; // Fix: Agent must not stop early during patrol

        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            agent.ResetPath();
            return;
        }

        // FIX 2 + 3: Issue the first destination if we don't have one yet
        // (covers the "just returned from chase/attack" case too)
        if (!hasPatrolDestination)
        {
            AdvanceToNextWaypoint();
            return;
        }

        // FIX 2: Check arrival by distance to the actual destination we stored,
        // not agent.remainingDistance (which can be unreliable at 0.25 s ticks)
        float distToDest = Vector3.Distance(transform.position, currentDestination);
        bool agentArrived = !agent.pathPending
                            && (agent.remainingDistance <= waypointReachDistance
                                || distToDest <= waypointReachDistance);

        // Also catch a stale/invalid path so the enemy never freezes
        bool pathBroken = agent.pathStatus == NavMeshPathStatus.PathInvalid
                          || (!agent.hasPath && !agent.pathPending);

        if (agentArrived || pathBroken)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void AdvanceToNextWaypoint()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            return;

        // Guard against null entries in the waypoint array
        int attempts = 0;
        while (patrolWaypoints[waypointIndex] == null && attempts < patrolWaypoints.Length)
        {
            waypointIndex = (waypointIndex + 1) % patrolWaypoints.Length;
            attempts++;
        }
        if (patrolWaypoints[waypointIndex] == null) return;

        currentDestination = patrolWaypoints[waypointIndex].transform.position;
        agent.SetDestination(currentDestination);
        hasPatrolDestination = true;

        // FIX 1 (extra spread): Skip a random number of waypoints occasionally
        // so enemies that share a waypoint array drift apart over time
        int step = (patrolWaypoints.Length > 2 && Random.value < 0.35f)
                   ? Random.Range(1, Mathf.Min(3, patrolWaypoints.Length))
                   : 1;
        waypointIndex = (waypointIndex + step) % patrolWaypoints.Length;
    }

    private void HandleChase()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.stoppingDistance = attackRange * 0.75f; // Fix: Restore stopping distance for chase/attack
        agent.SetDestination(GetChaseDestination());

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            currentState = State.Attack;
            agent.isStopped = true;
            return;
        }

        if (distance > detectionRange * 1.5f)
        {
            target = null;
            ReturnToPatrol();
        }
    }

    private void HandleAttack()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange + 0.2f)
        {
            currentState = State.Chase;
            agent.isStopped = false;
            return;
        }

        FaceTarget();

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        UnitHealth unitHealth = target.GetComponent<UnitHealth>();
        if (unitHealth != null)
        {
            unitHealth.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
            return;
        }

        HQHealth hqHealth = target.GetComponent<HQHealth>();
        if (hqHealth != null)
        {
            hqHealth.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
            return;
        }

        // Target has neither component — it's dead/invalid
        target = null;
        ReturnToPatrol();
    }

    // FIX 3: Central helper — ensures agent is fully un-stopped and patrol
    // will immediately pick a fresh waypoint instead of waiting on a stale path
    private void ReturnToPatrol()
    {
        target = null;
        currentState = State.Patrol;
        agent.isStopped = false;
        agent.ResetPath();          // clear leftover attack/chase path
        hasPatrolDestination = false; // force AdvanceToNextWaypoint on next tick
    }

    private Vector3 GetChaseDestination()
    {
        if (target == null)
            return transform.position;

        Vector3 offset = (transform.position - target.position).normalized * Random.Range(0.2f, 1f);
        return target.position + offset;
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * attackRotationSpeed);
    }
}