using UnityEngine;

using System;
using System.Collections;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using SIGVerse.ROSBridge.geometry_msgs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubTf : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private class TfInfo
		{
			public UnityEngine.Transform linkTransform;
			public TransformStamped      transformStamped;

			public TfInfo(UnityEngine.Transform linkTransform, TransformStamped transformStamped)
			{
				this.linkTransform    = linkTransform;
				this.transformStamped = transformStamped;
			}

			public void UpdateTransformForLocal()
			{
				UnityEngine.Vector3    pos = linkTransform.localPosition;
				UnityEngine.Quaternion qua = linkTransform.localRotation;

				this.transformStamped.transform.translation = new UnityEngine.Vector3(-pos.x, pos.y, pos.z);
				this.transformStamped.transform.rotation    = new UnityEngine.Quaternion(qua.x, -qua.y, -qua.z, qua.w);
			}

			public void UpdateTransformForGlobal()
			{
				UnityEngine.Vector3 pos = linkTransform.localPosition;
				UnityEngine.Quaternion qua = linkTransform.localRotation;

				this.transformStamped.transform.translation = new UnityEngine.Vector3(pos.z, -pos.x, pos.y);
				this.transformStamped.transform.rotation    = new UnityEngine.Quaternion(-qua.z, qua.x, -qua.y, qua.w);
			}
		}

		private int publishSequenceNumber;

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		private SIGVerseROSBridgeMessage<TransformStamped[]> transformStampedMsg = null;

		private List<TfInfo> localTfInfoList = new List<TfInfo>();

		private float elapsedTime;

		private bool isPublishing = false;


		void Awake()
		{
			List<UnityEngine.Transform> localLinkList = HSRCommon.GetLinksInChildren(this.transform.root);

			foreach(UnityEngine.Transform localLink in localLinkList)
			{
				TransformStamped localTransformStamped = new TransformStamped();

				localTransformStamped.header.frame_id = localLink.parent.name;
				localTransformStamped.child_frame_id  = localLink.name;

				TfInfo localTfInfo = new TfInfo(localLink, localTransformStamped);

				this.localTfInfoList.Add(localTfInfo);
			}

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

			this.tcpClient = HSRCommon.GetSIGVerseRosbridgeConnection(this.rosBridgeIP, this.sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;

			this.transformStampedMsg = new SIGVerseROSBridgeMessage<TransformStamped[]>("publish", this.topicName, "sigverse/TfList", null);
		}

		void OnDestroy()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient     != null) { this.tcpClient.Close(); }
		}

		void Update()
		{
			if(this.tcpClient==null) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.isPublishing || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!HSRPubSynchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.isPublishing = true;
			this.elapsedTime = 0.0f;

			StartCoroutine(this.PubTF());
		}

		private IEnumerator PubTF()
		{
			yield return new WaitForEndOfFrame();

//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			TransformStamped[] transformStampedArray = new TransformStamped[localTfInfoList.Count];

			// Add local TF infos
			for (int i=0; i<localTfInfoList.Count; i++)
			{
				localTfInfoList[i].UpdateTransformForLocal();

				localTfInfoList[i].transformStamped.header.Update();

				transformStampedArray[i] = localTfInfoList[i].transformStamped;
			}

			this.transformStampedMsg.msg = transformStampedArray;

			Task.Run(() => 
			{
				this.transformStampedMsg.SendMsg(this.networkStream);
				this.isPublishing = false;
			});

//			sw.Stop();
//			Debug.Log("tf sending time="+sw.Elapsed);
		}
	}
}

