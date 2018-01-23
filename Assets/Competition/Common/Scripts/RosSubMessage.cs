using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.Common;
using System.Collections.Generic;

namespace SIGVerse.Competition
{
	abstract public class RosSubMessage<Tmsg> : MonoBehaviour where Tmsg : ROSMessage, new()
	{
		public List<GameObject> destinations;

		public string rosBridgeIP;
		public int rosBridgePort;

		public string receivingTopicName = "/xxx/message/to_moderator";

		//--------------------------------------------------
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		protected ROSBridgeSubscriber<Tmsg> subscriber = null;

		void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP   = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.rosBridgePort == 0)
			{
				this.rosBridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}

			this.webSocketConnection = new SIGVerse.ROSBridge.ROSBridgeWebSocketConnection(rosBridgeIP, rosBridgePort);

			this.subscriber = this.webSocketConnection.Subscribe<Tmsg>(receivingTopicName, this.SubscribeMessageCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unsubscribe(this.subscriber);

				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
			if(this.webSocketConnection==null || !this.webSocketConnection.IsConnected) { return; }

			this.webSocketConnection.Render();
		}

		abstract public void SubscribeMessageCallback(Tmsg rosMsg);
	}
}
