using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PowerupSpawner : NetworkBehaviour 
{
	public Transform[] powerupSpawns;
	public int numPowerups = 2;
	public float spawnPowerupCooldown = 10.0f;
	public GameObject powerupFuelPrefab = null;
	public GameObject powerupSpeedPrefab = null;

	void Start()
	{

	}

	void Update()
	{
		//CmdSpawnPickups ();
	}

	public override void OnStartServer()
	{
		for(int i = 0; i < numPowerups + 1; i++)
		{
			int randomSpawnLocation = (int)Random.Range (0, powerupSpawns.Length);
			GameObject powerup = Instantiate (powerupFuelPrefab, powerupSpawns [randomSpawnLocation].position, powerupSpawns [randomSpawnLocation].rotation);
			NetworkServer.Spawn (powerup);
		}
	}
}
