using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.Common;


namespace SIGVerse.ToyotaHSR
{
	public class HSRSubTwist : MonoBehaviour
	{
//		private const float wheelInclinationThreshold = 0.985f; // 80[deg]
		private const float wheelInclinationThreshold = 0.965f; // 75[deg]
//		private const float wheelInclinationThreshold = 0.940f; // 70[deg]

		public string rosBridgeIP;
		public int    rosBridgePort;

		public string topicName;

		//--------------------------------------------------

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		private ROSBridgeSubscriber<SIGVerse.ROSBridge.geometry_msgs.Twist> subscriber = null;

		private Transform baseFootPrint;

		private float linearVel  = 0.0f;
		private float angularVel = 0.0f;

		private bool isMoving = false;


		void Awake()
		{
			this.baseFootPrint = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);
		}

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

			this.subscriber = this.webSocketConnection.Subscribe<SIGVerse.ROSBridge.geometry_msgs.Twist>(topicName, this.TwistCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		public void TwistCallback(SIGVerse.ROSBridge.geometry_msgs.Twist twist)
		{
			this.linearVel  = Mathf.Sign(twist.linear.x)  * Mathf.Clamp(Mathf.Abs(twist.linear.x),  0.0f, HSRCommon.MaxSpeedBase);
			this.angularVel = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, HSRCommon.MaxSpeedBaseRad);

			this.isMoving = Mathf.Abs(this.linearVel) >= 0.001f || Mathf.Abs(this.angularVel) >= 0.001f;
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

			if (Mathf.Abs(this.baseFootPrint.forward.y) < wheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			UnityEngine.Vector3 robotLocalPosition = -this.baseFootPrint.right * linearVel * Time.deltaTime;

			this.baseFootPrint.position += robotLocalPosition;
			this.baseFootPrint.Rotate(0.0f, 0.0f, -angularVel * Mathf.Rad2Deg * Time.deltaTime);
		}
	}
}

