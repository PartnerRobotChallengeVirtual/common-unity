using UnityEngine;

using System;
using System.Collections;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using System.Threading;
using System.Threading.Tasks;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

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

		private int publishSequenceNumber;

		private System.Net.Sockets.TcpClient tcpClientCameraInfo = null;
		private System.Net.Sockets.TcpClient tcpClientImage      = null;

		private System.Net.Sockets.NetworkStream networkStreamCameraInfo = null;
		private System.Net.Sockets.NetworkStream networkStreamImage      = null;

		SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>      imageMsg      = null;

		// Xtion
		private Camera xtionDepthCamera;
		private Texture2D imageTexture;
		byte[]  byteArray; 


		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private float elapsedTime;

		private bool isPublishingCameraInfo = false;
		private bool isPublishingImage      = false;


		void Awake()
		{
			this.publishSequenceNumber = HSRPubSynchronizer.GetAssignedSequenceNumber();
		}

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


			this.tcpClientCameraInfo = HSRCommon.GetSIGVerseRosbridgeConnection(this.rosBridgeIP, this.sigverseBridgePort);
			this.tcpClientImage      = HSRCommon.GetSIGVerseRosbridgeConnection(this.rosBridgeIP, this.sigverseBridgePort);

			this.networkStreamCameraInfo = this.tcpClientCameraInfo.GetStream();
			this.networkStreamCameraInfo.ReadTimeout  = 100000;
			this.networkStreamCameraInfo.WriteTimeout = 100000;

			this.networkStreamImage = this.tcpClientImage.GetStream();
			this.networkStreamImage.ReadTimeout  = 100000;
			this.networkStreamImage.WriteTimeout = 100000;


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
			if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
			if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

			if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
			if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		}

		void Update()
		{
			if(this.networkStreamCameraInfo==null || this.networkStreamImage==null) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.isPublishingCameraInfo || this.isPublishingImage || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!HSRPubSynchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.isPublishingCameraInfo = true;
			this.isPublishingImage      = true;

			this.elapsedTime = 0.0f;

			StartCoroutine(this.PubImage());
		}


		private IEnumerator PubImage()
		{
			yield return new WaitForEndOfFrame();

//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.xtionDepthCamera.targetTexture;

			// Apply depth information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0, false);
			this.imageTexture.Apply();

			// Convert pixel values to depth buffer for ROS message
			byte[] depthBytes = this.imageTexture.GetRawTextureData();

//			yield return null;

			this.header.Update();

			//  [camera/depth/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			Task.Run(() => 
			{
				this.cameraInfoMsg.SendMsg(this.networkStreamCameraInfo);
				this.isPublishingCameraInfo = false;
			});

//			yield return null;

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

//			yield return null;

			this.imageData.header = this.header;
			this.imageData.data = this.byteArray;
			this.imageMsg.msg = this.imageData;

			Task.Run(() => 
			{
				this.imageMsg.SendMsg(this.networkStreamImage);
				this.isPublishingImage = false;
			});

//			sw.Stop();
//			UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}
	}
}
