// Generated by gencs from sensor_msgs/Imu.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;

using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.geometry_msgs;

namespace SIGVerse.RosBridge 
{
	namespace sensor_msgs 
	{
		[System.Serializable]
		public class Imu : RosMessage
		{
			public std_msgs.Header header;
			public UnityEngine.Quaternion orientation;
			public double[]  orientation_covariance;
			public UnityEngine.Vector3 angular_velocity;
			public double[]  angular_velocity_covariance;
			public UnityEngine.Vector3 linear_acceleration;
			public double[]  linear_acceleration_covariance;


			public Imu()
			{
				this.header = new std_msgs.Header();
				this.orientation = new UnityEngine.Quaternion();
				this.orientation_covariance = new double[9];
				this.angular_velocity = new UnityEngine.Vector3();
				this.angular_velocity_covariance = new double[9];
				this.linear_acceleration = new UnityEngine.Vector3();
				this.linear_acceleration_covariance = new double[9];
			}

			public Imu(std_msgs.Header header, UnityEngine.Quaternion orientation, double[]  orientation_covariance, UnityEngine.Vector3 angular_velocity, double[]  angular_velocity_covariance, UnityEngine.Vector3 linear_acceleration, double[]  linear_acceleration_covariance)
			{
				this.header = header;
				this.orientation = orientation;
				this.orientation_covariance = orientation_covariance;
				this.angular_velocity = angular_velocity;
				this.angular_velocity_covariance = angular_velocity_covariance;
				this.linear_acceleration = linear_acceleration;
				this.linear_acceleration_covariance = linear_acceleration_covariance;
			}

			new public static string GetMessageType()
			{
				return "sensor_msgs/Imu";
			}

			new public static string GetMD5Hash()
			{
				return "6a62c6daae103f4ff57a132d6f95cec2";
			}
		} // class Imu
	} // namespace sensor_msgs
} // namespace SIGVerse.ROSBridge

