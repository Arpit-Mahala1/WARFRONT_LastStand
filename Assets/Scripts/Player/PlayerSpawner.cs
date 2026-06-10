using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject unitPrefab;
    public Transform spawnPoint;
    public int spawnCost = 20;
    public int maxUnits = 10;

    private int currentUnitCount = 0;

    public void SpawnUnit()
    {
        if (GameManager.Instance.supplyAmount < spawnCost || currentUnitCount >= maxUnits)
            return;

        GameManager.Instance.SpendSupply(spawnCost);

        GameObject newUnit = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        currentUnitCount++;

        UnitHealth unitHealth = newUnit.GetComponent<UnitHealth>();
        if (unitHealth != null)
            unitHealth.OnDeath += () => currentUnitCount--;
    }
}
