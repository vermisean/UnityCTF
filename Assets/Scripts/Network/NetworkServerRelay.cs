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

	// Registers the movement and flagPos messages
	private void RegisterNetworkMessages()	
	{
		NetworkServer.RegisterHandler(movement_msg, OnReceivePlayerMovementMessage);
		NetworkServer.RegisterHandler(flagPos_msg, OnReceivePlayerMovementMessage);
	}

	// Makes the player movement smooth, sends to all clients
	private void OnReceivePlayerMovementMessage(NetworkMessage _message)
	{
		PlayerMovementMessage _msg = _message.ReadMessage<PlayerMovementMessage>();
		NetworkServer.SendToAll(movement_msg, _msg);
	}

	// Makes the flag movement smooth, sends to all clients
	private void OnReceiveFlagMovementMessage(NetworkMessage _message)
	{
		FlagMovementMessage _msg = _message.ReadMessage<FlagMovementMessage>();
		NetworkServer.SendToAll(flagPos_msg, _msg);
	}

}