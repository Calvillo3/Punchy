using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class SpiderSwarmController : EnemyController
{
    public enum SwarmState : int { Chase = 0, Scatter, Regroup };
    List<SpiderController> swarm;
    Vector3 centerOfMass;
    public Vector3 CenterOfMass { get { return centerOfMass; } }
    Vector3 averageHeading;
    public Vector3 AverageHeading { get { return averageHeading; } }
    NavMeshPath path;
    Vector3 goalPoint;
    [SerializeField] float lengthOfScatter;
    [SerializeField] float scatterLengthVariance;
    [SerializeField] NavMeshAgent navAgent;
    float scatterTimer;
    float scatterTimeout;
    int scoreValue = 10;

    [SerializeField] float cohesionWeight;
    public float CohesionWeight { get { return cohesionWeight; } }
    [SerializeField] float separationWeight;
    public float SeparationWeight { get { return separationWeight; } }
    [SerializeField] float goalWeight;
    public float GoalWeight { get { return goalWeight; } }
    [SerializeField] float neighborRadius;
    public float NeighborRadius { get { return neighborRadius; } }
    [SerializeField] float maxSteering;
    public float MaxSteering { get { return maxSteering; } }


    SwarmState currentState;
    public SwarmState CurrentState { get { return currentState; } }

    // Start is called before the first frame update
    protected override void Start()
    {
        type = SpawnManager.EnemyType.SpiderSwarm;
        swarm = this.GetComponentsInChildren<SpiderController>().ToList();
        currentState = SwarmState.Chase;
        foreach(SpiderController spider in swarm)
        {
            spider.SetController(this);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 lastPoint = Vector3.negativeInfinity;
            foreach (Vector3 point in navAgent.path.corners)
            {
                Gizmos.DrawWireSphere(point, 0.2f);
                if (lastPoint != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(point, lastPoint);
                }
                lastPoint = point;
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (currentState)
        {
            case (SwarmState.Chase):
                {
                    centerOfMass = CalculateCenterOfMass();
                    navAgent.gameObject.transform.position = centerOfMass;
                    //navAgent.CalculatePath(player.transform.position, path);


                    //NavMesh.CalculatePath(navAgent.transform.position, player.transform.position, NavMesh.AllAreas, navAgent.path);
                    navAgent.SetDestination(player.transform.position);
                    foreach (SpiderController spider in swarm)
                    {
                        if (navAgent.path.corners.Length > 1)
                        {
                            spider.SetHeading(navAgent.path.corners[1], centerOfMass);
                        }
                    }
                    break;
                }
            case (SwarmState.Scatter):
                {
                    scatterTimer += Time.deltaTime;
                    if (scatterTimer >= scatterTimeout)
                    {
                        currentState = SwarmState.Regroup;
                    }
                    break;
                }
            case (SwarmState.Regroup):
                {
                    //what does regroup detection look like? maybe calculate center of mass and check max distance from CoM?
                    break;
                }
        }
    }

    public void Scatter()
    {
        currentState = SwarmState.Scatter;
        scatterTimer = 0;
        scatterTimeout = lengthOfScatter + Random.Range(-scatterLengthVariance, scatterLengthVariance);
    }

    private Vector3 CalculateCenterOfMass()
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach(SpiderController spider in swarm)
        {
            x += spider.gameObject.transform.position.x;
            y += spider.gameObject.transform.position.y;
            z += spider.gameObject.transform.position.z;
        }
        return new Vector3(x / swarm.Count, y / swarm.Count, z / swarm.Count);
    }

    public void SpiderDestroyed(SpiderController spider)
    {
        swarm.Remove(spider);
        if (swarm.Count == 0)
        {
            playerCamera.gameObject.GetComponent<ScoreTracker>().ChangeScore(scoreValue, centerOfMass);
            spawnManager.DestroyEnemy(this.gameObject);
        }
    }
}
