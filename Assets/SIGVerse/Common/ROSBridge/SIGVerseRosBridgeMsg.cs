using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SIGVerse.Common;

namespace SIGVerse.SIGVerseRosBridge
{
	[System.Serializable]
	public class SIGVerseRosBridgeMessage<RosMessage>
	{
		public string op { get; set; }
		public string topic { get; set; }
		public string type { get; set; }
		public RosMessage msg { get; set; }

		public SIGVerseRosBridgeMessage(string op, string topic, string type, RosMessage msg)
		{
			this.op = op;
			this.topic = topic;
			this.type = type;
			this.msg = msg;
		}


		public void SendMsg(NetworkStream networkStream)
		{
			MemoryStream memoryStream = new MemoryStream();
			BsonWriter writer = new BsonWriter(memoryStream);

			JsonSerializer serializer = new JsonSerializer();
			serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			serializer.Serialize(writer, this); // high load

			byte[] msgBinary = memoryStream.ToArray();

//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			try
			{
				if(networkStream.CanWrite)
				{
					networkStream.Write(msgBinary, 0, msgBinary.Length);
				}

				// Receive the time gap between Unity and ROS
				if(networkStream.DataAvailable)
				{
					byte[] byteArray = new byte[256];

					if(networkStream.CanRead)
					{
						networkStream.Read(byteArray, 0, byteArray.Length);
					}
				
					string message = System.Text.Encoding.UTF8.GetString(byteArray);
					string[] messageArray = message.Split(',');

					if (messageArray.Length == 3)
					{
						SIGVerseLogger.Info("Time gap sec=" + messageArray[1] + ", msec=" + messageArray[2]);

						SIGVerse.RosBridge.std_msgs.Header.SetTimeGap(Int32.Parse(messageArray[1]), Int32.Parse(messageArray[2]));
					}
					else
					{
						SIGVerseLogger.Error("Illegal message. Time gap message=" + message);
					}
				}
			}
			catch (ObjectDisposedException exception)
			{
				SIGVerseLogger.Warn(exception.Message);
			}

//			sw.Stop();
//			UnityEngine.Debug.Log("time=" + sw.Elapsed + ", size=" + msgBinary.Length);
		}
	}
}
