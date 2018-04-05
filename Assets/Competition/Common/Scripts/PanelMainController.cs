using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SIGVerse.Competition
{
	public interface ITimeIsUpHandler : IEventSystemHandler
	{
		void OnTimeIsUp();
	}

	public class PanelMainController : MonoBehaviour
	{
		private const string TimeFormat = "#####0";
		private const string TagNameAudienceCamera = "AudienceCamera";

		public GameObject mainPanel;
		public GameObject giveUpPanel;

		// ---------------------------------------

		private Text trialNumberText;
		private Text timeLeftValText;
		private Text taskMessageText;

		private List<Camera> audienceCameras;
		private int mainAudienceCameraNo;


		void Awake()
		{
			this.trialNumberText = this.mainPanel.transform.Find("TargetsOfHiding/TrialNumberText")             .GetComponent<Text>();
			this.timeLeftValText = this.mainPanel.transform.Find("TargetsOfHiding/TimeLeftInfo/TimeLeftValText").GetComponent<Text>();
			this.taskMessageText = this.mainPanel.transform.Find("TargetsOfHiding/TaskMessageText")             .GetComponent<Text>();


			List<GameObject> audienceCameraObjs = GameObject.FindGameObjectsWithTag(TagNameAudienceCamera).ToList();

			this.audienceCameras = new List<Camera>();

			foreach(GameObject audienceCameraObj in audienceCameraObjs)
			{
				this.audienceCameras.Add(audienceCameraObj.GetComponent<Camera>());
			}

			// Sort
			this.audienceCameras = this.audienceCameras.OrderByDescending(camera => camera.depth).ToList();
		}

		// Use this for initialization
		void Start()
		{
			this.mainAudienceCameraNo = 0;

			this.UpdateAudienceCameraDepth();
		}


		public void SetTimeLeft(float timeLeft)
		{
			this.timeLeftValText.text = timeLeft.ToString(TimeFormat);
		}

		public void SetTrialNumberText(int numberOfTrials)
		{
			string ordinal;

			if (numberOfTrials == 11 || numberOfTrials == 12 || numberOfTrials == 13)
			{
				ordinal = "th";
			}
			else
			{
				if (numberOfTrials % 10 == 1)
				{
					ordinal = "st";
				}
				else if (numberOfTrials % 10 == 2)
				{
					ordinal = "nd";
				}
				else if (numberOfTrials % 10 == 3)
				{
					ordinal = "rd";
				}
				else
				{
					ordinal = "th";
				}
			}

			this.trialNumberText.text = numberOfTrials + ordinal + " challenge";
		}

		public string GetTrialNumberText()
		{
			return this.trialNumberText.text;
		}

		public void SetTaskMessageText(string taskMessage)
		{
			this.taskMessageText.text = taskMessage;
		}


		public void OnCameraButtonClick()
		{
			this.mainAudienceCameraNo++;

			if(mainAudienceCameraNo >= audienceCameras.Count)
			{
				mainAudienceCameraNo = 0;
			}

			this.UpdateAudienceCameraDepth();
		}

		private void UpdateAudienceCameraDepth()
		{
			for (int i = 0; i < this.audienceCameras.Count; i++)
			{
				if (i == mainAudienceCameraNo)
				{
					this.audienceCameras[i].depth = 10;
				}
				else
				{
					this.audienceCameras[i].depth = 0;
				}
			}
		}


		public void OnGiveUpButtonClick()
		{
			if (this.giveUpPanel.activeSelf)
			{
				this.giveUpPanel.SetActive(false);
			}
			else
			{
				this.giveUpPanel.SetActive(true);
			}
		}
	}
}

