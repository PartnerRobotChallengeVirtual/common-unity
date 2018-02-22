using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace SIGVerse.Competition
{
	public class PanelsController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[HeaderAttribute("Panels")]
		public GameObject mainPanel;

		public List<GameObject> otherPanels;
		//---------------------------------------------------
		private GameObject draggingPanel;

		private Image mainPanelImage;
		private GameObject targetsOfHiding;

		private bool isMainPanelVisible;

		private List<bool> isOtherPanelsVisible;


		void Awake()
		{
			this.mainPanelImage = this.mainPanel.GetComponent<Image>();

			this.targetsOfHiding = this.mainPanel.transform.Find("TargetsOfHiding").gameObject;
		}

		// Use this for initialization
		void Start()
		{
			this.isOtherPanelsVisible = new List<bool>();

			for(int i=0; i<this.otherPanels.Count; i++)
			{
				this.isOtherPanelsVisible.Add(false);
			}
		}

		// Update is called once per frame
		void Update()
		{
		}

		public void OnHiddingButtonClick()
		{
			if (this.mainPanelImage.enabled)
			{
				this.isMainPanelVisible = this.mainPanelImage.enabled;

				for(int i=0; i<this.otherPanels.Count; i++)
				{
					this.isOtherPanelsVisible[i] = this.otherPanels[i].activeSelf;
				}

				this.mainPanelImage .enabled = false;
				this.targetsOfHiding.SetActive(false);

				foreach(GameObject otherPanel in this.otherPanels)
				{
					otherPanel.SetActive(false);
				}
			}
			else
			{
				if (this.isMainPanelVisible)
				{
					this.mainPanelImage.enabled = true;
					this.targetsOfHiding.SetActive(true);
				}

				for(int i=0; i<this.otherPanels.Count; i++)
				{
					if(this.isOtherPanelsVisible[i])
					{
						this.otherPanels[i].SetActive(true);
					}
				}
			}
		}


		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.pointerEnter == null) { return; }

			Transform selectedObj = eventData.pointerEnter.transform;

			do
			{
				if (this.IsPanelSelected(selectedObj.gameObject))
				{
					this.draggingPanel = selectedObj.gameObject;
					break;
				}

				selectedObj = selectedObj.transform.parent;

			} while (selectedObj.transform.parent != null);
		}


		private bool IsPanelSelected(GameObject selectedObj)
		{
			if (selectedObj == this.mainPanel){ return true; }

			foreach(GameObject otherPanel in this.otherPanels)
			{
				if (selectedObj == otherPanel){ return true; }
			}

			return false;
		}


		public void OnDrag(PointerEventData eventData)
		{
			if (this.draggingPanel == null) { return; }

			this.draggingPanel.transform.position += (Vector3)eventData.delta;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this.draggingPanel = null;
		}
	}
}

