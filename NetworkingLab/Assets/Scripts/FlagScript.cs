using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagScript : MonoBehaviour 
{
	public Transform flagTransform;
	public bool isTaken = false;

	void Start () 
	{
		flagTransform = this.transform;
	}

	void Update ()
	{
		
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Player" && !isTaken)
		{
			Debug.Log ("Grabbed flag!");
			col.GetComponent<PlayerControls> ().hasFlag = true;
			//this.GetComponent<Rigidbody> ().isKinematic = true;
			isTaken = true;
			this.transform.parent.transform.parent = col.gameObject.transform;
			this.transform.parent.transform.localPosition = Vector3.zero;
			this.transform.parent.transform.localRotation = Quaternion.identity;
			this.transform.parent.transform.localScale *= 0.5f;
		}
	}
}
