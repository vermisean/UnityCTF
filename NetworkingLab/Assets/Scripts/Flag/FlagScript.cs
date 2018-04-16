using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FlagScript : NetworkMessageHandler
{
	public enum FlagState
	{
		Available = 0,
		Captured
	}

	[Header("Flag Properties")]
	public string flagOwnerName;
	[SyncVar]
	public FlagState flagState;
	public bool isTaken = false;
	public ParticleSystem particleSys = null;

	[Header("Transform Update Properties")]
	public bool canSendNetworkMovement;
	public float networkSendRate = 5.0f;
	public float timeBetweenMovementStart;
	public float timeBetweenMovementEnd;

	[Header("Lerping Properties")]
	public bool isLerpingPos = false;
	public bool isLerpingRot = false;
	public Vector3 realPos;
	public Quaternion realRot;
	public Vector3 lastRealPos;
	public Quaternion lastRealRot;
	public float timeStartedLerp;
	public float timeToLerp;

	[Header("Flag Properties")]
	public bool hasFlag;

	void Start () 
	{
		flagOwnerName = "none";
		particleSys.Play ();
		flagState = FlagState.Available;
	}

	void Update ()
	{
/*		if (!canSendNetworkMovement)
		{
			canSendNetworkMovement = true;
			StartCoroutine(StartNetworkSendCooldown());
		}*/
	}

	void OnTriggerEnter(Collider col)
	//local authority and visible - should be disabled while flag in possession
	{
		if(col.gameObject.tag == "Player" && flagState == FlagState.Available)
		{
			isTaken = true;
			flagState = FlagState.Captured;
			particleSys.Stop ();
			//this.GetComponent<Rigidbody> ().isKinematic = true;
			this.transform.parent = col.gameObject.transform;
			col.GetComponent<NetworkPlayer> ().hasFlag = true;
			flagOwnerName = this.transform.parent.name;
			this.transform.localPosition = Vector3.zero;
			this.transform.localRotation = Quaternion.identity;
		}
	}

	/*// Ensures the movement is sent at appropriate times
	private IEnumerator StartNetworkSendCooldown()
	{
		timeBetweenMovementStart = Time.time;
		yield return new WaitForSeconds((1 / networkSendRate));
		SendNetworkFlagMovement();
	}

	// Ensures the exact time taken to send the message lines up precisely with the player
	private void SendNetworkFlagMovement()
	{
		timeBetweenMovementEnd = Time.time;
		SendFlagMovementMessage(flagOwnerName, transform.position, transform.rotation, (timeBetweenMovementEnd - timeBetweenMovementStart));
		canSendNetworkMovement = false;
	}

	// Sends the transform and time through to the clients
	public void SendFlagMovementMessage(string _flagOwnerName, Vector3 _position, Quaternion _rotation, float _timeTolerp)
	{
		FlagMovementMessage _msg = new FlagMovementMessage()
		{
			flagOwnerName = _flagOwnerName,
			flagPosition = _position,
			flagRotation = _rotation,
			time = _timeTolerp
		};

		NetworkManager.singleton.client.Send(movement_msg, _msg);
	}*/
}
