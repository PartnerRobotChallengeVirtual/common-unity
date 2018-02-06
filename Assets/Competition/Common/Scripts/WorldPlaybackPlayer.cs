using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.Common;

namespace SIGVerse.Competition
{
	[RequireComponent(typeof (WorldPlaybackCommon))]
	public class WorldPlaybackPlayer : MonoBehaviour
	{
		protected class UpdatingTransformData
		{
			public Transform UpdatingTransform { get; set; }
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
			public Vector3 Scale    { get; set; }

			public void UpdateTransform()
			{
				this.UpdatingTransform.position    = this.Position;
				this.UpdatingTransform.eulerAngles = this.Rotation;
				this.UpdatingTransform.localScale  = this.Scale;
			}
		}

		protected class UpdatingTransformList
		{
			public float ElapsedTime { get; set; }
			private List<UpdatingTransformData> updatingTransformList;

			public UpdatingTransformList()
			{
				this.updatingTransformList = new List<UpdatingTransformData>();
			}

			public void AddUpdatingTransform(UpdatingTransformData updatingTransformData)
			{
				this.updatingTransformList.Add(updatingTransformData);
			}

			public List<UpdatingTransformData> GetUpdatingTransformList()
			{
				return this.updatingTransformList;
			}
		}

		//-----------------------------------------------------
		protected enum Step
		{
			Waiting,
			Initializing,
			Initialized, 
			Playing,
		}


		protected Step step = Step.Waiting;

		protected float elapsedTime = 0.0f;

		protected string filePath;

		protected List<Transform> targetTransforms;

		protected Dictionary<string, Transform> targetObjectsPathMap  = new Dictionary<string, Transform>();

		protected Queue<UpdatingTransformList> playingTransformQue;
		protected List<Transform> transformOrder = new List<Transform>();


		// Use this for initialization
		protected virtual void Start()
		{
			WorldPlaybackCommon common = this.GetComponent<WorldPlaybackCommon>();

			this.targetTransforms = common.GetTargetTransforms();

			foreach (Transform targetTransform in this.targetTransforms)
			{
				this.targetObjectsPathMap.Add(WorldPlaybackCommon.GetLinkPath(targetTransform), targetTransform);
			}

			this.playingTransformQue = new Queue<UpdatingTransformList>();
			this.transformOrder = new List<Transform>();
		}

		// Update is called once per frame
		protected virtual void Update()
		{
			this.elapsedTime += Time.deltaTime;

			if (this.step == Step.Playing)
			{
				this.UpdateData();
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

		public bool Play()
		{
			if (this.step == Step.Initialized)
			{
				this.StartPlaying();
				return true;
			}

			return false;
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
			this.step = Step.Initializing;

			this.StartInitializingTransforms();

			Thread threadWriteMotions = new Thread(new ParameterizedThreadStart(this.ReadDataFromFile));
			threadWriteMotions.Start(this.filePath);
		}

		protected virtual void StartInitializingTransforms()
		{
			// Disable Rigidbodies and colliders
			foreach (Transform targetTransform in this.targetTransforms)
			{
				// Disable rigidbodies
				Rigidbody[] rigidbodies = targetTransform.GetComponentsInChildren<Rigidbody>(true);

				foreach (Rigidbody rigidbody in rigidbodies)
				{
					rigidbody.isKinematic     = true;
					rigidbody.velocity        = Vector3.zero;
					rigidbody.angularVelocity = Vector3.zero;
				}

				// Disable colliders
				Collider[] colliders = targetTransform.GetComponentsInChildren<Collider>(true);

				foreach (Collider collider in colliders)
				{
					collider.enabled = false;
				}
			}
		}

		protected virtual void ReadDataFromFile(object args)
		{
			try
			{
				string filePath = (string)args;

				if (!File.Exists(filePath))
				{
					throw new Exception("Playback file NOT found. Path=" + filePath);
				}

				// File open
				StreamReader streamReader = new StreamReader(filePath);

				while (streamReader.Peek() >= 0)
				{
					string lineStr = streamReader.ReadLine();

					string[] columnArray = lineStr.Split(new char[] { '\t' }, 2);

					if (columnArray.Length < 2) { continue; }

					string headerStr = columnArray[0];
					string dataStr   = columnArray[1];

					string[] headerArray = headerStr.Split(',');

					this.ReadData(headerArray, dataStr);
				}

				streamReader.Close();

				SIGVerseLogger.Info("Playback player : File reading finished.");

				this.step = Step.Initialized;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
				Application.Quit();
			}
		}

		protected virtual void ReadData(string[] headerArray, string dataStr)
		{
			this.ReadTransforms(headerArray, dataStr);
		}

		protected virtual bool ReadTransforms(string[] headerArray, string dataStr)
		{
			// Motion data
			if (headerArray[1] == WorldPlaybackCommon.DataType1Transform)
			{
				string[] dataArray = dataStr.Split('\t');

				// Definition
				if (headerArray[2] == WorldPlaybackCommon.DataType2TransformDef)
				{
					this.transformOrder.Clear();

					SIGVerseLogger.Info("Playback player : transform data num=" + dataArray.Length);

					foreach (string transformPath in dataArray)
					{
						if (!this.targetObjectsPathMap.ContainsKey(transformPath))
						{
							SIGVerseLogger.Error("Couldn't find the object that path is " + transformPath);
						}

						this.transformOrder.Add(this.targetObjectsPathMap[transformPath]);
					}
				}
				// Value
				else if (headerArray[2] == WorldPlaybackCommon.DataType2TransformVal)
				{
					if (this.transformOrder.Count == 0) { return false; }

					UpdatingTransformList timeSeriesMotionsData = new UpdatingTransformList();

					timeSeriesMotionsData.ElapsedTime = float.Parse(headerArray[0]);

					for (int i = 0; i < dataArray.Length; i++)
					{
						string[] transformValues = dataArray[i].Split(',');

						UpdatingTransformData transformPlayer = new UpdatingTransformData();
						transformPlayer.UpdatingTransform = this.transformOrder[i];

						transformPlayer.Position = new Vector3(float.Parse(transformValues[0]), float.Parse(transformValues[1]), float.Parse(transformValues[2]));
						transformPlayer.Rotation = new Vector3(float.Parse(transformValues[3]), float.Parse(transformValues[4]), float.Parse(transformValues[5]));

						if (transformValues.Length == 6)
						{
							transformPlayer.Scale = Vector3.one;
						}
						else if (transformValues.Length == 9)
						{
							transformPlayer.Scale = new Vector3(float.Parse(transformValues[6]), float.Parse(transformValues[7]), float.Parse(transformValues[8]));
						}

						timeSeriesMotionsData.AddUpdatingTransform(transformPlayer);
					}
							
					this.playingTransformQue.Enqueue(timeSeriesMotionsData);
				}

				return true;
			}

			return false;
		}


		protected virtual void StartPlaying()
		{
			SIGVerseLogger.Info("Start the world playback playing");

			this.step = Step.Playing;

			// Reset elapsed time
			this.elapsedTime = 0.0f;
		}


		protected virtual void StopPlaying()
		{
			SIGVerseLogger.Info("Stop the world playback playing");

			this.step = Step.Waiting;
		}

		protected virtual void UpdateData()
		{
			if (this.playingTransformQue.Count == 0)
			{
				this.Stop();
				return;
			}

			this.UpdateTransform();
		}

		protected virtual void UpdateTransform()
		{
			if(this.playingTransformQue.Count == 0){ return; }

			UpdatingTransformList updatingTransformList = null;

			while (this.elapsedTime >= this.playingTransformQue.Peek().ElapsedTime)
			{
				updatingTransformList = this.playingTransformQue.Dequeue();

				if (this.playingTransformQue.Count == 0) { break; }
			}

			if (updatingTransformList == null) { return; }

			foreach (UpdatingTransformData updatingTransformData in updatingTransformList.GetUpdatingTransformList())
			{
				updatingTransformData.UpdateTransform();
			}
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

