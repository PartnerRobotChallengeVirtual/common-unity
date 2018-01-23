// Generated by gencs from sensor_msgs/CameraInfo.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.ROSBridge;
using UnityEngine;

using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.ROSBridge.sensor_msgs;

namespace SIGVerse.ROSBridge 
{
	namespace sensor_msgs 
	{
		[System.Serializable]
		public class CameraInfoForSIGVerseBridge : ROSMessage
		{
			public std_msgs.Header header;
			public System.UInt32 height;
			public System.UInt32 width;
			public string distortion_model;
			public double[]  D;
			public double[]  K;
			public double[]  R;
			public double[]  P;
			public System.UInt32 binning_x;
			public System.UInt32 binning_y;
			public sensor_msgs.RegionOfInterest roi;


			public CameraInfoForSIGVerseBridge()
			{
				this.header = new std_msgs.Header();
				this.height = 0;
				this.width = 0;
				this.distortion_model = "";
				this.D = new double[0];
				this.K = new double[9];
				this.R = new double[9];
				this.P = new double[12];
				this.binning_x = 0;
				this.binning_y = 0;
				this.roi = new sensor_msgs.RegionOfInterest();
			}

			public CameraInfoForSIGVerseBridge(std_msgs.Header header, System.UInt32 height, System.UInt32 width, string distortion_model, double[] D, double[]  K, double[]  R, double[]  P, System.UInt32 binning_x, System.UInt32 binning_y, sensor_msgs.RegionOfInterest roi)
			{
				this.header = header;
				this.height = height;
				this.width = width;
				this.distortion_model = distortion_model;
				this.D = D;
				this.K = K;
				this.R = R;
				this.P = P;
				this.binning_x = binning_x;
				this.binning_y = binning_y;
				this.roi = roi;
			}

			new public static string GetMessageType()
			{
				return "sensor_msgs/CameraInfo";
			}

			new public static string GetMD5Hash()
			{
				return "c9a58c1b0b154e0e6da7578cb991d214";
			}
		} // class CameraInfo
	} // namespace sensor_msgs
} // namespace SIGVerse.ROSBridge

