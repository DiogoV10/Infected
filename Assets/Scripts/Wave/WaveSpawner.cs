using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] public List<Enemies> enemies = new List<Enemies>();
    

    [SerializeField] public int currentWave;
    private int waveValue;

    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    [SerializeField] public Transform[] spawnLocation;
    private int spawnIndex;

    [SerializeField] public int waveDuration;
    [SerializeField] public int waveDelay;
    private float waveDelayTimer;
    private float waveTimer;
    private float spawnInterval;
    private float spawnTimer;

    public List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        GenerateWave();
    }

    void FixedUpdate()
    {
        if (spawnTimer <= 0)
        {
            if (enemiesToSpawn.Count > 0)
            {
                GameObject enemy = (GameObject)Instantiate(enemiesToSpawn[0], GetRandomSpawnLocation(), Quaternion.identity);
                enemiesToSpawn.RemoveAt(0);
                spawnedEnemies.Add(enemy);
                spawnTimer = spawnInterval;

                if (spawnIndex + 1 <= spawnLocation.Length - 1)
                {
                    spawnIndex++;
                }
                else
                {
                    spawnIndex = 0;
                }
            }
            else
            {
                waveTimer = 0;
            }
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
            waveTimer -= Time.fixedDeltaTime;
        }

        if (waveTimer <= 0 && spawnedEnemies.Count <= 0)
        {
            waveDelayTimer -= Time.fixedDeltaTime;
            if (waveDelayTimer <= 0)
            {
                currentWave++;
                GenerateWave();
            }
        }
    }

    Vector3 GetRandomSpawnLocation()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        Vector3 spawnPosition = Vector3.zero;
        bool positionFound = false;
        int maxAttempts = 10; // Maximum attempts to find a suitable spawn position

        for (int i = 0; i < maxAttempts; i++)
        {
            int randomIndex = Random.Range(0, spawnLocation.Length);
            spawnPosition = spawnLocation[randomIndex].position;

            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(spawnPosition, Vector3.one)))
            {
                positionFound = true;
                break;
            }
        }

        if (!positionFound)
        {
            int randomIndex = Random.Range(0, spawnLocation.Length);
            spawnPosition = spawnLocation[randomIndex].position;
        }

        return spawnPosition;
    }

    public void GenerateWave()
    {
        waveValue = currentWave * 10;
        GenerateEnemies();

        spawnInterval = Mathf.Max(waveDuration / enemiesToSpawn.Count, 0.5f);
        waveTimer = waveDuration;

        waveDelayTimer = waveDelay;
    }

    public void GenerateEnemies()
    {
        List<GameObject> generatedEnemies = new List<GameObject>();

        while (waveValue > 0 || generatedEnemies.Count < 50)
        {
            int randEnemyId = Random.Range(0, enemies.Count);
            int randEnemyCost = enemies[randEnemyId].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemyId].enemyPrefab);
                waveValue -= randEnemyCost;
            }
            else if (waveValue <= 0)
            {
                break;
            }
        }

        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
    }
}

[System.Serializable]
public class Enemies
{
    public GameObject enemyPrefab;
    public int cost;
}
