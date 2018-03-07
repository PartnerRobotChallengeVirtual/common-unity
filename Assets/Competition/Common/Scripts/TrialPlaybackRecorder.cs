using SIGVerse.ToyotaHSR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (TrialPlaybackCommon))]
	public class TrialPlaybackRecorder : WorldPlaybackRecorder, IScoreHandler, IHSRCollisionHandler, IPanelNoticeHandler
	{
		public GameObject mainPanel;
		public GameObject scorePanel;

		protected ScoreStatus latestScoreStatus = new ScoreStatus();

		protected Text trialNumberText;
		protected Text timeLeftValText;
		protected Text taskMessageText;

		protected Text totalValText;


		protected override void Awake()
		{
			base.Awake();

			if(this.isRecord)
			{
				this.trialNumberText = this.mainPanel.transform.Find("TargetsOfHiding/TrialNumberText")             .GetComponent<Text>();
				this.timeLeftValText = this.mainPanel.transform.Find("TargetsOfHiding/TimeLeftInfo/TimeLeftValText").GetComponent<Text>();
				this.taskMessageText = this.mainPanel.transform.Find("TargetsOfHiding/TaskMessageText")             .GetComponent<Text>();

				this.totalValText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();
			}
		}

		protected override List<string> GetDefinitionLines()
		{
			List<string> definitionLines = base.GetDefinitionLines();

			string definitionLine;

			// Task Info
			definitionLine = "0.0," + TrialPlaybackCommon.DataType1TaskInfo; // Elapsed time is dummy.

			definitionLine += "\t"+Regex.Escape(this.trialNumberText.text) + "\t" + Regex.Escape(this.timeLeftValText.text) + "\t" + Regex.Escape(this.taskMessageText.text);

			definitionLines.Add(definitionLine);

			// Score (Initial status of score)
			definitionLine = "0.0," + TrialPlaybackCommon.DataType1Score; // Elapsed time is dummy.

			definitionLine += "\t0,0," + this.totalValText.text;

			definitionLines.Add(definitionLine);

			return definitionLines;
		}


		protected override void StopRecording()
		{
			// Add a line of latest total score
			if( this.latestScoreStatus.Score > 0)
			{
				this.latestScoreStatus.Total += this.latestScoreStatus.Score;
			}

			this.latestScoreStatus.Subscore = 0;

			this.dataLines.Add(CreateScoreDataLine(this.latestScoreStatus));

			base.StopRecording();
		}

		private string CreateScoreDataLine(ScoreStatus scoreStatus)
		{
			// Score 
			string dataLine = this.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1Score;

			dataLine += "\t" + scoreStatus.Subscore + "," + scoreStatus.Score + "," + scoreStatus.Total;

			return dataLine;
		}

		private string CreatePanelNoticeDataLine(PanelNoticeStatus panelNoticeStatus)
		{
			// Notice of a Panel
			string dataLine = this.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1PanelNotice;

			dataLine += "\t" + 
				Regex.Escape(panelNoticeStatus.Message) + "\t" + 
				panelNoticeStatus.FontSize + "\t" + 
				panelNoticeStatus.Color.r + "\t" + panelNoticeStatus.Color.g + "\t" + panelNoticeStatus.Color.b + "\t" + panelNoticeStatus.Color.a + "\t" + 
				panelNoticeStatus.Duration;

			return dataLine;
		}

		private string CreateHsrCollisionDataLine(Vector3 contactPoint)
		{
			// HSR Collision
			string dataLine = this.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1HsrCollision;

			dataLine += "\t" + contactPoint.x + "," + contactPoint.y + "," + contactPoint.z;

			return dataLine;
		}

		public void OnScoreChange(ScoreStatus scoreStatus)
		{
			this.dataLines.Add(this.CreateScoreDataLine(scoreStatus));

			this.latestScoreStatus = scoreStatus;
		}

		public void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus)
		{
			this.dataLines.Add(this.CreatePanelNoticeDataLine(panelNoticeStatus));
		}

		public void OnHsrCollisionEnter(Vector3 contactPoint)
		{
			this.dataLines.Add(this.CreateHsrCollisionDataLine(contactPoint));
		}
	}
}
