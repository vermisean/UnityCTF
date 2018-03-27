using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour 
{
	public float linearSpeed = 5.0f;
	public float angularSpeed = 5.0f;

	private Rigidbody rb = null;

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
	}

	void FixedUpdate()
	{
		float rotationalInput = Input.GetAxis ("Horizontal");
		float forwardInput = Input.GetAxis ("Vertical");

		Vector3 forwardVector = this.transform.forward;
		Vector3 linearVelocity = this.transform.forward * (forwardInput * linearSpeed);
		rb.velocity = linearVelocity;

		Vector3 angularVelocity = this.transform.up * (rotationalInput * angularSpeed);
		rb.angularVelocity = angularVelocity;
	}
}