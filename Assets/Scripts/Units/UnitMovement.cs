using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public bool IsSelected;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveToPosition(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    public void StopMoving()
    {
        agent.ResetPath();
    }
}
