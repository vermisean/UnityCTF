using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour 
{
	public enum PowerupType
	{
		ForFlag = 0,
		ForNonFlag
	}


	public PowerupType powerupType;

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}

	void OnTriggerEnter(Collider col)
	{
		Debug.Log (col.name.ToString() + " GOT PICKUP!");
		if(col.gameObject.tag == "Player" && powerupType == PowerupType.ForNonFlag)
		{
			col.GetComponent<NetworkPlayer> ().isPowerUp = true;
			Destroy (this.gameObject);
		}

		if(col.gameObject.tag == "Player" && powerupType == PowerupType.ForFlag)
		{
			col.GetComponent<NetworkPlayer> ().isFlagPowerUp = true;
			Destroy (this.gameObject);
		}

	}
}
