using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.Common;

namespace SIGVerse.Competition
{
	abstract public class RosPubMessage<Tmsg> : MonoBehaviour where Tmsg : ROSMessage
	{
		public string rosBridgeIP;
		public int rosBridgePort;

		public string sendingTopicName = "/xxx/message/to_robot";

		//--------------------------------------------------
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		protected ROSBridgePublisher<Tmsg> publisher;


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

			this.publisher = this.webSocketConnection.Advertise<Tmsg>(sendingTopicName);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.publisher);

				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
			if(this.webSocketConnection==null || !this.webSocketConnection.IsConnected) { return; }

			this.webSocketConnection.Render();
		}
	}
}

