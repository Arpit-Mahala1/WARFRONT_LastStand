using System;
using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemySpawnPoint;
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
        while (!GameManager.Instance.gameIsOver)
        {
            // Count down to next wave, one tick per second
            while (timeUntilNextWave > 0f)
            {
                yield return new WaitForSeconds(1f);
                timeUntilNextWave -= 1f;

                if (GameManager.Instance.gameIsOver)
                    yield break;
            }

            // Advance wave
            waveNumber++;
            timeUntilNextWave = timeBetweenWaves;

            OnWaveStarted?.Invoke(waveNumber);

            int spawnCount = baseEnemyCount + waveNumber * 2;
            float spawnY = enemySpawnPoint.position.y;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 3f;
                Vector3 spawnPos = new Vector3(
                    enemySpawnPoint.position.x + offset.x,
                    spawnY,
                    enemySpawnPoint.position.z + offset.z
                );

                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}