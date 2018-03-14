using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using SIGVerse.Competition;

namespace SIGVerse.Competition
{
	public class PlaybackPanelNoticeEvent : PlaybackEventBase
	{
		public PanelNoticeStatus PanelNoticeStatus { get; set; }
		public GameObject Destination{ get; set; }

		public void Execute()
		{
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.Destination, 
				eventData: null, 
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(PanelNoticeStatus)
			);
		}
	}


	public class PlaybackPanelNoticeEventList : PlaybackEventListBase<PlaybackPanelNoticeEvent>
	{
		public PlaybackPanelNoticeEventList()
		{
			this.EventList = new List<PlaybackPanelNoticeEvent>();
		}

		public PlaybackPanelNoticeEventList(PlaybackPanelNoticeEventList panelNoticeEventList)
		{
			this.ElapsedTime = panelNoticeEventList.ElapsedTime;
			this.EventList   = new List<PlaybackPanelNoticeEvent>();

			foreach(PlaybackPanelNoticeEvent panelNoticeEventOrg in panelNoticeEventList.EventList)
			{
				PanelNoticeStatus panelNoticeStatus  = new PanelNoticeStatus(panelNoticeEventOrg.PanelNoticeStatus);
				GameObject        destination        = panelNoticeEventOrg.Destination;

				PlaybackPanelNoticeEvent panelNoticeEvent = new PlaybackPanelNoticeEvent();
				panelNoticeEvent.PanelNoticeStatus = panelNoticeStatus;
				panelNoticeEvent.Destination       = destination;

				this.EventList.Add(panelNoticeEvent);
			}
		}
	}

	// ------------------------------------------------------------------

	public class PlaybackPanelNoticeEventController : PlaybackEventControllerBase<PlaybackPanelNoticeEventList, PlaybackPanelNoticeEvent>
	{
		private TrialPlaybackPlayer playbackPlayer;
		private GameObject destination;

		public PlaybackPanelNoticeEventController(TrialPlaybackPlayer playbackPlayer, GameObject destination)
		{
			this.playbackPlayer = playbackPlayer;
			this.destination    = destination;
		}

		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackPanelNoticeEventList>();
		}

		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// Notice of a Panel
			if (headerArray[1] == TrialPlaybackCommon.DataType1PanelNotice)
			{
				PlaybackPanelNoticeEvent panelNotice = new PlaybackPanelNoticeEvent();

				string[] dataArray = dataStr.Split('\t');

				string message  = Regex.Unescape(dataArray[0]);
				int    fontSize = int.Parse(dataArray[1]);
				Color  color    = new Color(float.Parse(dataArray[2]), float.Parse(dataArray[3]), float.Parse(dataArray[4]), float.Parse(dataArray[5]));
				float  duration = float.Parse(dataArray[6]);

				PanelNoticeStatus panelNoticeStatus = new PanelNoticeStatus(message, fontSize, color, duration);

				panelNotice.PanelNoticeStatus = panelNoticeStatus;
				panelNotice.Destination       = this.destination;

				PlaybackPanelNoticeEventList panelNoticeEventList = new PlaybackPanelNoticeEventList();
				panelNoticeEventList.ElapsedTime = float.Parse(headerArray[0]);
				panelNoticeEventList.EventList.Add(panelNotice);

				this.eventLists.Add(panelNoticeEventList);

				return true;
			}

			return false;
		}


		protected override PlaybackPanelNoticeEventList ModifyExecutionEventList(PlaybackPanelNoticeEventList executionEventList)
		{
			PlaybackPanelNoticeEventList panelNoticeEventList = new PlaybackPanelNoticeEventList(executionEventList);

			foreach(PlaybackPanelNoticeEvent panelNoticeEvent in panelNoticeEventList.EventList)
			{
				if(this.playbackPlayer.GetStep()==WorldPlaybackPlayer.Step.Playing)
				{
					panelNoticeEvent.PanelNoticeStatus.Duration /= this.playbackPlayer.GetPlayingSpeed();
				}
				else
				{
					panelNoticeEvent.PanelNoticeStatus.Duration = 0.5f; // Fixed time if moving manually.
				}
			}

			return panelNoticeEventList;
		}



		public static string GetDataLine(string elapsedTime, PanelNoticeStatus panelNoticeStatus)
		{
			string dataLine = elapsedTime + "," + TrialPlaybackCommon.DataType1PanelNotice;

			dataLine += "\t" + 
				Regex.Escape(panelNoticeStatus.Message) + "\t" + 
				panelNoticeStatus.FontSize + "\t" + 
				panelNoticeStatus.Color.r + "\t" + panelNoticeStatus.Color.g + "\t" + panelNoticeStatus.Color.b + "\t" + panelNoticeStatus.Color.a + "\t" + 
				panelNoticeStatus.Duration;

			return dataLine;
		}
	}
}

