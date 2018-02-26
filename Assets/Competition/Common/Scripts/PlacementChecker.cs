using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Competition
{
	public class PlacementChecker : MonoBehaviour
	{
		private const float MaxWaitingTime = 2.0f;
		private const string JudgeTriggersName = "JudgeTriggers";

		private Dictionary<GameObject, int> placedObjectMap;


		void Start ()
		{
			this.placedObjectMap = new Dictionary<GameObject, int>();

			CheckExistanceOfColliders(this.transform);
		}

		public IEnumerator<bool?> IsPlaced(GameObject targetObj)
		{
			Rigidbody targetRigidbody = targetObj.GetComponent<Rigidbody>();

			float timeLimit = Time.time + MaxWaitingTime;

			while (!this.IsPlacedNow(targetObj, targetRigidbody) && Time.time < timeLimit)
			{
				yield return null;
			}
		
			if(Time.time < timeLimit)
			{
				yield return true;
			}
			else
			{
				SIGVerseLogger.Info("Target placement failed: Time out.");

				yield return false;
			}
		}


		private static void CheckExistanceOfColliders(Transform rootTransform)
		{
			Transform judgeTriggersTransform = rootTransform.Find(JudgeTriggersName);

			if (judgeTriggersTransform==null)
			{
				SIGVerseLogger.Error("No Judge Triggers object");
				throw new Exception("No Judge Triggers object");
			}

			BoxCollider[] boxColliders = judgeTriggersTransform.GetComponents<BoxCollider>();
			
			if(boxColliders.Length==0)
			{
				SIGVerseLogger.Error("No Box colliders");
				throw new Exception("No Box colliders");
			}
		}


		private bool IsPlacedNow(GameObject targetObj, Rigidbody targetRigidbody)
		{
			return targetRigidbody.IsSleeping() && this.placedObjectMap.ContainsKey(targetObj) && this.placedObjectMap[targetObj] > 0;
		}


		private void OnTriggerEnter(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			GameObject contactedObj = other.attachedRigidbody.gameObject;

			if(!this.placedObjectMap.ContainsKey(contactedObj))
			{
				this.placedObjectMap.Add(contactedObj, 0);
			}

			this.placedObjectMap[contactedObj]++;
		}


		private void OnTriggerExit(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			GameObject placedObj = other.attachedRigidbody.gameObject;

			this.placedObjectMap[placedObj]--;
		}
	}
}


