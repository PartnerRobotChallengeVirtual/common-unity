using UnityEngine;

using System;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using System.Collections;
using System.Threading;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubLaserRangeSensor : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		public Transform sensorLink;

		public LayerMask layerMask = -1;

		[HeaderAttribute("DEBUG")]
		public bool showDebugRay = true;
		public Color debugRayColor = Color.red;

		//--------------------------------------------------

		private const float HalfOfLaserAngle = 120.0f;
		private const float LaserDistance = 20;
		private const float LaserAngle = HalfOfLaserAngle * 2.0f;
		private const float LaserAngularResolution = 0.25f;

		private int numLines;

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseROSBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg;

		private LaserScanForSIGVerseBridge laserScan;

		// Message header
		private Header header;

		private float elapsedTime;

		private bool isPublishing = false;


		void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP        = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.sigverseBridgePort == 0)
			{
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}

			this.tcpClient = HSRCommon.GetSIGVerseRosbridgeConnection(this.rosBridgeIP, this.sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), this.sensorLink.name);

			this.laserScan = new LaserScanForSIGVerseBridge();

			this.numLines = (int)Mathf.Round(LaserAngle / LaserAngularResolution) + 1;

			this.laserScan.header = this.header;

			this.laserScan.angle_min = -HalfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_max = +HalfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_increment = LaserAngularResolution * Mathf.Deg2Rad;
			this.laserScan.time_increment = 0.0;
			this.laserScan.scan_time = 0.1; // tentative
			this.laserScan.range_min = 0.05;
			this.laserScan.range_max = 20.0;
			this.laserScan.ranges      = new double[this.numLines];
			this.laserScan.intensities = new double[this.numLines];

			this.laserScanMsg = new SIGVerseROSBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);

//			Debug.Log("this.layerMask.value = "+this.layerMask.value);
		}

		void OnDestroy()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient != null) { this.tcpClient.Close(); }
		}

		void Update()
		{
			if(this.tcpClient==null) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.isPublishing || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			this.isPublishing = true;
			this.elapsedTime = 0.0f;

			StartCoroutine(this.PubSensorData());
		}

		private IEnumerator PubSensorData()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			// Set current time to the header
			this.laserScan.header.Update();

			for (int index = 0; index < this.numLines; index++)
			{
				Vector3 ray = this.sensorLink.rotation * Quaternion.AngleAxis(LaserAngle * 0.5f + (-1 * index * LaserAngularResolution), Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorLink.position, ray, out hit, LaserDistance, this.layerMask))
				{
					distance = hit.distance;
				}

				this.laserScan.ranges     [index] = distance;
				this.laserScan.intensities[index] = 0.0;

				if (this.showDebugRay)
				{
					Debug.DrawRay(this.sensorLink.position, ray * distance, this.debugRayColor);
				}
			}

			yield return null;

			this.laserScanMsg.msg = this.laserScan;
			//this.laserScanMsg.sendMsg(this.networkStream);
			//this.isPublishing = false;

			Thread thSendSensorData = new Thread(new ParameterizedThreadStart(this.SendSensorData));

			ThreadArgsData thArgsData = new ThreadArgsData();
			thArgsData.sigverseRosBridgeMessage = this.laserScanMsg;
			thArgsData.networkStream = this.networkStream;

			thSendSensorData.Start(thArgsData);

//			sw.Stop();
//			Debug.Log("LRF sending time=" + sw.Elapsed);
		}

		private struct ThreadArgsData
		{
			public SIGVerseROSBridgeMessage<LaserScanForSIGVerseBridge> sigverseRosBridgeMessage;
			public System.Net.Sockets.NetworkStream networkStream;
		}

		private void SendSensorData(object obj)
		{
			ThreadArgsData argsData = (ThreadArgsData)obj;

			argsData.sigverseRosBridgeMessage.sendMsg(argsData.networkStream);

			this.isPublishing = false;
		}
	}
}
