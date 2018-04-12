using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkServerRelay : NetworkMessageHandler
{
	private void Start()
	{
		if(isServer)
		{
			RegisterNetworkMessages();
		}
	}

	// Registers the movement message
	private void RegisterNetworkMessages()	
	{
		NetworkServer.RegisterHandler(movement_msg, OnReceivePlayerMovementMessage);
	}

	// Makes the movement smooth, sends to all clients
	private void OnReceivePlayerMovementMessage(NetworkMessage _message)
	{
		PlayerMovementMessage _msg = _message.ReadMessage<PlayerMovementMessage>();
		NetworkServer.SendToAll(movement_msg, _msg);
	}
}