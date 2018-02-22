using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SIGVerse.Competition
{
	public interface IPanelScoreHandler : IEventSystemHandler
	{
		void OnScoreChange(float score);
		void OnScoreChange(float score, float total);
	}
	
	public class PanelScoreController : MonoBehaviour, IPanelScoreHandler
	{
		public GameObject scorePanel;

		private Text scoreValText;
		private Text totalValText;


		void Awake()
		{
			this.scoreValText = this.scorePanel.transform.Find("ScoreValText").GetComponent<Text>();
			this.totalValText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();
		}

		public void OnScoreChange(float score)
		{
			this.scoreValText.text = score.ToString();
		}

		public void OnScoreChange(float score, float total)
		{
			this.scoreValText.text = score.ToString();
			this.totalValText.text = total.ToString();
		}
	}
}

