using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StillWaters
{

	//this script is attached to every object that is supposed to change a game state by any means
	public class GameStateChanger : MonoBehaviour
	{
		public string stateName;
		public float stateValue;
		public bool addInsteadSet;
		public bool destroyAfterChange;
		public bool destroyGoAfterChange;
		public bool changeOnCollision;
		public string targetCollisionTag;
		public bool destroyOtherGoAfterChange;


		public float coolDown;
		float coolDownTimer;
		float lastChangeTime;
		public bool resetOnCollisionExit;
		[HideInInspector]
		public GameStateManager gameStateManager;

		private void Start()
		{
			gameStateManager = GameStateManager.instance;
		}

		public void Change()
		{
			gameStateManager.ChangeGameState(stateName, stateValue, addInsteadSet);
			if (destroyAfterChange)
			{
				Destroy(this);
			}
			if (destroyGoAfterChange)
			{
				Grabable _grabable = this.gameObject.GetComponent<Grabable>();
				if (!_grabable)
				{
					_grabable = this.gameObject.GetComponentInChildren<Grabable>();
					
				}
				if (_grabable)
				{
					if (_grabable.myGrabee)
					{
						_grabable.myGrabee.grabableList.Remove(this.gameObject);
						_grabable.myGrabee.currentlyGrabbed = null;
						_grabable.myGrabee.ThrowGrabable(_grabable, Vector3.zero, false);
					}
				}
			}
			else if (coolDown > 0)
			{
				lastChangeTime = Time.time;
			}

		}

		private void OnCollisionEnter(Collision collision)
		{

			if (changeOnCollision)
			{
				if (collision.gameObject.CompareTag(targetCollisionTag))
				{
					if (coolDownTimer <= 0)
					{
						Change();
						if (coolDown > 0)
						{
							coolDownTimer = coolDown;
						}
						if (destroyOtherGoAfterChange)
						{
							Grabable _grabable = collision.gameObject.GetComponent<Grabable>();
							if (!_grabable)
							{
								_grabable = collision.gameObject.GetComponentInChildren<Grabable>();
							}
							if (_grabable)
							{
								if (_grabable.myGrabee)
								{
									_grabable.myGrabee.grabableList.Remove(collision.gameObject);
									_grabable.myGrabee.currentlyGrabbed = null;
									_grabable.myGrabee.ThrowGrabable(_grabable,Vector3.zero,false);
									Destroy(collision.gameObject, Time.deltaTime);
								}
							}
						}
					}
					else
					{
						coolDownTimer = coolDown - (Time.time - lastChangeTime);
					}
				}
			}
		}

		private void OnCollisionExit(Collision collision)
		{

			if (changeOnCollision)
			{
				if (resetOnCollisionExit)
				{
					if (collision.gameObject.CompareTag(targetCollisionTag))
					{
						coolDownTimer = 0;
					}
				}
			}
		}
	}
}
