using UnityEngine;
using System;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Reflection;

namespace SIGVerse.Competition
{
	public class PlaybackCollisionEvent : PlaybackEventBase
	{
		public Vector3    Position       { get; set; }
		public float      EffectScale    { get; set; }
		public GameObject CollisionEffect{ get; set; }

		public void Execute()
		{
			// Instantiate the collision effect
			GameObject effect = MonoBehaviour.Instantiate(this.CollisionEffect);
			
			effect.transform.position   = this.Position;
			effect.transform.localScale = new Vector3(this.EffectScale, this.EffectScale, this.EffectScale);

			UnityEngine.Object.Destroy(effect, 1.0f);
		}
	}


	public class PlaybackCollisionEventList : PlaybackEventListBase<PlaybackCollisionEvent>
	{
		public PlaybackCollisionEventList()
		{
			this.EventList = new List<PlaybackCollisionEvent>();
		}
	}


	// ------------------------------------------------------------------

	public class PlaybackCollisionEventController : PlaybackEventControllerBase<PlaybackCollisionEventList, PlaybackCollisionEvent>
	{
		private const string DataType1 = TrialPlaybackCommon.DataType1Collision;

		protected string dataType1 = DataType1;

		private GameObject collisionEffect;

		public PlaybackCollisionEventController(GameObject collisionEffect)
		{
			this.collisionEffect = collisionEffect;
		}

		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackCollisionEventList>();
		}

		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// Collision
			if (headerArray[1] == dataType1)
			{
				string[] dataArray = dataStr.Split('\t');

				PlaybackCollisionEventList collisionEventList = new PlaybackCollisionEventList();
				collisionEventList.ElapsedTime = float.Parse(headerArray[0]);

				PlaybackCollisionEvent collisionEvent = new PlaybackCollisionEvent();
				collisionEvent.Position    = new Vector3(float.Parse(dataArray[0]), float.Parse(dataArray[1]), float.Parse(dataArray[2]));
				collisionEvent.EffectScale = float.Parse(dataArray[3]);
				collisionEvent.CollisionEffect = this.collisionEffect;

				collisionEventList.EventList.Add(collisionEvent);

				this.eventLists.Add(collisionEventList);

				return true;
			}

			return false;
		}



		public static string GetDataLine(string elapsedTime, Collision collision, float collisionVelocity, float effectScalen)
		{
			return GetDataLine(elapsedTime, collision, collisionVelocity, effectScalen, DataType1);
		}


		protected static string GetDataLine(string elapsedTime, Collision collision, float collisionVelocity, float effectScale, string dataType)
		{
			Vector3 contactAve = SIGVerseUtils.CalcContactAveragePoint(collision);

			string dataLine = elapsedTime + "," + dataType;

			dataLine += "\t" + contactAve.x + "\t" + contactAve.y + "\t" + contactAve.z + "\t" + effectScale;

			// Following data is unused now
			dataLine += "\t" + collisionVelocity + 
				"\t" + SIGVerseUtils.GetHierarchyPath(collision.contacts[0].thisCollider .transform) + 
				"\t" + SIGVerseUtils.GetHierarchyPath(collision.contacts[0].otherCollider.transform);

			return dataLine;
		}
	}
}

