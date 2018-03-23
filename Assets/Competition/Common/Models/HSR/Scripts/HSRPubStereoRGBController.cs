using UnityEngine;

using System;
using System.Collections;
using SIGVerse.Common;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubStereoRGBController : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		[HeaderAttribute("Left Camera")]
		public HSRPubStereoRGB leftPublisher;
		public string topicNameLeftCameraInfo;
		public string topicNameLeftImage;

		[HeaderAttribute("Right Camera")]
		public HSRPubStereoRGB rightPublisher;
		public string topicNameRightCameraInfo;
		public string topicNameRightImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 250;

		//--------------------------------------------------

		private HSRPubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private float elapsedTime;


		void Awake()
		{
			this.synchronizer = this.GetComponent<HSRPubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();
		}

		void Start()
		{
			this.leftPublisher .Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameLeftCameraInfo,  this.topicNameLeftImage,  false);
			this.rightPublisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameRightCameraInfo, this.topicNameRightImage, true);
		}

		void Update()
		{
			if(!this.leftPublisher.IsConnected() || !this.rightPublisher.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.leftPublisher.IsPublishing() || this.rightPublisher.IsPublishing() || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!this.synchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			this.leftPublisher .SendMessageInThisFrame();
			this.rightPublisher.SendMessageInThisFrame();
		}
	}
}
