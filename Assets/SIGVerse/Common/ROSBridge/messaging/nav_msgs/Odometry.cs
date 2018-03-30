// Generated by gencs from nav_msgs/Odometry.msg
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
	namespace nav_msgs 
	{
		[System.Serializable]
		public class Odometry : RosMessage
		{
			public std_msgs.Header header;
			public string child_frame_id;
			public geometry_msgs.PoseWithCovariance pose;
			public geometry_msgs.TwistWithCovariance twist;


			public Odometry()
			{
				this.header = new std_msgs.Header();
				this.child_frame_id = "";
				this.pose = new geometry_msgs.PoseWithCovariance();
				this.twist = new geometry_msgs.TwistWithCovariance();
			}

			public Odometry(std_msgs.Header header, string child_frame_id, geometry_msgs.PoseWithCovariance pose, geometry_msgs.TwistWithCovariance twist)
			{
				this.header = header;
				this.child_frame_id = child_frame_id;
				this.pose = pose;
				this.twist = twist;
			}

			new public static string GetMessageType()
			{
				return "nav_msgs/Odometry";
			}

			new public static string GetMD5Hash()
			{
				return "cd5e73d190d741a2f92e81eda573aca7";
			}
		} // class Odometry
	} // namespace nav_msgs
} // namespace SIGVerse.ROSBridge

