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

	// Registers all of the different messages
	private void RegisterNetworkMessages()	
	{
		NetworkServer.RegisterHandler(movement_msg, OnReceivePlayerMovementMessage);
		NetworkServer.RegisterHandler(flagPos_msg, OnReceivePlayerMovementMessage);
		//NetworkServer.RegisterHandler(playerReady_msg, OnReceivePlayerMovementMessage);
		NetworkServer.RegisterHandler(gameTime_msg, OnReceivePlayerMovementMessage);
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

/*	// Tells the host this player is ready
	private void OnReceiveReadyMessage(NetworkMessage _message)
	{
		FlagMovementMessage _msg = _message.ReadMessage<PlayerReadyMessage>();
		//NetworkServer.
	}*/

	// Gives the players the current time
	private void OnReceiveGameTimeMessage(NetworkMessage _message)
	{
		HostGameTimeMessage _msg = _message.ReadMessage<HostGameTimeMessage>();
		NetworkServer.SendToAll(gameTime_msg, _msg);
	}
}