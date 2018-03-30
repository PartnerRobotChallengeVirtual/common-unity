// Generated by gencs from sensor_msgs/PointCloud.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;

using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.geometry_msgs;
using SIGVerse.RosBridge.sensor_msgs;

namespace SIGVerse.RosBridge 
{
	namespace sensor_msgs 
	{
		[System.Serializable]
		public class PointCloud : RosMessage
		{
			public std_msgs.Header header;
			public System.Collections.Generic.List<geometry_msgs.Point32>  points;
			public System.Collections.Generic.List<sensor_msgs.ChannelFloat32>  channels;


			public PointCloud()
			{
				this.header = new std_msgs.Header();
				this.points = new System.Collections.Generic.List<geometry_msgs.Point32>();
				this.channels = new System.Collections.Generic.List<sensor_msgs.ChannelFloat32>();
			}

			public PointCloud(std_msgs.Header header, System.Collections.Generic.List<geometry_msgs.Point32>  points, System.Collections.Generic.List<sensor_msgs.ChannelFloat32>  channels)
			{
				this.header = header;
				this.points = points;
				this.channels = channels;
			}

			new public static string GetMessageType()
			{
				return "sensor_msgs/PointCloud";
			}

			new public static string GetMD5Hash()
			{
				return "d8e9c3f5afbdd8a130fd1d2763945fca";
			}
		} // class PointCloud
	} // namespace sensor_msgs
} // namespace SIGVerse.ROSBridge

