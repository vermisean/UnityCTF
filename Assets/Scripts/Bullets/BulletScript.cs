using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour 
{
	public float bulletForce = 25.0f;
	private ParticleSystem pSystem;

	void Start()
	{
		pSystem = GetComponentInChildren<ParticleSystem> ();
	}

	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.tag == "Player")
		{
			pSystem.Play ();
			NetworkPlayer nPlayer = col.gameObject.GetComponent<NetworkPlayer> ();
			Rigidbody colRB = col.gameObject.GetComponent<Rigidbody> ();
			colRB.AddForce (transform.forward * bulletForce, ForceMode.VelocityChange);
			//colRB.AddForce (-colRB.transform.forward * bulletForce, ForceMode.VelocityChange);
			if(nPlayer.hasFlag)
			{
				nPlayer.DropFlag ();
			}
				
			Destroy (this.gameObject);
		}
	}
}
