using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using PlayerManager;

public class NetworkPlayer : NetworkMessageHandler
{
	[Header("Player Properties")]
	public string playerID;

	[Header("Transform Update Properties")]
	public bool canSendNetworkMovement;
	public float networkSendRate = 5.0f;
	public float timeBetweenMovementStart;
	public float timeBetweenMovementEnd;

	[Header("Player Physics Properties")]
	public float linearSpeed = 5.0f;
	public float angularSpeed = 5.0f;

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


	private Rigidbody rb = null;

	private void Start()
	{
		playerID = "player" + GetComponent<NetworkIdentity>().netId.ToString();
		transform.name = playerID;
		Manager.Instance.AddPlayerToConnectedPlayers(playerID, gameObject);

		rb = GetComponent<Rigidbody> ();

		if (isLocalPlayer)
		{
			Manager.Instance.SetLocalPlayerID(playerID);

			canSendNetworkMovement = false;
			RegisterNetworkMessages();
		} 
		else // This never happens locally
		{
			isLerpingPos = false;
			isLerpingRot = false;

			realPos = transform.position;
			realRot = transform.rotation;
		}

	}

	private void RegisterNetworkMessages()
	{
		NetworkManager.singleton.client.RegisterHandler(movement_msg, OnReceiveMovementMessage);
	}

	private void OnReceiveMovementMessage(NetworkMessage _message)
	{
		PlayerMovementMessage _msg = _message.ReadMessage<PlayerMovementMessage>();

		if (_msg.objectTransformName != transform.name)
		{
			Manager.Instance.ConnectedPlayers[_msg.objectTransformName].GetComponent<NetworkPlayer>().ReceiveMovementMessage(_msg.objectPosition, _msg.objectRotation, _msg.time);
		}
	}

	// Set positions, if positions are not equal, start lerping
	public void ReceiveMovementMessage(Vector3 _pos, Quaternion _rot, float _timeToLerp)
	{
		lastRealPos = realPos;
		lastRealRot = realRot;

		// We want to lerp from last known real pos/rot to movement message's new pos/rot
		realPos = _pos;
		realRot = _rot;

		// Makes it look smoother with exact numbers
		timeToLerp = _timeToLerp;

		if(realPos != transform.position)
		{
			isLerpingPos = true;
		}

		// Eulers are better for Unity to compare
		if(realRot.eulerAngles != transform.rotation.eulerAngles)
		{
			isLerpingRot = true;
		}

		timeStartedLerp = Time.time;
	}

	private void Update()
	{
		if(isLocalPlayer)
		{
			UpdatePlayerMovement();
			Jump ();
		}
		else
		{
			return;
		}
	}

	private void UpdatePlayerMovement()
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

		if (!canSendNetworkMovement)
		{
			canSendNetworkMovement = true;
			StartCoroutine(StartNetworkSendCooldown());
		}
	}

	public void Jump()
	{
		if(Input.GetKey(KeyCode.Space))
		{
			Vector3 jumpVelocity = Vector3.up * 5.0f;
			rb.velocity += jumpVelocity;
		}
	}

	// Ensures the movement is sent at appropriate times
	private IEnumerator StartNetworkSendCooldown()
	{
		timeBetweenMovementStart = Time.time;
		yield return new WaitForSeconds((1 / networkSendRate));
		SendNetworkMovement();
	}

	// Ensures the exact time taken to send the message lines up precisely with the player
	private void SendNetworkMovement()
	{
		timeBetweenMovementEnd = Time.time;
		SendMovementMessage(playerID, transform.position, transform.rotation, (timeBetweenMovementEnd - timeBetweenMovementStart));
		canSendNetworkMovement = false;
	}

	// Sends the transform and time through to the clients
	public void SendMovementMessage(string _playerID, Vector3 _position, Quaternion _rotation, float _timeTolerp)
	{
		PlayerMovementMessage _msg = new PlayerMovementMessage()
		{
			objectPosition = _position,
			objectRotation = _rotation,
			objectTransformName = _playerID,
			time = _timeTolerp
		};

		NetworkManager.singleton.client.Send(movement_msg, _msg);
	}

	// Allows for frame rate not to handle lerp (could be imprecise)
	private void FixedUpdate()
	{
		if(!isLocalPlayer)
		{
			NetworkLerp ();
		}
	}

	private void NetworkLerp()
	{
		if(isLerpingPos)
		{
			// Gets a percentage so that lerp is properly used through timesteps
			float lerpPercent = (Time.time - timeStartedLerp) / timeToLerp;

			transform.position = Vector3.Lerp (lastRealPos, realPos, lerpPercent);

			if(lerpPercent >= 1.0f)
			{
				isLerpingPos = false;
			}
		}

		if(isLerpingRot)
		{
			float lerpPercent = (Time.time - timeStartedLerp) / timeToLerp;

			transform.rotation = Quaternion.Lerp (lastRealRot, realRot, lerpPercent);

			if(lerpPercent >= 1.0f)
			{
				isLerpingRot = false;
			}
		}
	}
}