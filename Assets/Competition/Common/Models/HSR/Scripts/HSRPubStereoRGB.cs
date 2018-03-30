using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Threading;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubStereoRGB : MonoBehaviour
	{
		//--------------------------------------------------

		private System.Net.Sockets.TcpClient tcpClientCameraInfo = null;
		private System.Net.Sockets.TcpClient tcpClientImage      = null;

		private System.Net.Sockets.NetworkStream networkStreamCameraInfo = null;
		private System.Net.Sockets.NetworkStream networkStreamImage      = null;

		private SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		private SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>      imageMsg      = null;

		private GameObject cameraFrameObj;

		private Camera rgbCamera;
		private Texture2D imageTexture;

		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private byte[] rgbBytes;

		private bool isPublishingCameraInfo = false;
		private bool isPublishingImage      = false;

		private bool shouldSendMessage = false;


		void Awake()
		{
			this.cameraFrameObj = this.transform.parent.gameObject;
		}

		public void Initialize(string rosBridgeIP, int sigverseBridgePort, string topicNameCameraInfo, string topicNameImage, bool isRight)
		{
			this.tcpClientCameraInfo = SIGVerseRosBridgeConnection.GetConnection(rosBridgeIP, sigverseBridgePort);
			this.tcpClientImage      = SIGVerseRosBridgeConnection.GetConnection(rosBridgeIP, sigverseBridgePort);

			this.networkStreamCameraInfo = this.tcpClientCameraInfo.GetStream();
			this.networkStreamCameraInfo.ReadTimeout  = 100000;
			this.networkStreamCameraInfo.WriteTimeout = 100000;

			this.networkStreamImage = this.tcpClientImage.GetStream();
			this.networkStreamImage.ReadTimeout  = 100000;
			this.networkStreamImage.WriteTimeout = 100000;


			// RGB Camera
			this.rgbCamera = this.cameraFrameObj.GetComponentInChildren<Camera>();

			int imageWidth  = this.rgbCamera.targetTexture.width;
			int imageHeight = this.rgbCamera.targetTexture.height;

			this.imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);


			//  [camera/rgb/CameraInfo]
			string distortionModel = "plumb_bob";

			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 968.765, 0.0, 640, 0.0, 968.77, 480, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 968.765, 0.0, 640, 0.0, 0.0, 968.77, 480, 0.0, 0.0, 0.0, 1.0, 0.0 }; 

			if(isRight)
			{
				P[3] = -135.627;  // -135.627 = - 968.765 * 0.14(baseline=distance between both eyes)
			}

			//double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			//double[] K = { 968.7653251755174, 0.0, 640.5, 0.0, 968.7653251755174, 480.5, 0.0, 0.0, 1.0 };
			//double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			// Left Camera
			//double[] P = { 968.7653251755174, 0.0, 640.5, -0.0, 0.0, 968.7653251755174, 480.5, 0.0, 0.0, 0.0, 1.0, 0.0 };
			// Right Camera
			//double[] P = { 968.7653251755174, 0.0, 640.5, -135.62714552457246, 0.0, 968.7653251755174, 480.5, 0.0, 0.0, 0.0, 1.0, 0.0 };

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);
			
			//  [camera/rgb/Image_raw]
			string encoding = "rgb8";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 3;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), this.cameraFrameObj.name);

			this.cameraInfoMsg = new SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge>("publish", topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>     ("publish", topicNameImage,      ImageForSIGVerseBridge.GetMessageType(),      this.imageData);
		}

		void OnDestroy()
		{
			if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
			if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

			if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
			if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		}


		public void SendMessageInThisFrame()
		{
			this.shouldSendMessage = true;
		}

		public bool IsConnected()
		{
			return this.tcpClientCameraInfo != null && this.tcpClientImage != null && this.networkStreamCameraInfo != null && this.networkStreamImage !=null;
		}

		public bool IsPublishing()
		{
			return this.isPublishingCameraInfo || this.isPublishingImage;
		}


		//void Update()
		//{
		//}

		void OnPostRender()
		{
			if(this.shouldSendMessage)
			{
				this.PubImage();

				this.shouldSendMessage = false;
			}
		}

		private void PubImage()
		{
			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();

			this.isPublishingCameraInfo = true;
			this.isPublishingImage      = true;

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.rgbCamera.targetTexture;

			// Apply rgb information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0, false);
			this.imageTexture.Apply();


			// Convert pixel values for ROS message
			this.rgbBytes = this.imageTexture.GetRawTextureData();


			this.header.Update();

//			yield return null;


			//  [camera/rgb/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg     = this.cameraInfoData;

			Thread threadCameraInfo = new Thread(new ThreadStart(SendCameraInfo));
			threadCameraInfo.Start();

//			yield return null;

			//  [camera/rgb/Image_raw]
			this.imageData.header = this.header;
			this.imageData.data   = this.rgbBytes;

			this.imageMsg.msg = this.imageData;

			Thread threadImage = new Thread(new ThreadStart(SendImage));
			threadImage.Start();

			//sw.Stop();
			//UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}

		private void SendCameraInfo()
		{
			this.cameraInfoMsg.SendMsg(this.networkStreamCameraInfo);
			this.isPublishingCameraInfo = false;
		}

		private void SendImage()
		{
			this.imageMsg.SendMsg(this.networkStreamImage);
			this.isPublishingImage = false;
		}
	}
}
