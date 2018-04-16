using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour 
{
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

	void Update()
	{
		CheckForPowerups ();
	}

	void CheckForPowerups()
	{
		if(numPowerups < totalPowerups && !isSpawningPowerup)
		{
			isSpawningPowerup = true;
			StartCoroutine ("SpawnCooldown");
		}
	}

	void SpawnPowerup()
	{
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

		numPowerups++;
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
- 

*/
