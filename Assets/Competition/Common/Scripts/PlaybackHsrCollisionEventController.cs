using UnityEngine;
using System;
using System.Collections.Generic;

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
				string[] dataArray = dataStr.Split(',');

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
	}
}

