// Generated by gencs from nav_msgs/SetMap.srv
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;


namespace SIGVerse.RosBridge 
{
	namespace nav_msgs 
	{
		[System.Serializable]
		public class SetMapResponse : ServiceResponse
		{
			public bool success;


			public SetMapResponse()
			{
				this.success = false;
			}

			public SetMapResponse(bool success)
			{
				this.success = success;
			}

			new public static string GetMessageType()
			{
				return "nav_msgs/SetMapResponse";
			}

			new public static string GetMD5Hash()
			{
				return "358e233cde0c8a8bcfea4ce193f8fc15";
			}
		} // class SetMapResponse
	} // namespace nav_msgs
} // namespace SIGVerse.ROSBridge

