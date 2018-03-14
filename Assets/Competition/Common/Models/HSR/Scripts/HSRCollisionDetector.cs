using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using SIGVerse.Competition;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public interface IHSRCollisionHandler : IEventSystemHandler
	{
		void OnHsrCollisionEnter(Collision collision, float effectScale);
	}


	public class HSRCollisionDetector : MonoBehaviour
	{
		private const float CollisionInterval = 1.0f; //[s]

		public List<GameObject> collisionNotificationDestinations;

		public List<string> exclusionColliderTags;

		private GameObject collisionEffect;

		//------------------------

		private List<Collider> exclusionColliderList;

		private float collidedTime;

		protected void Awake()
		{
			this.collisionEffect = (GameObject)Resources.Load(CompetitionUtils.CollisionEffectPath);

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
		//void Update()
		//{
		//}


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
			SIGVerseLogger.Info("Collision detection! parts[0]=" +collision.contacts[0].thisCollider.name + " Collided object=" + SIGVerseUtils.GetHierarchyPath(collision.collider.transform));

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			
			Vector3 contactPoint = SIGVerseUtils.CalcContactAveragePoint(collision);

			effect.transform.position = contactPoint;
			effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			Destroy(effect, 1.0f);

			// Send the collision notification
			foreach(GameObject destination in this.collisionNotificationDestinations)
			{
				ExecuteEvents.Execute<IHSRCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnHsrCollisionEnter(collision, 0.5f)
				);
			}

			this.collidedTime = Time.time;
		}
	}
}

