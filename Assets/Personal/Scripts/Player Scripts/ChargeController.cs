using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeController : MonoBehaviour {
	private float currentCharge;
	private float chargedTimer;
    private float cooldownTimer;
	private bool chargeCooling;
	private bool charged;
    private PlayerMover playerMover;
    private PlayerValues playerValues;
	private float chargeTimeout;
	private float timeToCharge;
	private float chargeCooldownTime;
	private float slowmoTimescale;
	private Image chargeWheel;
    private Image slowmoWheel;
	private float attackRange;
	private LayerMask enemyMask;
    private LayerMask emptyMask;
    private LayerMask enemyAndDefaultMask;
    TimeScaleManager timeScaleManager;
	Camera mainCamera;

#if UNITY_EDITOR
    //For gizmos/debugging purposes
    Ray debugCheckRay;
    Ray debugLine;
    Ray playerRay;
    Vector3 point;
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(debugCheckRay.origin, debugCheckRay.direction*10);
        Gizmos.DrawRay(debugLine.origin, debugLine.direction);
        Gizmos.DrawRay(playerRay.origin, playerRay.direction * 10);

        Gizmos.DrawWireSphere(point, 0.1f);
    }
#endif

    // Use this for initialization
    void Start () {
        playerMover = gameObject.GetComponent<PlayerMover>();
        playerValues = playerMover.playerValues;
        chargeTimeout = playerValues.chargeValues.ChargeTimeout;
        timeToCharge = playerValues.chargeValues.TimeToCharge;
        chargeCooldownTime = playerValues.chargeValues.ChargeCooldownTime;
        slowmoTimescale = playerValues.chargeValues.SlowmoTimescale;
        chargeWheel = playerValues.chargeValues.ChargeWheel;
        slowmoWheel = playerValues.chargeValues.SlowmoWheel;
        attackRange = playerValues.chargeValues.AttackRange;

		currentCharge = 0;
		chargedTimer = 0;
		chargeWheel.type = Image.Type.Filled;
		chargeWheel.fillMethod = Image.FillMethod.Radial360;
		chargeWheel.fillAmount = 0f;
        slowmoWheel.type = Image.Type.Filled;
        slowmoWheel.fillMethod = Image.FillMethod.Radial360;
        slowmoWheel.fillAmount = 0f;
        enemyMask = LayerMask.GetMask("Enemy", "Spiders");
        enemyAndDefaultMask = LayerMask.GetMask("Enemy", "Spiders", "Default");
        emptyMask = LayerMask.GetMask();
		mainCamera = Camera.main;
        timeScaleManager = mainCamera.GetComponent<TimeScaleManager>();
	}


    
	//returns a Raycasthit with non-null collider if we should attack
	public RaycastHit Charge(bool charging)
	{
        RaycastHit hit = GetNullHit();
		if (!charging && !charged && !chargeCooling && currentCharge > 0) {
			currentCharge -= Time.fixedDeltaTime * 2;
			UpdateChargeWheel ();
			return hit;
		}

		if (chargeCooling) {
			cooldownTimer += Time.deltaTime;
			if (cooldownTimer >= chargeCooldownTime) {
				chargeCooling = false;
				chargedTimer = 0;
				chargeWheel.color = Color.white;
			}
			UpdateChargeWheel ();
			return hit;
		}

		if (charging && !charged && !chargeCooling) {
			currentCharge += Time.fixedDeltaTime;
			if (currentCharge >= timeToCharge)
			{
				charged = true;
                timeScaleManager.ChangeTimeScale(slowmoTimescale, chargeTimeout, 0f);
				currentCharge = 0;
			} 
			else
			{
				UpdateChargeWheel ();
				return hit;
			}
		}
			
		if (charged) {
            //Hopefully just a stopgap - if this is not done, then pausing the game causes fixedUnscaledDeltaTime to return the length of the pause, so if you pause
            //with a charge, it's guaranteed to try to execute right away when you unpause. 
            if (Time.fixedUnscaledDeltaTime < 1f)
            {
                chargedTimer += Time.fixedUnscaledDeltaTime;
            }
			chargeWheel.fillAmount = 1;
            UpdateSlowmoWheel();

            //if we're charged too long, attempt an attack by default
            if (chargedTimer >= chargeTimeout)
            {
                charging = false;
            }

			//if we're charged and the fire button is released, check if we can successfully attack
			if (!charging)
			{
                hit = CheckAttack();
				if (hit.collider != null)
				{
					chargeWheel.color = Color.white;
				}
				else
				{
					chargeCooling = true;
                    cooldownTimer = 0;
					chargeWheel.color = Color.red;
                    hit = GetNullHit();
				}
				timeScaleManager.FullspeedTimeScale();
				chargedTimer = 0;
				charged = false;
				UpdateChargeWheel ();
                UpdateSlowmoWheel();
				return hit;
			}

			//while charged, reticle is green while enemy in range and in sights
			if (CheckAttack().collider != null)
			{
				chargeWheel.color = Color.green;
				return hit;
			} 
			else
			{
				chargeWheel.color = Color.white;
			}
		}
		return hit;
	}

    private RaycastHit GetNullHit()
    {
        Ray attackRay = new Ray();
        RaycastHit attackHit;

        Physics.Raycast(attackRay, out attackHit, attackRange, emptyMask);
        return attackHit;
    }

    private void UpdateSlowmoWheel()
    {
        if (charged)
        {
            slowmoWheel.fillAmount = 1 - (chargedTimer / chargeTimeout);
        }
        else
        {
            slowmoWheel.fillAmount = 0;
        }
    }

    private void UpdateChargeWheel()
    {
        if (chargeCooling)
        {
            chargeWheel.fillAmount = 1 - (cooldownTimer / chargeCooldownTime);
        }
        else
        {
            chargeWheel.fillAmount = currentCharge / timeToCharge;
        }
    }


    private RaycastHit CheckAttack()
    {

        Ray attackRay;
        RaycastHit attackHit;
        RaycastHit nullHit = GetNullHit();
        attackRay = mainCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
#if UNITY_EDITOR
        //For gizmos/debugging targeting;
        playerRay = attackRay;
#endif
        Ray checkRay;
        float radius = 1f;

        if (Physics.Raycast(attackRay, out attackHit, attackRange, enemyAndDefaultMask))
        {
            if (attackHit.transform.gameObject.tag == "Enemy")
            {
                return attackHit;
            }
        }

        RaycastHit[] hits = Physics.SphereCastAll(attackRay, radius, attackRange, enemyMask);

        if (hits.Length > 0)
        {
            RaycastHit closestHit = nullHit;
            float distance = Mathf.Infinity;
            foreach (RaycastHit hit in hits)
            {
                //if hit is closest to attackRay and tagged as enemy
                checkRay = new Ray(mainCamera.transform.position, (RayFromNearestPointOnLine(attackRay, hit).point - mainCamera.transform.position).normalized);
#if UNITY_EDITOR
                //for debug
                debugCheckRay = checkRay;
#endif


                if (Physics.Raycast(checkRay, out attackHit, attackRange, enemyAndDefaultMask))
                {
                    if (attackHit.transform.gameObject.tag == "Enemy")
                    {
                        if (attackHit.distance < distance)
                        {
                            closestHit = hit;
                            distance = attackHit.distance;
                        }
                    }
                }
            }
            return closestHit;
        }
        return GetNullHit();
    }
    
    private RaycastHit RayFromNearestPointOnLine(Ray line, RaycastHit hit)
    {
        Vector3 targetPosition = hit.collider.gameObject.transform.position;
        Vector3 positionDifference = targetPosition - mainCamera.transform.position;
        float magnitude = Vector3.Dot(positionDifference, line.direction);
        Vector3 origin = mainCamera.transform.position + line.direction * magnitude;
        RaycastHit rayHit;
        Ray ray = new Ray(origin, (targetPosition - origin).normalized);
        Physics.Raycast(ray, out rayHit, enemyAndDefaultMask);
#if UNITY_EDITOR
        //for debugging
        debugLine = ray;
        point = rayHit.point;
#endif
        return rayHit;
    }
}
