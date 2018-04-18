using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkHUDScript : MonoBehaviour
{
	public GameObject panel;
	private NetworkManager netManager;

	void Start () 
	{
		netManager = FindObjectOfType<NetworkManager> ();

	}

	public void ClientStart()
	{
		netManager.StartClient ();
		panel.SetActive(false);
	}

	public void ServerStart()
	{
		netManager.StartHost ();
		panel.SetActive(false);
	}
}

