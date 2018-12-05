﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class AirState : PlayerState {
	PlayerMover playerMover;
	Vector3 move;
	bool charging;
	bool grounded;
    bool groundPound;
    float gravityMultiplier = 2;
	float airSpeedMultiplier = 1;
	float initialVerticalSpeed;
    float groundPoundStaminaCost = 25f;
	private ChargeController chargeController;
    private PlayerStamina playerStamina;
    RaycastHit hit;

	public AirState(PlayerMover pm) : base(pm)
	{
		playerMover = pm;
		initialVerticalSpeed = 0;
        chargeController = playerMover.ChargeController;
        playerStamina = playerMover.PlayerStamina;
        groundPound = false;
        vulnerable = true;
    }

	public AirState(PlayerMover pm, float verticalSpeed) : base(pm)
	{
		playerMover = pm;
		initialVerticalSpeed = verticalSpeed;
        chargeController = playerMover.ChargeController;
        playerStamina = playerMover.PlayerStamina;
        groundPound = false;
        vulnerable = true;
    }

	public override PlayerState FixedUpdate()
	{
		if (grounded)
		{
			return new GroundState (playerMover);
		}
        else if (groundPound)
		{
            if (playerStamina.UseStamina(groundPoundStaminaCost))
            {
                return new GroundPoundState(playerMover);
            }
		}

		Vector3 desiredMove = GetStandardDesiredMove (playerMover.speed * airSpeedMultiplier);

		move = new Vector3 (desiredMove.x, move.y, desiredMove.z);
		move += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;

		hit = chargeController.Charge (charging);
        if (hit.collider != null)
        {
            return new ChargeAttackState(playerMover, hit);
        }
		playerMover.Move (move);

		MouseLookFixedUpdate ();

		return null;
	}

	public override void Update()
	{
		charging = Input.GetButton ("Fire1");
		grounded = playerMover.isGrounded ();
		MouseLookUpdate ();
	    if (Input.GetButton("Crouch"))
	    {
	        groundPound = true;
	    }
	}

	public override void Enter ()
	{
		Vector3 desiredMove = GetStandardDesiredMove (playerMover.speed);
		move = new Vector3 (desiredMove.x, move.y+ initialVerticalSpeed, desiredMove.z);
		playerMover.Move (move);
        charging = Input.GetButton("Fire1");
    }
}