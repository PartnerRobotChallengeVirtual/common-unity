using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ToyotaHSR
{
	public class HSRCommon
	{
		public const float MaxSpeedBase  = 0.22f; // [m/s]
		public const float MaxSpeedBaseRad = MaxSpeedBase / 0.133f; // [rad/s] Roughly value. 
		public const float MaxSpeedTorso = 0.15f; // [m/s]
//		public const float MaxSpeedArm   = 1.1f / 0.6f; // [rad/s] Roughly value. Max speed of Hand is 1.1[m/s]. And a radius of the orbit is 0.6[m]=Hand length.
		public const float MaxSpeedArm   = 1.2f;        // [rad/s] Roughly value. 
		public const float MaxSpeedHead  = MaxSpeedArm; // [rad/s] Roughly value. 
		public const float MinSpeed    = 0.001f; // [m/s]
		public const float MinSpeedRad = 0.01f; // [rad/s]

		// Link names
		// TODO Want to change into Enum.
//		public const string MapName           = "map";
		public const string OdomName          = "odom";
		public const string BaseFootPrintName = "base_footprint";
		public const string BaseLinkName      = "base_link";

		public const string ArmLiftLinkName             = "arm_lift_link";
		public const string ArmFlexLinkName             = "arm_flex_link";
		public const string ArmRollLinkName             = "arm_roll_link";
		public const string WristFlexLinkName           = "wrist_flex_link";
		public const string WristRollLinkName           = "wrist_roll_link";
		public const string HandPalmLinkName            = "hand_palm_link";
		public const string HandCameraFrameName         = "hand_camera_frame";
//		public const string HandCameraGazeboFrameName   = "hand_camera_gazebo_frame";
		public const string HandCameraRgbFrameName      = "hand_camera_rgb_frame";
		public const string HandLProximalLinkName       = "hand_l_proximal_link";
		public const string HandLSpringProximalLinkName = "hand_l_spring_proximal_link";
		public const string HandLMimicDistalLinkName    = "hand_l_mimic_distal_link";
		public const string HandLDistalLinkName         = "hand_l_distal_link";
		public const string HandLFingerTipFrameName     = "hand_l_finger_tip_frame";
		public const string HandLFingerVacuumFrameName  = "hand_l_finger_vacuum_frame";
		public const string HandMotorDummyLinkName      = "hand_motor_dummy_link";
		public const string HandRProximalLinkName       = "hand_r_proximal_link";
		public const string HandRSpringProximalLinkName = "hand_r_spring_proximal_link";
		public const string HandRMimicDistalLinkName    = "hand_r_mimic_distal_link";
		public const string HandRDistalLinkName         = "hand_r_distal_link";
		public const string HandRFingerTipFrameName     = "hand_r_finger_tip_frame";
		public const string WristFtSensorFrameName      = "wrist_ft_sensor_frame";

		public const string BaseImuFrameName    = "base_imu_frame";
		public const string BaseRangeSensorLink = "base_range_sensor_link";

		public const string BaseRollLlinkName           = "base_roll_link";
		public const string BaseLDriveWheelLinkName     = "base_l_drive_wheel_link";
		public const string BaseLPassiveWheelXFrameName = "base_l_passive_wheel_x_frame";
		public const string BaseLPassiveWheelYFrameName = "base_l_passive_wheel_y_frame";
		public const string BaseLPassiveWheelZLinkName  = "base_l_passive_wheel_z_link";
		public const string BaseRDriveWheelLinkName     = "base_r_drive_wheel_link";
		public const string BaseRPassiveWheelXFramName  = "base_r_passive_wheel_x_frame";
		public const string BaseRPassiveWheelYFrameName = "base_r_passive_wheel_y_frame";
		public const string BaseRPassiveWheelZLinkName  = "base_r_passive_wheel_z_link";

		public const string TorsoLiftLinkName                = "torso_lift_link";
		public const string HeadPanLinkName                  = "head_pan_link";
		public const string HeadTiltLinkName                 = "head_tilt_link";
		public const string HeadCenterCameraFrameName        = "head_center_camera_frame";
//		public const string HeadCenterCameraGazeboFrameName  = "head_center_camera_gazebo_frame";
		public const string HeadCenterCameraRgbFrameName     = "head_center_camera_rgb_frame";
		public const string HeadLStereoCameraLinkName        = "head_l_stereo_camera_link";
//		public const string HeadLStereoCameraGazeboFrameName = "head_l_stereo_camera_gazebo_frame";
		public const string HeadLStereoCameraRgbFrameName    = "head_l_stereo_camera_rgb_frame";
		public const string HeadRStereoCameraLinkName        = "head_r_stereo_camera_link";
//		public const string HeadRStereoCameraGazeboFrameName = "head_r_stereo_camera_gazebo_frame";
		public const string HeadRStereoCameraRgbFrameName    = "head_r_stereo_camera_rgb_frame";
		public const string HeadRgbdSensorLinkName           = "head_rgbd_sensor_link";
//		public const string HeadRgbdSensorGazeboFrameName    = "head_rgbd_sensor_gazebo_frame";
		public const string HeadRgbdSensorRgbFrameName       = "head_rgbd_sensor_rgb_frame";
		public const string HeadRgbdSensorDepthFrameName     = "head_rgbd_sensor_depth_frame";

		// Joint names
		public const string ArmLiftJointName       = "arm_lift_joint";
		public const string ArmFlexJointName       = "arm_flex_joint";
		public const string ArmRollJointName       = "arm_roll_joint";
		public const string WristFlexJointName     = "wrist_flex_joint";
		public const string WristRollJointName     = "wrist_roll_joint";
		public const string HeadPanJointName       = "head_pan_joint";
		public const string HeadTiltJointName      = "head_tilt_joint";
		public const string TorsoLiftJointName     = "torso_lift_joint";
		public const string HandLProximalJointName = "hand_l_proximal_joint";
		public const string HandRProximalJointName = "hand_r_proximal_joint";

		private const int sigverseRosbridgeConnectionTimeOut = 5000;
		private static bool canConnect = true;

		private static bool CanConnect 
		{
			get 
			{
				return canConnect;
			}
			set
			{
				canConnect = value;
			}
		}

		public static System.Net.Sockets.TcpClient GetSIGVerseRosbridgeConnection(string rosBridgeIP, int sigverseBridgePort)
		{
			if(!HSRCommon.CanConnect)
			{
				throw new Exception("Cannot connect HSR. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
			}

			System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();

			IAsyncResult connectResult = tcpClient.BeginConnect(rosBridgeIP, sigverseBridgePort, null, null);

			bool isConnected = connectResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(sigverseRosbridgeConnectionTimeOut));

			if (!isConnected)
			{
				HSRCommon.CanConnect = false;

				SIGVerseLogger.Error("Failed to connect. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
				throw new Exception("Failed to connect. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
			}

			return tcpClient;
		}


		public static Transform FindGameObjectFromChild(Transform root, string name)
		{
			Transform[] transforms = root.GetComponentsInChildren<Transform>();

			foreach (Transform transform in transforms)
			{
				if (transform.name == name)
				{
					return transform;
				}
			}

			return null;
		}

		public static List<Transform> GetLinksInChildren(Transform root)
		{
			List<Transform> linkList = new List<Transform>();

			AddLink(linkList, FindGameObjectFromChild(root, BaseFootPrintName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, ArmLiftLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, ArmFlexLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, ArmRollLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, WristFlexLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, WristRollLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandPalmLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandCameraFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HandCameraRgbFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLProximalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLSpringProximalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLMimicDistalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLDistalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLFingerTipFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HandLFingerVacuumFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HandMotorDummyLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandRProximalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandRSpringProximalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandRMimicDistalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandRDistalLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HandRFingerTipFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, WristFtSensorFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseImuFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRangeSensorLink));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRollLlinkName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseLDriveWheelLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseLPassiveWheelXFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseLPassiveWheelYFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseLPassiveWheelZLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRDriveWheelLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRPassiveWheelXFramName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRPassiveWheelYFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, BaseRPassiveWheelZLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, TorsoLiftLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadPanLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadTiltLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadCenterCameraFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadCenterCameraRgbFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadLStereoCameraLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadLStereoCameraRgbFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadRStereoCameraLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadRStereoCameraRgbFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadRgbdSensorLinkName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadRgbdSensorRgbFrameName));
			AddLink(linkList, FindGameObjectFromChild(root, HeadRgbdSensorDepthFrameName));

			return linkList;
		}

		private static void AddLink(List<Transform> linkList, Transform link)
		{
			if(link!=null)
			{
				linkList.Add(link);
			}
		}

		public static float GetClampedPosition(float value, string name)
		{
			if (name == HSRCommon.ArmLiftJointName)       { return Mathf.Clamp(value, +0.000f, +0.690f); }
			if (name == HSRCommon.ArmFlexJointName)       { return Mathf.Clamp(value, -2.617f, +0.000f); }
			if (name == HSRCommon.ArmRollJointName)       { return Mathf.Clamp(value, -1.919f, +3.665f); }
			if (name == HSRCommon.WristFlexJointName)     { return Mathf.Clamp(value, -1.919f, +1.221f); }
			if (name == HSRCommon.WristRollJointName)     { return Mathf.Clamp(value, -1.919f, +3.665f); }
			if (name == HSRCommon.HeadPanJointName)       { return Mathf.Clamp(value, -3.839f, +1.745f); }
			if (name == HSRCommon.HeadTiltJointName)      { return Mathf.Clamp(value, -1.570f, +0.523f); }
			if (name == HSRCommon.HandLProximalJointName) { return Mathf.Clamp(value, -0.052f, +0.611f); }
			if (name == HSRCommon.HandRProximalJointName) { return Mathf.Clamp(value, -0.611f, +0.052f); }

			return value;
		}

		public static float GetCorrectedJointsEulerAngle(float value, string name)
		{
			if (name == HSRCommon.ArmFlexJointName)       { value = GetCorrectedEulerAngle(value, -150f,   0f); }
			if (name == HSRCommon.ArmRollJointName)       { value = GetCorrectedEulerAngle(value, -110f, 210f); }
			if (name == HSRCommon.WristFlexJointName)     { value = GetCorrectedEulerAngle(value, -110f,  70f); }
			if (name == HSRCommon.WristRollJointName)     { value = GetCorrectedEulerAngle(value, -110f, 210f); }
			if (name == HSRCommon.HeadPanJointName)       { value = GetCorrectedEulerAngle(value, -220f, 100f); }
			if (name == HSRCommon.HeadTiltJointName)      { value = GetCorrectedEulerAngle(value, - 90f,  30f); }
			if (name == HSRCommon.HandLProximalJointName) { value = GetCorrectedEulerAngle(value, -  3f,  35f); }
			if (name == HSRCommon.HandRProximalJointName) { value = GetCorrectedEulerAngle(value, - 35f,   3f); }
			return value;
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue)
		{
			float Play = 5.0f;
			value = (value > maxValue + Play) ? value - 360f : value;
			value = (value < minValue - Play) ? value + 360f : value;
//			Debug.Log("roll=" + value);
			return value;
		}

		//public static Vector3 ConvertPositionHSRToUnity(Vector3 position)
		//{
		//	return new Vector3(-position.y, position.z, position.x);
		//}

		//public static Vector3 ConvertPositionUnityToHSR(Vector3 position)
		//{
		//	return new Vector3(position.z, -position.x, position.y);
		//}
	}
}

