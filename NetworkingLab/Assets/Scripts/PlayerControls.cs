using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerControls : NetworkBehaviour 
{
	public float linearSpeed = 5.0f;
	public float angularSpeed = 5.0f;
	public bool hasFlag;

	// Networking
	//public float updateTransformLatency = 0.5f;				// how often to send packets
	//private float currentUpdateTransformLatency = 0.0f;		// 
	//NetworkClient myClient;

	private Rigidbody rb = null;

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
		hasFlag = false;

/*		if(!isLocalPlayer)
		{
			return;
		}

		myClient = NetworkManager.singleton.client;
		myClient.RegisterHandler (CustomMsgType.Transform, OnTransformReceived);*/
	}

	void FixedUpdate()
	{
		if(!isLocalPlayer)
		{
			return;
		}

		Move ();
		if(Input.GetKeyDown(KeyCode.Space))
			CmdJump ();
		DropFlag ();
	}

	void Move()
	{
		float rotationalInput = Input.GetAxis ("Horizontal");
		float forwardInput = Input.GetAxis ("Vertical");

		Vector3 forwardVector = this.transform.forward;
		Vector3 linearVelocity = this.transform.forward * (forwardInput * linearSpeed);
		float yVelocity = rb.velocity.y;
		linearVelocity.y = yVelocity;
		rb.velocity = linearVelocity;

		Vector3 angularVelocity = this.transform.up * (rotationalInput * angularSpeed);
		rb.angularVelocity = angularVelocity;
	}
		
	public void Jump()
	{
		Vector3 jumpVelocity = Vector3.up * 5.0f;
		rb.velocity += jumpVelocity;
	}

	[ClientRpc]
	public void RpcJump()
	{
		Jump ();
	}

	[Command]
	public void CmdJump()
	{
		Jump ();
		RpcJump ();
	}

	void DropFlag()
	{
		if(hasFlag && Input.GetKeyDown(KeyCode.Q))
		{
			hasFlag = false;
			GameObject flag = GameObject.FindWithTag("Flag");

			Rigidbody flag_rb = flag.GetComponent<Rigidbody> ();
			//flag_rb.isKinematic = false;
			flag_rb.AddForce (Vector3.up * 200.0f);
			flag.GetComponentInChildren<FlagScript> ().isTaken = false;
			flag.transform.parent = null;			
			//flag.transform.localScale *= 2.0f;
			//flag.transform = flag.GetComponent<FlagScript> ().flagTransform;

		}
	}
}