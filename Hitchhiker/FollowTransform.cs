using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HitchHiker
{
	//a script that can be used to let a number of gameObjects follow a transform, with a hard follow and soft follow range and rotation reorientation according to the follow direction
	//this was created to control a flock of birds with the intention to enable the designers to add or deduct birds at will without the need of knowing what happens in the script itself
	public class FollowTransform : MonoBehaviour
	{
		public Transform targetTrans;
		public Vector3 offSet;
		public float minFollowPerSec;
		public float maxFollowPerSec;
		[Tooltip("Distance at which the following starts")]
		public float minDistance;
		[Tooltip("Distance at which the followspeed will be max")]
		public float maxDistance;
		[Tooltip("The distance at which we do not fall behind anymore")]
		public float hardMaxDistance;
		public bool followRotation;
		[Tooltip("X for horizontal rotation, Y for vertical")]
		public Vector2 rotMaxFollow;
		public bool rotateChildrenToVelo;
		[Tooltip("percentage of rotational difference"), Range(0, 100)]
		public float childRotPerSec;
		public List<RuntimeAnimatorController> animControllerList;
		List<Transform> childList;
		List<Animator> childAnims;
		Transform myTrans;
		bool deactivated;
		// Start is called before the first frame update

		private void OnEnable()
		{
			if (UpdateManager.IsInstantiated)
				UpdateManager.Instance.onUpdate += OnUpdate;
		}

		private void OnDisable()
		{
			if (UpdateManager.IsInstantiated)
				UpdateManager.Instance.onUpdate -= OnUpdate;
		}

		void Start()
		{
			myTrans = this.transform;
			//we get a list of all child transforms for rotation later on
			childList = new List<Transform>();
			childAnims = new List<Animator>();
			foreach (Transform child in transform)
			{
				if (child != this.transform)
				{
					childList.Add(child);
				}
			}
			foreach (Transform child in childList)
			{
				childAnims.Add(child.GetComponent<Animator>());
			}
			if (animControllerList.Count != 0)
			{
				foreach (Animator childAnim in childAnims)
				{
					RuntimeAnimatorController targetController = animControllerList[Random.Range(0, animControllerList.Count)];
					childAnim.runtimeAnimatorController = targetController;
					childAnim.Play("Flying " + Random.Range(0, 4).ToString());
				}
			}
		}

		// Update is called once per frame
		void OnUpdate()
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				foreach(Transform child in childList)
				{
					child.gameObject.SetActive(!child.gameObject.activeSelf);
				}
				if (deactivated)
				{
					if (animControllerList.Count != 0)
					{
						foreach (Animator childAnim in childAnims)
						{
							RuntimeAnimatorController targetController = animControllerList[Random.Range(0, animControllerList.Count)];
							childAnim.runtimeAnimatorController = targetController;
							childAnim.Play("Flying " + Random.Range(0, 4).ToString());
						}
					}
				}
				deactivated = !deactivated;
			}

			if (deactivated)
			{
				return;
			}

			Vector3 targetPosition = targetTrans.position + offSet.z*targetTrans.forward+offSet.x*targetTrans.right + offSet.y*targetTrans.up;
			Vector3 targetVector = myTrans.position - targetPosition;
			//we only follow if we hit a certain distance
			if (targetVector.magnitude < minDistance)
			{
				return;
			}
			//if we are past a certain distance, we instantly close into that distance and recalculate the targetVector
			if (targetVector.magnitude > hardMaxDistance)
			{
				myTrans.position = targetPosition - targetVector;
				targetVector = myTrans.position - targetPosition;
			}
			//we calculate how much of the targetVector we are going to follow in this frame, with a smallest and biggest value
			float followStep = Mathf.Clamp(targetVector.magnitude / maxDistance, minFollowPerSec, maxFollowPerSec) * Time.deltaTime;
			//and then interpolate between our current position and the target position accordingly
			myTrans.position = Vector3.Slerp(myTrans.position, targetPosition, followStep);
			if (followRotation)
			{
				//we calculate the amount of how the rotation follows the target Vector
				Vector3 forwardVector = Vector3.Slerp(myTrans.forward, targetTrans.forward, rotMaxFollow.y);
				Vector3 upVector = Vector3.Slerp(myTrans.up, targetTrans.up, rotMaxFollow.x);
				myTrans.rotation = Quaternion.LookRotation(forwardVector, upVector);
			}
			//if we want to rotate our children to make them look like they anticipate the movement
			if (rotateChildrenToVelo)
			{
				Vector3 planarVector = Vector3.right + Vector3.forward;
				// for each child individually
				foreach (Transform child in childList)
				{
					//we calculate the amount of how the rotation follows the target Vector
					Vector3 forwardVector = Vector3.Slerp(child.forward,targetVector, (childRotPerSec/100)*Time.deltaTime);
					child.rotation = Quaternion.LookRotation(forwardVector);
				}
			}
		}
	}
}
