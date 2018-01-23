using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public enum FingerType
	{
		Left,
		Right,
	}

	public interface IFingerTriggerHandler : IEventSystemHandler
	{
		void OnTransferredTriggerEnter(Collider other, FingerType fingerType);
		void OnTransferredTriggerExit (Collider other, FingerType fingerType);
	}

	public class HSRFingerTriggerTransferer : MonoBehaviour
	{

		public FingerType fingerType;

		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{
		}

		void OnTriggerEnter(Collider other)
		{
			if(other.isTrigger) { return; }

			if (other.attachedRigidbody == null) { return; }

//			Debug.Log("HSRFingerTriggerTransferer OnTriggerEnter");

			ExecuteEvents.Execute<IFingerTriggerHandler>
			(
				target: this.transform.root.gameObject,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(other, this.fingerType)
			);
		}

		void OnTriggerExit(Collider other)
		{
			if(other.isTrigger) { return; }

			if (other.attachedRigidbody == null) { return; }

			ExecuteEvents.Execute<IFingerTriggerHandler>
			(
				target: this.transform.root.gameObject,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(other, this.fingerType)
			);
		}
	}
}

