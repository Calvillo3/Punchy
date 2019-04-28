using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    public enum EnemyType : int { Cylinder = 0, Humanoid = 1, SpiderSwarm = 2 };
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] DifficultyManager difficultyManager;
    List<GameObject>[] enemies = new List<GameObject>[System.Enum.GetValues(typeof(EnemyType)).Length];
    int[] baseSpawnLimits;
    float[] spawnLimitsGrowth;
    int[] spawnLimits; 
    [SerializeField] GameObject enemiesParent;
    float[] spawnTimers = { 0, 0, 0 };
    float[] baseTimesToNextSpawn = {1, 3, 4 };
    float[] spawnTimeRanges = {0.5f/2, 2/2, 2/2};
    float[] timesOfNextSpawn = {0, 0, 0}; 
    GameObject player;
    TethersTracker tethersTracker;
    DifficultyValues difficultyValues;
    bool[] hasTetherBeenSpawnedInto;

    int totalEnemiesSpawned = 0;

    // Use this for initialization
    void Start () {
        //might need to be done differently? 
        difficultyValues = difficultyManager.DifficultyValues;
        baseSpawnLimits = difficultyValues.SpawnDifficultyValues.BaseSpawnLimits;
        spawnLimitsGrowth = difficultyValues.SpawnDifficultyValues.SpawnLimitsGrowth;

        spawnLimits = new int[baseSpawnLimits.Length];
        for (int i = 0; i < spawnLimits.Length; i++)
        {
            spawnLimits[i] = baseSpawnLimits[i];
        }

        //subject to change where this is relative to other things?
        player = gameObject;
        tethersTracker = player.GetComponentInChildren<TethersTracker>();
        for (int i = 0; i < System.Enum.GetValues(typeof(EnemyType)).Length; i++)
        {
            enemies[i] = new List<GameObject>();
        }

        foreach (EnemyController enemy in enemiesParent.GetComponentsInChildren<EnemyController>())
        {
            enemies[(int)enemy.Type].Add(enemy.gameObject);
        }

        for (int i = 0; i < spawnTimers.Length; i++)
        {
            timesOfNextSpawn[i] = baseTimesToNextSpawn[i] + Random.Range(-spawnTimeRanges[i], spawnTimeRanges[i]);
        }

        hasTetherBeenSpawnedInto = new bool[tethersTracker.Tethers.Length];
    }

    // Update is called once per frame
    void Update() {
        /*
        for (int i = 0; i < spawnTimers.Length; i++)
        {
            spawnTimers[i] += Time.deltaTime;
            if (CheckSpawn((EnemyType)i))
            {
                GameObject spawner = FindSpawner();
                SpawnEnemy((EnemyType)i, spawner);
            }
        }
        updateMaxSpawns();
        */
    }

    public void ContinuouslySpawn()
    {
        for (int i = 0; i < spawnTimers.Length; i++)
        {
            spawnTimers[i] += Time.deltaTime;
            if (CheckSpawn((EnemyType)i))
            {
                GameObject spawner = FindSpawner();
                SpawnEnemy((EnemyType)i, spawner);
            }
        }
    }

    public void SpawnSubwave(int numToSpawn)
    {
        int totalSpawnLimit = spawnLimits.Sum();
        float[] spawnPercent = new float[enemies.Length];
        int[] spawnNumber = new int[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            spawnPercent[i] = (float)spawnLimits[i] / (float)totalSpawnLimit;
            spawnNumber[i] = Mathf.FloorToInt(spawnPercent[i] * numToSpawn);
        }
        //Debug.Log("Percent of wave that is cylinders: " + spawnPercent[0]);

        if (spawnNumber.Sum() != numToSpawn)
        {
            if (spawnNumber.Sum() > numToSpawn) {  
                Debug.LogError("More enemies will spawn than intended in this subwave");
            }
            else {
                //  if total enemies that will spawn < numToSpawn, fill with cylinder enemies
                spawnNumber[0] += (numToSpawn - spawnNumber.Sum());
            }
        }

        // for each type of enemy, spawn the calculated number of that enemy
        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < spawnNumber[i]; j++)
            {
                GameObject spawner = FindSpawner();
                SpawnEnemy((EnemyType)i, spawner);
            }
        }
    }

    bool CheckSpawn(EnemyType type)
    {
        if (enemies[(int)type].Count < spawnLimits[(int)type] && (spawnTimers[(int)type] >= timesOfNextSpawn[(int)type]))
        {
            return true;
        }
        return false;
    }

    public GameObject FindSpawner()
    {
        TetherController[] tethers = tethersTracker.Tethers;
        int[] weights = new int[tethers.Length];
        int minWeightIndex = -1;
        int minWeight = int.MaxValue;

        for (int i = 0; i < tethers.Length; i++)
        {
            weights[i] += 10 * tethers[i].Occupants ^ 2;

            //We want distance from tether to player to be 30, so weight the difference of actual distance from that by difference*2
            weights[i] += (int)(Mathf.Abs(
                Vector3.Distance(tethers[i].gameObject.transform.position, player.transform.position) - 30)) * 2;

            //Weight of trace ratio at inverseRatio*50. Perfect visibility weights 50, no visibility weights 0;
            weights[i] += (int)((tethers[i].TraceRatio) * 50);
            

            if (weights[i] < minWeight && hasTetherBeenSpawnedInto[i] == false)
            {
                minWeightIndex = i;
                minWeight = weights[i];
            }
        }
        hasTetherBeenSpawnedInto[minWeightIndex] = true;
        return tethers[minWeightIndex].gameObject;
    }

    public void SpawnEnemy(EnemyType type, GameObject tether)
    {
        GameObject enemy = Instantiate(enemyPrefabs[(int)type], tether.transform.position, tether.transform.rotation, enemiesParent.transform);
        enemy.SetActive(true);
        enemy.layer = 9;
        spawnTimers[(int)type] = 0;
        timesOfNextSpawn[(int)type] = baseTimesToNextSpawn[(int)type] + Random.Range(-spawnTimeRanges[(int)type], spawnTimeRanges[(int)type]);
        enemies[(int)type].Add(enemy);
        totalEnemiesSpawned++;
    }
    public void DestroyEnemy(GameObject enemy)
    {
        enemies[(int)enemy.GetComponent<EnemyController>().Type].Remove(enemy);
        Destroy(enemy);
    }

    public void UpdateMaxSpawns(float difficulty)
    {
        //float difficulty = difficultyManager.Difficulty;
        for (int i = 0; i < spawnLimits.Length; i++)
        {
            spawnLimits[i] = baseSpawnLimits[i] + Mathf.FloorToInt(spawnLimitsGrowth[i] * difficulty);
        }
    }

    public void ClearHasTetherBeenSpawnedInto()
    {
        for (int i = 0; i < hasTetherBeenSpawnedInto.Length; i++)
        {
            hasTetherBeenSpawnedInto[i] = false;
        }
    }

    public int CurrNumOfEnemiesAlive()
    {
        int currNumOfEnemiesAlive = 0;
        foreach (List<GameObject> enemyType in enemies)
        {
            currNumOfEnemiesAlive += enemyType.Count;
        }
        return currNumOfEnemiesAlive;
    }

    public int TotalEnemiesSpawned
    {
        get
        {
            return totalEnemiesSpawned;
        }

        set
        {
            totalEnemiesSpawned = value;
        }
    }
}
