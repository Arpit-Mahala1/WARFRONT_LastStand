using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject riflemanPrefab;
    public GameObject sniperPrefab;
    public GameObject grenadierPrefab;
    public GameObject engineerPrefab;
    public Transform spawnPoint;
    public int maxUnits = 50;

    private int activeUnits;
    private int activeEngineers;

    public void SpawnRifleman()
    {
        SpawnUnit(UnitType.Rifleman);
    }

    public void SpawnSniper()
    {
        SpawnUnit(UnitType.Sniper);
    }

    public void SpawnGrenadier()
    {
        SpawnUnit(UnitType.Grenadier);
    }

    public void SpawnEngineer()
    {
        SpawnUnit(UnitType.Engineer);
    }

    public void SpawnReinforcements(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnUnitInternal(riflemanPrefab, false);
        }
    }

    private void SpawnUnit(UnitType unitType)
    {
        GameObject prefab = null;
        int cost = 0;

        switch (unitType)
        {
            case UnitType.Rifleman:
                prefab = riflemanPrefab;
                cost = 20;
                break;
            case UnitType.Sniper:
                prefab = sniperPrefab;
                cost = 45;
                break;
            case UnitType.Grenadier:
                prefab = grenadierPrefab;
                cost = 60;
                break;
            case UnitType.Engineer:
                prefab = engineerPrefab;
                cost = 50;
                break;
        }

        if (prefab == null || activeUnits >= maxUnits || !GameManager.Instance.SpendSupply(cost))
            return;

        SpawnUnitInternal(prefab, unitType == UnitType.Engineer);
    }

    private void SpawnUnitInternal(GameObject prefab, bool isEngineer)
    {
        GameObject newUnit = Instantiate(prefab, spawnPoint.position + Vector3.up * 0.5f, Quaternion.identity);
        activeUnits++;

        UnitHealth unitHealth = newUnit.GetComponent<UnitHealth>();
        if (unitHealth != null)
        {
            unitHealth.OnDeath += () =>
            {
                activeUnits--;
                if (isEngineer)
                {
                    activeEngineers = Mathf.Max(0, activeEngineers - 1);
                    GameManager.Instance.RegisterEngineerCount(-1);
                }
            };
        }

        if (isEngineer)
        {
            activeEngineers++;
            GameManager.Instance.RegisterEngineerCount(1);
        }
    }

    public bool CanSpawnEngineer()
    {
        return GameManager.Instance.activeEngineers < GameManager.Instance.maxEngineers;
    }
}
