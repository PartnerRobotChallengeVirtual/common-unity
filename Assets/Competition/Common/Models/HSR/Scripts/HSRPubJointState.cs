using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubJointState : MonoBehaviour
	{
		public string rosBridgeIP;
		public int rosBridgePort;

		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private Transform armLiftLink;
		private Transform armFlexLink;
		private Transform armRollLink;
		private Transform wristFlexLink;
		private Transform wristRollLink;
		private Transform headPanLink;
		private Transform headTiltLink;

		private float armLiftLinkIniPosZ;

		private JointState jointState;

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		private ROSBridgePublisher<JointState> jointStatePublisher;

		private float elapsedTime;


		void Awake()
		{
			this.armLiftLink   = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.ArmLiftLinkName );
			this.armFlexLink   = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.ArmFlexLinkName );
			this.armRollLink   = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.ArmRollLinkName );
			this.wristFlexLink = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.WristFlexLinkName );
			this.wristRollLink = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.WristRollLinkName );
			this.headPanLink   = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.HeadPanLinkName );
			this.headTiltLink  = SIGVerseUtil.FindTransformFromChild(this.transform.root, HSRCommon.HeadTiltLinkName );

			this.armLiftLinkIniPosZ = this.armLiftLink.localPosition.z;
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

			this.jointStatePublisher = this.webSocketConnection.Advertise<JointState>(topicName);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), "hsrb_joint_states");

			this.jointState.name = new List<string>();
			this.jointState.name.Add(HSRCommon.ArmLiftJointName);   //1
			this.jointState.name.Add(HSRCommon.ArmFlexJointName);   //2
			this.jointState.name.Add(HSRCommon.ArmRollJointName);   //3
			this.jointState.name.Add(HSRCommon.WristFlexJointName); //4
			this.jointState.name.Add(HSRCommon.WristRollJointName); //5
			this.jointState.name.Add(HSRCommon.HeadPanJointName);   //6
			this.jointState.name.Add(HSRCommon.HeadTiltJointName);  //7

			this.jointState.position = null;
			this.jointState.velocity = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
			this.jointState.effort   = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
		}

		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.jointStatePublisher);

				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
			if(this.webSocketConnection==null || !this.webSocketConnection.IsConnected) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			List<double> positions = new List<double>();

			//1 ArmLiftJoint
			positions.Add(this.armLiftLink.localPosition.z - this.armLiftLinkIniPosZ);
			//2 ArmFlexJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.armFlexLink.localEulerAngles.y, HSRCommon.ArmFlexJointName) * Mathf.Deg2Rad);
			//3 ArmRollJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.armRollLink.localEulerAngles.z, HSRCommon.ArmRollJointName) * Mathf.Deg2Rad);
			//4 WristFlexJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.wristFlexLink.localEulerAngles.y, HSRCommon.WristFlexJointName) * Mathf.Deg2Rad);
			//5 WristRollJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.wristRollLink.localEulerAngles.z, HSRCommon.WristRollJointName) * Mathf.Deg2Rad);
			//6 HeadPanJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.headPanLink.localEulerAngles.z, HSRCommon.HeadPanJointName) * Mathf.Deg2Rad);
			//7 HeadTiltJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.headTiltLink.localEulerAngles.y, HSRCommon.HeadTiltJointName) * Mathf.Deg2Rad);

			this.jointState.header.Update();
			this.jointState.position = positions;

//			float position = HSRCommon.GetClampedPosition(value, name);

			this.jointStatePublisher.Publish(this.jointState);
		}
	}
}

