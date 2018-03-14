using UnityEngine;
using System;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Linq;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	public class PlaybackVideoPlayerEvent : PlaybackEventBase
	{
		public VideoPlayer TargetVideoPlayer { get; set; }
		public long        Frame             { get; set; }

		public void Execute()
		{
			this.TargetVideoPlayer.frame = this.Frame;
		}
	}


	public class PlaybackVideoPlayerEventList : PlaybackEventListBase<PlaybackVideoPlayerEvent>
	{
		public PlaybackVideoPlayerEventList()
		{
			this.EventList = new List<PlaybackVideoPlayerEvent>();
		}
	}


	// ------------------------------------------------------------------

	public class PlaybackVideoPlayerEventController : PlaybackEventControllerBase<PlaybackVideoPlayerEventList, PlaybackVideoPlayerEvent>
	{
		private WorldPlaybackCommon common;

		private List<VideoPlayer> targetVideoPlayers;

		private Dictionary<string, VideoPlayer> targetVideoPlayerPathMap;

		private List<VideoPlayer> videoPlayerOrder;

		
		public PlaybackVideoPlayerEventController(WorldPlaybackCommon playbackCommon)
		{
			this.common = playbackCommon;

			// Video Player
			this.targetVideoPlayers = this.common.GetTargetVideoPlayers();

			this.targetVideoPlayerPathMap = new Dictionary<string, VideoPlayer>();

			foreach (VideoPlayer targetVideoPlayer in this.targetVideoPlayers)
			{
				this.targetVideoPlayerPathMap.Add(SIGVerseUtils.GetHierarchyPath(targetVideoPlayer.transform), targetVideoPlayer);
			}
		}
		

		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackVideoPlayerEventList>();

			this.videoPlayerOrder = new List<VideoPlayer>();
			
			// Clear the video player list if don't play
			if(!this.common.IsReplayVideoPlayers())
			{
				this.targetVideoPlayers = new List<VideoPlayer>();
			}
		}


		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			if(!this.common.IsReplayVideoPlayers()){ return true; }

			// Video Player
			if (headerArray[1] == WorldPlaybackCommon.DataType1VideoPlayer)
			{
				string[] dataArray = dataStr.Split('\t');

				// Definition
				if (headerArray[2] == WorldPlaybackCommon.DataType2VideoPlayerDef)
				{
					this.videoPlayerOrder.Clear();

					SIGVerseLogger.Info("Playback player : VideoPlayer data num=" + dataArray.Length);

					foreach (string videoPlayerPath in dataArray)
					{
						if (!this.targetVideoPlayerPathMap.ContainsKey(videoPlayerPath))
						{
							SIGVerseLogger.Error("Couldn't find the VideoPlayer that path is " + videoPlayerPath);
						}

						this.videoPlayerOrder.Add(this.targetVideoPlayerPathMap[videoPlayerPath]);
					}
				}
				// Value
				else if (headerArray[2] == WorldPlaybackCommon.DataType2VideoPlayerVal)
				{
					if (this.videoPlayerOrder.Count == 0) { return false; }

					PlaybackVideoPlayerEventList playbackVideoPlayerEventList = new PlaybackVideoPlayerEventList();

					playbackVideoPlayerEventList.ElapsedTime = float.Parse(headerArray[0]);

					for (int i = 0; i < dataArray.Length; i++)
					{
						PlaybackVideoPlayerEvent videoPlayerEvent = new PlaybackVideoPlayerEvent();

						videoPlayerEvent.TargetVideoPlayer = this.videoPlayerOrder[i];
						videoPlayerEvent.Frame = long.Parse(dataArray[i]);

						playbackVideoPlayerEventList.EventList.Add(videoPlayerEvent);
					}
					
					this.eventLists.Add(playbackVideoPlayerEventList);
				}

				return true;
			}

			return false;
		}


		public List<VideoPlayer> GetTargetVideoPlayers()
		{
			return this.targetVideoPlayers;
		}



		public static string GetDefinitionLine(List<VideoPlayer> targetVideoPlayers)
		{
			string definitionLine = "0.0," + WorldPlaybackCommon.DataType1VideoPlayer + "," + WorldPlaybackCommon.DataType2VideoPlayerDef; // Elapsed time is dummy.

			foreach (VideoPlayer targetVideoPlayer in targetVideoPlayers)
			{
				// Make a header line
				definitionLine += "\t" + SIGVerseUtils.GetHierarchyPath(targetVideoPlayer.transform);
			}

			return definitionLine;
		}

		public static string GetDataLine(string elapsedTime, List<VideoPlayer> targetVideoPlayers)
		{
			string dataLine = string.Empty;

			// Video Player
			dataLine += elapsedTime + "," + WorldPlaybackCommon.DataType1VideoPlayer + "," + WorldPlaybackCommon.DataType2VideoPlayerVal;

			foreach (VideoPlayer targetVideoPlayer in targetVideoPlayers)
			{
				dataLine += "\t" + targetVideoPlayer.frame;
			}

			return dataLine;
		}
	}
}

