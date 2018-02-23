using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SIGVerse.Competition
{
	public class PlaybackScoreEvent : PlaybackEventBase
	{
		public Text ScoreText { get; set; }
		public Text TotalText { get; set; }

		public ScoreStatus ScoreStatus { get; set; }

		public void Execute()
		{
			// Update the score panel
			this.ScoreText.text = this.ScoreStatus.Score.ToString();
			this.TotalText.text = this.ScoreStatus.Total.ToString();
		}
	}

	public class PlaybackScoreEventList : PlaybackEventListBase<PlaybackScoreEvent>
	{
		public PlaybackScoreEventList()
		{
			base.EventList = new List<PlaybackScoreEvent>();
		}
	}

	// ------------------------------------------------------------------

	public class PlaybackScoreEventController : PlaybackEventControllerBase<PlaybackScoreEventList, PlaybackScoreEvent>
	{
		private Text scoreText;
		private Text totalText;

		public PlaybackScoreEventController(Text scoreText, Text totalText)
		{
			this.scoreText = scoreText;
			this.totalText = totalText;
		}


		public override void StartInitializingEvents()
		{
			base.eventLists = new List<PlaybackScoreEventList>();
		}


		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// Score
			if (headerArray[1] == TrialPlaybackCommon.DataType1Score)
			{
				string[] dataArray = dataStr.Split(',');

				PlaybackScoreEventList scoreEventList = new PlaybackScoreEventList();
				scoreEventList.ElapsedTime = float.Parse(headerArray[0]);

				PlaybackScoreEvent scoreEvent = new PlaybackScoreEvent();
				scoreEvent.ScoreText = this.scoreText;
				scoreEvent.TotalText = this.totalText;
				scoreEvent.ScoreStatus = new ScoreStatus(int.Parse(dataArray[0]), int.Parse(dataArray[1]), int.Parse(dataArray[2]));

				scoreEventList.EventList.Add(scoreEvent);

				base.eventLists.Add(scoreEventList);

				return true;
			}

			return false;
		}
	}
}
