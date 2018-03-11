using UnityEngine;

using System;
using System.Collections;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using System.Threading.Tasks;

namespace SIGVerse.ToyotaHSR
{
	public class HSRStereoCameraData
	{
		private System.Net.Sockets.TcpClient tcpClientCameraInfo = null;
		private System.Net.Sockets.TcpClient tcpClientImage      = null;

		private System.Net.Sockets.NetworkStream networkStreamCameraInfo = null;
		private System.Net.Sockets.NetworkStream networkStreamImage      = null;

		private SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		private SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>      imageMsg      = null;

		// Camera
		private Camera rgbCamera;
		private Texture2D imageTexture;

		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private byte[] rgbBytes;

		private bool isPublishingCameraInfo;
		private bool isPublishingImage;


		public HSRStereoCameraData(string rosBridgeIP, int sigverseBridgePort, GameObject cameraObj, string topicNameCameraInfo, string topicNameImage, bool isRight)
		{
			this.tcpClientCameraInfo = HSRCommon.GetSIGVerseRosbridgeConnection(rosBridgeIP, sigverseBridgePort);
			this.tcpClientImage      = HSRCommon.GetSIGVerseRosbridgeConnection(rosBridgeIP, sigverseBridgePort);

			this.networkStreamCameraInfo = this.tcpClientCameraInfo.GetStream();
			this.networkStreamCameraInfo.ReadTimeout  = 100000;
			this.networkStreamCameraInfo.WriteTimeout = 100000;

			this.networkStreamImage = this.tcpClientImage.GetStream();
			this.networkStreamImage.ReadTimeout  = 100000;
			this.networkStreamImage.WriteTimeout = 100000;


			// RGB Camera
			this.rgbCamera = cameraObj.GetComponentInChildren<Camera>();

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

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), cameraObj.name);

			this.cameraInfoMsg = new SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge>("publish", topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>     ("publish", topicNameImage,      ImageForSIGVerseBridge.GetMessageType(),      this.imageData);

			this.isPublishingCameraInfo = false;
			this.isPublishingImage      = false;
		}


		public bool IsConnected()
		{
			return this.networkStreamCameraInfo != null && this.networkStreamImage !=null;
		}

		public void ReadPixels()
		{
			this.isPublishingCameraInfo = true;
			this.isPublishingImage      = true;

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.rgbCamera.targetTexture;

			// Apply rgb information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0, false);
			this.imageTexture.Apply();


			// Convert pixel values for ROS message
			this.rgbBytes = this.imageTexture.GetRawTextureData();
		}

		public void UpdateHeader()
		{
			this.header.Update();
		}

		public Header GetHeader()
		{
			return this.header;
		}

		public void SetHeaderTime(Header otherHeader)
		{
			this.header.seq         = otherHeader.seq;
			this.header.stamp.secs  = otherHeader.stamp.secs;
			this.header.stamp.nsecs = otherHeader.stamp.nsecs;
		}

		public void SendMsg()
		{
			//  [camera/rgb/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg     = this.cameraInfoData;

			Task.Run(() => 
			{
				this.cameraInfoMsg.SendMsg(this.networkStreamCameraInfo);
				this.isPublishingCameraInfo = false;
			});

//			yield return null;

			//  [camera/rgb/Image_raw]
			this.imageData.header = this.header;
			this.imageData.data   = this.rgbBytes;

			this.imageMsg.msg = this.imageData;

			Task.Run(() => 
			{
				this.imageMsg.SendMsg(this.networkStreamImage);
				this.isPublishingImage = false;
			});
		}

		public void OnDestroy()
		{
			if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
			if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

			if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
			if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		}

		public bool IsPublishing()
		{
			return this.isPublishingCameraInfo || this.isPublishingImage;
		}
	}



	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubStereoRGB : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		[HeaderAttribute("Left Camera")]
		public GameObject leftCameraObj;
		public string topicNameLeftCameraInfo;
		public string topicNameLeftImage;

		[HeaderAttribute("Right Camera")]
		public GameObject rightCameraObj;
		public string topicNameRightCameraInfo;
		public string topicNameRightImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 250;

		//--------------------------------------------------

		private int publishSequenceNumber;

		private HSRStereoCameraData leftCamera;
		private HSRStereoCameraData rightCamera;


		private float elapsedTime;


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


			this.leftCamera  = new HSRStereoCameraData(this.rosBridgeIP, this.sigverseBridgePort, this.leftCameraObj,  this.topicNameLeftCameraInfo,  this.topicNameLeftImage,  false);
			this.rightCamera = new HSRStereoCameraData(this.rosBridgeIP, this.sigverseBridgePort, this.rightCameraObj, this.topicNameRightCameraInfo, this.topicNameRightImage, true);
		}

		void OnDestroy()
		{
			this.leftCamera.OnDestroy();
			this.rightCamera.OnDestroy();
		}

		void Update()
		{
			if(!this.leftCamera .IsConnected() || !this.rightCamera.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.leftCamera.IsPublishing() || this.rightCamera.IsPublishing() || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!HSRPubSynchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			StartCoroutine(this.PubImage());
		}


		private IEnumerator PubImage()
		{
			yield return new WaitForEndOfFrame();

			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();

			this.leftCamera .ReadPixels();
			this.rightCamera.ReadPixels();

			this.leftCamera.UpdateHeader();
			this.rightCamera.SetHeaderTime(this.leftCamera.GetHeader());

//			yield return null;

			this.leftCamera .SendMsg();
			this.rightCamera.SendMsg();

			//sw.Stop();
			//UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}
	}
}
