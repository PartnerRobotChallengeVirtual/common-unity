using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace SIGVerse.Competition
{
	public class PlaybackTaskInfoEvent : PlaybackEventBase
	{
		public Text TeamNameText    { get; set; }
		public Text TrialNumberText { get; set; }
		public Text TimeLeftValText { get; set; }
		public Text TaskMessageText { get; set; }

		public string TeamName    { get; set; }
		public string TrialNumber { get; set; }
		public string TimeLeftVal { get; set; }
		public string TaskMessage { get; set; }

		public void Execute()
		{
			// Update the Main panel
			this.TeamNameText.text    = this.TeamName;
			this.TrialNumberText.text = this.TrialNumber;
			this.TimeLeftValText.text = this.TimeLeftVal;
			this.TaskMessageText.text = this.TaskMessage;
		}
	}

	public class PlaybackTaskInfoEventList : PlaybackEventListBase<PlaybackTaskInfoEvent>
	{
		public PlaybackTaskInfoEventList()
		{
			this.EventList = new List<PlaybackTaskInfoEvent>();
		}
	}

	// ------------------------------------------------------------------

	public class PlaybackTaskInfoEventController : PlaybackEventControllerBase<PlaybackTaskInfoEventList, PlaybackTaskInfoEvent>
	{
		private Text teamNameText;
		private Text trialNumberText;
		private Text timeLeftValText;
		private Text taskMessageText;

		public PlaybackTaskInfoEventController(Text teamNameText, Text trialNumberText, Text timeLeftValText, Text taskMessageText)
		{
			this.teamNameText    = teamNameText;
			this.trialNumberText = trialNumberText;
			this.timeLeftValText = timeLeftValText;
			this.taskMessageText = taskMessageText;
		}


		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackTaskInfoEventList>();
		}


		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// TaskInfo
			if (headerArray[1] == TrialPlaybackCommon.DataType1TaskInfo)
			{
				string[] dataArray = dataStr.Split('\t');

				PlaybackTaskInfoEventList TaskInfoEventList = new PlaybackTaskInfoEventList();
				TaskInfoEventList.ElapsedTime = float.Parse(headerArray[0]);

				PlaybackTaskInfoEvent TaskInfoEvent = new PlaybackTaskInfoEvent();
				TaskInfoEvent.TeamNameText    = this.teamNameText;
				TaskInfoEvent.TrialNumberText = this.trialNumberText;
				TaskInfoEvent.TimeLeftValText = this.timeLeftValText;
				TaskInfoEvent.TaskMessageText = this.taskMessageText;
				TaskInfoEvent.TeamName    = Regex.Unescape(dataArray[0]);
				TaskInfoEvent.TrialNumber = Regex.Unescape(dataArray[1]);
				TaskInfoEvent.TimeLeftVal = Regex.Unescape(dataArray[2]);
				TaskInfoEvent.TaskMessage = Regex.Unescape(dataArray[3]);

				TaskInfoEventList.EventList.Add(TaskInfoEvent);

				this.eventLists.Add(TaskInfoEventList);

				return true;
			}

			return false;
		}



		public static string GetDefinitionLine(string teamNameText, string trialNumberText, string timeLeftValText, string taskMessageText)
		{
			string definitionLine = "0.0," + TrialPlaybackCommon.DataType1TaskInfo; // Elapsed time is dummy.

			definitionLine += "\t" + Regex.Escape(teamNameText) + "\t"+Regex.Escape(trialNumberText) + "\t" + Regex.Escape(timeLeftValText) + "\t" + Regex.Escape(taskMessageText);

			return definitionLine;
		}
	}
}

