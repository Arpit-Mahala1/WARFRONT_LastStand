using UnityEngine;

public class EnemyCommander : MonoBehaviour
{
    public CaptureZone[] laneFlags;
    public float evaluationInterval = 5f;
    public int aggressionLevel = 1;
    public int maxAggression = 5;
    public float aggressionIncreaseSeconds = 120f;

    public int AggressionLevel => aggressionLevel;

    private float evaluationTimer;
    private float aggressionTimer;
    private int selectedLaneIndex;

    private void Start()
    {
        evaluationTimer = evaluationInterval;
        aggressionTimer = aggressionIncreaseSeconds;
        SelectLane();
    }

    private void Update()
    {
        evaluationTimer -= Time.deltaTime;
        aggressionTimer -= Time.deltaTime;

        if (evaluationTimer <= 0f)
        {
            SelectLane();
            evaluationTimer = evaluationInterval;
        }

        if (aggressionTimer <= 0f)
        {
            aggressionLevel = Mathf.Min(maxAggression, aggressionLevel + 1);
            aggressionTimer = aggressionIncreaseSeconds;
        }
    }

    public int GetNextLane()
    {
        if (laneFlags == null || laneFlags.Length == 0)
            return 0;

        return selectedLaneIndex % laneFlags.Length;
    }

    private void SelectLane()
    {
        if (laneFlags == null || laneFlags.Length == 0)
            return;

        int bestIndex = 0;
        float lowestDefenderScore = float.MaxValue;

        for (int i = 0; i < laneFlags.Length; i++)
        {
            CaptureZone zone = laneFlags[i];
            if (zone == null)
                continue;

            float score = zone.playerUnitsInside;
            if (zone.IsContested)
                score += 2f;
            if (zone.IsEnemyControlled)
                score -= 1f;

            if (score < lowestDefenderScore)
            {
                lowestDefenderScore = score;
                bestIndex = i;
            }
        }

        selectedLaneIndex = bestIndex;
    }
}
// Force recompile
