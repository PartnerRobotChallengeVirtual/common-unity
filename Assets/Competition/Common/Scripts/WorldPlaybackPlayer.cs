using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Linq;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (WorldPlaybackCommon))]
	public class WorldPlaybackPlayer : MonoBehaviour
	{
		public enum Step
		{
			Waiting,
			Initializing,
			Playing,
		}


		protected bool isPlay = true;

		protected Step step = Step.Waiting;
		protected bool isInitialized = false;

		protected string errorMsg = string.Empty;

		protected float elapsedTime = 0.0f;
		protected float deltaTime   = 0.0f;

		protected float startTime = 0.0f;
		protected float endTime = 0.0f;

		protected float playingSpeed = 1.0f;
		protected bool  isRepeating = false;

		protected PlaybackTransformEventController   transformController;   // Transform
		protected PlaybackVideoPlayerEventController videoPlayerController; // Video Player

		protected string filePath;


		protected virtual void Awake()
		{
			if(!this.isPlay)
			{
				this.enabled = false;
			}
		}

		// Use this for initialization
		protected virtual void Start()
		{
			WorldPlaybackCommon common = this.GetComponent<WorldPlaybackCommon>();

			this.filePath = common.GetFilePath();

			this.transformController   = new PlaybackTransformEventController  (common);  // Transform
			this.videoPlayerController = new PlaybackVideoPlayerEventController(common);  // Video Player
		}

		// Update is called once per frame
		protected virtual void Update()
		{
			if (this.step == Step.Playing)
			{
				this.Update(Time.deltaTime * this.playingSpeed);
			}
			else if (this.step == Step.Waiting && this.isInitialized)
			{
				this.Update(this.deltaTime);

				this.deltaTime = 0.0f;
			}
		}


		private void Update(float deltaTime)
		{
			this.deltaTime = deltaTime;

			this.elapsedTime += this.deltaTime;

			this.UpdateData();
		}


		public bool Initialize(string filePath)
		{
			if(this.step == Step.Waiting)
			{
				this.step = Step.Initializing;

				this.filePath = filePath;

				this.errorMsg = string.Empty;

				this.StartInitializing();

				Thread threadReadData = new Thread(new ThreadStart(this.ReadDataFromFile));
				threadReadData.Start();

				return true;
			}

			return false;
		}

		public bool Initialize()
		{
			return this.Initialize(this.filePath);
		}


		public bool Play(float startTime)
		{
			if (this.step == Step.Waiting && this.isInitialized)
			{
				this.StartPlaying(startTime);
				return true;
			}

			return false;
		}

		public bool Play()
		{
			return this.Play(0.0f);
		}


		public bool Stop()
		{
			if (this.step == Step.Playing)
			{
				this.StopPlaying();
				return true;
			}

			return false;
		}


		protected virtual void StartInitializing()
		{
			this.transformController  .StartInitializingEvents(); // Transform
			this.videoPlayerController.StartInitializingEvents(); // Video Player
		}


		protected virtual void ReadDataFromFile()
		{
			try
			{
				if (!File.Exists(this.filePath))
				{
					throw new Exception("Playback file NOT found. Path=" + this.filePath);
				}

				// File open
				StreamReader streamReader = new StreamReader(this.filePath);

				while (streamReader.Peek() >= 0)
				{
					string lineStr = streamReader.ReadLine();

					string[] columnArray = lineStr.Split(new char[]{'\t'}, 2);

					if (columnArray.Length < 2) { continue; }

					string headerStr = columnArray[0];
					string dataStr   = columnArray[1];

					string[] headerArray = headerStr.Split(',');

					this.ReadData(headerArray, dataStr);
				}

				streamReader.Close();

				SIGVerseLogger.Info("Playback player : File reading finished.");

				this.endTime = this.GetTotalTime();

				SIGVerseLogger.Info("Playback player : Total time=" + this.endTime);

				this.isInitialized = true;

				this.step = Step.Waiting;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);

				this.errorMsg = "Cannot read the file !";
				this.step = Step.Waiting;
			}
		}


		protected virtual void ReadData(string[] headerArray, string dataStr)
		{
			this.transformController  .ReadEvents(headerArray, dataStr); // Transform
			this.videoPlayerController.ReadEvents(headerArray, dataStr); // Video Player
		}


		protected virtual void StartPlaying(float startTime)
		{
			SIGVerseLogger.Info("( Start the world playback playing from "+startTime+"[s] )");

			this.step = Step.Playing;

			this.UpdateIndexAndElapsedTime(startTime);
		}


		protected virtual void UpdateIndexAndElapsedTime(float elapsedTime)
		{
			this.elapsedTime = elapsedTime;

			this.deltaTime = 0.0f;

			this.transformController  .UpdateIndex(elapsedTime); // Transform
			this.videoPlayerController.UpdateIndex(elapsedTime); // Video Player
		}


		protected virtual void StopPlaying()
		{
			SIGVerseLogger.Info("( Stop the world playback playing )");

			this.step = Step.Waiting;
		}


		protected virtual void UpdateData()
		{
			if (this.elapsedTime > this.endTime)
			{
				if(this.isRepeating)
				{
					// Wait 10 seconds until the next start
					if(this.elapsedTime > this.endTime + 10.0f)
					{
						this.UpdateDataByLatest(this.startTime);
					}
				}
				else
				{
					this.Stop();
				}
				return;
			}

			this.transformController  .ExecutePassedLatestEvents(this.elapsedTime, this.deltaTime); // Transform
			this.videoPlayerController.ExecutePassedLatestEvents(this.elapsedTime, this.deltaTime); // Video Player
		}


		protected virtual void UpdateDataByLatest(float elapsedTime)
		{
			this.UpdateIndexAndElapsedTime(elapsedTime);

			this.transformController  .ExecuteLatestEvents(); // Transforms
			this.videoPlayerController.ExecuteLatestEvents(); // Video Players
		}

		protected virtual float GetTotalTime()
		{
			return Mathf.Max(this.transformController.GetTotalTime(), this.videoPlayerController.GetTotalTime());
		}

		protected float GetMax(float x, float y)
		{
			return 1.0f;
		}


		public Step GetStep()
		{
			return this.step;
		}

		public float GetPlayingSpeed()
		{
			return this.playingSpeed;
		}
	}
}

