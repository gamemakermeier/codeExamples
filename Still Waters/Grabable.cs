using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StillWaters
{
	[RequireComponent(typeof(InvisibleProber))]
	public class Grabable : MonoBehaviour
	{
		// This class holds everything needed for the grabee to grab stuff
		// and needed to return the object to the dedicated position if the player drops it in inappropriate places
		Transform myTrans;
		public Transform grabAnchor;
		public Vector3 positionOffset;
		public Vector3 grabRotationOffset;
		public string designatedInvSlotName;
		public Vector3 inventoryRotationOffset;
		[HideInInspector]
		public Grabee myGrabee;
		public InventorySlot myInventorySlot;
		public Rigidbody myRig;
		public float returnWaitTime = .5f;
		public bool isVisible;

		public bool hasRigidbody;
		public bool isFloating;
		public bool startsKinematic;

		public Transform originalParent;
		Transform playerHead;

		public List<MonoBehaviour> activateOnGrab;
		public List<MonoBehaviour> deactivateOnGrab;

		private void Start()
		{
			AdjustScriptsOnGrabable(false);
			if (grabAnchor.parent != null)
			{
				originalParent = grabAnchor.parent;
			}
			if (myRig == null)
			{
				Rigidbody targetRig = GetComponent<Rigidbody>();
				if (targetRig != null)
				{
					myRig = targetRig;
					hasRigidbody = true;
				}
				else
				{
					hasRigidbody = false;
				}
			}
			else
			{
				hasRigidbody = true;
			}
			if (hasRigidbody)
			{
				if (startsKinematic)
				{
					myRig.isKinematic = true;
				}
				else
				{
					myRig.isKinematic = false;
				}
			}
			playerHead = Camera.main.transform;

		}
		public IEnumerator ReturnTimer()
		{
			float returnWaitTimer = returnWaitTime;
			while (!myGrabee)
			{
				while (returnWaitTimer > 0)
				{
					returnWaitTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
				while (isVisible || !(Vector3.Dot((myInventorySlot.myTrans.position - playerHead.position).normalized,playerHead.forward)<0))
				{
					yield return new WaitForSeconds(.2f);
				}
				GameStateManager.instance.AddItemToInventorySlot(this, myInventorySlot);
				yield break;
			}
		}

		public void AdjustScriptsOnGrabable(bool grabbing)
		{
			foreach (MonoBehaviour script in activateOnGrab)
			{
				script.enabled = grabbing;
			}
			foreach (MonoBehaviour script in deactivateOnGrab)
			{
				script.enabled = !grabbing;
			}
		}
	}
}
