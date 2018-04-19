using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkMessageHandler : NetworkBehaviour
{
	public const short movement_msg = 100;
	public const short flagPos_msg = 101;
	public const short playerReady_msg = 102;
	public const short gameTime_msg = 103;

	public class PlayerMovementMessage : MessageBase
	{
		public string objectTransformName;
		public Vector3 objectPosition;
		public Quaternion objectRotation;
		public float time;
	}

	public class FlagMovementMessage : MessageBase
	{
		public string flagOwnerName;
		public Vector3 flagPosition;
		public Quaternion flagRotation;
		public float time;
	}

	public class PlayerReadyMessage : MessageBase
	{
		public int playerId;
	}

	public class HostGameTimeMessage : MessageBase
	{
		public string playerName;
		public float currentGameTime;
	}
}