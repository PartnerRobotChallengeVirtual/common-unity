using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	public class WorldPlaybackCommon : MonoBehaviour
	{
		public const int PlaybackTypeNone   = 0;
		public const int PlaybackTypeRecord = 1;
		public const int PlaybackTypePlay   = 2;

		// Status
		public const string DataType1Transform   = "11";
		public const string DataType1VideoPlayer = "12";

		// Events
		//public const string DataType1Event1 = "21";
		//public const string DataType1Event2 = "22";
		//public const string DataType1Event3 = "23";
		//public const string DataType1Event4 = "24";
		//public const string DataType1Event5 = "25";

		public const string DataType2TransformDef = "0";
		public const string DataType2TransformVal = "1";

		public const string DataType2VideoPlayerDef = "0";
		public const string DataType2VideoPlayerVal = "1";

		//---------------------------------------

		[HeaderAttribute("File Path")]
		public string filePath;

		[HeaderAttribute("Playback Targets")]
		public List<string> playbackTargetTags;
		public List<string> playbackTargetFromChildrenTags;
		
		[HeaderAttribute("Video Player Settings")]
		public bool replayVideoPlayers = false;

		//---------------------------------------

		protected List<Transform>   targetTransforms;
		protected List<VideoPlayer> targetVideoPlayers;

		protected virtual void Awake()
		{
			// Transform
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
						if(playbackTargetTransform.tag!="DontRecord")
						{
							this.targetTransforms.Add(playbackTargetTransform);
						}
					}
				}
			}

			// Video Player
			this.targetVideoPlayers = new List<VideoPlayer>();

			VideoPlayer[] videoPlayersAll = Resources.FindObjectsOfTypeAll(typeof(VideoPlayer)) as VideoPlayer[];

			foreach (VideoPlayer videoPlayerAll in videoPlayersAll)
			{
				if(videoPlayerAll.GetInstanceID() <= 0){ continue; }

				this.targetVideoPlayers.Add(videoPlayerAll);
			}
		}


		public string GetFilePath()
		{
			return this.filePath;
		}

		public List<Transform> GetTargetTransforms()
		{
			return this.targetTransforms;
		}

		public List<VideoPlayer> GetTargetVideoPlayers()
		{
			return this.targetVideoPlayers;
		}


		public bool IsReplayVideoPlayers()
		{
			return replayVideoPlayers;
		}
	}
}

