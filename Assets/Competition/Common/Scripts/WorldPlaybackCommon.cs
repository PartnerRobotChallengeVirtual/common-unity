using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Competition
{
	public class WorldPlaybackCommon : MonoBehaviour
	{
		public const int PlaybackTypeNone   = 0;
		public const int PlaybackTypeRecord = 1;
		public const int PlaybackTypePlay   = 2;

		// Status
		public const string DataType1Transform = "11";

		// Events
		//public const string DataType1Event1 = "21";
		//public const string DataType1Event2 = "22";
		//public const string DataType1Event3 = "23";
		//public const string DataType1Event4 = "24";
		//public const string DataType1Event5 = "25";

		public const string DataType2TransformDef = "0";
		public const string DataType2TransformVal = "1";

		//---------------------------------------

		protected List<Transform> targetTransforms;

		public List<Transform> GetTargetTransforms()
		{
			return this.targetTransforms;
		}


		public static string GetLinkPath(Transform transform)
		{
			string path = transform.name;

			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			return path;
		}

		//---------------------------------------

		public List<string> playbackTargetTags;
		public List<string> playbackTargetFromChildrenTags;
		
		protected virtual void Awake()
		{
			this.targetTransforms = new List<Transform>();

			foreach (string playbackTargetTag in playbackTargetTags)
			{
				GameObject[] playbackTargetObjects = GameObject.FindGameObjectsWithTag(playbackTargetTag);

				foreach(GameObject playbackTargetObject in playbackTargetObjects)
				{
					this.targetTransforms.Add(playbackTargetObject.transform);
				}
			}

			foreach (string playbackTargetTag in playbackTargetFromChildrenTags)
			{
				GameObject[] playbackTargetObjects = GameObject.FindGameObjectsWithTag(playbackTargetTag);

				foreach(GameObject playbackTargetObject in playbackTargetObjects)
				{
					Transform[] playbackTargetTransforms = playbackTargetObject.GetComponentsInChildren<Transform>(true);

					foreach(Transform playbackTargetTransform in playbackTargetTransforms)
					{
						this.targetTransforms.Add(playbackTargetTransform);
					}
				}
			}
		}
	}
}

