using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SIGVerse.Competition
{
	public class CollisionTransfererManager : MonoBehaviour
	{
		public List<string> targetTags;

		public List<GameObject> collisionNotificationDestinations;

		public float velocityThreshold = 1.0f;
		public float minimumSendingInterval = 0.1f;


		//------------------------

		private List<GameObject> targets;

		void Awake()
		{
			this.targets = new List<GameObject>();

			foreach(string targetTag in this.targetTags)
			{
				this.targets.AddRange(GameObject.FindGameObjectsWithTag(targetTag).ToList<GameObject>());
			}
		}

		// Use this for initialization
		void Start()
		{
			foreach(GameObject target in this.targets)
			{
				CollisionTransferer collisionTransferer = target.AddComponent<CollisionTransferer>();

				collisionTransferer.Initialize(this.collisionNotificationDestinations, this.velocityThreshold, this.minimumSendingInterval);
			}
		}
	}
}

