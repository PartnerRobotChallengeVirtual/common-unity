using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Competition
{
	public interface IScoreHandler : IEventSystemHandler
	{
		void OnScoreChange(ScoreStatus scoreStatus);
	}

	public class ScoreStatus
	{
		public int   Subscore    { get; set; }
		public int   Score       { get; set; }
		public int   Total       { get; set; }

		public ScoreStatus()
		{
		}

		public ScoreStatus(int subscore, int score, int total)
		{
			this.Subscore    = subscore;
			this.Score       = score;
			this.Total       = total;
		}
	}
}


