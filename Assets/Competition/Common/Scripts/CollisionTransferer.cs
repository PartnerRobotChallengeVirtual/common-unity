using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Competition
{
	public interface ITransferredCollisionHandler : IEventSystemHandler
	{
		void OnTransferredCollisionEnter(Collision collision, GameObject collidingObject);
	}

	public class CollisionTransferer : MonoBehaviour
	{
		private List<GameObject> destinations;
		private float velocityThreshold;
		private float minimumSendingInterval;

		private float lastSendingTime = 0.0f;


		public void Initialize(List<GameObject> destinations, float velocityThreshold=0.1f, float minimumSendingInterval=0.1f)
		{
			this.destinations           = destinations;
			this.velocityThreshold      = velocityThreshold;
			this.minimumSendingInterval = minimumSendingInterval;
		}


		void OnCollisionEnter(Collision collision)
		{
			if(collision.relativeVelocity.magnitude < this.velocityThreshold){ return; }

			if(Time.time - this.lastSendingTime < this.minimumSendingInterval){ return; }

			this.lastSendingTime = Time.time;

			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<ITransferredCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredCollisionEnter(collision, this.gameObject)
				);
			}
		}
	}
}

