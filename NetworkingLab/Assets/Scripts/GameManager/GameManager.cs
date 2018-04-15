using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour 
{
	public Transform[] powerupSpawns;
	public static int numPowerups = 0;
	public int totalPowerups = 2;
	public float spawnPowerupCooldown = 10.0f;
	public GameObject powerupFuelPrefab = null;
	public GameObject powerupSpeedPrefab = null;

	private bool isSpawningPowerup = true;

	public override void OnStartServer()
	{
		isSpawningPowerup = true;

		for(int i = 0; i < numPowerups; i++)
		{
			SpawnPowerup ();
		}

		isSpawningPowerup = false;
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
		isSpawningPowerup = false;
		SpawnPowerup ();
	}
}
