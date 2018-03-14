using UnityEngine;
using System;
using System.Collections.Generic;
using SIGVerse.Common;

namespace SIGVerse.Competition
{
	public class PlaybackHsrCollisionEvent : PlaybackCollisionEvent
	{
	}

	public class PlaybackHsrCollisionEventList : PlaybackCollisionEventList
	{
	}

	// ------------------------------------------------------------------

	public class PlaybackHsrCollisionEventController : PlaybackCollisionEventController
	{
		private const string DataType1 = TrialPlaybackCommon.DataType1HsrCollision;

		public PlaybackHsrCollisionEventController(GameObject collisionEffect) : base(collisionEffect)
		{
			this.dataType1 = DataType1;
		}


		new public static string GetDataLine(string elapsedTime, Collision collision, float effectScalen)
		{
			return GetDataLine(elapsedTime, collision, effectScalen, DataType1);
		}
	}
}

