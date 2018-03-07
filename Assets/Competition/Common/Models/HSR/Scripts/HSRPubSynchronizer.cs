using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubSynchronizer : MonoBehaviour
	{
		private static int sequenceNumberForAssignment = 0;

		private static bool executed = false;

		private static List<int> waitingSequenceNumbers = new List<int>();

		private static bool isInitialized = false;

		void Start()
		{
			isInitialized = true;
		}

		public static int GetAssignedSequenceNumber()
		{
			if(isInitialized) { throw new Exception("Please call " + System.Reflection.MethodBase.GetCurrentMethod().Name + " in Awake. (" + nameof(HSRPubSynchronizer) + ")"); }

			sequenceNumberForAssignment++;

			return sequenceNumberForAssignment;
		}

		public static bool CanExecute(int sequenceNumber)
		{
			if (!executed && (waitingSequenceNumbers.Count==0 || waitingSequenceNumbers[0]==sequenceNumber))
			{
				executed = true;

				if(waitingSequenceNumbers.Count!=0)
				{
					waitingSequenceNumbers.RemoveAt(0);
				}

				return true;
			}
			else
			{
				if(!waitingSequenceNumbers.Contains(sequenceNumber))
				{
					waitingSequenceNumbers.Add(sequenceNumber);
				}

				return false;
			}
		}
	
		void LateUpdate()
		{
			executed = false;
		}

		private void OnDestroy()
		{
			sequenceNumberForAssignment = 0;

			executed = false;

			waitingSequenceNumbers = new List<int>();

			isInitialized = false;
		}
	}
}
