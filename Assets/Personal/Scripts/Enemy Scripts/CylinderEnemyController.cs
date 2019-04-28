using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityScript.Steps;

public class CylinderEnemyController : EnemyController
{

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] float firingFrequency;
    [SerializeField] float firingFrequencyRange;
    [SerializeField] float bulletSpeed;
    [SerializeField] int bulletDamage;
    [SerializeField] float runAwayDistance;
    [SerializeField] GameObject fractures;
    [SerializeField] int scoreValue;
    [SerializeField] float bulletForce;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] float fireWindupTime;
    [SerializeField] float reevaluateTetherTime;
    [SerializeField] AudioClip firingSound;
    [SerializeField] AudioClip laserSound;
    [SerializeField] float laserRotationLerp;
    [SerializeField] LineRenderer targetLine;
    [SerializeField] LineRenderer laserLine;
    [SerializeField] float laserMaxLength;
    [SerializeField] float laserForce;
    [SerializeField] int laserDamagePerFrame;

    [SerializeField] float laserWindupTime;
    [SerializeField] float laserFiringTime;
    [SerializeField] float laserTargetingTime;

    Color defaultColor;
    Color fireColor = Color.magenta;
    Material material;
    MeshRenderer cachedRenderer;
    AudioSource audioSource;
    private bool firing;
    private float timeToNextFire;
    Vector3 destination;
    private float stateTimer;
    private float fireTimer;
    private GameObject tether;
    private float tetherRadius;
    bool dead;
    float maxDeadTime = 3;
    private TethersTracker tetherTracker;
    EnemyAttackTokenPool.Token token;
    float defaultSpeed;
    LineRenderer activeLaser;
    bool laserSoundPlayed;

    private enum enemyState { movingState, tetheredState, laserState };
    private enemyState state;

    // Use this for initialization
    protected override void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        type = SpawnManager.EnemyType.Cylinder;
        spawnManager = player.GetComponent<SpawnManager>();
        cachedRenderer = gameObject.GetComponent<MeshRenderer>();
        material = cachedRenderer.material;
        defaultColor = material.color;
        enemyAttackTokenPool = player.GetComponentInChildren<EnemyAttackTokenPool>();
        tetherTracker = player.gameObject.GetComponentInChildren<TethersTracker>();
        type = SpawnManager.EnemyType.Cylinder;
        stateTimer = 0;
        timeToNextFire = firingFrequency + Random.Range(-firingFrequencyRange, firingFrequencyRange);
        nav = GetComponent<NavMeshAgent>();
        defaultSpeed = nav.speed;
        dead = false;
        firing = false;
        fireTimer = 0;

        tether = this.findBestTether();
        tetherRadius = tether.GetComponent<TetherController>().Radius;
        destination = FindNewPositionInTether();
        state = enemyState.movingState;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (isVisible())
        {
            nav.speed = defaultSpeed;
        }
        else
        {
            nav.speed = defaultSpeed*2;
        }
    }

    protected override void FixedUpdate()
    {
        // state machine/swiiitch
        // need enumerable
        // moving to tether
        // in tether
        // attacking
        // escape! ?

        // move to the best tether you can
        // attack the player while you're in a tether
        // reevaluate your tether (every once in a while and when player too close)

        if (dead)
        {
            stateTimer += Time.unscaledDeltaTime;
            if (stateTimer >= maxDeadTime)
            {
                Destroy(this.gameObject);
            }
        }
        if (!dead && nav.enabled)
        {
            stateTimer += Time.unscaledDeltaTime;
            switch (state)
            {
                case enemyState.movingState:
                    state = MoveToTether();
                    CheckIfFiring();
                    break;
                case enemyState.tetheredState:
                    state = TetheredBehavior();
                    CheckIfFiring();
                    break;
                case enemyState.laserState:
                    state = LaserBehavior();
                    break;
            }
        }
    }

    private void CheckIfFiring()
    {
        fireTimer += Time.fixedDeltaTime;
        if (!firing && fireTimer >= timeToNextFire && CheckLineOfSight())
        {
            //only called with 0 for now, since only one attack type exists
            if (Vector3.Distance(player.transform.position, gameObject.transform.position) > 20 && Mathf.Abs(player.transform.position.y - transform.position.y) < 0.5f)
            {
                if (TryAttack(1))
                {
                    state = enemyState.laserState;
                    LaserSetup();
                }
            }
            if (state != enemyState.laserState)
            {
                if (TryAttack(0))
                {
                    firing = true;
                    fireTimer = 0;
                }
            }
        }

        if (firing)
        {
            material.color = Color.Lerp(defaultColor, fireColor, fireTimer / fireWindupTime);
            //stopgap to prevent firing into walls - cylinder will hold token indefinitely if it can never see player
            if (fireTimer >= fireWindupTime && CheckLineOfSight())
            {
                audioSource.PlayOneShot(firingSound, 0.8f);
                FireProjectile();
            }
        }
    }

    private enemyState MoveToTether()
    {
        //PROBLEM: tether = this.findBestTether();
        // check if position is in radius of tether
        
        if (Vector3.Distance(tether.transform.position, this.transform.position) <= tetherRadius)
        {
            stateTimer = 0;
            return enemyState.tetheredState;
        }
        else
        {
            nav.SetDestination(destination);
            return enemyState.movingState;        
        }      
    }

    private void LaserSetup()
    {
        nav.isStopped = true;
        fireTimer = 0;
        targetLine.transform.LookAt(player.gameObject.transform.position);
        targetLine.gameObject.SetActive(true);
        activeLaser = targetLine;
        laserSoundPlayed = false;
    }

    private enemyState TetheredBehavior()
    {
        // if player gets too close re-evaluate or wait the appropriate amount of time to evaluate next tether
        if (playerTooClose || stateTimer >= reevaluateTetherTime)
        {
            GameObject newTether = this.findBestTether();
            if (newTether == tether)
            {
                nav.SetDestination(this.FindNewPositionInTether());
                return enemyState.tetheredState;
            }
            else
            {
                tether = newTether;
                tetherRadius = tether.GetComponent<TetherController>().Radius;
                destination = FindNewPositionInTether();
                return enemyState.movingState;
            }
        }
        return enemyState.tetheredState;
    }

    private enemyState LaserBehavior()
    {
        fireTimer += Time.fixedDeltaTime;

        if (!firing && fireTimer <= laserTargetingTime)
        {
            activeLaser.transform.LookAt(player.gameObject.transform.position);
        }
        else if (!laserSoundPlayed)
        {
            audioSource.PlayOneShot(laserSound);
            laserSoundPlayed = true;
        }

        RaycastHit hit;
        if (Physics.Raycast(activeLaser.transform.position, activeLaser.transform.forward, out hit, laserMaxLength, LayerMask.GetMask("Default", "Enemy", "Spiders", "Player")))
        {
            activeLaser.SetPosition(1, new Vector3(0, 0, Vector3.Distance(hit.point, transform.position)));
            if (firing)
            {
                if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Spiders")) 
                    && hit.collider.gameObject != this.gameObject)
                {
                    hit.collider.gameObject.GetComponent<EnemyController>().takeDamage(hit.collider.gameObject.transform.position);
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    hit.collider.gameObject.GetComponent<PlayerHealth>().TakeDamage(laserDamagePerFrame, player.transform.position - transform.position, laserForce);
                }
            }
        }
        else
        {
            activeLaser.SetPosition(1, new Vector3(0, 0, laserMaxLength));
        }

        if (!firing && fireTimer >= laserTargetingTime + laserWindupTime)
        {
            firing = true;
            laserLine.gameObject.SetActive(true);
            laserLine.transform.SetPositionAndRotation(targetLine.transform.position, targetLine.transform.rotation);
            laserLine.SetPosition(1, targetLine.GetPosition(1));
            targetLine.gameObject.SetActive(false);

            activeLaser = laserLine;
            fireTimer = 0;
        }

        //Below code to be used to lerp laser towards player - this is unnecessary for laser type being currently implemented
        /*
        Vector3 relativePos = player.gameObject.transform.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, laserRotationLerp);
        */

        if (firing && fireTimer >= laserFiringTime)
        {
            EndAttack();
            nav.isStopped = false;
            laserLine.gameObject.SetActive(false);
            activeLaser = null;
            return enemyState.tetheredState;
        }
        return enemyState.laserState;
    }
    

    private Vector3 FindNewPositionInTether()
    {
        return tether.transform.position + (Vector3)Random.insideUnitCircle * tetherRadius;
    }

    private bool TryAttack(int attackType)
    {
        token = enemyAttackTokenPool.RequestToken(this.gameObject, attackType);
        return (token != null);
    }

    private void EndAttack()
    {
        enemyAttackTokenPool.ReturnToken(this.type, token);
        token = null;
        firing = false;
        material.color = defaultColor;
        timeToNextFire = firingFrequency + Random.Range(-firingFrequencyRange, firingFrequencyRange);
        fireTimer = 0;
    }

    private void FireProjectile()
    {
        transform.LookAt(player.gameObject.transform.position);
        GameObject bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
        bullet.GetComponent<Projectile>().Fire(this.transform.position, player.transform.position, bulletSpeed, bulletDamage, bulletForce);

        EndAttack();
    }

    protected override bool CheckLineOfSight()
    {
        RaycastHit seePlayer;
        Ray ray = new Ray(transform.position, player.transform.position - transform.position);

        if (Physics.Raycast(ray, out seePlayer, Mathf.Infinity))
        {
            if (seePlayer.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public override void takeDamage(Vector3 point)
    {
        if (!dead)
        {
            if (token != null)
            {
                enemyAttackTokenPool.ReturnToken(type, token);
            }
            playerCamera.gameObject.GetComponent<ScoreTracker>().ChangeScore(scoreValue, transform.position);
            dead = true;
            stateTimer = 0;
            GameObject fractureInstance = Instantiate(fractures, transform.position, transform.rotation);
            fractureInstance.GetComponent<AudioSpeedByTime>().AssignTimeScaleManager(player.GetComponentInChildren<TimeScaleManager>());
            Instantiate(explosion, point, transform.rotation);
            explosion.Play();

            DestroyThis();
        }
    }

    private bool playerTooClose
    {
        get
        {
            return (Vector3.Distance(player.transform.position, this.transform.position) < runAwayDistance);
        }
    }

    public override void freeze()
    {
        nav.enabled = false;
    }

    private GameObject findBestTether()
    {
        //4 data points to weigh
        //Sight Ratio (visibility)
        //Distance from player - probably want around 20 units of distance
        //Distance from tether
        //Current occupants of tether

        TetherController[] tethers = tetherTracker.Tethers;
        int[] weights = new int[tethers.Length];
        int minWeightIndex = -1;
        int minWeight = int.MaxValue;

        for (int i = 0; i < tethers.Length; i++)
        {
            weights[i] += 10*tethers[i].Occupants^2;
            //weight distance from tether to AI as distance/4
            weights[i] += (int)(Vector3.Distance(tethers[i].gameObject.transform.position, gameObject.transform.position)/4);

            //We want distance from tether to player to be 20, so weight the difference of actual distance from that by difference*2
            weights[i] += (int)(Mathf.Abs(
                Vector3.Distance(tethers[i].gameObject.transform.position, player.transform.position) - 20))*2;

            //Weight inverse of trace ratio at inverseRatio*50. Perfect visibility weights 0, no visibility weights 50;
            weights[i] += (int)((1 - tethers[i].TraceRatio) * 50);

            if (weights[i] < minWeight)
            {
                minWeightIndex = i;
                minWeight = weights[i];
            }
        }

        if (tether != null)
        {
            tether.GetComponent<TetherController>().incrementOccupantsBy(-1);
        }
        tethers[minWeightIndex].incrementOccupantsBy(1);

        return tethers[minWeightIndex].gameObject;
    }

    protected override bool isVisible()
    {
        return cachedRenderer.isVisibleFrom(playerCamera);
    }
}