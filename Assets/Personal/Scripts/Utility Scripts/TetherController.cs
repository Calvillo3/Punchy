﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TetherController : MonoBehaviour {

    [SerializeField] float radius;

    Transform[] traces;

    [SerializeField] bool[] traceCanSeePlayer;

    int occupants;

    [SerializeField] float seenRatio = 0f;

    //float ratioDiff;

    LayerMask mask;

    TethersTracker tetherManager;
    GameObject player;

    // Use this for initialization
    void Awake() {
        //subject to change
        tetherManager = Camera.main.gameObject.GetComponent<TethersTracker>();
        tetherManager.AddTether(this);

        player = GameObject.Find("Player");

        traces = this.gameObject.GetComponentsInChildren<Transform>();
        traceCanSeePlayer = new bool[traces.Length];
        //ratioDiff = 1 / traces.Length;

        mask = LayerMask.GetMask("Default", "Player");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            traces = this.gameObject.GetComponentsInChildren<Transform>();
            Gizmos.color = Color.cyan;

            for (int i = 0; i < traces.Length; i++)
            {
                Gizmos.DrawWireSphere(traces[i].position - new Vector3(0, 2, 0), 0.1f);
            }
            return;
        }

        for (int i = 0; i < traces.Length; i++)
        {
            if (traceCanSeePlayer[i])
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireSphere(traces[i].position - new Vector3(0, 2, 0), 0.1f);
        }
    }

    public float UpdateTrace(int traceNum)
    {
        //bool previousStatus = traceCanSeePlayer[traceNum];
        traceCanSeePlayer[traceNum] = CheckLineOfSight(traceNum);

        /*
        if (traceCanSeePlayer[traceNum] == previousStatus)
        {
            return seenRatio;
        }
        else if (traceCanSeePlayer[traceNum])
        {
            seenRatio += ratioDiff;
        }
        else
        {
            seenRatio -= ratioDiff;
        }
        */

        seenRatio = (float)(traceCanSeePlayer.Count(c => c)) / traceCanSeePlayer.Count();

        return seenRatio;
    }

    bool CheckLineOfSight(int traceNum)
    {
        RaycastHit seePlayer;
        Ray ray = new Ray(traces[traceNum].position, player.transform.position - (traces[traceNum].position));

        if (Physics.Raycast(ray, out seePlayer, Mathf.Infinity, mask))
        {
            if (seePlayer.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public int NumberOfTraces
    {
        get
        {
            return traces.Length;
        }
    }

    public float Radius
    {
        get
        {
            return radius;
        }
    }

    public int Occupants
    {
        get
        {
            return occupants;
        }
    }

    public void incrementOccupantsBy(int Count)
    {
        occupants += Count;
    }

    public float TraceRatio
    {
        get
        {
            return seenRatio;
        }
    }
}
