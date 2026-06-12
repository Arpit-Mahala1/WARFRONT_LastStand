using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public bool IsSelected;
    public float selectionScale = 1.12f;
    public float stoppingDistance = 0.8f;
    public float moveSpeed = 4f;
    public float retreatSpeed = 5f;
    public float formationRadius = 1.2f;

    private Vector3 originalScale;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalScale = transform.localScale;
        agent.avoidancePriority = UnityEngine.Random.Range(20, 80);
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.speed = moveSpeed;
    }

    public void MoveToPosition(Vector3 destination, bool useFormationOffset = true)
    {
        if (agent == null)
            return;

        Vector3 target = destination;
        if (useFormationOffset)
        {
            Vector3 offset = Random.insideUnitSphere * formationRadius;
            offset.y = 0f;
            target += offset;
        }

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(target);
    }

    public void RetreatTo(Vector3 destination)
    {
        if (agent == null)
            return;

        agent.isStopped = false;
        agent.speed = retreatSpeed;
        agent.SetDestination(destination);
    }

    public void StopMoving()
    {
        if (agent == null)
            return;

        agent.ResetPath();
        agent.isStopped = true;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        transform.localScale = selected ? originalScale * selectionScale : originalScale;
    }
}
