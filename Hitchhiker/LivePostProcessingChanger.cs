using UnityEngine;
using System.Collections.Generic;

namespace HitchHiker
{
	/* this script was used to detect if the player at an area to activate or deactivate a post processing effect
	 due to the fact that the player position is fixed within a relative system, we could use touples that describe the Y and X rotation to calculate the areas
	 this made this tool pretty flexible and usable for artists, since all they had to do is rotate the camera ingame and provide the rotation data for the desired area within the inspector
	*/
	public class LivePostProcessingChanger : MonoBehaviour
	{
		public Transform myRig;
		[Tooltip("Provide x and y rotation values to line out the bottom side of the windows")]
		public Vector2[] bottomPointList;
		[Tooltip("Provide x and y rotation values to line out the top side of the windows")]
		public Vector2[] topPointList;
		public enum mode
		{
			activateWithin,
			activateOutside
		}
		public mode myMode;
		List<float> bottomSlopes;
		List<float> topSlopes;
		public string effectName = "DoF";
		/* //raycast variation
		public LayerMask outSideLayer;
		public float maxCheckDist;

		private RaycastHit hit;
		*/
		private Transform myTrans;

#if UNITY_STANDALONE && !UNITY_EDITOR
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
#endif

		private void Start()
		{
			bottomSlopes = new List<float>();
			topSlopes = new List<float>();
			//we fill our slope lists, which hold the slopes between every pair of points. Y holds our horizontal rotation while x holds the vertical
			for (int i = 0; i < bottomPointList.Length - 1; i++)
			{
				bottomSlopes.Add((bottomPointList[i + 1].x - bottomPointList[i].x) / (bottomPointList[i + 1].y - bottomPointList[i].y));
			}
			for (int i = 0; i < topPointList.Length - 1; i++)
			{
				topSlopes.Add((topPointList[i + 1].x - topPointList[i].x) / (topPointList[i + 1].y - topPointList[i].y));
			}
			myTrans = this.transform;
		}
#if UNITY_EDITOR
		private void Update()
		{
			if (bottomPointList.Length > 0 && topPointList.Length > 0)
			{
				OnUpdate();
			}
		}
#endif

		private void OnUpdate()
		{
			if (!GameManager.IsInstantiated || myRig == null)
				return;

			/* //raycast variant
			if (!Physics.Raycast(myTrans.position, myTrans.forward, out hit, maxCheckDist, outSideLayer))
			{
				GameManager.Instance.CameraManager.StartEffect(effectName);
			}
			else
			{
				GameManager.Instance.CameraManager.StopEffect(effectName);
			}
			*/
			//setting up our equation variables
			float currentYRot = myRig.localRotation.eulerAngles.y;
			if (currentYRot > 180)
			{
				currentYRot -= 360;
			}
			float currentXRot = myRig.localRotation.eulerAngles.x;
			if (currentXRot > 180)
			{
				currentXRot -= 360;
			}
			Vector2 closestBotLeftPoint = Vector2.zero;
			Vector2 closestTopLeftPoint = Vector2.zero;
			float oldDiff = 100;
			//index tracks the actual targetPoint for later
			int targetIndex = 0;
			//we check for the closest point to the left that
			for (int i = 0; i < bottomPointList.Length - 1; i++)
			{
				float diffY = currentYRot - bottomPointList[i].y;
				if (diffY > 0)
				{
					if (diffY < oldDiff)
					{
						targetIndex = i;
						closestBotLeftPoint = bottomPointList[i];
						oldDiff = diffY;
					}
				}
			}
			// we get the horizontal rotation difference towards our closest left point
			float currentRun = currentYRot - closestBotLeftPoint.y;
			// and use that for our equation to get the border vertical rotation value corresponding to that horizontal rotation value
			float currentBotBorderX = currentRun * bottomSlopes[targetIndex] + closestBotLeftPoint.x;
			//since x rotation gets smaller as the view gets higher, if our current rotation is smaller than the smallest rotation to hit the window, we continue
			// otherwise we know the view is below the activation window and just go ahead and activate our effect
			if (currentXRot < currentBotBorderX)
			{
				// now we need to check if we are below the top border
				oldDiff = 100;
				//index tracks the actual targetPoint for later
				targetIndex = 0;
				//we check for the closest point to the left that
				for (int i = 0; i < topPointList.Length - 1; i++)
				{
					float diffY = currentYRot - topPointList[i].y;
					if (diffY > 0)
					{
						if (diffY < oldDiff)
						{
							targetIndex = i;
							closestTopLeftPoint = topPointList[i];
							oldDiff = diffY;
						}
					}
				}
				// we get the horizontal rotation difference towards our closest left point
				currentRun = currentYRot - closestBotLeftPoint.y;
				// and use that for our equation to get the border vertical rotation value corresponding to that horizontal rotation value
				float currentTopBorderX = currentRun * topSlopes[targetIndex] + closestTopLeftPoint.x;
				//if the X rotation is bigger, which means our view is below the top border of our window
				if (currentXRot > currentTopBorderX)
				{
					//we can finally validate that we are within the activation window and go ahead to activating or deactivating
					if (myMode == mode.activateOutside)
					{
						GameManager.Instance.CameraManager.StopEffect(effectName);
					}
					if (myMode == mode.activateWithin)
					{
						GameManager.Instance.CameraManager.StartEffect(effectName);
					}
				}
				//otherwise, we now know that we are above the activation window and set the activation respectively
				else
				{
					if (myMode == mode.activateOutside)
					{
						GameManager.Instance.CameraManager.StartEffect(effectName);
					}
					if (myMode == mode.activateWithin)
					{
						GameManager.Instance.CameraManager.StopEffect(effectName);
					}
				}
			}
			//if we detected our position below the activation window, we can already activate or deactivate on that information
			else
			{
				if (myMode == mode.activateOutside)
				{
					GameManager.Instance.CameraManager.StartEffect(effectName);
				}
				if (myMode == mode.activateWithin)
				{
					GameManager.Instance.CameraManager.StopEffect(effectName);
				}
			}
		}
	}
}
