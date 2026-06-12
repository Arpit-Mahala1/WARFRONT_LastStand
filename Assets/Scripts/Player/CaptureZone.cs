using UnityEngine;

public enum CaptureState
{
    Neutral,
    Player,
    Enemy,
    Contested
}

public enum PointType
{
    Flag,
    CommandPost
}

[RequireComponent(typeof(SphereCollider))]
public class CaptureZone : MonoBehaviour
{
    public PointType pointType = PointType.Flag;
    public float detectionRadius = 5f;
    public CaptureState currentState = CaptureState.Neutral;
    public int playerUnitsInside;
    public int enemyUnitsInside;

    private SphereCollider col;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(EvaluateControl), 0.2f, 0.5f);
    }

    private void EvaluateControl()
    {
        playerUnitsInside = 0;
        enemyUnitsInside = 0;

        Vector3 center = transform.position;
        float radius = detectionRadius;

        if (col != null)
        {
            center = transform.TransformPoint(col.center);
            radius = col.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        }

        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.GetComponent<UnitHealth>() != null)
                playerUnitsInside++;
            else if (hit.GetComponent<EnemyHealth>() != null)
                enemyUnitsInside++;
        }

        if (playerUnitsInside > 0 && enemyUnitsInside > 0)
        {
            currentState = CaptureState.Contested;
        }
        else if (playerUnitsInside > 0)
        {
            currentState = CaptureState.Player;
        }
        else if (enemyUnitsInside > 0)
        {
            currentState = CaptureState.Enemy;
        }
        else
        {
            currentState = CaptureState.Neutral;
        }
    }

    public bool IsPlayerControlled => currentState == CaptureState.Player;
    public bool IsEnemyControlled => currentState == CaptureState.Enemy;
    public bool IsContested => currentState == CaptureState.Contested;
    public bool IsNeutral => currentState == CaptureState.Neutral;

    public int GetSupplyValue()
    {
        return pointType == PointType.CommandPost ? 20 : 8;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentState == CaptureState.Player ? Color.blue : currentState == CaptureState.Enemy ? Color.red : currentState == CaptureState.Contested ? Color.yellow : Color.gray;
        
        Vector3 center = transform.position;
        float radius = detectionRadius;

        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc != null)
        {
            center = transform.TransformPoint(sc.center);
            radius = sc.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        }

        Gizmos.DrawWireSphere(center, radius);
    }
}
