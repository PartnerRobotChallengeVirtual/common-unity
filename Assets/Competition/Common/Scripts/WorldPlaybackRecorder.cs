using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using SIGVerse.Common;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (WorldPlaybackCommon))]
	public class WorldPlaybackRecorder : MonoBehaviour
	{
		[TooltipAttribute("milliseconds")]
		public int recordInterval = 20;

		//-----------------------------------------------------
		private enum Step
		{
			Waiting,
			Initializing,
			Initialized, 
			Recording,
			Writing,
		}

		private Step step = Step.Waiting;

		private float elapsedTime = 0.0f;
		private float previousRecordedTime = 0.0f;

		private string filePath;

		protected List<Transform> targetTransforms;

		private List<string> dataLines;


		// Update is called once per frame
		void Update()
		{
			this.elapsedTime += Time.deltaTime;

			if (this.step == Step.Recording)
			{
				this.SaveTransforms();
			}
		}

		public bool Initialize(int numberOfTrials)
		{
			if(this.step == Step.Waiting)
			{
				this.StartInitializing(numberOfTrials);
				return true;
			}

			return false;
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

		protected virtual void StartInitializing(int numberOfTrials)
		{
			this.step = Step.Initializing;

			this.filePath = String.Format(Application.dataPath + WorldPlaybackCommon.filePathFormat, numberOfTrials);

			SIGVerseLogger.Info("Output Playback file Path=" + this.filePath);

			// File open
			StreamWriter streamWriter = new StreamWriter(this.filePath, false);

			// Write header line and get transform instances
			string definitionLine = string.Empty;

			definitionLine += "0.0," + WorldPlaybackCommon.DataType1Transform + "," + WorldPlaybackCommon.DataType2TransformDef; // Elapsed time is dummy.

			foreach (Transform targetTransform in this.targetTransforms)
			{
				// Make a header line
				definitionLine += "\t" + WorldPlaybackCommon.GetLinkPath(targetTransform);
			}

			streamWriter.WriteLine(definitionLine);

			streamWriter.Flush();
			streamWriter.Close();

			this.dataLines = new List<string>();

			this.step = Step.Initialized;
		}


		protected virtual void StartRecording()
		{
			SIGVerseLogger.Info("Start the world playback recording");

			this.step = Step.Recording;

			// Reset elapsed time
			this.elapsedTime = 0.0f;
			this.previousRecordedTime = 0.0f;
		}

		protected virtual void StopRecording()
		{
			SIGVerseLogger.Info("Stop the world playback recording");

			this.step = Step.Writing;

			Thread threadWriteData = new Thread(new ThreadStart(this.WriteDataToFile));
			threadWriteData.Start();
		}
		
		protected virtual void WriteDataToFile()
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(this.filePath, true);

				foreach (string dataLine in dataLines)
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

		protected virtual void SaveTransforms()
		{
			if (1000.0 * (this.elapsedTime - this.previousRecordedTime) < recordInterval) { return; }

			string dataLine = string.Empty;

			dataLine += Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero) + "," + WorldPlaybackCommon.DataType1Transform + "," + WorldPlaybackCommon.DataType2TransformVal;

			foreach (Transform transform in this.targetTransforms)
			{
				dataLine += "\t" +
					Math.Round(transform.position.x,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.position.y,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.position.z,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.eulerAngles.x, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.eulerAngles.y, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.eulerAngles.z, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.x,  4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.y,  4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.z,  4, MidpointRounding.AwayFromZero);
			}

			this.dataLines.Add(dataLine);

			this.previousRecordedTime = this.elapsedTime;
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


