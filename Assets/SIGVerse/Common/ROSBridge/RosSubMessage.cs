using UnityEngine;
using SIGVerse.Common;
using System.Collections.Generic;

namespace SIGVerse.RosBridge
{
	abstract public class RosSubMessage<Tmsg> : MonoBehaviour, IRosConnection where Tmsg : RosMessage, new()
	{
		public string rosBridgeIP;
		public int    rosBridgePort;

		public string topicName;

		//--------------------------------------------------
		protected RosBridgeWebSocketConnection webSocketConnection = null;

		protected RosBridgeSubscriber<Tmsg> subscriber = null;


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

			this.subscriber = this.webSocketConnection.Subscribe<Tmsg>(topicName, this.SubscribeMessageCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		protected virtual void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unsubscribe(this.subscriber);

				this.webSocketConnection.Disconnect();
			}
		}

		protected virtual void Update()
		{
			if(!this.IsConnected()) { return; }

			this.webSocketConnection.Render();
		}

		abstract protected void SubscribeMessageCallback(Tmsg rosMsg);


		public virtual bool IsConnected()
		{
			return this.webSocketConnection!=null && this.webSocketConnection.IsConnected;
		}
	}
}
