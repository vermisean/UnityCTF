using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TransformUpdate : NetworkBehaviour 
{
	public float updateTransformLatency = 0.5f;				// how often to send packets
	private float currentUpdateTransformLatency = 0.0f;		// 

	NetworkClient m_client;

	void Start () 
	{
		if (!isLocalPlayer)
		{
			m_client = NetworkManager.singleton.client;
			m_client.RegisterHandler(CustomMsgType.Transform, OnTransformReceived);
		}
	}

	void Update () 
	{
		currentUpdateTransformLatency += Time.deltaTime;
		if(currentUpdateTransformLatency >= updateTransformLatency)
		{
			TransformMessage msg = new TransformMessage ();
			msg.netID = this.netId.Value;
			msg.position = this.transform.position;
			msg.rotation = this.transform.rotation;

			if(isServer)
			{
				NetworkServer.SendToAll (CustomMsgType.Transform, msg);
			}
			else
			{
				NetworkManager.singleton.client.Send (CustomMsgType.Transform, msg);
			}


			//Debug.Log ("Msg NetID: " + msg.netID.ToString ());
			//Debug.Log ("SendtoAll: " + isSuccess);

			currentUpdateTransformLatency = 0.0f;

		}
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority ();
		//Debug.Log ("Started OnStartAuthority");

	}

	public override void OnStartClient()
	{
		base.OnStartClient ();
		//Debug.Log ("Started OnStartClient");

	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer ();
		//Debug.Log ("Started OnStartLocalPlayer");

	}

	public override void OnStartServer()
	{
		base.OnStartServer ();
		//Debug.Log ("Started OnStartServer");

	}

	//Packets
	public class CustomMsgType
	{
		public static short Transform = MsgType.Highest + 1;
	}

	public class TransformMessage : MessageBase
	{
		public uint netID;
		public Vector3 position;
		public Quaternion rotation;
		//public Vector3 scale;
	}

	public void OnTransformReceived(NetworkMessage netMsg)
	{
		TransformMessage msg = netMsg.ReadMessage<TransformMessage> ();
		//Debug.Log ("On Transform Received! isLocalPlayer: " + isLocalPlayer);
		//Debug.Log ("NetID: " + msg.netID);
		// Dont want local player getting this - should have a check (assert below)
		//Debug.Assert (!isLocalPlayer);

		if(msg.netID == this.netId.Value)
		{
			this.transform.position = msg.position;
			this.transform.rotation = msg.rotation;
			//this.transform.localScale = msg.scale;
		}

	}
}
