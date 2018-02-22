using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Competition
{
	public interface IGiveUpHandler : IEventSystemHandler
	{
		void OnGiveUp();
	}

	public class PanelGiveUpController : MonoBehaviour
	{
		public GameObject giveUpPanel;

		public List<string> giveUpDestinationTags;


		private List<GameObject> giveUpDestinations;


		void Awake()
		{
			this.giveUpDestinations = new List<GameObject>();

			foreach (string giveUpDestinationTag in this.giveUpDestinationTags)
			{
				GameObject[] giveUpDestinationArray = GameObject.FindGameObjectsWithTag(giveUpDestinationTag);

				foreach(GameObject giveUpDestination in giveUpDestinationArray)
				{
					this.giveUpDestinations.Add(giveUpDestination);
				}
			}
		}

		void Start()
		{
			this.giveUpPanel.SetActive(false);
		}


		public void OnGiveUpYesButtonClick()
		{
			foreach(GameObject giveUpDestination in this.giveUpDestinations)
			{
				ExecuteEvents.Execute<IGiveUpHandler>
				(
					target: giveUpDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnGiveUp()
				);
			}
		}

		public void OnGiveUpNoButtonClick()
		{
			if (this.giveUpPanel.activeSelf)
			{
				this.giveUpPanel.SetActive(false);
			}
		}
	}
}

