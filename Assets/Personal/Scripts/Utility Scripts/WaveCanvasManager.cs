using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class WaveCanvasManager : MonoBehaviour
{
    private UnityAction<string> waveListener;
    private UnityAction<string> pauseListener;

    [SerializeField] float nextWaveTime;
    [SerializeField] float waveCompleteTime;
    float timer;
    float countdownTimer;
    float countdownTime;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] WaveManager waveManager;
    bool displaying;
    bool nextWaveDisplayed;
    bool countdownDisplayed;
    string wave;

    private void Awake()
    {
        waveListener = new UnityAction<string>(ActivateCanvas);
        pauseListener = new UnityAction<string>(ToggleCanvasWithPause);
    }

    private void OnEnable()
    {
        EventManager.StartListening("newWave", waveListener);
        EventManager.StartListening("pause", pauseListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening("newWave", waveListener);
        EventManager.StopListening("pause", pauseListener);
    }

    // Start is called before the first frame update
    void Start()
    {
        countdownTime = waveManager.TimeBetweenWaves - waveCompleteTime;
        wave = "1";
        timer = waveCompleteTime;
        nextWaveDisplayed = true;
        displaying = true;
        waveText.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (displaying)
        {
            timer += Time.deltaTime;
        }
        if (countdownDisplayed)
        {
            countdownTimer -= Time.deltaTime;
        }
        
        if (wave == "1")
        {
            if (timer < waveCompleteTime + nextWaveTime)
            {
                waveText.text = "Wave " + wave.ToString();
                nextWaveDisplayed = true;
            }
            

            else if (timer >= waveCompleteTime + nextWaveTime && displaying)  // remove text
            {
                waveText.gameObject.SetActive(false);
                displaying = false;
                nextWaveDisplayed = false;
                countdownDisplayed = false;
                countdownTimer = countdownTime;
                timer = 0;
            }
        }
        else
        {
            if (timer >= waveCompleteTime && countdownTimer <= 1f && displaying)  // remove text
            {
                waveText.gameObject.SetActive(false);
                displaying = false;
                nextWaveDisplayed = false;
                countdownDisplayed = false;
                countdownTimer = countdownTime;
                timer = 0;
            }
            else if (timer >= waveCompleteTime)  // display after wave ends (countdown to next wave)
            {
                waveText.text = "Wave " + wave.ToString() + " Starts in " + Mathf.Floor(countdownTimer).ToString();
                countdownDisplayed = true;
            }
        }
    }

    void ActivateCanvas(string waveNumber)
    {
        waveText.text = "Wave " + wave + " complete";
        wave = waveNumber;
        waveText.gameObject.SetActive(true);
        displaying = true;
    }

    void ToggleCanvasWithPause(string none)
    {
        if (waveText.gameObject.activeSelf) waveText.gameObject.SetActive(false);
        else if (displaying) waveText.gameObject.SetActive(true);
    }
}
