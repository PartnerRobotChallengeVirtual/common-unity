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
		public GameObject scorePanel;

		protected ScoreStatus latestScoreStatus = new ScoreStatus();

		protected Text totalValText;


		protected override void Awake()
		{
			base.Awake();

			if(this.isRecord)
			{
				this.totalValText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();
			}
		}

		protected override List<string> GetDefinitionLines()
		{
			List<string> definitionLines = base.GetDefinitionLines();

			// Score (Initial status of score)
			string definitionLine = "0.0," + TrialPlaybackCommon.DataType1Score; // Elapsed time is dummy.

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

			base.dataLines.Add(CreateScoreDataLine(this.latestScoreStatus));

			base.StopRecording();
		}

		private string CreateScoreDataLine(ScoreStatus scoreStatus)
		{
			// Score 
			string dataLine = base.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1Score;

			dataLine += "\t" + scoreStatus.Subscore + "," + scoreStatus.Score + "," + scoreStatus.Total;

			return dataLine;
		}

		private string CreateHsrCollisionDataLine(Vector3 contactPoint)
		{
			// HSR Collision
			string dataLine = base.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1HsrCollision;

			dataLine += "\t" + contactPoint.x + "," + contactPoint.y + "," + contactPoint.z;

			return dataLine;
		}

		private string CreatePanelNoticeDataLine(PanelNoticeStatus panelNoticeStatus)
		{
			// Notice of a Panel
			string dataLine = base.GetHeaderElapsedTime() + "," + TrialPlaybackCommon.DataType1PanelNotice;

			dataLine += "\t" + 
				Regex.Escape(panelNoticeStatus.Message) + "\t" + 
				panelNoticeStatus.FontSize + "\t" + 
				panelNoticeStatus.Color.r + "\t" + panelNoticeStatus.Color.g + "\t" + panelNoticeStatus.Color.b + "\t" + panelNoticeStatus.Color.a + "\t" + 
				panelNoticeStatus.Duration;

			return dataLine;
		}

		public void OnScoreChange(ScoreStatus scoreStatus)
		{
			base.dataLines.Add(this.CreateScoreDataLine(scoreStatus));

			this.latestScoreStatus = scoreStatus;
		}

		public void OnHsrCollisionEnter(Vector3 contactPoint)
		{
			base.dataLines.Add(this.CreateHsrCollisionDataLine(contactPoint));
		}

		public void OnChange(PanelNoticeStatus panelNoticeStatus)
		{
			base.dataLines.Add(this.CreatePanelNoticeDataLine(panelNoticeStatus));
		}
	}
}
