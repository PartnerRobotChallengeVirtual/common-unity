using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (TrialPlaybackCommon))]
	public class TrialPlaybackRecorder : WorldPlaybackRecorder
	{
		protected List<VideoPlayer> targetVideoPlayers;

		private TrialPlaybackCommon common;

		protected override void Start()
		{
			base.Start();

			// Video Player
			this.common = this.GetComponent<TrialPlaybackCommon>();

			this.targetVideoPlayers = this.common.GetTargetVideoPlayers();
		}

		protected override List<string> GetDefinitionLines()
		{
			List<string> definitionLines = base.GetDefinitionLines(); // Transforms

			if(this.common.IsRecordVideoPlayers())
			{
				// Video Player
				string definitionLine = "0.0," + TrialPlaybackCommon.DataType1VideoPlayer + "," + TrialPlaybackCommon.DataType2VideoPlayerDef; // Elapsed time is dummy.

				foreach (VideoPlayer targetVideoPlayer in this.targetVideoPlayers)
				{
					// Make a header line
					definitionLine += "\t" + WorldPlaybackCommon.GetLinkPath(targetVideoPlayer.transform);
				}

				definitionLines.Add(definitionLine);
			}

			return definitionLines;
		}

		protected override void SaveData()
		{
			if (1000.0 * (this.elapsedTime - this.previousRecordedTime) < recordInterval) { return; }

			this.SaveTransforms();   // Transform
			this.SaveVideoPlayers(); // Video Player

			this.previousRecordedTime = this.elapsedTime;
		}

		protected virtual void SaveVideoPlayers()
		{
			if(!this.common.IsRecordVideoPlayers()){ return; }

			string dataLine = string.Empty;

			// Video Player
			dataLine += Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero) + "," + TrialPlaybackCommon.DataType1VideoPlayer + "," + TrialPlaybackCommon.DataType2VideoPlayerVal;

			foreach (VideoPlayer targetVideoPlayer in this.targetVideoPlayers)
			{
				dataLine += "\t" + targetVideoPlayer.frame;
			}

			this.dataLines.Add(dataLine);
		}
	}
}
