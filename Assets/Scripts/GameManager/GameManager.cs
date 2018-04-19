using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using PlayerManager;

public class GameManager : NetworkBehaviour 
{
	[Header("Game Properties")]
	//[SyncVar]
	public static int numPlayers;
	public int maxPlayers = 2;
	public GameObject endGamePanel;

	[Header("Powerup Properties")]
	public Transform[] powerupSpawns;
	public static int numPowerups = 0;
	public int totalPowerups = 1;
	public float spawnPowerupCooldown = 10.0f;
	public GameObject powerupFuelPrefab = null;
	public GameObject powerupSpeedPrefab = null;

	private bool isSpawningPowerup = true;

	[Header("Flag Properties")]
	public Transform flagSpawn;
	public bool isSpawningFlag;

/*	[Header("Game Time")]
	public Text timeText;
	[SyncVar]
	public float gameTime = 360.0f;
	//[SyncVar]
	public static bool isGameStarted = false;			//TODO stop players from moving til this is thrown
	[SyncVar]
	public bool isGameOver = false;*/

	public override void OnStartServer()
	{
		isSpawningPowerup = true;

		for(int i = 0; i < totalPowerups; i++)
		{
			SpawnPowerup ();
		}

		isSpawningPowerup = false;
		isSpawningFlag = false;
		SpawnFlag ();
	}

	//public override void On

	void Start()
	{
		//timeText.text = gameTime.ToString ("000");
		//isGameStarted = true;
	}

	void Update()
	{
		if (numPowerups < totalPowerups && !isSpawningPowerup)
		{
			CheckForPowerups ();
		}

		//GameTimerUpdate ();

		//if(!isGameStarted && !isGameOver)
		//{
		//	RpcStartGame ();
		//}
	}

	[ClientRpc]
	public void RpcStartGame()
	{
		Debug.Log ("Game starting!");
		//isGameStarted = true;
	}

/*	void GameTimerUpdate()
	{
		if(gameTime > 0 && isGameStarted && !isGameOver)			//TODO sync this with everyone else
		{
			gameTime -= Time.deltaTime;
			timeText.text = gameTime.ToString ("000");
		}
		else if (gameTime <= 0)
		{
			timeText.text = "000";
			isGameOver = true;
			RpcEndGame ();
		}
	}

	[ClientRpc]
	public void RpcEndGame()
	{
		endGamePanel.SetActive (true);
	}*/

/*	public override void OnServerConnect(NetworkConnection conn)
	{
		if (conn.connectionId != 0) 
		{
			//Gameti
			//conn.Send()
		}
	}*/

	public void SendGameTimeToPlayers()
	{
		if(isServer)
		{
			
		}
	}

	void CheckForPowerups()
	{
		isSpawningPowerup = true;
		StartCoroutine ("SpawnCooldown");
	}

	void SpawnPowerup()
	{
		numPowerups++;
		int randomSpawnLocation = (int)Random.Range (0, powerupSpawns.Length);
		int randomPowerup = (int)Random.Range (0, 2);
		GameObject powerupPrefab;

		switch(randomPowerup)
		{
		case(0):
			powerupPrefab = powerupFuelPrefab;
			break;
		case(1):
			powerupPrefab = powerupSpeedPrefab;
			break;
		default:
			powerupPrefab = powerupSpeedPrefab;
			break;
		}


		GameObject powerup = Instantiate (powerupPrefab, powerupSpawns [randomSpawnLocation].position, powerupSpawns [randomSpawnLocation].rotation);
		NetworkServer.Spawn (powerup);
	}

	IEnumerator SpawnCooldown()
	{
		//Debug.Log ("Spawn cooldown started");
		yield return new WaitForSeconds (spawnPowerupCooldown);
		SpawnPowerup ();
		isSpawningPowerup = false;
	}


	void SpawnFlag()
	{
		isSpawningFlag = true;
	}
}

// Notes
/* 
TODO:
- Command RPC for host to ensure two characters dont choose the same colour
- Syncvar to ensure the flag is captured/available
- Syncvar for player place (1st, 2nd, etc)
- Update the flag transform separately - spawn flag in gamemanager (needs to spawn before "start" method)
- Respawn players after falling
- Respawn flag after falling
- Knock flag out of player hands with shot
- host/server is the only one tracking game stats so send msgs to update all ui
- Call an rpc on onclientconnect - use firas' github for this

- TIMER
- RPC when all clients are connected to start the countdown. This is called after 2-3 commands are sent from the players, then:
- You could have the server give the signal to start the timer, 
	along with a DateTime. Instead of starting the timer at the same time as the message,
	give the clients a DateTime occurring a few seconds after the message is sent.
	That way the clients won't start the timer as soon as they get the message,
	allowing for those with lag to 'catch up.' (e.g. The signal is sent at 7:00:00 AM, 
	containing the time 7:00:05. As the clients recieve the message, 
	they wait to start the timer until 7:00:05, so all the timers can start at the same time.)
*/
