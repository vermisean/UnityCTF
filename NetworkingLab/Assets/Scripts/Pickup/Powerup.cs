using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour 
{
	public enum PowerupType
	{
		Fuel = 0,
		Speed,

	}

	public PowerupType powerupType;


	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Player" && powerupType == PowerupType.Fuel)
		{
			col.GetComponent<NetworkPlayer> ().isFuelPowerUp = true;
			//Debug.Log (col.name.ToString() + " GOT FUEL PICKUP!");
			GameManager.numPowerups--;
			Destroy (this.gameObject);
		}

		if(col.gameObject.tag == "Player" && powerupType == PowerupType.Speed)
		{
			col.GetComponent<NetworkPlayer> ().isSpeedPowerUp = true;
			//Debug.Log (col.name.ToString() + " GOT SPEED PICKUP!");
			GameManager.numPowerups--;
			Destroy (this.gameObject);
		}

	}
}
