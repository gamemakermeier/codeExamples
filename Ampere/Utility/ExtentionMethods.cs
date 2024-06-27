
using Ampere.Utility;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ampere
{
	public static class ExtentionMethods
	{
		public static bool CheckIfLoadedProperly(this Component componentThatChecks)
		{
			Scene myScene = componentThatChecks.gameObject.scene;
			GameObject[] allSceneGOs = LogicUtility.GetAllLoadedObjectsInScene<GameObject>(myScene).ToArray();
			if (!GameManager.INSTANCE)
			{
				for (int i = allSceneGOs.Length - 1; i >= 0; --i)
				{
					if (allSceneGOs[i].TryGetComponent(out SceneSetup sceneSetup))
					{
						sceneSetup.SetupThisScene();
						return false;
					}
				}
				Debug.LogError($"Please make sure the scene has a Scene Setup Component before loading it");
				return false;
			}
			return true;
		}

		public static bool IsPlaying(this EventInstance targetEventInstance)
		{
			targetEventInstance.getPlaybackState(out PLAYBACK_STATE targetPlaybackState);
			return (targetPlaybackState == PLAYBACK_STATE.STARTING || targetPlaybackState == PLAYBACK_STATE.PLAYING || targetPlaybackState == PLAYBACK_STATE.SUSTAINING);
		}

		public static bool InsertItemIntoArray<T>(this T[] array, T item) where T : UnityEngine.Object
		{
			for (int i = 0; i < array.Length; ++i)
			{
				if (array[i] == null)
				{
					array[i] = item;
					return true;
				}
			}
			return false;
		}

		public static T RemoveItemFromArray<T>(this T[] array, T item) where T : UnityEngine.Object
		{
			for (int i = 0; i < array.Length; ++i)
			{
				if (array[i] == item)
				{
					array[i] = null;
					return item;
				}
			}
			return null;
		}

		public static bool ArrayIsEmpty<T>(this T[] array) where T : UnityEngine.Object
		{
			if (array.Length == 0 || array == null)
			{
				Debug.LogWarning($"The array you tried to check for empty does not exist or has a length of 0, so is naturally empty");
				return true;
			}
			for (int i = 0; i < array.Length; ++i)
			{
				if (!array[i].IsNull())
				{
					return false;
				}
			}
			return true;
		}

		public static void RemoveUnorderedAt<T>(this List<T> list, int index)
		{
			list[index] = list[^1];
			list.RemoveAt(list.Count - 1);
		}

		public static void RemoveUnordered<T>(this List<T> list, T item) where T : UnityEngine.Object
		{
			for (int i = 0; i < list.Count; ++i)
			{
				if (list[i].Equals(item))
				{
					list.RemoveUnorderedAt(i);
					return;
				}
			}
		}

		public static List<T2> GetSpecificsFromList<T1, T2>(List<T1> originalList) where T1 : class where T2 : class
		{
			List<T2> list = new();
			for (int i = 0; i < originalList.Count; ++i)
			{
				if (!originalList[i].GetType().Equals(typeof(T2)))
				{
					continue;
				}
				list.Add(originalList[i] as T2);
			}
			return list;
		}

		public static void AddAllNewObjects<T>(this List<T> targetList, List<T> listToAddFrom)
		{
			for (int i = 0; i < targetList.Count; ++i)
			{
				if (targetList.Contains(listToAddFrom[i]))
				{
					continue;
				}
				targetList.Add(listToAddFrom[i]);
			}
		}

		public static void SortByType<T>(this List<T> targetList, params T[] typeOrder) where T : Type
		{
			int highestIndex = 0;
			for (int i = 0; i < typeOrder.Length; ++i)
			{
				for (int j = highestIndex; j < targetList.Count; ++j)
				{
					if (!targetList[j].GetType().Equals(typeOrder[i]))
					{
						for (int k = j; k < targetList.Count; ++k)
						{
							if (targetList[k].GetType().Equals(typeOrder[i]))
							{
								targetList.SwapItems(j, k);
							}
						}
					}
				}
			}
		}

		public static void SwapItems<T>(this List<T> targetList, int index1, int index2)
		{
			(targetList[index1], targetList[index2]) = (targetList[index2], targetList[index1]);
		}

		public static bool IsNull<T>(this T targetObject) where T : UnityEngine.Object
		{
			if (targetObject != null)
			{
				if (targetObject.ToString() != "null" && targetObject.ToString() != "" && targetObject.ToString() != "[]")
				{
					//Debug.Log($"Object is not null but {targetObject}");
					return false;
				}
			}
			return true;
		}


		public static T GetNonEmpty<T>(this T[] array) where T : UnityEngine.Object
		{
			if (array.Length == 0 || array == null)
			{
				Debug.LogWarning($"The array you tried to check for empty does not exist or has a length of 0, so is naturally empty");
				return null;
			}
			for (int i = 0; i < array.Length; ++i)
			{
				if (array[i] != null)
				{
					return array[i];
				}
			}
			return null;
		}

		public static T GetItemByName<T>(this T[] array, string nameToSearchFor) where T : UnityEngine.Object
		{
			for (int i = array.Length - 1; i >= 0; --i)
			{
				if (array[i] == null)
				{
					continue;
				}
				if (array[i].name.Equals(nameToSearchFor))
				{
					return array[i];
				}
			}
			return null;
		}

		public static Vector2 GetHighestLocalPoint(this PolygonCollider2D collider)
		{
			Vector2 highestPoint = Vector2.negativeInfinity;
			for (int i = 0; i < collider.points.Length; ++i)
			{
				if (collider.points[i].y > highestPoint.y)
				{
					highestPoint = collider.points[i];
				}
			}
			return highestPoint;
		}
		public static Vector2 GetHighestGlobalPoint(this PolygonCollider2D collider)
		{
			return ((Vector2)collider.transform.position + collider.GetHighestLocalPoint());
		}

#if UNITY_EDITOR
		public static void AddRange(this SerializedProperty vector3List, List<Vector3> listToAdd)
		{
			for (int i = 0; i < listToAdd.Count; ++i)
			{
				vector3List.arraySize++;
				SerializedProperty currentItem = vector3List.GetArrayElementAtIndex(vector3List.arraySize - 1);
				currentItem.vector3Value = listToAdd[i];
			}
		}

		public static void Add(this SerializedProperty list, UnityEngine.Object itemToAdd)
		{
			list.arraySize++;
			SerializedProperty currentItem = list.GetArrayElementAtIndex(list.arraySize - 1);
			currentItem.objectReferenceValue = itemToAdd;
		}
#endif
		public static T GetByID<T>(this List<T> targetList, int ID) where T : SaveData
		{
			for (int i = 0; i < targetList.Count; ++i)
			{
				if (targetList[i].UniqueID.Equals(ID))
				{
					return targetList[i];
				}
			}
			return null;
		}
		public static int GetIndexByID<T>(this List<T> targetList, int ID) where T : SaveData
		{
			for (int i = 0; i < targetList.Count; ++i)
			{
				if (targetList[i].UniqueID.Equals(ID))
				{
					return i;
				}
			}
			return -1;
		}
		public static void AlignSortingLayers(this GameObject targetGO, int targetLayerValue)
		{
			//Debug.Log($"Trying to align to layer {targetLayerValue} for {targetGO.name}");

			int defaultOffset = SortingLayer.layers[0].value;
			Renderer targetRenderer = targetGO.GetComponent<Renderer>();
			List<Renderer> allRenderer = LogicUtility.GetAllObjectsOfTypeInFamily<Renderer>(targetGO);
			if (targetGO.TryGetComponent(out CableAnchor cableAnchor))
			{
				if (cableAnchor._otherAnchor._currentSocket == null && GameManager.INSTANCE.PlayerTrans.GetComponent<PlayerActions>()._selectionChannel.CurrentPickup != cableAnchor._otherAnchor)
				{
					allRenderer = LogicUtility.GetAllObjectsOfTypeInFamily<Renderer>(cableAnchor.transform.parent.gameObject);
				}
			}
			for (int i = 0; i < allRenderer.Count; ++i)
			{
				int targetValueDif = SortingLayer.GetLayerValueFromID(allRenderer[i].sortingLayerID) - targetLayerValue;
				//Debug.Log($"For {allRenderer[i].gameObject.name} assigning layer with value {SortingLayer.GetLayerValueFromID(allRenderer[i].sortingLayerID) - defaultOffset - targetValueDif} with default offset of {defaultOffset} and biggestDif of {targetValueDif} and original value being {SortingLayer.GetLayerValueFromID(allRenderer[i].sortingLayerID)}");
				allRenderer[i].sortingLayerName = SortingLayer.layers[SortingLayer.GetLayerValueFromID(allRenderer[i].sortingLayerID) - defaultOffset - targetValueDif].name;
			}
		}

		public static string GetFrontmostSortingLayer(this GameObject targetGO)
		{
			int highestValue = int.MinValue;
			foreach (Renderer renderer in LogicUtility.GetAllObjectsOfTypeInFamily<Renderer>(targetGO))
			{
				highestValue = Mathf.Max(SortingLayer.GetLayerValueFromID(renderer.sortingLayerID));
			}
			return LogicUtility.GetLayerNameFromValue(highestValue);
		}

		public static string GetBackmostSortingLayer(this GameObject targetGO)
		{
			int lowestValue = int.MaxValue;
			foreach (Renderer renderer in LogicUtility.GetAllObjectsOfTypeInFamily<Renderer>(targetGO))
			{
				lowestValue = Mathf.Min(SortingLayer.GetLayerValueFromID(renderer.sortingLayerID));
			}
			return LogicUtility.GetLayerNameFromValue(lowestValue);
		}
	}
}