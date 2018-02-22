using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public interface IHSRCollisionHandler : IEventSystemHandler
	{
		void OnHsrCollisionEnter(Vector3 contactPoint);
	}


	public class HSRCollisionDetector : MonoBehaviour
	{
		private const float CollisionInterval = 1.0f; //[s]

		public List<GameObject> collisionNotificationDestinations;

		public List<string> exclusionColliderTags;

		public GameObject collisionEffect;

		//------------------------

		private List<Collider> exclusionColliderList;

		private float collidedTime;

		protected void Awake()
		{
			this.exclusionColliderList = new List<Collider>();

			foreach(string exclusionColliderTag in exclusionColliderTags)
			{
				List<GameObject> exclusionColliderObjects = GameObject.FindGameObjectsWithTag(exclusionColliderTag).ToList<GameObject>();

				foreach(GameObject exclusionColliderObject in exclusionColliderObjects)
				{
					List<Collider> colliders = exclusionColliderObject.GetComponentsInChildren<Collider>().ToList<Collider>();

					this.exclusionColliderList.AddRange(colliders);
				}
			}
		}

		// Use this for initialization
		void Start()
		{
			this.collidedTime = 0.0f;
//			Debug.Log("HSRCollisionDetector:"+this.graspables.Count);
		}

		// Update is called once per frame
		void Update()
		{
		}


		void OnCollisionEnter(Collision collision)
		{
			if(Time.time - this.collidedTime < CollisionInterval) { return; }

			if(collision.collider.transform.root==this.transform.root) { return; }

			if(collision.collider.isTrigger) { return; }

			foreach(Collider collider in exclusionColliderList)
			{
				if(collision.collider==collider) { return; }
			}

			this.ExecCollisionProcess(collision);
		}


		private void ExecCollisionProcess(Collision collision)
		{
			SIGVerseLogger.Info("Collision detection! Collided object="+collision.collider.name);

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			
			Vector3 contactPoint = this.CalcContactPoint(collision);

			effect.transform.position = contactPoint;
			Destroy(effect, 1.0f);

			// Send the collision notification
			foreach(GameObject destination in this.collisionNotificationDestinations)
			{
				ExecuteEvents.Execute<IHSRCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnHsrCollisionEnter(contactPoint)
				);
			}

			this.collidedTime = Time.time;
		}

		private Vector3 CalcContactPoint(Collision collision)
		{
			ContactPoint[] contactPoints = collision.contacts;

			Vector3 contactPointAve = Vector3.zero;

			foreach(ContactPoint contactPoint in contactPoints)
			{
				contactPointAve += contactPoint.point;
			}

			contactPointAve /= contactPoints.Length;

			return contactPointAve;
		}
	}
}

