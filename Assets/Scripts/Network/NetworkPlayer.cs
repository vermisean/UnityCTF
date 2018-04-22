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
	public string playerName;
	public float possessionTime = 0.0f;
	public int playerFuel = 100;
	public int maxPlayerFuel = 100;
	public float jetpackSpeed = 1.5f;
	public float jetpackCoolDown = 1.5f;
	public Slider fuelSlider = null;
	public ParticleSystem jetpackParticles = null;
	public bool isThrusting = false;
	public bool isFuelPowerUp = false;
	public bool isSpeedPowerUp = false;
	private bool isFacingLeft = true;
	private GameObject[] spawnPoints;
	private Text respawnText;
	private bool isRespawning;
	private bool respawnStarted;
	private float respawnTime = 2.5f;
	private float respawnTimeForPlayer = 2.5f;
	private string winner;
	private Text winnerText;
	private Text endGamePanel;

	[Header("Score Properties")]
	public Text topPlayerText;
	public Text secondPlayerText;
	public Dictionary<string, float> playerScoreList;

	[Header("Bullet Properties")]
	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public float bulletSpeed = 8.0f;


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
	public GameObject flagSpawn;

	[Header("Game Time")]
	public Text timeText;
	public float gameTime = 120.0f;
	public bool isGameStarted = false;			//TODO stop players from moving til this is thrown
	[SyncVar]
	public bool isGameOver = false;

	private float timeStamp;
	private Rigidbody rb = null;


	/* ~~~~ Player ~~~~ */
	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		//gManager = FindObjectOfType<GameManager> ();
		CmdUpdatePlayers ();
		if(!isServer)
			CmdGetGameTime ();
		isGameStarted = true;
		GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 0.0f);
	}

	private void Start()
	{
		// Basic variable init
		Manager.Instance.AddPlayerToConnectedPlayers(playerID, gameObject);
		playerID = "player" + GetComponent<NetworkIdentity>().netId.ToString();
		transform.name = playerID;
		winner = "";
		isRespawning = false;
		jetpackParticles = GetComponentInChildren<ParticleSystem> ();
		rb = GetComponent<Rigidbody> ();

		// Spawns
		flagSpawn = GameObject.FindGameObjectWithTag ("FlagSpawn");
		fuelSlider = GameObject.FindGameObjectWithTag ("fuelSlider").GetComponent<Slider>();
		fuelSlider.value = playerFuel;
		spawnPoints = GameObject.FindGameObjectsWithTag ("SpawnPoints");

		// Text
		topPlayerText = GameObject.FindGameObjectWithTag("topPlayerText").GetComponent<Text>();
		topPlayerText.text = "";
		endGamePanel = GameObject.FindGameObjectWithTag ("endgamePanel").GetComponent<Text>();
		endGamePanel.text = "";
		respawnText = GameObject.FindGameObjectWithTag("respawnText").GetComponent<Text>();
		respawnText.text = "";
		timeText = GameObject.FindGameObjectWithTag("timerText").GetComponent<Text>();
		timeText.text = gameTime.ToString ("000");

		playerScoreList = new Dictionary<string, float>();

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
		
	private void Update()
	{
		if(isLocalPlayer && isRespawning)
		{
			if(respawnTimeForPlayer > 0 && !isGameOver)
			{
				respawnTimeForPlayer -= Time.deltaTime;
				respawnText.text = respawnTimeForPlayer.ToString ("0.0");
			}
			else if (respawnTimeForPlayer <= 0)
			{
				respawnText.text = "";
				isRespawning = false;
				respawnTimeForPlayer = 2.5f;
			}
		}

		if(isLocalPlayer) //&& isGameStarted)
		{
			GameTimerUpdate ();
			if(isFuelPowerUp)
			{
				StartCoroutine (FuelPowerUp());
			}

			if(isSpeedPowerUp)
			{
				StartCoroutine (SpeedPowerUp());
			}

			UpdatePlayerMovement();
			CmdJump ();		

			Shoot ();

			RechargeJetPack ();
			fuelSlider.value = playerFuel;

			if(hasFlag)
			{
				possessionTime += Time.deltaTime;
			}

			if(Input.GetKeyDown(KeyCode.F))
			{
				DropFlag ();
			}

			Vector3 mPos = Input.mousePosition;

			if(mPos.x > Screen.width / 2.0f && isFacingLeft)
			{
				isFacingLeft = false;
				Quaternion newRot = new Quaternion (transform.rotation.x, -transform.rotation.y, transform.rotation.z, transform.rotation.w);
				rb.rotation = newRot;
			}
			else if(mPos.x < Screen.width / 2.0f && !isFacingLeft)
			{
				isFacingLeft = true;
				Quaternion newRot = new Quaternion (transform.rotation.x, -transform.rotation.y, transform.rotation.z, transform.rotation.w);
				rb.rotation = newRot;
			}

		
		}
		else
		{
			if(rb.velocity.y > Mathf.Epsilon)
			{
				if(jetpackParticles.isStopped)
				{
					jetpackParticles.Play ();
				}
			}
			else
			{
				jetpackParticles.Stop ();
			}

			return;
		}
	}

	// Allows for frame rate not to handle lerp (could be imprecise)
	private void FixedUpdate()
	{
		if(!isLocalPlayer)
		{
			NetworkLerp ();
		}
	}

	private void UpdatePlayerMovement()
	{
		if(!isFacingLeft)
		{
			float forwardInput = Input.GetAxis ("Horizontal");

			Vector3 forwardVector = this.transform.forward;
			Vector3 linearVelocity = forwardVector * (forwardInput * linearSpeed);

			float yVelocity = rb.velocity.y;
			linearVelocity.y = yVelocity;
			rb.velocity = linearVelocity;
		}
		else
		{
			float forwardInput = Input.GetAxis ("Horizontal");

			Vector3 forwardVector = -this.transform.forward;
			Vector3 linearVelocity = forwardVector * (forwardInput * linearSpeed);

			float yVelocity = rb.velocity.y;
			linearVelocity.y = yVelocity;
			rb.velocity = linearVelocity;
		}


		if (!canSendNetworkMovement)
		{
			canSendNetworkMovement = true;
			StartCoroutine(StartNetworkSendCooldown());
		}
	}
		
	public void CmdJump()
	{
		if(Input.GetKey(KeyCode.Space) && playerFuel > 5)
		{
			if(jetpackParticles.isStopped)
			{
				jetpackParticles.Play ();		
			}
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

	public void Shoot()
	{
		if (Input.GetMouseButtonDown(0))
		{
			CmdFire();
		}

	}

	[Command]
	void CmdFire()
	{
		// Create the Bullet from the Bullet Prefab
		var bullet = (GameObject)Instantiate (
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

		// Spawn bullet
		NetworkServer.Spawn(bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 5.0f);
	}
		
	public void DropFlag()
	{
		if(hasFlag)
		{
			Debug.Log ("Dropped flag");
			GameObject flag = GameObject.FindGameObjectWithTag ("Flag");
			flag.transform.parent = null;
			SendFlagLostMessage (this.playerID, this.possessionTime);
			//flag.GetComponent<Rigidbody> ().isKinematic = false;
			flag.transform.position = flagSpawn.transform.position;
			flag.GetComponent<FlagScript> ().flagState = FlagScript.FlagState.Available;
		}
	}

	void GameTimerUpdate()
	{
		if(gameTime > 0 && isGameStarted && !isGameOver)
		{
			gameTime -= Time.deltaTime;
			timeText.text = gameTime.ToString ("000");
		}
		else if (gameTime <= 0 && isGameStarted)
		{
			timeText.text = "000";
			isGameOver = true;
			endGamePanel.gameObject.SetActive (false);
			//endGamePanel.SetActive (true);
			winnerText = GameObject.FindGameObjectWithTag("winnerText").GetComponent<Text>();
			winnerText.text = winner;
			endGamePanel.text = "WINNER, WINNER,\nTURKEY DINNER!";
			//RpcEndGame ();
		}
	}
		
	public void RechargeJetPack()
	{
		if(playerFuel < 100 && Time.time > timeStamp + jetpackCoolDown)
			playerFuel += 2;
	}

	private IEnumerator FuelPowerUp()			// TODO: could use a text label to tell all players about this
	{
		this.jetpackCoolDown = 0.5f; 
		yield return new WaitForSeconds (10.0f);
		isFuelPowerUp = false;
		this.jetpackCoolDown = 1.5f;

	}

	private IEnumerator SpeedPowerUp()			// TODO: could use a text label to tell all players about this
	{
		this.linearSpeed = 20.0f;
		yield return new WaitForSeconds (10.0f);
		isSpeedPowerUp = false;
		this.linearSpeed = 10.0f;
	}

	public void RespawnPlayer()
	{
		int randomSpawnLocation = (int)Random.Range (0, spawnPoints.Length);
		this.transform.position = spawnPoints [randomSpawnLocation].transform.position;
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Respawn")
		{
			if(isLocalPlayer && !isRespawning)
			{
				isRespawning = true;
				respawnStarted = true;
				if(respawnStarted)
				{
					respawnStarted = false;
					respawnText.text = respawnTime.ToString ();
					Debug.Log ("Respawning: Player " + this.playerID.ToString());
					StartCoroutine ("RespawnPlayerCountDown");
				}
			}
		}
	}

	public IEnumerator RespawnPlayerCountDown()
	{
		yield return new WaitForSeconds (respawnTime);
		RespawnPlayer ();
	}
		

	/* ~~~~ Network ~~~~ */
	[ClientRpc]
	public void RpcEndGame()
	{
		//endGamePanel.SetActive (true);
	}

	// Ensures the movement is sent at appropriate times
	private IEnumerator StartNetworkSendCooldown()
	{
		timeBetweenMovementStart = Time.time;
		yield return new WaitForSeconds((1 / networkSendRate));
		SendNetworkMovement();
	}

	[Command]
	public void CmdGetGameTime()
	{
		SendGameTimeMessage ();
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
		
	private void RegisterNetworkMessages()
	{
		NetworkManager.singleton.client.RegisterHandler(movement_msg, OnReceiveMovementMessage);
		NetworkManager.singleton.client.RegisterHandler(flagPos_msg, OnReceiveFlagMovementMessage);
		NetworkManager.singleton.client.RegisterHandler(gameTime_msg, OnReceiveGameTimeMessage);
		NetworkManager.singleton.client.RegisterHandler(flagLost_msg, OnReceiveFlagLostMessage);
	}

	// Ensures the exact time taken to send the message lines up precisely with the player
	private void SendNetworkMovement()
	{
		timeBetweenMovementEnd = Time.time;
		SendMovementMessage(playerID, transform.position, transform.rotation, (timeBetweenMovementEnd - timeBetweenMovementStart));
		canSendNetworkMovement = false;
	}

	public void SendFlagLostMessage(string _playerID, float timePossessed)
	{
		PlayerLostFlagMessage _msg = new PlayerLostFlagMessage () 
		{
			playerID = this.playerID,
			possessionTime = timePossessed
		};

		NetworkManager.singleton.client.Send (flagLost_msg, _msg);
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

	public void SendGameTimeMessage()
	{
		if(isServer)
		{
			HostGameTimeMessage _msg = new HostGameTimeMessage()
			{
				currentGameTime = gameTime
			};
			NetworkServer.SendToAll(gameTime_msg, _msg);
		}
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

	private void OnReceiveGameTimeMessage(NetworkMessage _message)
	{
		HostGameTimeMessage _msg = _message.ReadMessage<HostGameTimeMessage>();

		GetComponent<NetworkPlayer> ().ReceiveGameTimeMessage (_msg.currentGameTime);
	}

	private void OnReceiveFlagLostMessage(NetworkMessage _message)
	{
		PlayerLostFlagMessage _msg = _message.ReadMessage<PlayerLostFlagMessage> ();

		GetComponent<NetworkPlayer> ().ReceiveFlagLostMessage (_msg.playerID, _msg.possessionTime);
	}

	[Command]
	public void CmdUpdatePlayers()
	{
		GameManager.numPlayers++;
		//Debug.Log (GameManager.numPlayers.ToString ());
	}

	public void ReceiveGameTimeMessage(float _gameTime)
	{
		this.isGameStarted = true;
		this.gameTime = _gameTime;
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

	public void ReceiveFlagLostMessage(string _pId, float _time)
	{
		// this dictionary only needs to hold values if a player gets some points
		if(playerScoreList.ContainsKey(_pId))	// if the dictionary contains playerID, we can add to its value in score
		{
			//float nonValue = 0.0f;
			float currentVal = playerScoreList [_pId];
			currentVal += _time;
			playerScoreList.Remove (_pId);
			playerScoreList.Add (_pId, currentVal);
		}
		else  									// add a new entry to the dictionary with the player and score
		{
			playerScoreList.Add (_pId, _time);
		}
			
		float lastValue = 0.0f;
		string topPlayer = "";
		foreach(KeyValuePair<string, float> entry in playerScoreList)
		{
			float firstValue = entry.Value;
			if (firstValue > lastValue) 
			{
				firstValue = lastValue;
				topPlayer = entry.Key;
				topPlayerText.text = topPlayer;
				winner = topPlayer;

				if(topPlayer == this.playerID)
				{
					topPlayerText.text = topPlayer + " (you)";
				}
			}
		}
	}
}
