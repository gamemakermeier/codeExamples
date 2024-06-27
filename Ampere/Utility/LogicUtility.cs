using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ampere.Utility
{
	public static class LogicUtility
	{
		public static List<Transform> GetFamilyTree(Transform parentTransform)
		{
			List<Transform> family = new();
			family.Add(parentTransform);
			for (int i = 0; i < parentTransform.childCount; i++)
			{
				Transform newFoundChild = parentTransform.GetChild(i);
				family.Add(newFoundChild);
				if (newFoundChild.childCount > 0)
				{
					family.AddRange(GetFamilyTree(newFoundChild));
				}
			}
			return family;
		}
		public static List<GameObject> GetFamilyTree(GameObject parentGO)
		{
			List<GameObject> family = new()
			{
				parentGO
			};
			for (int i = 0; i < parentGO.transform.childCount; i++)
			{
				GameObject newFoundChild = parentGO.transform.GetChild(i).gameObject;
				family.Add(newFoundChild);
				if (newFoundChild.transform.childCount > 0)
				{
					family.AddRange(GetFamilyTree(newFoundChild));
				}
			}
			return family;
		}

		public static List<T> GetAllObjectsOfTypeInFamily<T>(GameObject familyHead) where T : class
		{
			List<T> objects = new();
			if (familyHead as T != null)
			{
				objects.Add(familyHead as T);
			}
			foreach (Component obj in familyHead.GetComponents<Component>())
			{
				if (obj as T != null)
				{
					objects.Add(obj as T);
				}
			}
			for (int i = 0; i < familyHead.transform.childCount; i++)
			{
				objects.AddRange(GetAllObjectsOfTypeInFamily<T>(familyHead.transform.GetChild(i).gameObject));
			}
			return objects;
		}

		public static GameObject FindGameObjectInTargetScene(string gameObjectName, Scene targetScene)
		{
			foreach (GameObject go in GetAllLoadedObjectsInScene<GameObject>(targetScene))
			{
				if (go.name == gameObjectName)
				{
					return go;
				}
			}
			return null;
		}

		public static int GetSortingLayerListIndex(int sortingLayerID)
		{
			int index = -1;
			SortingLayer[] sortingLayerArray = SortingLayer.layers;
			for (int i = 0; i < sortingLayerArray.Length; i++)
			{
				if (sortingLayerArray[i].id == sortingLayerID)
				{
					index = i;
					break;
				}
			}
			return index;
		}

		public static List<T> GetAllLoadedObjectsInScene<T>(Scene loadedScene) where T : class
		{
			List<T> objects = new();
			foreach (GameObject go in loadedScene.GetRootGameObjects())
			{
				objects.AddRange(GetAllObjectsOfTypeInFamily<T>(go));
			}
			return objects;
		}

		public static T GetPickupableByID<T>(Scene loadedScene, int ID) where T : Pickupable
		{
			List<T> allTargetsInScene = GetAllLoadedObjectsInScene<T>(loadedScene);
			for (int i = 0; i < allTargetsInScene.Count; ++i)
			{
				if (allTargetsInScene[i].GetUniqueID().Equals(ID))
				{
					return allTargetsInScene[i];
				}
			}
			return null;
		}

		public static T GetFirstFoundInScene<T>(Scene loadedScene) where T : class
		{
			foreach (GameObject go in loadedScene.GetRootGameObjects())
			{
				if (go.TryGetComponent(out T foundObject))
				{
					return foundObject;
				}
			}
			return null;
		}

		public static void SetSortingLayerRelativeToOther(Renderer referenceRenderer, Renderer targetRenderer, int offSet)
		{
			int parentSortingLayerListIndex = GetSortingLayerListIndex(referenceRenderer.sortingLayerID);
			targetRenderer.sortingLayerID = SortingLayer.layers[parentSortingLayerListIndex + offSet].id;
		}

		public static FMOD.Studio.PLAYBACK_STATE GetPlaybackState(FMOD.Studio.EventInstance instance)
		{
			instance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE pS);
			return pS;
		}

		public static List<T> DeepCopyList<T>(List<T> listToCopy)
		{
			List<T> deepCopiedList = new();
			for (int i = 0; i < listToCopy.Count; ++i)
			{
				deepCopiedList.Add(listToCopy[i]);
			}
			return deepCopiedList;
		}
		public static T[] DeepCopyListToArray<T>(List<T> listToCopy)
		{
			T[] deepCopiedArray = new T[listToCopy.Count];
			for (int i = 0; i < listToCopy.Count; ++i)
			{
				deepCopiedArray[i] = (listToCopy[i]);
			}
			return deepCopiedArray;
		}

		public static int GetDuplicateCount<T>(T itemToSearchFor, List<T> listToSearch)
		{
			int count = 0;
			for (int i = 0; i < listToSearch.Count; ++i)
			{
				if (IsNullOrDestroyed(listToSearch[i]))
				{
					continue;
				}
				if (listToSearch[i].Equals(itemToSearchFor))
				{
					count++;
				}
			}
			return count;
		}

		public static T[] DeepCopyArray<T>(T[] arrayToCopy, int additionalLength = 0)
		{
			T[] deepCopiedArray = new T[arrayToCopy.Length + additionalLength];
			for (int i = 0; i < arrayToCopy.Length; ++i)
			{
				deepCopiedArray[i] = arrayToCopy[i];
			}
			return deepCopiedArray;
		}

		public static void ShallowCopyArray<T>(ref T[] arrayToCopyInto, T[] arrayToCopy, int additionalLength = 0)
		{
			for (int i = 0; i < arrayToCopy.Length; ++i)
			{
				arrayToCopyInto[i] = arrayToCopy[i];
			}
		}

		public static Bounds ScaleBounds(Bounds originalBounds, Vector3 scaleVector)
		{
			return new Bounds(originalBounds.center, Vector3.Scale(originalBounds.size, scaleVector));
		}

		public static bool LayerIsInLayerMask(int layer, LayerMask layerMask)
		{
			return ((layerMask & 1 << layer) == 1 << layer);
		}

		public static bool InsertItemIntoArray<T>(T item, ref T[] array) where T : UnityEngine.Object
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

		public static bool RemoveItemFromArray<T>(T item, ref T[] array) where T : UnityEngine.Object
		{
			for (int i = 0; i < array.Length; ++i)
			{
				if (array[i] == item)
				{
					array[i] = null;
					return true;
				}
			}
			return false;
		}
		public static Vector3 GetCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector3 p = uuu * p0;
			p += 3 * uu * t * p1;
			p += 3 * u * tt * p2;
			p += ttt * p3;

			return p;
		}

		public static Vector3 GetNextBezierPointInReach(ref float currentT, float baseTIncrement, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float targetDistance)
		{
			Vector3 startPos = GetCubicBezierPoint(currentT, p0, p1, p2, p3);
			float maxT = currentT + baseTIncrement;
			if (maxT >= 1)
			{
				currentT = 1;
				return p3;
			}
			Vector3 endPos = Vector3.zero;
			int safeGuard = 0;
			while ((endPos - startPos).magnitude > targetDistance)//distance shouldn't be longer than the targetDistance
			{
				maxT -= baseTIncrement * 0.1f;
				endPos = GetCubicBezierPoint(maxT, p0, p1, p2, p3);
				if (++safeGuard > 100)
				{
					Debug.Log("While hit saveguard on target Distance being too long");
					break;
				}
			}
			safeGuard = 0;
			while ((endPos - startPos).magnitude < targetDistance * 0.9f)//distance shouldn't be shorter than 90% of targetDistance
			{
				maxT += baseTIncrement * 0.1f;
				endPos = GetCubicBezierPoint(maxT, p0, p1, p2, p3);
				if (++safeGuard > 100)
				{
					Debug.Log("While hit saveguard on target Distance being too short");
					break;
				}
			}
			currentT = maxT;
			return endPos;
		}

		public static List<Vector3> GetBezierCurvePoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float maxPointDistance) //does not add start point, but does add endpoint
		{
			List<Vector3> points = new();
			float currentT = 0;
			float baseTIncrement = 0.05f;
			int safeGuard = 0;
			while (currentT < 1)
			{
				points.Add(GetNextBezierPointInReach(ref currentT, baseTIncrement, p0, p1, p2, p3, maxPointDistance));
				if (++safeGuard > 1000)
				{
					Debug.Log($"While hit saveguard on going along curve between{p0} and {p3}");
					break;
				}
			}
			return points;
		}

		public static bool AnyTransformIsAtPosition(List<Transform> transformsList, Vector3 targetPosition)
		{
			for (int i = 0; i < transformsList.Count; ++i)
			{
				if (transformsList[i].position == targetPosition)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsNullOrDestroyed<T>(T nullableItem)
		{
			if (nullableItem == null || nullableItem.ToString().Equals("") || nullableItem.ToString().Equals("[]") || nullableItem.ToString().Equals("null"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void FillObjectPool(ref GameObject[] pool, GameObject poolableObject, Transform parent = null)
		{
			for (int i = 0; i < pool.Length; ++i)
			{
				if (LogicUtility.IsNullOrDestroyed(pool[i]))
				{
					pool[i] = GameObject.Instantiate(poolableObject, parent);
					pool[i].SetActive(false);
					if (parent)
					{
						pool[i].transform.SetPositionAndRotation(parent.position, parent.rotation);
					}
				}
			}
		}

		public static int GetNextAvailablePoolObjectIndex(GameObject[] objectPool)
		{
			for (int i = 0; i < objectPool.Length; ++i)
			{
				if (objectPool[i].activeSelf)
				{
					continue;
				}
				return i;
			}
			return -1;
		}

		public static string GetLayerNameFromValue(int value)
		{
			SortingLayer[] sortingLayers = SortingLayer.layers;
			for (int i = 0; i < sortingLayers.Length; ++i)
			{
				if (sortingLayers[i].value.Equals(value))
				{
					return sortingLayers[i].name;
				}
			}
			return null;
		}
	}
}
