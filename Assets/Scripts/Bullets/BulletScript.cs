using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour 
{
	public float bulletForce = 5.0f;

	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.tag == "Player")
		{
			NetworkPlayer nPlayer = col.gameObject.GetComponent<NetworkPlayer> ();
			Rigidbody colRB = col.gameObject.GetComponent<Rigidbody> ();
			colRB.AddForce (colRB.transform.up * bulletForce, ForceMode.VelocityChange);
			//colRB.AddForce (-colRB.transform.forward * bulletForce, ForceMode.VelocityChange);
			if(nPlayer.hasFlag)
			{
				nPlayer.DropFlag ();
			}
				
			Destroy (this.gameObject);
		}
	}
}
