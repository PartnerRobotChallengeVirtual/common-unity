using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Competition
{
	public interface ITransferredCollisionHandler : IEventSystemHandler
	{
		void OnTransferredCollisionEnter(Collision collision, float collisionVelocity, float effectScale);
	}

	public class CollisionTransferer : MonoBehaviour
	{
		private List<GameObject> destinations;
		private float velocityThreshold;
		private float minimumSendingInterval;

		private float lastSendingTime = 0.0f;

		private GameObject collisionEffect;


		protected void Awake()
		{
			this.collisionEffect = (GameObject)Resources.Load(CompetitionUtils.CollisionEffectPath);
		}


		public void Initialize(List<GameObject> destinations, float velocityThreshold=1.0f, float minimumSendingInterval=0.1f)
		{
			this.destinations           = destinations;
			this.velocityThreshold      = velocityThreshold;
			this.minimumSendingInterval = minimumSendingInterval;
		}


		void OnCollisionEnter(Collision collision)
		{
			if(collision.relativeVelocity.magnitude < this.velocityThreshold){ return; }

			if(Time.time - this.lastSendingTime < this.minimumSendingInterval){ return; }

			foreach(ContactPoint contactPoint in collision.contacts)
			{
				if(contactPoint.otherCollider.CompareTag("NonDeductionCollider")){ return; }
			}

			this.ExecCollisionProcess(collision);
		}


		private void ExecCollisionProcess(Collision collision)
		{
			SIGVerseLogger.Info("Object collision occurred. name=" + this.name + " Collided object=" + SIGVerseUtils.GetHierarchyPath(collision.collider.transform));

			this.lastSendingTime = Time.time;

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			
			Vector3 contactPoint = SIGVerseUtils.CalcContactAveragePoint(collision);

			effect.transform.position = contactPoint;
			effect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			Destroy(effect, 1.0f);


			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<ITransferredCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredCollisionEnter(collision, collision.relativeVelocity.magnitude, 0.1f)
				);
			}
		}
	}
}

