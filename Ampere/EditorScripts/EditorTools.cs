#if UNITY_EDITOR
using UnityEngine;
using Ampere.Utility;
using System.Reflection;
using UnityEditorInternal;
using System;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Ampere
{
	public static class EditorTools
	{
		static GameObject darkness;
		[MenuItem("BehindUnyieldingEyes/Utility/Selected pos.z = 0")]
		private static void SetSelectedZPosToZero()
		{
			Object[] selection = Selection.objects;
			foreach (GameObject go in selection)
			{
				if (go.transform.position.z != 0)
				{
					go.transform.position = Vector3.Scale(go.transform.position, Vector3.right + Vector3.up);
				}
			}
		}
		[MenuItem("BehindUnyieldingEyes/Utility/Selected scale.z = 1")]
		private static void SetSelectedZScaleToZero()
		{
			Object[] selection = Selection.objects;
			foreach (GameObject go in selection)
			{
				if (go.transform.localScale.z != 1)
				{
					go.transform.localScale = Vector3.Scale(go.transform.localScale, Vector3.right + Vector3.up) + Vector3.forward;
				}
			}
		}

		[MenuItem("BehindUnyieldingEyes/Utility/All GameObjects scale.z = 1 and pos.z = 0")]
		private static void SetAllZToZero()
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				foreach (GameObject go in SceneManager.GetSceneAt(i).GetRootGameObjects())
				{
					foreach (Transform goTrans in LogicUtility.GetFamilyTree(go.transform))
					{
						if (goTrans.CompareTag("MainCamera"))
						{
							continue;
						}
						if (goTrans.position.z != 0)
						{
							goTrans.position = Vector3.Scale(goTrans.position, Vector3.right + Vector3.up);
						}
						if (goTrans.localScale.z != 1)
						{
							goTrans.localScale = Vector3.Scale(goTrans.localScale, Vector3.right + Vector3.up) + Vector3.forward;
						}
						if (goTrans.localRotation.x != 0 || goTrans.localRotation.y != 0)
						{
							goTrans.localRotation = Quaternion.Euler(Vector3.Scale(goTrans.eulerAngles, Vector3.forward));
						}
					}
				}
			}
		}

		#region Renderlayer setting menu points
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Background 4")]
		private static void SetRenderLayerToBackGround4()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "Background 4");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Background 3")]
		private static void SetRenderLayerToBackGround3()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "Background 3");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Background 2")]
		private static void SetRenderLayerToBackGround2()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "Background 2");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Background 1")]
		private static void SetRenderLayerToBackGround1()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "Background 1");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Default")]
		private static void SetRenderLayerToDefault()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "Default");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/MiddleGround")]
		private static void SetRenderLayerToMiddleGround()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "MiddleGround");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/ForeGround")]
		private static void SetRenderLayerToForeGround()
		{
			SetRenderLayerOfTargetFamily(Selection.activeTransform, "ForeGround");
		}
		[MenuItem("BehindUnyieldingEyes/Utility/SetRenderLayer/Automatic setup for all loaded scenes")]
		private static void SetUpSceneRenderLayers()
		{
			SetLayerOfFoundGOFamily("Layer -1", "Background 1");
			SetLayerOfFoundGOFamily("Layer -2", "Background 2");
			SetLayerOfFoundGOFamily("Layer -3", "Background 3");
			SetLayerOfFoundGOFamily("Layer -4", "Background 4");
			SetLayerOfFoundGOFamily("MAP", "Background 4");
		}

		private static void SetLayerOfFoundGOFamily(string targetGOName, string layerName)
		{
			Scene originalActiveScene = SceneManager.GetActiveScene();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
				GameObject targetGO = LogicUtility.FindGameObjectInTargetScene(targetGOName, SceneManager.GetSceneAt(i));
				Debug.Log(targetGO);
				if (!targetGO)
				{
					targetGO = new GameObject(targetGOName);
				}
				SetRenderLayerOfTargetFamily(targetGO.transform, layerName);
			}
			SceneManager.SetActiveScene(originalActiveScene);
		}

		#endregion
		private static void SetRenderLayerOfTargetFamily(Transform target, string layerName)
		{
			foreach (Transform targetTransform in LogicUtility.GetFamilyTree(target))
			{
				foreach (Renderer renderer in targetTransform.GetComponents<Renderer>())
				{
					Undo.RecordObject(renderer, $"changed spriterenderer layer for {targetTransform.name}");
					if (renderer.sortingLayerName != layerName)
					{
						renderer.sortingLayerName = layerName;
					}
				}
				foreach (SpriteMask spriteMask in targetTransform.GetComponents<SpriteMask>())
				{
					Undo.RecordObject(spriteMask, $"changed spritemask layer for {targetTransform.name}");
					if (spriteMask.frontSortingLayerID != SortingLayer.NameToID(layerName) || spriteMask.backSortingLayerID != SortingLayer.NameToID(layerName))
					{
						spriteMask.frontSortingLayerID = SortingLayer.NameToID(layerName);
						spriteMask.backSortingLayerID = SortingLayer.NameToID(layerName);
					}
				}
			}
		}
		[MenuItem("BehindUnyieldingEyes/Utility/Flatten Selected Connection Points")]
		private static void SetPointsZToZero()
		{
			Vector3 XYFlattener = Vector3.right + Vector3.up;
			GameObject[] selection = Selection.gameObjects;
			foreach (GameObject gameObject in selection)
			{
				foreach (LineRenderer lineRenderer in gameObject.GetComponents<LineRenderer>())
				{
					Undo.RecordObject(lineRenderer, "flattening linerenderer points");
					for (int i = 0; i < lineRenderer.positionCount; i++)
					{
						lineRenderer.SetPosition(i, Vector3.Scale(lineRenderer.GetPosition(i), XYFlattener));
					}
				}
			}
		}
		[MenuItem("BehindUnyieldingEyes/Utility/Toggle Darkness")]
		private static void ToggleDarkness()
		{
			if (!darkness)
			{
				GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
				foreach (GameObject gameObject in allObjects)
				{
					if (gameObject.name == "ForeGroundDarkness")
					{
						darkness = gameObject;
						Debug.Log("FoundDarkness");
						break;
					}
				}
			}
			if (darkness)
			{
				darkness.SetActive(!darkness.activeSelf);
			}
		}

		public static void DrawLines(Vector3[] positions, float lineThickness)
		{
			Color origHandlesColor = Handles.color;
			for (int i = 0; i < positions.Length - 1; ++i)
			{
				Handles.color = new Color(UnityEngine.Random.Range(0, 100) / 100f, UnityEngine.Random.Range(0, 100) / 100f, UnityEngine.Random.Range(0, 100) / 100f, 1);
				Handles.DrawLine(positions[i], positions[i + 1], lineThickness);
			}
			Handles.color = origHandlesColor;
		}

		public static void DrawBoundsInEditor(Vector3 center, Bounds bounds, float duration, Color color)
		{
			Vector3 bottomBackLeft = center - bounds.extents.z * Vector3.forward - bounds.extents.y * Vector3.up - bounds.extents.x * Vector3.right;
			Vector3 bottomBackRight = bottomBackLeft + bounds.size.x * Vector3.right;
			Vector3 topBackLeft = bottomBackLeft + bounds.size.y * Vector3.up;
			Vector3 topBackRight = bottomBackRight + bounds.size.y * Vector3.up;
			Vector3 bottomFrontLeft = bottomBackLeft + bounds.size.z * Vector3.forward;
			Vector3 bottomFrontRight = bottomBackRight + bounds.size.z * Vector3.forward;
			Vector3 topFrontLeft = topBackLeft + bounds.size.z * Vector3.forward;
			Vector3 topFrontRight = topBackRight + bounds.size.z * Vector3.forward;

			Debug.DrawLine(bottomBackLeft, bottomBackRight, color, duration);
			Debug.DrawLine(bottomBackLeft, bottomFrontLeft, color, duration);
			Debug.DrawLine(bottomBackLeft, topBackLeft, color, duration);
			Debug.DrawLine(bottomBackRight, bottomFrontRight, color, duration);
			Debug.DrawLine(bottomBackRight, topBackRight, color, duration);
			Debug.DrawLine(topBackLeft, topFrontLeft, color, duration);
			Debug.DrawLine(topBackRight, topFrontRight, color, duration);
			Debug.DrawLine(topFrontLeft, topFrontRight, color, duration);
			Debug.DrawLine(bottomFrontLeft, topFrontLeft, color, duration);
			Debug.DrawLine(bottomFrontRight, topFrontRight, color, duration);
		}

		public static string GetFileNameFromPath(string path)
		{
			return (path.Split("/")[^1].Split("\\")[^1].Split(".")[0]);
		}

		public static string GetLocalPath(string globalPath)
		{
			return $"Assets/{globalPath.Replace(Application.dataPath, "")}";
		}

		public static int[] GetSortingLayerUniqueIDs()
		{
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
		}
		public static string[] GetSortingLayerNames()
		{
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}

		public static Vector2 GetEditorWindowScreenPosition(Vector3 position)
		{
			Rect currentSceneViewSize = SceneView.lastActiveSceneView.camera.pixelRect;
			Camera currentScieneViewCamera = SceneView.lastActiveSceneView.camera;
			Vector3 screenPos = currentScieneViewCamera.ScreenToViewportPoint(position);
			return (new Vector2(screenPos.x*currentSceneViewSize.width, currentSceneViewSize.height - screenPos.y*currentSceneViewSize.height));
		}
		public static Vector3 GetEditorWindowMouseWorldPosition()
		{
			Camera currentScieneViewCamera = SceneView.lastActiveSceneView.camera;
			Event currentEvent = Event.current;
			Vector2 mousePos = currentEvent.mousePosition;
			mousePos.x = currentEvent.mousePosition.x;
			mousePos.y = currentScieneViewCamera.pixelHeight - currentEvent.mousePosition.y;
			Vector3 screenPos = currentScieneViewCamera.ScreenToWorldPoint(new Vector3 (mousePos.x,mousePos.y,currentScieneViewCamera.nearClipPlane));
			return (screenPos);
		}
	}
}
#endif //UNITY_EDITOR
