using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public GameObject basicSoldierPrefab;
    public GameObject heavyGunnerPrefab;
    public GameObject marksmanPrefab;
    public GameObject sapperPrefab;

    public Transform[] laneSpawnPoints;
    public Transform[] laneWaypointParents;
    public EnemyCommander enemyCommander;

    public int baseEnemyCount = 3;
    public int waveNumber = 0;
    public float timeBetweenWaves = 30f;
    public float minTimeBetweenWaves = 10f;
    public float timeUntilNextWave;
    public float spawnDelay = 0.25f;
    public float enemyHealthIncreasePerWave = 7f;

    public static event Action<int> OnWaveStarted;
    public static event Action<float> OnWaveTimerUpdated;

    private float initialTimeBetweenWaves;

    private void Start()
    {
        initialTimeBetweenWaves = timeBetweenWaves;
        timeUntilNextWave = timeBetweenWaves;
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        timeUntilNextWave = minTimeBetweenWaves; // Give initial buffer before wave 1

        while (!GameManager.Instance.gameIsOver)
        {
            while (timeUntilNextWave > 0f)
            {
                OnWaveTimerUpdated?.Invoke(timeUntilNextWave);
                yield return new WaitForSeconds(1f);
                timeUntilNextWave -= 1f;
            }

            if (GameManager.Instance.gameIsOver)
                yield break;

            waveNumber++;
            OnWaveStarted?.Invoke(waveNumber);
            
            // Wait until all enemies finish spawning
            yield return StartCoroutine(SpawnWaveCoroutine());

            // Wait until all enemies are defeated
            while (!GameManager.Instance.gameIsOver)
            {
                EnemyHealth[] activeEnemies = FindObjectsOfType<EnemyHealth>();
                if (activeEnemies.Length == 0)
                {
                    break;
                }
                
                OnWaveTimerUpdated?.Invoke(0f); 
                yield return new WaitForSeconds(1f);
            }

            // Short buffer time before the next wave starts
            timeUntilNextWave = 5f;
        }
    }

    private IEnumerator SpawnWaveCoroutine()
    {
        int aggression = enemyCommander != null ? enemyCommander.AggressionLevel : 1;
        int totalEnemies = baseEnemyCount + waveNumber * 2 + aggression;
        List<GameObject> composition = BuildWaveComposition(totalEnemies);

        for (int i = 0; i < composition.Count; i++)
        {
            if (laneSpawnPoints == null || laneSpawnPoints.Length == 0)
                break;

            // Select a random lane for this specific enemy so the wave splits up
            int laneIndex = UnityEngine.Random.Range(0, laneSpawnPoints.Length);

            Vector3 spawnPosition = laneSpawnPoints[laneIndex].position;
            spawnPosition += UnityEngine.Random.insideUnitSphere * 2f;
            spawnPosition.y = 0f;
            GameObject enemy = Instantiate(composition[i], spawnPosition, Quaternion.identity);
            InitializeEnemy(enemy, laneIndex);
            
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private List<GameObject> BuildWaveComposition(int totalCount)
    {
        List<GameObject> composition = new List<GameObject>();
        int heavies = Mathf.Clamp((waveNumber - 2) / 2, 0, totalCount);
        int marksmen = waveNumber >= 5 ? 1 : 0;
        int sappers = waveNumber >= 7 ? 1 : 0;

        for (int i = 0; i < heavies; i++)
            composition.Add(heavyGunnerPrefab);

        for (int i = 0; i < marksmen; i++)
            composition.Add(marksmanPrefab);

        for (int i = 0; i < sappers; i++)
            composition.Add(sapperPrefab);

        while (composition.Count < totalCount)
            composition.Add(basicSoldierPrefab);

        return composition;
    }

    private void InitializeEnemy(GameObject enemy, int laneIndex)
    {
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null && laneWaypointParents != null && laneWaypointParents.Length > laneIndex)
        {
            var parent = laneWaypointParents[laneIndex];
            int childCount = parent.childCount;
            GameObject[] waypoints = new GameObject[childCount];
            for (int j = 0; j < childCount; j++)
                waypoints[j] = parent.GetChild(j).gameObject;
            ai.SetWaypoints(waypoints);
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.InitializeHealth(enemyHealth.maxHealth + (waveNumber - 1) * enemyHealthIncreasePerWave);
    }
}
