using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using SIGVerse.Common;
using UnityEngine.Video;
using System.Text;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (WorldPlaybackCommon))]
	public class WorldPlaybackRecorder : MonoBehaviour
	{
		[TooltipAttribute("milliseconds")]
		public int recordInterval = 20;

		//-----------------------------------------------------
		protected enum Step
		{
			Waiting,
			Initializing,
			Initialized, 
			Recording,
			Writing,
		}

		protected bool isRecord = true;

		protected Step step = Step.Waiting;

		protected float elapsedTime = 0.0f;
		protected float previousRecordedTime = 0.0f;

		protected List<Transform>   targetTransforms;
		protected List<VideoPlayer> targetVideoPlayers;

		private List<string> dataLines;

		protected string filePath;

		protected bool isReplayVideoPlayers;


		protected virtual void Awake()
		{
			if(!this.isRecord)
			{
				this.enabled = false;
			}
		}

		// Use this for initialization
		protected virtual void Start()
		{
			WorldPlaybackCommon common = this.GetComponent<WorldPlaybackCommon>();

			this.filePath = common.GetFilePath();

			this.isReplayVideoPlayers = common.IsReplayVideoPlayers();

			this.targetTransforms   = common.GetTargetTransforms();   // Transform
			this.targetVideoPlayers = common.GetTargetVideoPlayers(); // Video Player
		}

		// Update is called once per frame
		protected virtual void Update()
		{
			if (this.step == Step.Recording)
			{
				this.elapsedTime += Time.deltaTime;

				if (1000.0 * (this.elapsedTime - this.previousRecordedTime) < this.recordInterval) { return; }

				this.SaveData();

				this.previousRecordedTime = this.elapsedTime;
			}
		}

		public bool Initialize(string filePath)
		{
			if(this.step == Step.Waiting)
			{
				this.filePath = filePath;

				this.StartInitializing();
				return true;
			}

			return false;
		}

		public bool Initialize()
		{
			return this.Initialize(this.filePath);
		}


		public bool Record()
		{
			if (this.step == Step.Initialized)
			{
				this.StartRecording();
				return true;
			}

			return false;
		}

		public bool Stop()
		{
			if (this.step == Step.Recording)
			{
				this.StopRecording();
				return true;
			}

			return false;
		}

		protected virtual void StartInitializing()
		{
			this.step = Step.Initializing;

			SIGVerseLogger.Info("Output Playback file Path=" + this.filePath);

			// File open
			StreamWriter streamWriter = new StreamWriter(this.filePath, false);

			List<string> definitionLines = this.GetDefinitionLines();

			foreach(string definitionLine in definitionLines)
			{
				streamWriter.WriteLine(definitionLine);
			}

			streamWriter.Flush();
			streamWriter.Close();

			this.dataLines = new List<string>();

			this.step = Step.Initialized;
		}

		public virtual void AddDataLine(string dataLine)
		{
			if (this.step != Step.Recording)
			{
				SIGVerseLogger.Warn("Illegal timing to add dataLine. data="+dataLine);
				return;
			}
			
			this.dataLines.Add(dataLine);
		}

		protected virtual List<string> GetDefinitionLines()
		{
			List<string> definitionLines = new List<string>();

			// Transform
			definitionLines.Add(PlaybackTransformEventController.GetDefinitionLine(this.targetTransforms));

			// Video Player
			if(this.isReplayVideoPlayers)
			{
				definitionLines.Add(PlaybackVideoPlayerEventController.GetDefinitionLine(this.targetVideoPlayers));
			}

			return definitionLines;
		}


		protected virtual void StartRecording()
		{
			SIGVerseLogger.Info("( Start the world playback recording )");

			this.step = Step.Recording;

			// Reset elapsed time
			this.elapsedTime = 0.0f;
			this.previousRecordedTime = 0.0f;
		}

		protected virtual void StopRecording()
		{
			SIGVerseLogger.Info("( Stop the world playback recording )");

			this.step = Step.Writing;

			Thread threadWriteData = new Thread(new ThreadStart(this.WriteDataToFile));
			threadWriteData.Start();
		}
		
		protected virtual void WriteDataToFile()
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(this.filePath, true);

				foreach (string dataLine in this.dataLines)
				{
					streamWriter.WriteLine(dataLine);
				}

				streamWriter.Flush();
				streamWriter.Close();

				this.step = Step.Waiting;
			}
			catch(Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
				Application.Quit();
			}
		}

		protected virtual void SaveData()
		{
			this.SaveTransforms();
			this.SaveVideoPlayers();
		}

		protected virtual string GetHeaderElapsedTime()
		{
			return Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero).ToString();
		}


		protected virtual void SaveTransforms()
		{
			this.AddDataLine(PlaybackTransformEventController.GetDataLine(this.GetHeaderElapsedTime(), this.targetTransforms));
		}

		protected virtual void SaveVideoPlayers()
		{
			if (!this.isReplayVideoPlayers) { return; }

			this.AddDataLine(PlaybackVideoPlayerEventController.GetDataLine(this.GetHeaderElapsedTime(), this.targetVideoPlayers));
		}


		public bool IsInitialized()
		{
			return this.step == Step.Initialized;
		}

		public bool IsFinished()
		{
			return this.step == Step.Waiting;
		}
	}
}


