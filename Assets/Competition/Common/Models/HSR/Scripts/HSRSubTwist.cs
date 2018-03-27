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
		private Rigidbody baseRigidbody;

		private float linearVelX  = 0.0f;
		private float linearVelY  = 0.0f;
		private float angularVelZ = 0.0f;

		private bool isMoving = false;


		void Awake()
		{
			this.baseFootPrint = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);

			this.baseRigidbody = this.baseFootPrint.GetComponent<Rigidbody>();
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
			float linearVel = Mathf.Sqrt(Mathf.Pow(twist.linear.x, 2) + Mathf.Pow(twist.linear.y, 2));

			float linearVelClamped = Mathf.Clamp(linearVel, 0.0f, HSRCommon.MaxSpeedBase);

			if(linearVel >= 0.001)
			{
				this.linearVelX  = twist.linear.x * linearVelClamped / linearVel;
				this.linearVelY  = twist.linear.y * linearVelClamped / linearVel;
			}
			else
			{
				this.linearVelX = 0.0f;
				this.linearVelY = 0.0f;
			}

			this.angularVelZ = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, HSRCommon.MaxSpeedBaseRad);

//			Debug.Log("linearVel=" + linearVel + ", angularVel=" + angularVel);
			this.isMoving = Mathf.Abs(this.linearVelX) >= 0.001f || Mathf.Abs(this.linearVelY) >= 0.001f || Mathf.Abs(this.angularVelZ) >= 0.001f;
		}


		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unsubscribe(this.subscriber);

				this.webSocketConnection.Disconnect();
			}
		}

		void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootPrint.forward.y) < wheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			UnityEngine.Vector3 deltaPosition = (-this.baseFootPrint.right * linearVelX + this.baseFootPrint.up * linearVelY) * Time.fixedDeltaTime;
			this.baseRigidbody.MovePosition(this.baseFootPrint.position + deltaPosition);

			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -angularVelZ * Mathf.Rad2Deg * Time.fixedDeltaTime));
			this.baseRigidbody.MoveRotation(this.baseRigidbody.rotation * deltaRotation);
		}

		private void Update()
		{
			if(this.webSocketConnection==null || !this.webSocketConnection.IsConnected) { return; }

			this.webSocketConnection.Render();

//			this.baseFootPrint.position += deltaPosition;
//			this.baseFootPrint.Rotate(0.0f, 0.0f, -angularVel * Mathf.Rad2Deg * Time.deltaTime);
		}
	}
}

