using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hitchhiker
{
	// a small tool used to randomly scale and rotate objects to make mass placed objects look more natural (e.g. trees, trash cans, rubble)
	public class RandomScaleRotate : MonoBehaviour
	{
		public Vector3 minScaleFactor = Vector3.one;
		public Vector3 maxScaleFactor = Vector3.one;
		public Vector3 rotationAxis;
		public bool absoluteOverwrite;
		public bool targetChildrenInstead = true;
		[Tooltip("if you choose uniform then only the x value is used")]
		public bool uniform;
#if UNITY_EDITOR
		[Sirenix.OdinInspector.Button("Scale"),Sirenix.OdinInspector.ButtonGroup]
		void Scale()
		{
			Transform myTrans = this.transform;
			List<Vector3> prevScales = new List<Vector3>();
			if (targetChildrenInstead)
			{
				foreach (Transform child in myTrans)
				{
					if (child != myTrans)
					{
						if (absoluteOverwrite)
						{
							if (uniform)
							{
								float scaleFactor = Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f;
								child.localScale = Vector3.right * scaleFactor +
												   Vector3.up * scaleFactor +
												   Vector3.forward * scaleFactor;
							}
							else
							{
								child.localScale = Vector3.right * (Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f) +
												   Vector3.up * (Random.Range(minScaleFactor.y * 100f, maxScaleFactor.y * 100f) / 100f) +
												   Vector3.forward * (Random.Range(minScaleFactor.z * 100f, maxScaleFactor.z * 100f) / 100f);
							}

						}
						else
						{
							Vector3 scaleVector = Vector3.right * (Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f) +
												  Vector3.up * (Random.Range(minScaleFactor.y * 100f, maxScaleFactor.y * 100f) / 100f) +
												  Vector3.forward * (Random.Range(minScaleFactor.z * 100f, maxScaleFactor.z * 100f) / 100f);
							if (uniform)
							{
								float scaleFactor = Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f;
								scaleVector = Vector3.right * scaleFactor +
											  Vector3.up * scaleFactor +
											  Vector3.forward * scaleFactor;
							}
							child.localScale = Vector3.Scale(child.localScale, scaleVector);
						}
					}
				}
			}
			else
			{
				if (absoluteOverwrite)
				{
					if (uniform)
					{
						float scaleFactor = Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f;
						myTrans.localScale = Vector3.right * scaleFactor +
											 Vector3.up * scaleFactor +
											 Vector3.forward * scaleFactor;
					}
					else
					{
						myTrans.localScale = Vector3.right * (Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f) +
											 Vector3.up * (Random.Range(minScaleFactor.y * 100f, maxScaleFactor.y * 100f) / 100f) +
											 Vector3.forward * (Random.Range(minScaleFactor.z * 100f, maxScaleFactor.z * 100f) / 100f);
					}

				}
				else
				{
					Vector3 scaleVector = Vector3.right * (Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f) +
										  Vector3.up * (Random.Range(minScaleFactor.y * 100f, maxScaleFactor.y * 100f) / 100f) +
										  Vector3.forward * (Random.Range(minScaleFactor.z * 100f, maxScaleFactor.z * 100f) / 100f);
					if (uniform)
					{
						float scaleFactor = Random.Range(minScaleFactor.x * 100f, maxScaleFactor.x * 100f) / 100f;
						scaleVector = Vector3.right * scaleFactor +
									  Vector3.up * scaleFactor +
									  Vector3.forward * scaleFactor;
					}
					myTrans.localScale = Vector3.Scale(myTrans.localScale, scaleVector);
				}
			}
		}
#endif
#if UNITY_EDITOR
		[Sirenix.OdinInspector.Button("Rotate"), Sirenix.OdinInspector.ButtonGroup]
		void Rotate()
		{
			if (targetChildrenInstead)
			{
				Transform myTrans = this.transform;
				foreach (Transform child in myTrans)
				{
					if (child != myTrans)
					{
						child.transform.Rotate(rotationAxis, Random.Range(0, 360));
					}
				}
			}
		}
#endif
	}
}
