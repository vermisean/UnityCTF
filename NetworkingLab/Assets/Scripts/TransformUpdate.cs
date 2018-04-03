using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TransformUpdate : NetworkBehaviour 
{
	public float updateTransformLatency = 0.5f;				// how often to send packets
	private float currentUpdateTransformLatency = 0.0f;		// 

	void Start () 
	{
		
	}

	void Update () 
	{
		currentUpdateTransformLatency += Time.deltaTime;
		if(currentUpdateTransformLatency >= updateTransformLatency)
		{
			//NetworkServer.SendToAll()
		}
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority ();
		Debug.Log ("Started OnStartAuthority");
	}

	public override void OnStartClient()
	{
		base.OnStartClient ();
		Debug.Log ("Started OnStartClient");
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer ();
		Debug.Log ("Started OnStartLocalPlayer");
		this.GetComponent<MeshRenderer> ().material.color = Color.cyan;
	}

	public override void OnStartServer()
	{
		base.OnStartServer ();
		Debug.Log ("Started OnStartServer");
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
		public Vector3 scale;
	}

	public void OnTransformReceived(NetworkMessage netMsg)
	{
		TransformMessage msg = netMsg.ReadMessage<TransformMessage> ();
		Debug.Log ("On Transform Received! isLocalPlayer: " + isLocalPlayer);
		// Dont want local player getting this - should have a check (assert below)
		Debug.Assert (!isLocalPlayer);

		this.transform.position = msg.position;
		this.transform.rotation = msg.rotation;
		this.transform.localScale = msg.scale;
	}
}
