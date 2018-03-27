using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagScript : MonoBehaviour 
{


	void Start () 
	{
		
	}

	void Update ()
	{
		
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Player")
		{
			Debug.Log ("Grabbed flag!");
			this.transform.parent.transform.parent = col.gameObject.transform;
			this.transform.parent.transform.localPosition -= new Vector3 (-0.25f, -1.5f, 0.55f);
			this.transform.parent.transform.localRotation = Quaternion.Euler(new Vector3(0.25f, 0.0f, 0.0f));
			this.transform.parent.transform.localScale *= 0.5f;
		}
	}
}
