using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace StillWaters
{
	public class GameFeedbackManager : MonoBehaviour
	{

		public static GameFeedbackManager instance;

		public SteamVR_Behaviour_Pose leftHand;
		public SteamVR_Behaviour_Pose rightHand;

		public SteamVR_Action_Vibration vibration;
		public float vibrationDefaultLength = .05f;
		[Range(0, 320f)]
		public int defaultFrequency = 1;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance!= this)
			{
				Destroy(this);
			}
		}

		#region Vibrate
		public void Vibrate(SteamVR_Input_Sources handType)
		{
			vibration.Execute(0, vibrationDefaultLength, defaultFrequency, 1, handType);
		}
		public void Vibrate(SteamVR_Input_Sources handType, float delay)
		{
			vibration.Execute(delay, vibrationDefaultLength, defaultFrequency, 1, handType);
		}
		public void Vibrate(SteamVR_Input_Sources handType, float delay, float length)
		{
			vibration.Execute(delay, length, defaultFrequency, 1, handType);
		}
		public void Vibrate(SteamVR_Input_Sources handType, float delay, float length, int frequency)
		{
			frequency = (int)Mathf.Clamp(frequency, 0, 320);
			vibration.Execute(delay, length, frequency, 1, handType);
		}
		public void Vibrate(SteamVR_Input_Sources handType, float delay, float length, int frequency, float amplitude)
		{
			frequency = (int)Mathf.Clamp(frequency, 0, 320);
			amplitude = (float)Mathf.Clamp(amplitude, 0, 1);
			vibration.Execute(delay, length, frequency, amplitude, handType);
		}
		#endregion
	}
}