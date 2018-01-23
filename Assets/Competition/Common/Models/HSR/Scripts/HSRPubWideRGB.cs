using UnityEngine;

using System;
using System.Collections;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using System.Threading;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubWideRGB : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public GameObject cameraObj;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		SIGVerseROSBridgeMessage<ImageForSIGVerseBridge> imageMsg = null;

		// Camera
		private Camera rgbCamera;
		private Texture2D imageTexture;

		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

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


			// RGB Camera
			this.rgbCamera = this.cameraObj.GetComponentInChildren<Camera>();

			int imageWidth  = this.rgbCamera.targetTexture.width;
			int imageHeight = this.rgbCamera.targetTexture.height;

			this.imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);


			//  [camera/rgb/CameraInfo]
			string distortionModel = "plumb_bob";

			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 205.47, 0.0, 320, 0.0, 205.47, 240, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 205.47, 0.0, 320, 0.0, 0.0, 205.47, 240, 0.0, 0.0, 0.0, 1.0, 0.0 };

			//double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			//double[] K = { 205.46963709898583, 0.0, 320.5, 0.0, 205.46963709898583, 240.5, 0.0, 0.0, 1.0 };
			//double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			//double[] P = { 205.46963709898583, 0.0, 320.5, 0.0, 0.0, 205.46963709898583, 240.5, 0.0, 0.0, 0.0, 1.0, 0.0 };

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);
			
			//  [camera/rgb/Image_raw]
			string encoding = "rgb8";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 3;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), this.cameraObj.name);

			this.cameraInfoMsg = new SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge>("publish", this.topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>     ("publish", this.topicNameImage,      ImageForSIGVerseBridge.GetMessageType(),      this.imageData);
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

			StartCoroutine(this.PubImage());
		}


		private IEnumerator PubImage()
		{
			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.rgbCamera.targetTexture;

			// Apply rgb information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0);

			this.imageTexture.Apply();


			// Convert pixel values to depth buffer for ROS message
			byte[] rgbBytes = this.imageTexture.GetRawTextureData();

			yield return null;

			this.header.Update();

			//  [camera/rgb/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			this.cameraInfoMsg.sendMsg(this.networkStream);

			yield return null;

			//  [camera/rgb/Image_raw]
			this.imageData.header = this.header;
			this.imageData.data = rgbBytes;
			this.imageMsg.msg = this.imageData;

			//this.isPublishing = false;
			//this.imageMsg.sendMsg(this.networkStream);

			Thread thSendSensorData = new Thread(new ParameterizedThreadStart(this.SendSensorData));

			ThreadArgsData thArgsData = new ThreadArgsData();
			thArgsData.sigverseRosBridgeMessage = this.imageMsg;
			thArgsData.networkStream = this.networkStream;

			thSendSensorData.Start(thArgsData);

			//sw.Stop();
			//UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}

		private struct ThreadArgsData
		{
			public SIGVerseROSBridgeMessage<ImageForSIGVerseBridge> sigverseRosBridgeMessage;
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
