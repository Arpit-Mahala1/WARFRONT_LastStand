using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float attackDamage = 8f;
    public float attackCooldown = 1.2f;
    public Transform[] patrolWaypoints;
    public LayerMask playerLayer;

    private int waypointIndex = 0;
    private Transform target;
    private float lastAttackTime;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        InvokeRepeating("UpdateState", 0f, 0.2f);
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase:  HandleChase();  break;
            case State.Attack: HandleAttack(); break;
        }
    }

    private void HandlePatrol()
    {
        if (patrolWaypoints.Length == 0)
            return;

        agent.SetDestination(patrolWaypoints[waypointIndex].position);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waypointIndex = (waypointIndex + 1) % patrolWaypoints.Length;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        if (hits.Length > 0)
        {
            target = hits[0].transform;
            currentState = State.Chase;
        }
    }

    private void HandleChase()
    {
        if (target == null || target.gameObject == null)
        {
            currentState = State.Patrol;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance > detectionRange * 1.5f)
        {
            target = null;
            currentState = State.Patrol;
        }
    }

    private void HandleAttack()
    {
        agent.isStopped = true;

        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            agent.isStopped = false;
            currentState = State.Chase;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            UnitHealth unitHealth = target.GetComponent<UnitHealth>();
            if (unitHealth != null)
                unitHealth.TakeDamage(attackDamage);

            lastAttackTime = Time.time;
        }
    }
}
