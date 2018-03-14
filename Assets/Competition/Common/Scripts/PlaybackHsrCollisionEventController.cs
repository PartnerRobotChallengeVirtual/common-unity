using UnityEngine;
using System;
using System.Collections.Generic;
using SIGVerse.Common;

namespace SIGVerse.Competition
{
	public class PlaybackHsrCollisionEvent : PlaybackEventBase
	{
		public Vector3    Position       { get; set; }
		public GameObject CollisionEffect{ get; set; }

		public void Execute()
		{
			// Instantiate the collision effect
			GameObject effect = MonoBehaviour.Instantiate(this.CollisionEffect);
			
			effect.transform.position = this.Position;
			UnityEngine.Object.Destroy(effect, 1.0f);
		}
	}


	public class PlaybackHsrCollisionEventList : PlaybackEventListBase<PlaybackHsrCollisionEvent>
	{
		public PlaybackHsrCollisionEventList()
		{
			this.EventList = new List<PlaybackHsrCollisionEvent>();
		}
	}


	// ------------------------------------------------------------------

	public class PlaybackHsrCollisionEventController : PlaybackEventControllerBase<PlaybackHsrCollisionEventList, PlaybackHsrCollisionEvent>
	{
		private GameObject collisionEffect;

		public PlaybackHsrCollisionEventController(GameObject collisionEffect)
		{
			this.collisionEffect = collisionEffect;
		}

		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackHsrCollisionEventList>();
		}

		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// HSR Collision
			if (headerArray[1] == TrialPlaybackCommon.DataType1HsrCollision)
			{
				string[] dataArray = dataStr.Split('\t');

				PlaybackHsrCollisionEventList hsrCollisionEventList = new PlaybackHsrCollisionEventList();
				hsrCollisionEventList.ElapsedTime = float.Parse(headerArray[0]);

				PlaybackHsrCollisionEvent hsrCollisionEvent = new PlaybackHsrCollisionEvent();
				hsrCollisionEvent.Position = new Vector3(float.Parse(dataArray[0]), float.Parse(dataArray[1]), float.Parse(dataArray[2]));
				hsrCollisionEvent.CollisionEffect = this.collisionEffect;

				hsrCollisionEventList.EventList.Add(hsrCollisionEvent);

				this.eventLists.Add(hsrCollisionEventList);

				return true;
			}

			return false;
		}



		public static string GetDataLine(string elapsedTime, Collision collision)
		{
			Vector3 contactAve = SIGVerseUtil.CalcContactAveragePoint(collision);

			string dataLine = elapsedTime + "," + TrialPlaybackCommon.DataType1HsrCollision;

			dataLine += "\t" + contactAve.x + "\t" + contactAve.y + "\t" + contactAve.z;

			foreach(ContactPoint contactPoint in collision.contacts)
			{
				// Following data is unused now
				dataLine += "\t" + SIGVerseUtil.GetHierarchyPath(contactPoint.thisCollider.transform) + "\t" + SIGVerseUtil.GetHierarchyPath(contactPoint.otherCollider.transform);
			}
			return dataLine;
		}
	}
}

