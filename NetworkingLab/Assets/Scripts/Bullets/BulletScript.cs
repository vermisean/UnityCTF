using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour 
{

	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.tag == "Player")
		{
			Rigidbody colRB = col.gameObject.GetComponent<Rigidbody> ();
			colRB.AddForce (new Vector3 (0.0f, 15.0f, 0.0f), ForceMode.VelocityChange);
			Destroy (this.gameObject);
		}
	}
}
