using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using PlayerManager;

public class NetworkPlayer : NetworkMessageHandler
{
	[Header("Player Properties")]
	public string playerID;
	public int playerFuel = 100;
	public int maxPlayerFuel = 100;
	public float jetpackSpeed = 1.5f;
	public float jetpackCoolDown = 1.5f;
	public Slider fuelSlider = null;
	public ParticleSystem jetpackParticles = null;
	public bool isThrusting = false;
	public bool isPowerUp = false;
	public bool isFlagPowerUp = false;

	private float timeStamp;

	[Header("Transform Update Properties")]
	public bool canSendNetworkMovement;
	public float networkSendRate = 5.0f;
	public float timeBetweenMovementStart;
	public float timeBetweenMovementEnd;

	[Header("Player Physics Properties")]
	public float linearSpeed = 10.0f;
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
		jetpackParticles = GetComponentInChildren<ParticleSystem> ();
		rb = GetComponent<Rigidbody> ();

		fuelSlider = GameObject.FindGameObjectWithTag ("fuelSlider").GetComponent<Slider>();
		fuelSlider.value = playerFuel;

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
		NetworkManager.singleton.client.RegisterHandler(flagPos_msg, OnReceiveFlagMovementMessage);
	}

	private void OnReceiveMovementMessage(NetworkMessage _message)
	{
		PlayerMovementMessage _msg = _message.ReadMessage<PlayerMovementMessage>();

		if (_msg.objectTransformName != transform.name)
		{
			Manager.Instance.ConnectedPlayers[_msg.objectTransformName].GetComponent<NetworkPlayer>().ReceiveMovementMessage(_msg.objectPosition, _msg.objectRotation, _msg.time);
		}
	}

	private void OnReceiveFlagMovementMessage(NetworkMessage _message)
	{
		FlagMovementMessage _msg = _message.ReadMessage<FlagMovementMessage>();

		if (_msg.flagOwnerName != transform.name) 
		{
			Manager.Instance.ConnectedPlayers [_msg.flagOwnerName].GetComponent<NetworkPlayer> ().ReceiveFlagMovementMessage (_msg.flagPosition, _msg.flagRotation, _msg.time);
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

	// Set positions, if positions are not equal, start lerping
	public void ReceiveFlagMovementMessage(Vector3 _pos, Quaternion _rot, float _timeToLerp)
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
			if(isPowerUp)
			{
				StartCoroutine (PowerUp());
			}

			if(isFlagPowerUp)
			{
				StartCoroutine (FlagPowerUp());
			}

			UpdatePlayerMovement();
			Jump ();
			RechargeJetPack ();
			fuelSlider.value = playerFuel;
		}
		else
		{
			if(rb.velocity.y > 0.0f)
			{
				if(jetpackParticles.isStopped)
					jetpackParticles.Play ();
			}
			else
			{
				jetpackParticles.Stop ();
			}

			return;
		}
	}

	private void UpdatePlayerMovement()
	{
		//float rotationalInput = Input.GetAxis ("Horizontal");
		float forwardInput = Input.GetAxis ("Vertical");

		Vector3 forwardVector = this.transform.forward;
		Vector3 linearVelocity = this.transform.forward * (forwardInput * linearSpeed);
		float yVelocity = rb.velocity.y;
		linearVelocity.y = yVelocity;
		rb.velocity = linearVelocity;

		if(Input.GetKey(KeyCode.A))
		{
			// go Left

		}

		if(Input.GetKey(KeyCode.D))
		{
			// go Right

		}

		//Vector3 angularVelocity = this.transform.up * (rotationalInput * angularSpeed);
		//rb.angularVelocity = angularVelocity;

		if (!canSendNetworkMovement)
		{
			canSendNetworkMovement = true;
			StartCoroutine(StartNetworkSendCooldown());
		}
	}

	public void Jump()
	{
		if(Input.GetKey(KeyCode.Space) && playerFuel > 5)
		{
			if(jetpackParticles.isStopped)
				jetpackParticles.Play ();
			Vector3 jumpVelocity = Vector3.up * jetpackSpeed;
			rb.velocity += jumpVelocity;
			playerFuel -= 3;
			timeStamp = Time.time + jetpackCoolDown;
		}
		else
		{
			jetpackParticles.Stop ();
		}
	}

	public void RechargeJetPack()
	{
		if(playerFuel < 100 && Time.time > timeStamp + jetpackCoolDown)
			playerFuel += 2;
	}

	private IEnumerator PowerUp()
	{
		Debug.Log ("powerup begins for: " + this.name);
		this.jetpackCoolDown = 0.5f;
		//fuelSlider.colors.normalColor = Color.blue;
		yield return new WaitForSeconds (10.0f);
		isPowerUp = false;
		this.jetpackCoolDown = 1.5f;
		//fuelSlider.GetComponent<
		Debug.Log ("powerup ends for: " + this.name);
	}

	private IEnumerator FlagPowerUp()
	{
		Debug.Log ("flag powerup begins for: " + this.name);
		this.linearSpeed = 20.0f;
		yield return new WaitForSeconds (10.0f);
		isFlagPowerUp = false;
		this.linearSpeed = 10.0f;
		Debug.Log ("flag powerup ends for: " + this.name);
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