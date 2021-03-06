﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TethersTracker : MonoBehaviour {
    [SerializeField] int traceUpdatesPerFrame;

    List<TetherController> tethers = new List<TetherController>();
    List<float> tetherTraceRatios = new List<float>();
    int currentTether = 0;
    int currentTrace = 0;
    int tracesThisFrame;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        tracesThisFrame = 0;
        if (tethers.Count == 0)
        {
            return;
        }
		while (traceUpdatesPerFrame > tracesThisFrame)
        {
            tethers[currentTether].UpdateTrace(currentTrace);
            currentTrace++;

            if (currentTrace >= tethers[currentTether].NumberOfTraces)
            {
                currentTrace = 0;
                currentTether++;
                if (currentTether >= tethers.Count)
                {
                    currentTether = 0;
                }
            }

            tracesThisFrame++;
        }
	}

    public float[] TraceRatios
    {
        get
        {
            return tetherTraceRatios.ToArray();
        }
    }

    public TetherController[] Tethers
    {
        get
        {
            return tethers.ToArray();
        }
    }

    public void AddTether(TetherController tether)
    {
        tethers.Add(tether);
        tetherTraceRatios.Add(tether.TraceRatio);
    }
}
