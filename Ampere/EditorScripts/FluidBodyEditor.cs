using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;
namespace Ampere
{
	[CustomEditor(typeof(FluidBody))]
	public class FluidBodyEditor : Editor
	{
		private bool editMode = false;
		private bool mouseDown = false;
		private bool skipNextFrame = false;
		Vector3[] positionArray = new Vector3[4];
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!editMode)
			{
				if (GUILayout.Button(new GUIContent("Edit shape", "Create a new shape for the water body by clicking in the editor")))
				{
					editMode = true;
					positionArray = (target as FluidBody).GetSplineBasePoints();
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Apply new shape"))
				{
					editMode = false;
					float heightCorrection = positionArray[1].y;
					for (int i = 0; i < positionArray.Length; ++i)
					{
						positionArray[i].y -= heightCorrection;
					}
					Undo.RecordObject((target as Component).transform, "Changing position to adjust for point changes");
					(target as Component).transform.position += Vector3.up * heightCorrection;
					(target as FluidBody).ReplaceSplinePoints(positionArray);
				}
				if (GUILayout.Button("Cancel editing"))
				{
					editMode = false;
				}
				GUILayout.EndHorizontal();
			}

			base.DrawDefaultInspector();
			serializedObject.ApplyModifiedProperties();
		}

		private void OnSceneGUI()
		{
			if (Application.isPlaying)
			{
				return;
			}
			if (editMode)
			{
				DrawCreationLines();
				(Vector3 leftBorderVector, Vector3 rightBorderVector) = (positionArray[0] - positionArray[1], positionArray[3] - positionArray[2]);
				bool leftIsShorter = leftBorderVector.magnitude < rightBorderVector.magnitude;
				float coef = Vector2.Dot(leftBorderVector.normalized, Vector2.up) / Vector2.Dot(rightBorderVector.normalized, Vector2.up);
				if (leftIsShorter)
				{
					rightBorderVector = rightBorderVector.normalized * leftBorderVector.magnitude * coef;
				}
				else
				{
					leftBorderVector = leftBorderVector.normalized * rightBorderVector.magnitude / coef;
				}
				Vector3 origin = (target as FluidBody).transform.position;
				Color originalColor = Handles.color;
				Handles.color = Color.red;
				serializedObject.FindProperty("LowestSurfacePoint").floatValue = Mathf.Max((leftBorderVector.magnitude - (Handles.DoPositionHandle(origin + positionArray[1] + leftBorderVector * (1 - serializedObject.FindProperty("LowestSurfacePoint").floatValue), Quaternion.identity) - (origin + positionArray[1])).magnitude) / leftBorderVector.magnitude, 0);
				serializedObject.FindProperty("LowestSurfacePoint").floatValue = Mathf.Max((rightBorderVector.magnitude - (Handles.DoPositionHandle(origin + positionArray[2] + rightBorderVector * (1 - serializedObject.FindProperty("LowestSurfacePoint").floatValue), Quaternion.identity) - (origin + positionArray[2])).magnitude) / rightBorderVector.magnitude, 0);
				Handles.DrawLine(origin + positionArray[1] + leftBorderVector * (1 - serializedObject.FindProperty("LowestSurfacePoint").floatValue), origin + positionArray[2] + rightBorderVector * (1 - serializedObject.FindProperty("LowestSurfacePoint").floatValue), 3f);
				Handles.color = originalColor;
			}
			serializedObject.ApplyModifiedProperties();
			SceneView.RepaintAll();
		}
		private void DrawCreationLines()
		{
			Vector3 origin = (target as Component).transform.position;
			Color originalColor = Handles.color;
			Handles.color = Color.blue;
			for (int i = 0; i < positionArray.Length - 1; ++i)
			{
				Handles.DrawLine(origin + positionArray[i], origin + positionArray[i + 1], 3f);
			}
			Handles.DrawLine(origin + positionArray[3], origin + positionArray[0], 3f);
			for (int i = 0; i < positionArray.Length; ++i)
			{
				positionArray[i] = Handles.DoPositionHandle(origin + positionArray[i], Quaternion.identity) - origin;
				if (i == 1)
				{
					positionArray[2].y = positionArray[i].y;
				}
				if (i == 2)
				{
					positionArray[1].y = positionArray[i].y;
				}
			}
			Handles.color = originalColor;
		}
	}
}
