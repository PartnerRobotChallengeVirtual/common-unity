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

		public const int TimeLimit = 600;

		public GameObject mainPanel;
		public GameObject giveUpPanel;

		public List<string> timeIsUpDestinationTags;

		// ---------------------------------------

		private Text trialNumberText;
		private Text timeLeftValText;
		private Text taskMessageText;

		private List<GameObject> timeIsUpDestinations;

		private float timeLeft;
		
		private List<Camera> audienceCameras;
		private int mainAudienceCameraNo;


		void Awake()
		{
			this.trialNumberText = this.mainPanel.transform.Find("TargetsOfHiding/TrialNumberText")             .GetComponent<Text>();
			this.timeLeftValText = this.mainPanel.transform.Find("TargetsOfHiding/TimeLeftInfo/TimeLeftValText").GetComponent<Text>();
			this.taskMessageText = this.mainPanel.transform.Find("TargetsOfHiding/TaskMessageText")             .GetComponent<Text>();


			this.timeIsUpDestinations = new List<GameObject>();

			foreach (string timeIsUpDestinationTag in this.timeIsUpDestinationTags)
			{
				GameObject[] timeIsUpDestinationArray = GameObject.FindGameObjectsWithTag(timeIsUpDestinationTag);

				foreach(GameObject timeIsUpDestination in timeIsUpDestinationArray)
				{
					this.timeIsUpDestinations.Add(timeIsUpDestination);
				}
			}


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
			this.timeLeft = (float)TimeLimit;

			this.SetTimeLeft(this.timeLeft);

			this.mainAudienceCameraNo = 0;

			this.UpdateAudienceCameraDepth();
		}

		// Update is called once per frame
		void Update()
		{
			this.timeLeft = Mathf.Max(0.0f, this.timeLeft-Time.deltaTime);

			this.SetTimeLeft(this.timeLeft);

			if(this.timeLeft == 0.0f)
			{
				foreach(GameObject timeIsUpDestination in this.timeIsUpDestinations)
				{
					ExecuteEvents.Execute<ITimeIsUpHandler>
					(
						target: timeIsUpDestination,
						eventData: null,
						functor: (reciever, eventData) => reciever.OnTimeIsUp()
					);
				}
			}
		}

		public void SetTimeLeft(float timeLeft)
		{
			this.timeLeftValText.text = timeLeft.ToString(TimeFormat);
		}

		public void ResetTimeLeftText()
		{
			this.timeLeft = (float)TimeLimit;
			this.SetTimeLeft(this.timeLeft);
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

