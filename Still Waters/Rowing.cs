using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StillWaters
{
	public class Rowing : MonoBehaviour
	{
		public float boatMoveMinOarSpd = 4f;
		[Tooltip("Amount of boat forward and Oar row direction matchup")]
		public float minMatchVeloBoat = 0.33f;
		public float boatPushFactor = 200f;
		//AddTorque rotation method
		public float boatRotFactor = 750f;
		public float rotForceDistriTime = 2f;
		Coroutine currentRotCR;
		//Quaternion lerp rotation method
		//public float maxRotPerRow = 30f;
		//public float maxTurnTime = 2f;
		Quaternion targetRotation;
		[HideInInspector]
		public bool rotationCR_breakRotation;
		bool rotationCR_isRunning;
		bool rotationCR_reEval;
		[Tooltip("how far away from the boat must the oar be for max rotation")]
		public float maxRotDist = 1f;
		[Range(0, 1)]
		public float minRotDist = 0.2f;
		public float sidewaysRowRotStart = 0.5f;
		public float maxBoatSpd = 3.5f;
		public float maxOarVelo = 15;
		public string waterLayerName = "Water";
		public bool hitsWater = false;
		public float maxBreakOarVelo = 1f;
		public float breakFriction = 1f;
		float noBreakFriction;
		public float breakTime = 1.5f;
		float breakTimer;
		public Rigidbody myOarRig;
		public Rigidbody myBoatRig;
		Transform myBoatTrans;
		Transform myTrans;
		Vector3 oldPos;
		bool rowDone;
		Vector3 oarVelocity;
		Vector3 startDirection;
		Vector3 targetDirection;
		Vector3 planarVector = Vector3.forward + Vector3.right;


		public AudioSource rightDip;
		public AudioSource rightLift;
		public AudioSource leftDip;
		public AudioSource leftLift;

		public GameObject playerHead;

		public Grabable myGrabable;
		private void Start()
		{
			myBoatTrans = myBoatRig.gameObject.transform;
			myTrans = this.transform;
			oldPos = myTrans.position - myBoatTrans.position;
			noBreakFriction = myBoatRig.drag;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.enabled)
			{
				//if we hit the water
				if (other.gameObject.layer == LayerMask.NameToLayer(waterLayerName))
				{
					//we set the initial oldPos
					oldPos = myTrans.position - myBoatTrans.position;
					//if we hit the water right from the player view we play the sound for the right dip
					if (Vector3.Dot(myTrans.position - playerHead.transform.position, playerHead.transform.right) > 0)
					{
						rightDip.Play();
					}
					//else we play the left dip
					else
					{
						leftDip.Play();
					}
				}
			}
		}
		private void OnTriggerStay(Collider other)
		{
			if (this.enabled)
			{
				hitsWater = (other.gameObject.layer == LayerMask.NameToLayer(waterLayerName));
				//while we are in the water
				if (hitsWater && myGrabable.myGrabee)
				{
					//we calculate the velocity through delta distance divided by delta time
					Vector3 newPos = myTrans.position - myBoatTrans.position;
					oarVelocity = (newPos - oldPos) / Time.deltaTime;
					//we cap the amount of velocity input that can come from the player to not insentivise massive swings
					if (oarVelocity.magnitude > maxOarVelo)
					{
						oarVelocity = oarVelocity.normalized * maxOarVelo;
					}
					oldPos = newPos;
					//we get the magnitude of our velocities
					float rowSpd = oarVelocity.magnitude;
					float boatSpd = myBoatRig.velocity.magnitude;
					//Debug.Log(oarVelocity.magnitude + " before");
					//if our oar moves fast enough and the oar moves backwards[currently we do not want the player to be able to move the boat backwards] and we didn't already row
					if (rowSpd > boatMoveMinOarSpd && Vector3.Dot(myBoatTrans.forward, -oarVelocity.normalized) > minMatchVeloBoat && !rowDone)
					{
						//we set the timer for breaking to 0
						breakTimer = 0;
						//we get make the oar velocity planar to the plane we want our boat to move (x-z plane in this case) and control the magnitude though a clamp between 0 and our upper velocity read limit
						//so that the player can't move himself in excessive speed
						//Debug.Log(oarVelocity.magnitude + " after");
						Vector3 planarOarVelo = Vector3.Scale(oarVelocity, planarVector).normalized * Mathf.Clamp(oarVelocity.magnitude, 0, maxOarVelo);
						//we add force in the direction of the planar oar velo and multiply the magnitude by a certain amount that we set in the inspector
						//but only if the boat isn't at max speed already
						if (boatSpd < maxBoatSpd)
						{
							myBoatRig.AddForce(-planarOarVelo * boatPushFactor);
						}

						//now we just need to add the desired rotation
						//for that we calculate the distance the oar is away from the boat, the further away, the more rotation force should be added
						Vector3 boatToOarVector = myTrans.position - myBoatTrans.position;
						//this works fine for rotation via rowing left or right from the boat. since we don't really want the boat to rotate when we row close to the fulcrum, this is a good start
						float torqueAmount = Mathf.Clamp(Vector3.Dot(boatToOarVector / maxRotDist, myBoatTrans.right), -1, 1);
						//Debug.Log(torqueAmount + "without SideSwipe");
						//but we still need to rotate in case the user is rowing infront of the boat, dragging the oar from left to right or vice versa
						//we just have to make sure that the row is actually being moved from left to right, rather than from front to back, in which case we don't want the boat to rotate (currently)
						if (Mathf.Abs(Vector3.Dot(planarOarVelo.normalized, myBoatTrans.forward)) < sidewaysRowRotStart)
						{
							//if we match that, we use the old rotation system and evaluate the direction of the oar velocity as a sign for our rotation
							torqueAmount = Vector3.Dot(planarOarVelo.normalized, myBoatTrans.right);
							//Debug.Log(torqueAmount + "with SideSwipe");
						}

						if (Mathf.Abs(torqueAmount) > minRotDist)
						{
							//Quaternion RotateTowards version
							//we calculate the target Rotation
							/*
							if (!rotationCR_isRunning)
							{
								targetRotation = Quaternion.AngleAxis(maxRotPerRow * torqueAmount, -Vector3.up) * myBoatTrans.rotation;
								StartCoroutine(RotateBoat(targetRotation, maxTurnTime));
							}
							else
							{
								targetRotation = Quaternion.AngleAxis(maxRotPerRow * torqueAmount, -Vector3.up) * targetRotation;
								rotationCR_reEval = true;
							}
							*/
							//Old AddTorque Version
							//myBoatRig.AddTorque(-Vector3.up * torqueAmount * boatRotFactor);
							//new addTorque Version
							if (!rotationCR_isRunning)
							{
								currentRotCR = StartCoroutine(RotateBoat(torqueAmount, boatRotFactor, rotForceDistriTime));
							}
							else
							{
								StopCoroutine(currentRotCR);
								currentRotCR = StartCoroutine(RotateBoat(torqueAmount, boatRotFactor, rotForceDistriTime));
							}
						}
						rowDone = true;

					}
					//if the player just straight up sticks the oar into the water while not rowing, we increase the friction on the boat and thus break. also works if we rowed but hold the oar in the water for an extended time
					else if (Mathf.Abs(rowSpd) <= maxBreakOarVelo && !rowDone || Mathf.Abs(rowSpd) <= maxBreakOarVelo && breakTimer > breakTime)
					{
						myBoatRig.drag = breakFriction;
						targetRotation = Quaternion.Lerp(myBoatTrans.rotation, targetRotation, 0.2f);
						rotationCR_reEval = true;
						//Debug.Log("break in use");
					}
					else
					{
						breakTimer += Time.deltaTime;
						myBoatRig.drag = noBreakFriction;
					}
				}
			}
		}
		//Quaternion Lerp version
		IEnumerator RotateBoat(Quaternion _targetRotation, float _turnTime)
		{
			//we declare that the coroutine is running
			rotationCR_isRunning = true;
			//we save our start Rotation
			Quaternion startRotation = myBoatTrans.rotation;
			//and reset the timer
			float turnTimer = 0f;
			//as long as we arent at our desided end rotation
			while (myBoatTrans.rotation != _targetRotation)
			{
				//we nudge our current rotation from our start Rotation to our target rotation, further with passing time
				myBoatTrans.rotation = Quaternion.Lerp(startRotation, _targetRotation, turnTimer / _turnTime);
				//we increase the timer with each step
				turnTimer += Time.deltaTime;
				//and make sure it doesn't get bigger than the max time
				if (turnTimer > _turnTime)
				{
					turnTimer = _turnTime;
				}
				//here we can end the coroutine from outside
				if (rotationCR_breakRotation)
				{
					rotationCR_isRunning = false;
					yield break;
				}
				//and here we can start a new rotation while we are still rotating
				if (rotationCR_reEval)
				{
					_targetRotation = targetRotation;
					startRotation = myBoatTrans.rotation;
					turnTimer = 0;
					rotationCR_reEval = false;
				}
				yield return new WaitForEndOfFrame();
			}
			//when the rotation ends, we signal it through this bool
			rotationCR_isRunning = false;
		}
		//Quaternion Lerp version
		IEnumerator RotateBoat(float _torqueAmount, float maxForce, float forceDistributionTime)
		{
			//we declare that the coroutine is running
			rotationCR_isRunning = true;
			//reset the timer
			float forceTimer = 0f;
			//as long as we arent at our desided end rotation
			while (forceTimer < forceDistributionTime)
			{
				//we nudge our current rotation from our start Rotation to our target rotation, further with passing time
				//the if statement makes it so we get 0 power at timer 0, full power at timer 0.5 of maxtime and 0 power at timer 1 of maxtime
				if (forceTimer < forceDistributionTime / 2)
				{
					myBoatRig.AddTorque(-Vector3.up * _torqueAmount * maxForce / forceDistributionTime * (Mathf.Clamp(forceTimer * 2 / forceDistributionTime, 0, 1)));
					//Debug.Log("Rotating with " + _torqueAmount * maxForce / forceDistributionTime * (Mathf.Clamp(forceTimer * 2 / forceDistributionTime, 0, 1)));
				}
				else
				{
					myBoatRig.AddTorque(-Vector3.up * _torqueAmount * maxForce / forceDistributionTime * (Mathf.Clamp(forceDistributionTime - (forceTimer - forceDistributionTime / 2) / forceDistributionTime, 0, 1)));
					//Debug.Log("Rotating with " + _torqueAmount * maxForce / forceDistributionTime * (Mathf.Clamp(forceDistributionTime - (forceTimer - forceDistributionTime / 2) / forceDistributionTime, 0, 1)));
				}
				//we increase the timer with each step
				forceTimer += Time.deltaTime;
				//and make sure it doesn't get bigger than the max time
				if (forceTimer > forceDistributionTime)
				{
					forceTimer = forceDistributionTime;
				}
				//here we can end the coroutine from outside
				if (rotationCR_breakRotation)
				{
					rotationCR_isRunning = false;
					yield break;
				}
				//we use waitforfixedupdate to actually apply the force
				yield return new WaitForFixedUpdate();
			}
			//when the rotation ends, we signal it through this bool
			rotationCR_isRunning = false;
		}
		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer(waterLayerName))
			{
				hitsWater = false;
				rowDone = false;
				myBoatRig.drag = noBreakFriction;
				breakTimer = 0;
				if (Vector3.Dot(this.transform.position - playerHead.transform.position, playerHead.transform.right) > 0)
				{
					rightLift.Play();
				}
				else
				{
					leftLift.Play();
				}
			}
		}
	}
}
