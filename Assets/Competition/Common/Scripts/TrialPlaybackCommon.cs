using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	public class TrialPlaybackCommon : WorldPlaybackCommon
	{
		// Status
		public const string DataType1VideoPlayer = "12";

		public const string DataType2VideoPlayerDef = "0";
		public const string DataType2VideoPlayerVal = "1";


		public bool recordVideoPlayers = false;

		protected List<VideoPlayer> targetVideoPlayers;

		public List<VideoPlayer> GetTargetVideoPlayers()
		{
			return this.targetVideoPlayers;
		}


		protected override void Awake()
		{
			base.Awake();

			// Video Player
			this.targetVideoPlayers = new List<VideoPlayer>();

			if(this.recordVideoPlayers)
			{
				VideoPlayer[] videoPlayersAll = Resources.FindObjectsOfTypeAll(typeof(VideoPlayer)) as VideoPlayer[];

				foreach (VideoPlayer videoPlayerAll in videoPlayersAll)
				{
					if(videoPlayerAll.GetInstanceID() <= 0){ continue; }

					this.targetVideoPlayers.Add(videoPlayerAll);
				}
			}
		}

		public bool IsRecordVideoPlayers()
		{
			return recordVideoPlayers;
		}
	}
}

