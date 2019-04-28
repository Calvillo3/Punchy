using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaPickup : PickupController
{
    [SerializeField] int StaminaAmount = 100;


    private void OnTriggerEnter(Collider col)
    {
        col.gameObject.GetComponent<PlayerStamina>().RegainStaminaWithoutRegen(StaminaAmount);
        col.gameObject.GetComponent<PickupSpawner>().PickedUp(gameObject);
    }

}
