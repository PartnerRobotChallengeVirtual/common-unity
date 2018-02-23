using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public interface IHSRGraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(GameObject graspedObject);
	}

	public class HSRGraspingDetector : MonoBehaviour, IFingerTriggerHandler
	{
		private const float OpeningAngleThreshold = 0.0f; // This parameter is meaningless currently.

		public GameObject handPalm;
		public GameObject handLeftProximalLink;  // localEularAngle.x : 0 - 34
		public GameObject handRightProximalLink; // localEularAngle.x : -34 - 0
		public GameObject handLeftFingerTip;
		public GameObject handRightFingerTip;

		public List<string> graspableTags;

		public List<GameObject> graspingNotificationDestinations;

		//------------------------

		private float handLeftAngleX;
		private float handRightAngleX;

		private float preHandLeftAngleX;
		private float preHandRightAngleX;

		private List<Collider> graspableColliders;

		private Rigidbody graspedRigidbody;
		private Transform savedParentObj;

		private bool  isHandClosing;
		private float openingAngle;

		private HashSet<Rigidbody> leftCollidingObjects;
		private HashSet<Rigidbody> rightCollidingObjects;

		protected void Awake()
		{
			this.graspableColliders = new List<Collider>();

			foreach(string graspableTag in graspableTags)
			{
				List<GameObject> graspableColliderObjects = GameObject.FindGameObjectsWithTag(graspableTag).ToList<GameObject>();

				foreach(GameObject graspableColliderObject in graspableColliderObjects)
				{
					List<Collider> colliders = graspableColliderObject.GetComponentsInChildren<Collider>().ToList<Collider>();

					this.graspableColliders.AddRange(colliders);
				}
			}

//			Debug.Log("(HSRGraspingDetector)graspable collider num=" + this.graspableColliders.Count);

			this.leftCollidingObjects  = new HashSet<Rigidbody>();
			this.rightCollidingObjects = new HashSet<Rigidbody>();
		}
		
		// Use this for initialization
		void Start()
		{
			this.handLeftAngleX  = this.handLeftProximalLink .transform.localEulerAngles.x;
			this.handRightAngleX = this.handRightProximalLink.transform.localEulerAngles.x;

			this.preHandLeftAngleX  = this.handLeftAngleX;
			this.preHandRightAngleX = this.handRightAngleX;

			this.graspedRigidbody    = null;
			this.isHandClosing = false;

			this.openingAngle = 0.0f;
		}

		// Update is called once per frame
		void Update()
		{
			this.handLeftAngleX  = this.handLeftProximalLink .transform.localEulerAngles.x;
			this.handRightAngleX = this.handRightProximalLink.transform.localEulerAngles.x;

			// Check hand closing
			if(this.handLeftAngleX < this.preHandLeftAngleX && this.handRightAngleX > this.preHandRightAngleX)
			{
				this.isHandClosing = true;
			}
			else
			{
				this.isHandClosing = false;
			}

			// Calc opening angle
			if(this.handLeftAngleX > this.preHandLeftAngleX && this.handRightAngleX < this.preHandRightAngleX)
			{
				this.openingAngle += (this.handLeftAngleX - this.preHandLeftAngleX) + (this.preHandRightAngleX - this.handRightAngleX);
			}
			else
			{
				this.openingAngle = 0.0f;
			}

			if(this.openingAngle > OpeningAngleThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preHandLeftAngleX  = this.handLeftAngleX;
			this.preHandRightAngleX = this.handRightAngleX;
		}


		public void OnTransferredTriggerEnter(Collider other, FingerType fingerType)
		{
//			if (this.transform.root == other.transform.root){ return; }

			if(!this.isGraspable(other)) { return; }

			if(fingerType==FingerType.Left)
			{
				this.leftCollidingObjects.Add(other.attachedRigidbody);
			}
			if(fingerType==FingerType.Right)
			{
				this.rightCollidingObjects.Add(other.attachedRigidbody);
			}

			if(this.isHandClosing && this.graspedRigidbody==null && this.leftCollidingObjects.Contains(other.attachedRigidbody) && this.rightCollidingObjects.Contains(other.attachedRigidbody))
			{
				this.Grasp(other.attachedRigidbody);
			}
		}

		public void OnTransferredTriggerExit(Collider other, FingerType fingerType)
		{
//			if (this.transform.root == other.transform.root){ return; }

			if(!this.isGraspable(other)) { return; }

			if(fingerType==FingerType.Left)
			{
				this.leftCollidingObjects.Remove(other.attachedRigidbody);
			}
			if(fingerType==FingerType.Right)
			{
				this.rightCollidingObjects.Remove(other.attachedRigidbody);
			}


			if (this.graspedRigidbody != null)
			{
				if (!this.leftCollidingObjects.Contains(this.graspedRigidbody) && !this.rightCollidingObjects.Contains(this.graspedRigidbody))
				{
					this.Release();
				}
			}
		}

		private bool isGraspable(Collider other)
		{
			foreach(Collider collider in this.graspableColliders)
			{
				if(other==collider) { return true; }
			}

			return false;
		}

		private void Grasp(Rigidbody collidedRigidbody)
		{
			this.savedParentObj = collidedRigidbody.gameObject.transform.parent;

			collidedRigidbody.gameObject.transform.parent = this.handPalm.transform;

			collidedRigidbody.useGravity  = false;
//			collidedRigidbody.isKinematic = true;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			collidedRigidbody.gameObject.AddComponent<HSRGraspedObjectFixer>();

			this.graspedRigidbody = collidedRigidbody;

			this.SendGraspedObjectInfo(this.graspedRigidbody.gameObject);

			SIGVerseLogger.Info("Grasped: "+this.graspedRigidbody.gameObject.name);
		}

		private void Release()
		{
			this.graspedRigidbody.transform.parent = this.savedParentObj;

			this.graspedRigidbody.useGravity  = true;
//			this.graspedRigidbody.isKinematic = false;

			HSRGraspedObjectFixer graspedObjectFixer = this.graspedRigidbody.gameObject.GetComponent<HSRGraspedObjectFixer>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

			this.graspedRigidbody = null;
			this.savedParentObj = null;

			this.SendGraspedObjectInfo(null);

			SIGVerseLogger.Info("Released the object");
		}

		private void SendGraspedObjectInfo(GameObject graspedObject)
		{
			foreach(GameObject graspingNotificationDestination in graspingNotificationDestinations)
			{
				ExecuteEvents.Execute<IHSRGraspedObjectHandler>
				(
					target: graspingNotificationDestination, 
					eventData: null, 
					functor: (reciever, eventData) => reciever.OnChangeGraspedObject(graspedObject)
				);
			}
		}


		public GameObject GetGraspedObject()
		{
			if(this.graspedRigidbody==null) { return null; }

			return this.graspedRigidbody.gameObject;
		}
	}
}

