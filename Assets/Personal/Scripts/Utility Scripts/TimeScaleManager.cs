﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeScaleManager : MonoBehaviour {

    //All time in this manager should be unscaled, as it is the manager for slow motion and times should be measured in realtime

    float timer;
    float timerEnd;
    float normalTimeScale = 1f;
    float currentTimeScale;
    bool lerping;
    bool normal;
    bool paused;
    float unpausedTimeScale;
    float lerpFactor;

    private UnityAction<string> pauseListener;

    private void Awake()
    {
        pauseListener = new UnityAction<string>(TogglePause);
    }

    private void OnEnable()
    {
        EventManager.StartListening("pause", pauseListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening("pause", pauseListener);
    }

    // Use this for initialization
    void Start () {
        lerping = false;
        lerpFactor = 0f;
        normal = true;
        currentTimeScale = normalTimeScale;
        timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (!normal && !paused)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= timerEnd)
            {
                currentTimeScale = normalTimeScale;
                normal = true;
            }
            else if (lerping)
            {
                currentTimeScale = Mathf.Lerp(currentTimeScale, normalTimeScale, lerpFactor);
            }
            UpdateTimeScale();
        }
    }

    public void ChangeTimeScale(float newTimeScale, float time, float lerpAmount)
    {
        currentTimeScale = newTimeScale;
        if (lerpAmount == float.NegativeInfinity)
        {
            lerping = false;
        }
        else
        {
            lerping = true;
        }
        if (currentTimeScale != 0)
        {
            normal = false;
        }
        else
        {
            normal = true;
        }
        lerpFactor = lerpAmount;
        timerEnd = time;
        timer = 0;
    }

    public void FullspeedTimeScale()
    {
        currentTimeScale = normalTimeScale;
        UpdateTimeScale();
    }

    void UpdateTimeScale()
    {
        Time.timeScale = currentTimeScale;
        Time.fixedDeltaTime = 0.01666667f * Time.timeScale;
    }

    public void TogglePause (string data)
    {
        if (!paused)
        {
            unpausedTimeScale = currentTimeScale;
            paused = true;
            currentTimeScale = 0f;
        }
        else
        {
            paused = false;
            currentTimeScale = unpausedTimeScale;
        }
        UpdateTimeScale();
    }

    public float Timescale
    {
        get
        {
            return currentTimeScale;
        }
    }
}
