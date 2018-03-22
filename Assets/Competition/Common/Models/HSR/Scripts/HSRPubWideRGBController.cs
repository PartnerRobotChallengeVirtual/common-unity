using UnityEngine;

using System;
using System.Collections;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubWideRGBController : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public HSRPubWideRGB publisher;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		private int publishSequenceNumber;

		private float elapsedTime;


		void Awake()
		{
			this.publishSequenceNumber = HSRPubSynchronizer.GetAssignedSequenceNumber();
		}

		void Start()
		{
			this.publisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameCameraInfo,  this.topicNameImage);
		}

		void Update()
		{
			if(!this.publisher.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.publisher.IsPublishing() || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!HSRPubSynchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			this.publisher.SendMessageInThisFrame();
		}
	}
}
