using SIGVerse.Common;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (TrialPlaybackCommon))]
	public class TrialPlaybackPlayer : WorldPlaybackPlayer
	{
		protected class UpdatingVideoPlayerData
		{
			public VideoPlayer UpdatingVideoPlayer { get; set; }
			public long frame { get; set; }

			public void Update()
			{
				this.UpdatingVideoPlayer.frame    = this.frame;
			}
		}

		protected class UpdatingVideoPlayerList
		{
			public float ElapsedTime { get; set; }
			private List<UpdatingVideoPlayerData> updatingVideoPlayerList;

			public UpdatingVideoPlayerList()
			{
				this.updatingVideoPlayerList = new List<UpdatingVideoPlayerData>();
			}

			public void AddUpdatingVideoPlayer(UpdatingVideoPlayerData updatingVideoPlayerData)
			{
				this.updatingVideoPlayerList.Add(updatingVideoPlayerData);
			}

			public List<UpdatingVideoPlayerData> GetUpdatingVideoPlayerList()
			{
				return this.updatingVideoPlayerList;
			}
		}

		protected List<VideoPlayer> targetVideoPlayers;

		protected Dictionary<string, VideoPlayer> targetVideoPlayerPathMap  = new Dictionary<string, VideoPlayer>();

		protected Queue<UpdatingVideoPlayerList> playingVideoPlayerQue;
		protected List<VideoPlayer> videoPlayerOrder = new List<VideoPlayer>();


		protected override void Start()
		{
			base.Start();

			// Video Player
			TrialPlaybackCommon common = this.GetComponent<TrialPlaybackCommon>();

			this.targetVideoPlayers = common.GetTargetVideoPlayers();

			foreach (VideoPlayer targetVideoPlayer in this.targetVideoPlayers)
			{
				this.targetVideoPlayerPathMap.Add(WorldPlaybackCommon.GetLinkPath(targetVideoPlayer.transform), targetVideoPlayer);
			}

			this.playingVideoPlayerQue = new Queue<UpdatingVideoPlayerList>();
			this.videoPlayerOrder = new List<VideoPlayer>();
		}


		protected override void StartInitializing()
		{
			this.step = Step.Initializing;

			this.StartInitializingTransforms();  // Transform
			this.StartInitializingVideoPlayer(); // Video Player

			Thread threadWriteMotions = new Thread(new ParameterizedThreadStart(this.ReadDataFromFile));
			threadWriteMotions.Start(this.filePath);
		}

		protected virtual void StartInitializingVideoPlayer()
		{
			foreach (VideoPlayer targetVideoPlayer in this.targetVideoPlayers)
			{
				targetVideoPlayer.Pause();
			}
		}

		protected override void ReadData(string[] headerArray, string dataStr)
		{
			this.ReadTransforms(headerArray, dataStr);  // Transform
			this.ReadVideoPlayer(headerArray, dataStr); // Video Player
		}

		protected virtual bool ReadVideoPlayer(string[] headerArray, string dataStr)
		{
			// Video Player
			if (headerArray[1] == TrialPlaybackCommon.DataType1VideoPlayer)
			{
				string[] dataArray = dataStr.Split('\t');

				// Definition
				if (headerArray[2] == TrialPlaybackCommon.DataType2VideoPlayerDef)
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
				else if (headerArray[2] == TrialPlaybackCommon.DataType2VideoPlayerVal)
				{
					if (this.videoPlayerOrder.Count == 0) { return false; }

					UpdatingVideoPlayerList timeSeriesVideoPlayerData = new UpdatingVideoPlayerList();

					timeSeriesVideoPlayerData.ElapsedTime = float.Parse(headerArray[0]);

					for (int i = 0; i < dataArray.Length; i++)
					{
						UpdatingVideoPlayerData videoPlayerPlayer = new UpdatingVideoPlayerData();
						videoPlayerPlayer.UpdatingVideoPlayer = this.videoPlayerOrder[i];

						videoPlayerPlayer.frame = long.Parse(dataArray[i]);

						timeSeriesVideoPlayerData.AddUpdatingVideoPlayer(videoPlayerPlayer);
					}
							
					this.playingVideoPlayerQue.Enqueue(timeSeriesVideoPlayerData);
				}

				return true;
			}

			return false;
		}

		protected override void UpdateData()
		{
			if (this.playingTransformQue.Count == 0 && this.playingVideoPlayerQue.Count == 0)
			{
				this.Stop();
				return;
			}

			this.UpdateTransform();   // Transform
			this.UpdateVideoPlayer(); // Video Player
		}

		protected virtual void UpdateVideoPlayer()
		{
			if(this.playingVideoPlayerQue.Count == 0){ return; }

			UpdatingVideoPlayerList updatingVideoPlayerList = null;

			while (this.elapsedTime >= this.playingVideoPlayerQue.Peek().ElapsedTime)
			{
				updatingVideoPlayerList = this.playingVideoPlayerQue.Dequeue();

				if (this.playingVideoPlayerQue.Count == 0) { break; }
			}

			if (updatingVideoPlayerList == null) { return; }

			foreach (UpdatingVideoPlayerData updatingVideoPlayerData in updatingVideoPlayerList.GetUpdatingVideoPlayerList())
			{
				updatingVideoPlayerData.Update();
			}
		}
	}
}

