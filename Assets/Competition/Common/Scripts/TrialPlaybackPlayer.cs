using SIGVerse.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (TrialPlaybackCommon))]
	public class TrialPlaybackPlayer : WorldPlaybackPlayer
	{
		private const string ElapsedTimeFormat = "###0.0";
		private const string TotalTimeFormat   = "###0";

		[HeaderAttribute("GUI")]
		public GameObject mainPanel;
		public GameObject scorePanel;
		public GameObject playbackPanel;

		[HeaderAttribute("Resources")]
		public GameObject collisionEffect;

		public Sprite playSprite;
		public Sprite pauseSprite;

		//--------------------------------------------------------------------------------------------

		protected Step preStep = (Step)0;

		protected int  trialNo = 0;
		protected int  totalTimeInt = 0;

		protected bool isStepChanged = true;
		protected bool isFileRead = false;

		protected PlaybackScoreEventController        scoreController;         // Score
		protected PlaybackHsrCollisionEventController hsrCollisionController;  // HSR Collision
		protected PlaybackPanelNoticeEventController  panelNoticeController;   // Notice of a Panel

		//----------------------------

		protected PanelMainController mainPanelController;

		protected GameObject mainMenu;

		protected Button giveUpButton;

		protected Text scoreText;
		protected Text totalText;

		protected Text statusText;
		protected InputField trialNoInputField;
		protected Button readFileButton;

		protected Text elapsedTimeText;
		protected Text totalTimeText;

		protected Slider timeSlider;

		protected Button playButton;

		protected Dropdown speedDropdown;
		protected Toggle   repeatToggle;

		protected InputField startTimeInputField;
		protected InputField endTimeInputField;


		protected override void Awake()
		{
			base.Awake();

			if(this.isPlay)
			{
				this.mainMenu = GameObject.FindGameObjectWithTag("MainMenu");

				this.mainPanelController = this.mainMenu.GetComponentInChildren<PanelMainController>();

				this.mainMenu.GetComponentInChildren<PanelMainController>().enabled = false;

				this.giveUpButton = this.mainPanel.transform.Find("TargetsOfHiding/Buttons/GiveUpButton").GetComponent<Button>();
				this.giveUpButton.interactable = false;

				this.scoreText = this.scorePanel.transform.Find("ScoreValText").GetComponent<Text>();
				this.totalText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();

				this.statusText          = this.playbackPanel.transform.Find("StatusText")                 .GetComponent<Text>();
				this.trialNoInputField   = this.playbackPanel.transform.Find("ReadFile/TrialNoInputField") .GetComponent<InputField>();
				this.readFileButton      = this.playbackPanel.transform.Find("ReadFile/ReadFileButton")    .GetComponent<Button>();
				this.elapsedTimeText     = this.playbackPanel.transform.Find("ElapsedTime/ElapsedTimeText").GetComponent<Text>();
				this.totalTimeText       = this.playbackPanel.transform.Find("TotalTime/TotalTimeText")    .GetComponent<Text>();
				this.timeSlider          = this.playbackPanel.transform.Find("TimeSlider")                 .GetComponent<Slider>();
				this.playButton          = this.playbackPanel.transform.Find("PlayButton")                 .GetComponent<Button>();
				this.speedDropdown       = this.playbackPanel.transform.Find("Speed/SpeedDropdown")        .GetComponent<Dropdown>();
				this.repeatToggle        = this.playbackPanel.transform.Find("Repeat/RepeatToggle")        .GetComponent<Toggle>();
				this.startTimeInputField = this.playbackPanel.transform.Find("StartTimeInputField")        .GetComponent<InputField>();
				this.endTimeInputField   = this.playbackPanel.transform.Find("EndTimeInputField")          .GetComponent<InputField>();
			}
			else
			{
				this.playbackPanel.SetActive(false);
			}
		}
		

		// Use this for initialization
		protected override void Start()
		{
			base.Start();

			this.scoreController        = new PlaybackScoreEventController(this.scoreText, this.totalText); // Score
			this.hsrCollisionController = new PlaybackHsrCollisionEventController(this.collisionEffect);    // HSR Collision
			this.panelNoticeController  = new PlaybackPanelNoticeEventController(this, this.mainMenu);      // Notice of a Panel
		}


		protected override void Update()
		{
			this.isStepChanged = base.step!=this.preStep;

			this.isFileRead = base.step==Step.Waiting && this.preStep==Step.Initializing;

			this.preStep = base.step;

			base.Update();
		}


		protected override void ReadData(string[] headerArray, string dataStr)
		{
			base.ReadData(headerArray, dataStr);

			this.scoreController       .ReadEvents(headerArray, dataStr); // Score
			this.hsrCollisionController.ReadEvents(headerArray, dataStr); // HSR Collision
			this.panelNoticeController .ReadEvents(headerArray, dataStr); // Notice of a Panel
		}

		protected override void StartInitializing()
		{
			// Pause the video players at first when playing by TrialPlaybackPlayer
			List<VideoPlayer> targetVideoPlayers = base.videoPlayerController.GetTargetVideoPlayers();

			foreach (VideoPlayer videoPlayers in targetVideoPlayers)
			{
				videoPlayers.Pause();
			}

			base.StartInitializing();

			this.scoreController       .StartInitializingEvents(); // Score
			this.hsrCollisionController.StartInitializingEvents(); // HSR Collision
			this.panelNoticeController .StartInitializingEvents(); // Notice of a Panel
		}

		protected override void UpdateIndexAndElapsedTime(float elapsedTime)
		{
			base.UpdateIndexAndElapsedTime(elapsedTime);

			this.scoreController       .UpdateIndex(elapsedTime); // Score
			this.hsrCollisionController.UpdateIndex(elapsedTime); // HSR Collision
			this.panelNoticeController .UpdateIndex(elapsedTime); // Notice of a Panel
		}


		protected override void UpdateData()
		{
			base.UpdateData();

			this.scoreController       .ExecutePassedLatestEvents(this.elapsedTime, this.deltaTime); // Score
			this.hsrCollisionController.ExecutePassedAllEvents(this.elapsedTime, this.deltaTime); // HSR Collision
			this.panelNoticeController .ExecutePassedAllEvents(this.elapsedTime, this.deltaTime); // Notice of a Panel
		}

		protected override void UpdateDataByLatest(float elapsedTime)
		{
			base.UpdateDataByLatest(elapsedTime);

			this.scoreController.ExecuteLatestEvents(); // Score
		}

		protected override float GetTotalTime()
		{
			return Mathf.Max(base.GetTotalTime(), this.scoreController.GetTotalTime(), this.hsrCollisionController.GetTotalTime(), this.panelNoticeController.GetTotalTime());
		}


		//----------------------------   GUI related codes are below   ---------------------------------------------


		void OnGUI()
		{
			// Update a text of status
			if(base.errorMsg != string.Empty)
			{
				this.statusText.text = base.errorMsg;
				this.SetTextColorAlpha(this.statusText, 1.0f);

				return;
			}

			switch(base.step)
			{
				case Step.Waiting:
				{
					if(this.isStepChanged)
					{
						Debug.Log("Waiting");

						if(base.isInitialized)
						{
							this.trialNoInputField  .interactable = true;
							this.readFileButton     .interactable = true;
							this.timeSlider         .interactable = true;
							this.playButton         .interactable = true;
							this.speedDropdown      .interactable = true;
							this.repeatToggle       .interactable = true;
							this.startTimeInputField.interactable = true;
							this.endTimeInputField  .interactable = true;
						}

						if(this.isFileRead)
						{
							this.mainPanelController.SetChallengeInfoText(this.trialNo);

							this.mainPanelController.SetTimeLeft(PanelMainController.TimeLimit);

							this.SetTextColorAlpha(this.statusText, 0.0f);

							this.totalTimeText.text = base.elapsedTime.ToString(ElapsedTimeFormat);

							this.totalTimeText.text = Math.Ceiling(this.GetTotalTime()).ToString(TotalTimeFormat);
							this.totalTimeInt   = int.Parse(this.totalTimeText.text);

							this.ResetTimeSlider();
							this.SetStartTime(0);
							this.SetEndTime(this.totalTimeInt);
						
							base.UpdateDataByLatest(0);

							this.isFileRead = false;
						}

						this.SetTextColorAlpha(this.statusText, 0.0f);

						this.playButton.image.sprite= this.playSprite;

						this.isStepChanged = false;
					}

					this.UpdateTimeDisplay();

					break;
				}
				case Step.Initializing:
				{
					if(this.isStepChanged)
					{
						SIGVerseLogger.Info("Initializing");

						this.statusText.text = "Reading...";

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);
					break;
				}
				case Step.Playing:
				{
					if(this.isStepChanged)
					{
						SIGVerseLogger.Info("Playing");

						this.statusText.text = "Playing...";

						this.playButton.image.sprite= this.pauseSprite;

						this.trialNoInputField  .interactable = false;
						this.readFileButton     .interactable = false;
						this.timeSlider         .interactable = false;
						this.speedDropdown      .interactable = false;
						this.repeatToggle       .interactable = false;
						this.startTimeInputField.interactable = false;
						this.endTimeInputField  .interactable = false;

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);

					this.UpdateTimeDisplay();

					this.timeSlider.value = Mathf.Clamp((base.elapsedTime-base.startTime)/(base.endTime-base.startTime), 0.0f, 1.0f);

					break;
				}
			}
		}

		private float GetElapsedTimeUsingSlider()
		{
			return base.startTime + (base.endTime - base.startTime) * this.timeSlider.value;
		}

		private void SetTextColorAlpha(Text text, float alpha)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
		}

		private void ResetTimeSlider()
		{
			this.timeSlider.value = 0.0f;
		}

		private void UpdateTimeDisplay()
		{
			float time = (base.elapsedTime < base.endTime)? base.elapsedTime : base.endTime; 

			this.elapsedTimeText.text = time.ToString(ElapsedTimeFormat);

			this.mainPanelController.SetTimeLeft(PanelMainController.TimeLimit - time);
		}

		private void SetStartTime(int startTime)
		{
			base.startTime                = startTime;
			this.startTimeInputField.text = startTime.ToString();
		}

		private void SetEndTime(int endTime)
		{
			base.endTime                = endTime;
			this.endTimeInputField.text = endTime.ToString();
		}



		public virtual void OnReadFileButtonClick()
		{
			base.Initialize(this.filePath);
		}

		public virtual void OnPlayButtonClick()
		{
			if (base.step == Step.Waiting && base.isInitialized)
			{
				switch(this.speedDropdown.value)
				{
					case 0: { base.playingSpeed = 1.0f; break; }
					case 1: { base.playingSpeed = 2.0f; break; }
					case 2: { base.playingSpeed = 4.0f; break; }
					case 3: { base.playingSpeed = 8.0f; break; }
				}

				base.isRepeating = this.repeatToggle.isOn;

				if (!base.Play(this.GetElapsedTimeUsingSlider())){ SIGVerseLogger.Warn("Cannot start the world playing");}
			}
			else if(base.step==Step.Playing)
			{ 
				if(!base.Stop()){ SIGVerseLogger.Warn("Cannot stop the world playing"); }
			}
		}

		public virtual void OnStartTimeEndEdit()
		{
			if(this.startTimeInputField.text!=string.Empty)
			{
				int startTimeInt = int.Parse(this.startTimeInputField.text);

				if(startTimeInt < 0)
				{
					this.SetStartTime(0);
				}
				else if(startTimeInt >= base.endTime)
				{
					this.SetStartTime((int)Math.Floor(base.endTime)-1);
				}
				else
				{
					this.SetStartTime(startTimeInt);
				}
			}
			else
			{
				this.SetStartTime(0);
			}

			this.ResetTimeSlider();
			base.UpdateDataByLatest(base.startTime);
			this.UpdateTimeDisplay();
		}

		public virtual void OnEndTimeEndEdit()
		{
			if(this.endTimeInputField.text!=string.Empty)
			{
				int endTimeInt = int.Parse(this.endTimeInputField.text);

				if(endTimeInt <= base.startTime)
				{
					this.SetEndTime((int)Math.Ceiling(base.startTime)+1);
				}
				else if(endTimeInt > this.totalTimeInt)
				{
					this.SetEndTime(this.totalTimeInt);
				}
				else
				{
					this.SetEndTime(endTimeInt);
				}
			}
			else
			{
				this.SetEndTime(this.totalTimeInt);
			}

			this.ResetTimeSlider();
			base.UpdateDataByLatest(base.startTime);
			this.UpdateTimeDisplay();
		}

		public virtual void OnSliderChanged()
		{
			if(!this.timeSlider.interactable){ return; }

			if(base.step == Step.Waiting && base.isInitialized)
			{
				base.deltaTime = this.GetElapsedTimeUsingSlider() - base.elapsedTime;
			}
		}
	}
}

