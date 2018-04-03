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
	public float updateTransformLatency = 0.5f;				// how often to send packets
	private float currentUpdateTransformLatency = 0.0f;		// 
	NetworkClient myClient;

	private Rigidbody rb = null;

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
		hasFlag = false;

		if(!isLocalPlayer)
		{
			return;
		}

		myClient = NetworkManager.singleton.client;
		myClient.RegisterHandler (CustomMsgType.Transform, OnTransformReceived);
	}

	void FixedUpdate()
	{
		if(!isLocalPlayer)
		{
			return;
		}

		Move ();
		DropFlag ();
	}

	void Move()
	{
		float rotationalInput = Input.GetAxis ("Horizontal");
		float forwardInput = Input.GetAxis ("Vertical");

		Vector3 forwardVector = this.transform.forward;
		Vector3 linearVelocity = this.transform.forward * (forwardInput * linearSpeed);
		rb.velocity = linearVelocity;

		Vector3 angularVelocity = this.transform.up * (rotationalInput * angularSpeed);
		rb.angularVelocity = angularVelocity;

		currentUpdateTransformLatency += Time.deltaTime;

		if(currentUpdateTransformLatency >= updateTransformLatency)
		{
			TransformMessage msg = new TransformMessage ();
			msg.position = this.transform.position;
			msg.rotation = this.transform.rotation;
			NetworkServer.SendToAll (CustomMsgType.Transform, msg);
			currentUpdateTransformLatency = 0.0f;
		}
	}

	void DropFlag()
	{
		if(hasFlag && Input.GetKeyDown(KeyCode.Q))
		{
			hasFlag = false;
			GameObject flag = GameObject.FindWithTag("Flag");
			Rigidbody flag_rb = flag.GetComponent<Rigidbody> ();
			//flag_rb.isKinematic = false;
			flag_rb.AddForce (Vector3.up * 20.0f);
			flag.GetComponentInChildren<FlagScript> ().isTaken = false;
			flag.transform.parent = null;			
			flag.transform.localScale *= 2.0f;

			//flag.transform = flag.GetComponent<FlagScript> ().flagTransform;
		}
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority ();
	}

	public override void OnStartClient()
	{
		base.OnStartClient ();
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer ();
		this.GetComponent<MeshRenderer> ().material.color = Color.cyan;

		myClient.UnregisterHandler (CustomMsgType.Transform);
	}

	public override void OnStartServer()
	{
		base.OnStartServer ();
	}

	//Packets
	public class CustomMsgType
	{
		public static short Transform = MsgType.Highest + 1;
	}

	public class TransformMessage : MessageBase
	{
		public Vector3 position;
		public Quaternion rotation;
		//public Vector3 scale;
	}

	public void OnTransformReceived(NetworkMessage netMsg)
	{
		TransformMessage msg = netMsg.ReadMessage<TransformMessage> ();
		Debug.Log ("On Transform Received! isLocalPlayer: " + isLocalPlayer);
		// Dont want local player getting this - should have a check (assert below)
		//Debug.Assert (!isLocalPlayer);

		this.transform.position = msg.position;
		this.transform.rotation = msg.rotation;
		//this.transform.localScale = msg.scale;
	}
}