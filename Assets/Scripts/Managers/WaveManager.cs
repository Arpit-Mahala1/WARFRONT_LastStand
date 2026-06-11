using UnityEngine;
using System;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemySpawnPoint;
    public GameObject enemyWaypointParent;
    public int baseEnemyCount = 3;
    public int waveNumber = 0;
    public float timeBetweenWaves = 30f;
    public float timeUntilNextWave;

    public static event Action<int> OnWaveStarted;

    private void Start()
    {
        timeUntilNextWave = timeBetweenWaves;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            while (timeUntilNextWave > 0)
            {
                timeUntilNextWave -= Time.deltaTime;
                yield return null;
            }

            if (GameManager.Instance.gameIsOver)
                yield break;

            waveNumber++;
            timeUntilNextWave = timeBetweenWaves;
            OnWaveStarted?.Invoke(waveNumber);

            int enemiesToSpawn = baseEnemyCount + (waveNumber * 2);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 3f;
                offset.y = 0;
                Vector3 spawnPos = enemySpawnPoint.position + offset;

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                EnemyAI ai = enemy.GetComponent<EnemyAI>();
                if (ai != null && enemyWaypointParent != null)
                {
                    int childCount = enemyWaypointParent.transform.childCount;
                    GameObject[] waypoints = new GameObject[childCount];
                    for (int j = 0; j < childCount; j++)
                        waypoints[j] = enemyWaypointParent.transform.GetChild(j).gameObject;
                    ai.SetWaypoints(waypoints);
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}