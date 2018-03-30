using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.RosBridge
{
	abstract public class RosPubMessage<Tmsg> : MonoBehaviour, IRosConnection where Tmsg : RosMessage
	{
		public string rosBridgeIP;
		public int    rosBridgePort;

		public string topicName;

		//--------------------------------------------------
		protected RosBridgeWebSocketConnection webSocketConnection = null;

		protected RosBridgePublisher<Tmsg> publisher;


		protected virtual void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP   = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.rosBridgePort == 0)
			{
				this.rosBridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}

			this.webSocketConnection = new SIGVerse.RosBridge.RosBridgeWebSocketConnection(rosBridgeIP, rosBridgePort);

			this.publisher = this.webSocketConnection.Advertise<Tmsg>(topicName);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		protected virtual void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.publisher);

				this.webSocketConnection.Disconnect();
			}
		}

		protected virtual void Update()
		{
			if(!this.IsConnected()) { return; }
		}

		public virtual bool IsConnected()
		{
			return this.webSocketConnection!=null && this.webSocketConnection.IsConnected;
		}
	}
}

