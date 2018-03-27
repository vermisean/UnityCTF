using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour 
{
	public GameObject[] spawners;
	public GameObject spawnGuy;
	public GameObject flagPrefab;

	private bool doOnce = true;
	private GameObject flag;
	private Vector3 flagPos;
	private Vector3 offset = new Vector3(0.0f, 1.05f, 0.0f);

	void Start () 
	{
		if(doOnce)
		{
			doOnce = false;
			//SpawnFlag ();
		}

	}

	void Update () 
	{
				
  		if(Input.GetKeyDown(KeyCode.F))
		{
			SpawnGuysAtSpawners ();
		}


	//	if(Input.GetKeyDown(KeyCode.G))
	//	{
	//		SpawnGuysRandomly ();
	//	}
	}

	void SpawnGuysAtSpawners()
	{
		int random = (int) Random.Range (0, spawners.Length - 1);
		GameObject playerPrefab = Instantiate (spawnGuy, spawners [random].transform.position + offset, Quaternion.identity);
	}

	void SpawnGuysRandomly()
	{
		Vector3 newLocation = EnsureDistance (flagPos);

		GameObject playerPrefab = Instantiate (spawnGuy, newLocation, Quaternion.identity);
	}

	void SpawnFlag()
	{
		float randomX = Random.Range (-10.0f, 10.0f);
		float randomZ = Random.Range (-10.0f, 10.0f);

		Vector3 flagPos = new Vector3 (randomX, -1.05f, randomZ);

		flag = Instantiate (flagPrefab, flagPos, Quaternion.identity);
	}

	Vector3 EnsureDistance(Vector3 flagPos)
	{
		float randomX = Random.Range (-10.0f, 10.0f);
		float randomZ = Random.Range (-10.0f, 10.0f);

		Vector3 newLocation = new Vector3 (randomX, 1.05f, randomZ);

		//if((Mathf.Abs(flagPos.x - newLocation.x) < 5.0f) || (Mathf.Abs(flagPos.z - newLocation.z) < 5.0f))
		if(Vector3.Distance(flagPos, newLocation) <= 10.0f)
		{
			EnsureDistance (flagPos);
		}

		return newLocation;
	}
}
