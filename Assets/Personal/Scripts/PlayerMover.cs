﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson; //FirstPersonController made this the namespace


public class PlayerMover : MonoBehaviour
{
	[SerializeField] public float speed;
	[SerializeField] public float stickToGroundForce;
	private CharacterController characterController;
	private CollisionFlags collisionFlags;
    public PlayerState currentState;
	[SerializeField] public float jumpSpeed;
    [SerializeField] private MouseLook _mouseLook;
    public MouseLook mouseLook
    {
        get { return _mouseLook; }
    }
    private Camera m_Camera;



    // Use this for initialization
    void Start () {
        m_Camera = Camera.main;
		characterController = GetComponent<CharacterController>();
        mouseLook.Init(transform, m_Camera.transform);
        currentState = new GroundState(this);
    }


	
	// Update is called once per frame
	void FixedUpdate ()
	{
	    PlayerState newState = currentState.FixedUpdate();
	    if (newState != null)
	    {
	        currentState.Exit();
	        newState.Enter();
	        currentState = newState;
	    }
	}



    void Update()
    {
        currentState.Update();
    }




	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		//dont move the rigidbody if the character is on top of it
		if (collisionFlags == CollisionFlags.Below)
		{
			return;
		}

		if (body == null || body.isKinematic)
		{
			return;
		}
		body.AddForceAtPosition(characterController.velocity*0.1f, hit.point, ForceMode.Impulse);
	}








	public void Move(Vector3 movement){
		collisionFlags = characterController.Move(movement*Time.fixedDeltaTime);
	}

	public void PlayLandSound(){
		
	}

	public RaycastHit GetSurfaceNormal()
	{
		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
			characterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		return hitInfo;
	}

	public bool isGrounded(){
		return characterController.isGrounded;
	}

}
