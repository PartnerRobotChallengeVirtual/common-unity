using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;


namespace SIGVerse.ToyotaHSR
{
	public class HSRSubTwist : RosSubMessage<SIGVerse.RosBridge.geometry_msgs.Twist>
	{
//		private const float wheelInclinationThreshold = 0.985f; // 80[deg]
		private const float wheelInclinationThreshold = 0.965f; // 75[deg]
//		private const float wheelInclinationThreshold = 0.940f; // 70[deg]

		//--------------------------------------------------

		private Transform baseFootPrint;
		private Rigidbody baseRigidbody;

		private float linearVelX  = 0.0f;
		private float linearVelY  = 0.0f;
		private float angularVelZ = 0.0f;

		private bool isMoving = false;


		void Awake()
		{
			this.baseFootPrint = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);

			this.baseRigidbody = this.baseFootPrint.GetComponent<Rigidbody>();
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.geometry_msgs.Twist twist)
		{
			float linearVel = Mathf.Sqrt(Mathf.Pow(twist.linear.x, 2) + Mathf.Pow(twist.linear.y, 2));

			float linearVelClamped = Mathf.Clamp(linearVel, 0.0f, HSRCommon.MaxSpeedBase);

			if(linearVel >= 0.001)
			{
				this.linearVelX  = twist.linear.x * linearVelClamped / linearVel;
				this.linearVelY  = twist.linear.y * linearVelClamped / linearVel;
			}
			else
			{
				this.linearVelX = 0.0f;
				this.linearVelY = 0.0f;
			}

			this.angularVelZ = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, HSRCommon.MaxSpeedBaseRad);

//			Debug.Log("linearVel=" + linearVel + ", angularVel=" + angularVel);
			this.isMoving = Mathf.Abs(this.linearVelX) >= 0.001f || Mathf.Abs(this.linearVelY) >= 0.001f || Mathf.Abs(this.angularVelZ) >= 0.001f;
		}


		void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootPrint.forward.y) < wheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			UnityEngine.Vector3 deltaPosition = (-this.baseFootPrint.right * linearVelX + this.baseFootPrint.up * linearVelY) * Time.fixedDeltaTime;
			this.baseRigidbody.MovePosition(this.baseFootPrint.position + deltaPosition);

			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -angularVelZ * Mathf.Rad2Deg * Time.fixedDeltaTime));
			this.baseRigidbody.MoveRotation(this.baseRigidbody.rotation * deltaRotation);
		}
	}
}

