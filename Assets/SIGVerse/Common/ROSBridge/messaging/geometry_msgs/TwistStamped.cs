// Generated by gencs from geometry_msgs/TwistStamped.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.ROSBridge;
using UnityEngine;

using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.ROSBridge.geometry_msgs;

namespace SIGVerse.ROSBridge 
{
	namespace geometry_msgs 
	{
		[System.Serializable]
		public class TwistStamped : ROSMessage
		{
			public std_msgs.Header header;
			public geometry_msgs.Twist twist;


			public TwistStamped()
			{
				this.header = new std_msgs.Header();
				this.twist = new geometry_msgs.Twist();
			}

			public TwistStamped(std_msgs.Header header, geometry_msgs.Twist twist)
			{
				this.header = header;
				this.twist = twist;
			}

			new public static string GetMessageType()
			{
				return "geometry_msgs/TwistStamped";
			}

			new public static string GetMD5Hash()
			{
				return "98d34b0043a2093cf9d9345ab6eef12e";
			}
		} // class TwistStamped
	} // namespace geometry_msgs
} // namespace SIGVerse.ROSBridge

