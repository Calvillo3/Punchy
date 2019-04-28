using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    private UnityAction<string> pauseListener;
    bool inWave = false;
    bool isPaused = false;
    float timeSinceLastWaveEnd = 0f;
    float timeSinceWaveStart = 0f;
    float timeSinceSubWaveStart = 0f;
    int totalEnemiesSpawned = 0;
    int waveCount = 0;
    [SerializeField] int totalEnemiesInWave = 3;
    [SerializeField] int timeBetweenWaves = 5; // seconds
    [SerializeField] float timeBetweenSubWaves = 10f;
    [SerializeField] SpawnManager spawnManager;
    //[SerializeField] DifficultyManager difficultyManager;
    float difficulty = 0;
    int subWaveEnemyNumber;
    bool spawning = false;
    float spawnTime = 0f;

    private void Awake()
    {
        pauseListener = new UnityAction<string>(Pause);
    }

    private void OnEnable()
    {
        EventManager.StartListening("pause", pauseListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening("pause", pauseListener);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // logic for checking the time since the wave ended and setting inWave = true
        // and reset that counter
        if (!isPaused) // make sure no spawning happens while the game is paused
        {
            if (inWave)
            {
                totalEnemiesSpawned = spawnManager.TotalEnemiesSpawned;
                if (totalEnemiesSpawned >= totalEnemiesInWave)
                {
                    WaveIsStagnant();
                }
                else
                {
                    InWave();
                }
            }
            else
            {
                if (timeSinceLastWaveEnd >= timeBetweenWaves || waveCount == 0)
                {
                    StartWave();
                }
                else
                {
                    timeSinceLastWaveEnd += Time.unscaledDeltaTime;
                }
            }
        }

        
    }


    void StartWave()
    {    
        if (waveCount == 0)
        {
            difficulty = 0.2f; // because Log(0) doesn't work
        }
        else if (waveCount == 1)
        {
            difficulty = 0.4f; // because Log(1) = 0
        }
        else
        {
            difficulty = 2 * Mathf.Log10(waveCount);
        }

        totalEnemiesInWave = (int)(20 * difficulty);

        if (totalEnemiesInWave < 10) subWaveEnemyNumber = totalEnemiesInWave;
        else subWaveEnemyNumber = (int)Mathf.Floor(totalEnemiesInWave / 3);

        spawnManager.UpdateMaxSpawns(difficulty);
        inWave = true;
        waveCount++;
        timeSinceLastWaveEnd = 0;
        timeSinceSubWaveStart = timeBetweenSubWaves / 2;
    }
    void InWave()
    {
        timeSinceWaveStart += Time.unscaledDeltaTime;
        timeSinceSubWaveStart += Time.unscaledDeltaTime;

        if (timeSinceSubWaveStart >= timeBetweenSubWaves || spawnManager.CurrNumOfEnemiesAlive() == 0)
        {
            spawnManager.SpawnSubwave(subWaveEnemyNumber);
            spawnManager.ClearHasTetherBeenSpawnedInto();
            timeSinceSubWaveStart = 0f;
        }
    }

    // The enemies that have already been spawned are the last ones in the wave. No more spawning
    void WaveIsStagnant()
    {
        if (spawnManager.CurrNumOfEnemiesAlive() == 0)
        {
            EndWave();
        }
    }
    
    void EndWave()
    {
        spawnManager.TotalEnemiesSpawned = 0; // reset the spawn manager's count
        inWave = false;
        EventManager.TriggerEvent("newWave", (waveCount+1).ToString());
    }

    public int TimeBetweenWaves
    {
        get
        {
            return timeBetweenWaves;
        }
    }

    public void Pause(string data)
    {
        isPaused = !isPaused;
    }

}
