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
	public class HSRPubXtionDepth : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public GameObject depthCamera;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		SIGVerseROSBridgeMessage<ImageForSIGVerseBridge> imageMsg = null;

		// Xtion
		private Camera xtionDepthCamera;
		private Texture2D imageTexture;
		byte[]  byteArray; 


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


			// Depth Camera
			this.xtionDepthCamera = this.depthCamera.GetComponentInChildren<Camera>();

			int imageWidth  = this.xtionDepthCamera.targetTexture.width;
			int imageHeight = this.xtionDepthCamera.targetTexture.height;

			this.byteArray = new byte[imageWidth * imageHeight * 2];

			for (int i = 0; i < this.byteArray.Length; i++)
			{
				this.byteArray[i] = 0;
			}

			this.imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);


			//  [camera/depth/CameraInfo]
			string distortionModel = "plumb_bob";

			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 554, 0.0, 320, 0.0, 554, 240, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 554, 0.0, 320, 0.0, 0.0, 554, 240, 0.0, 0.0, 0.0, 1.0, 0.0 };

			//double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			//double[] K = { 554.3827128226441, 0.0, 320.5, 0.0, 554.3827128226441, 240.5, 0.0, 0.0, 1.0 };
			//double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			//double[] P = { 554.3827128226441, 0.0, 320.5, 0.0, 0.0, 554.3827128226441, 240.5, 0.0, 0.0, 0.0, 1.0, 0.0 };

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);

			//  [camera/depth/Image_raw]
			string encoding = "16UC1";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 2;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), this.depthCamera.name);


			this.cameraInfoMsg = new SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge>("publish", this.topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>     ("publish", this.topicNameImage     , ImageForSIGVerseBridge.GetMessageType(),      this.imageData);
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
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.xtionDepthCamera.targetTexture;

			// Apply depth information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0);

			this.imageTexture.Apply();

			// Convert pixel values to depth buffer for ROS message
			byte[] depthBytes = this.imageTexture.GetRawTextureData();

			yield return null;


			this.header.Update();

			//  [camera/depth/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			this.cameraInfoMsg.sendMsg(this.networkStream);

//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			yield return null;

			// [camera/depth/Image_raw]
			int textureWidth = this.imageTexture.width;
			int textureHeight = this.imageTexture.height;

			for (int row = 0; row < textureHeight; row++)
			{
				for (int col = 0; col < textureWidth; col++)
				{
					int index = row * textureWidth + col;
					this.byteArray[index * 2 + 0] = depthBytes[index * 3 + 0];
					this.byteArray[index * 2 + 1] = depthBytes[index * 3 + 1];
				}
			}

			yield return null;

//			sw.Stop();
//			UnityEngine.Debug.Log("time="+sw.Elapsed);

			this.imageData.header = this.header;
			this.imageData.data = this.byteArray;
			this.imageMsg.msg = this.imageData;

			//this.isPublishing = false;
			//this.imageMsg.sendMsg(this.networkStream);

			Thread thSendSensorData = new Thread(new ParameterizedThreadStart(this.SendSensorData));

			ThreadArgsData thArgsData = new ThreadArgsData();
			thArgsData.sigverseRosBridgeMessage = this.imageMsg;
			thArgsData.networkStream = this.networkStream;

			thSendSensorData.Start(thArgsData);

//			sw.Stop();
//			UnityEngine.Debug.Log("time=" + sw.Elapsed);
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
