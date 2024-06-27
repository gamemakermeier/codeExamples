#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEditor;
namespace Ampere
{
	[CustomEditor(typeof(Cable), true)]
	public class CableEditor : Editor
	{
		private bool deleteMode = false;
		private bool eatMouseEvents = false;
		private void OnEnable()
		{
			GuaranteePlugsAsBezierPoints();
		}
		public override void OnInspectorGUI()
		{
			Cable script = target as Cable;
			base.OnInspectorGUI();
			if (GUILayout.Button("Add Bezier Point"))
			{
				GuaranteePlugsAsBezierPoints();
				Undo.RecordObject(script, "adding bezier point");
				Vector3 position = script._spawnCurvePoints[^2].position + (script._spawnCurvePoints[^1].position - script._spawnCurvePoints[^2].position) / 2f;
				Vector3 incomingBezierPoint = script._spawnCurvePoints[^2].incomingBezierPoint - script._spawnCurvePoints[^2].position + position;
				Vector3 outgoingBezierPoint = script._spawnCurvePoints[^2].outgoingBezierPoint - script._spawnCurvePoints[^2].position + position;
				CubicBezierPoint newBezierPoint = new(position, incomingBezierPoint, outgoingBezierPoint);
				script._spawnCurvePoints.Insert(script._spawnCurvePoints.Count - 1, newBezierPoint);
			}
			if (GUILayout.Button("Toggle Delete Mode"))
			{
				deleteMode = !deleteMode;
			}
		}
		private void OnSceneGUI()
		{
			if (Application.isPlaying)
			{
				return;
			}
			if (!deleteMode)
			{
				DrawBezierPoints(serializedObject.FindProperty("_spawnCurvePoints"));
			}
			else
			{
				DrawDeleteBeziers(serializedObject.FindProperty("_spawnCurvePoints"));
			}
			CreateCable();
			DrawCable();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawCable()
		{
			Cable script = (Cable)target;
			script.SetAnchorPositions(script._spawnedPieceHingeJoints, ref script._cablePiecePositions);
			EditorTools.DrawLines(script._cablePiecePositions, 3f);
		}

		private void DrawBezierPoints(SerializedProperty bezierPoints)
		{
			Cable script = target as Cable;
			Undo.RecordObject(script, "changing bezier points");
			for (int i = 0; i < bezierPoints.arraySize; ++i)
			{
				SerializedProperty bezierPointPosition = bezierPoints.GetArrayElementAtIndex(i).FindPropertyRelative("position");
				SerializedProperty incomingBezierPoint = bezierPoints.GetArrayElementAtIndex(i).FindPropertyRelative("incomingBezierPoint");
				SerializedProperty outgoingBezierPoint = bezierPoints.GetArrayElementAtIndex(i).FindPropertyRelative("outgoingBezierPoint");
				Vector3 oldPosition = bezierPointPosition.vector3Value;
				if (i != bezierPoints.arraySize - 1 && i != 0)
				{
					bezierPointPosition.vector3Value = Handles.PositionHandle(bezierPointPosition.vector3Value, Quaternion.identity);
				}
				else if (i == 0)
				{
					script._anchorA.transform.position = Handles.PositionHandle(script._anchorA.transform.position, Quaternion.identity);
					bezierPointPosition.vector3Value = script._anchorA.transform.position;
				}
				else if (i == bezierPoints.arraySize - 1)
				{
					script._anchorB.transform.position = Handles.PositionHandle(script._anchorB.transform.position, Quaternion.identity);
					bezierPointPosition.vector3Value = script._anchorB.transform.position;
				}
				outgoingBezierPoint.vector3Value += (bezierPointPosition.vector3Value - oldPosition);
				incomingBezierPoint.vector3Value = bezierPointPosition.vector3Value - (outgoingBezierPoint.vector3Value - bezierPointPosition.vector3Value);
				incomingBezierPoint.vector3Value = Handles.PositionHandle(incomingBezierPoint.vector3Value, Quaternion.identity);
				outgoingBezierPoint.vector3Value = bezierPointPosition.vector3Value - (incomingBezierPoint.vector3Value - bezierPointPosition.vector3Value);
				outgoingBezierPoint.vector3Value = Handles.PositionHandle(outgoingBezierPoint.vector3Value, Quaternion.identity);
				Color origColor = Handles.color;
				Handles.color = Color.yellow;
				Handles.DrawLine(bezierPointPosition.vector3Value, incomingBezierPoint.vector3Value);
				Handles.DrawLine(bezierPointPosition.vector3Value, outgoingBezierPoint.vector3Value);
				Handles.color = origColor;
			}
		}

		private void DrawDeleteBeziers(SerializedProperty bezierPoints)
		{
			Color origColor = Handles.color;
			Handles.color = Color.red;
			Camera editorCam = UnityEditor.SceneView.lastActiveSceneView.camera;
			if (eatMouseEvents)
			{
				Event.current.Use();
				if (Event.current.type == EventType.MouseUp)
				{
					eatMouseEvents = false;
				}
			}
			for (int i = bezierPoints.arraySize - 1; i >= 0; --i)
			{
				if (i == bezierPoints.arraySize - 1 || i == 0)
				{
					continue;
				}
				Vector3 bezierPointPosition = bezierPoints.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value;
				if ((Event.current.mousePosition - (editorCam.WorldToScreenPoint(bezierPointPosition).x * Vector2.right + (editorCam.pixelHeight - editorCam.WorldToScreenPoint(bezierPointPosition).y) * Vector2.up)).magnitude < 25f)
				{
					Handles.color = Color.green;
				}
				else
				{
					Handles.color = Color.red;
				}
				Handles.DrawSolidDisc(bezierPointPosition, -Vector3.forward, .25f);
				Debug.Log($"Handles color check is {Handles.color == Color.green} current button value is {Event.current.button} and isMouse is {Event.current.isMouse}");
				if (Handles.color == Color.green && Event.current.button == 0 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
				{
					Debug.Log("Delete pressed");
					Event.current.Use();
					bezierPoints.DeleteArrayElementAtIndex(i);
					Selection.activeGameObject = (target as Cable).gameObject;
					eatMouseEvents = true;
					GuaranteePlugsAsBezierPoints();
					bezierPoints.serializedObject.ApplyModifiedProperties();
				}
			}
			Handles.color = origColor;
		}

		private void GuaranteePlugsAsBezierPoints()
		{
			Cable script = target as Cable;
			Undo.RecordObject(script, "Matching beziers to plugs");
			if (script._spawnCurvePoints.Count < 2)
			{
				if (script._spawnCurvePoints.Count == 1)
				{
					serializedObject.FindProperty("_spawnedPieceHingeJoints").ArrayClear();
					serializedObject.FindProperty("_spawnedPieceHingeJoints").arraySize = 2;
				}
				InsertBezier(serializedObject.FindProperty("_spawnedPieceHingeJoints"), serializedObject.FindProperty("_spawnedPieceHingeJoints").arraySize - 1, new CubicBezierPoint(script._anchorA.transform.position, script._anchorA.transform.position - script._anchorA.transform.right, script._anchorA.transform.position + script._anchorA.transform.right));
				InsertBezier(serializedObject.FindProperty("_spawnedPieceHingeJoints"), serializedObject.FindProperty("_spawnedPieceHingeJoints").arraySize - 1, new CubicBezierPoint(script._anchorB.transform.position, script._anchorB.transform.position - script._anchorB.transform.right, script._anchorB.transform.position + script._anchorB.transform.right));
				serializedObject.ApplyModifiedProperties();
			}
			else
			{
				script._spawnCurvePoints[0].position = script._anchorA.transform.position;
				script._spawnCurvePoints[^1].position = script._anchorB.transform.position;
			}
		}
		public void CreateCable()
		{
			Cable script = (Cable)target;
			Undo.RecordObject(script, "creating new cable");
			GameObject _cablePiece = serializedObject.FindProperty("_cablePiece").objectReferenceValue as GameObject;
			GameObject _anchorB = serializedObject.FindProperty("_anchorB").objectReferenceValue as GameObject;
			GameObject _anchorA = serializedObject.FindProperty("_anchorA").objectReferenceValue as GameObject;
			GameObject _cablePieceParent = serializedObject.FindProperty("_cablePieceParent").objectReferenceValue as GameObject;
			GameObject _cableLineRenderer = serializedObject.FindProperty("_cableLineRenderer").objectReferenceValue as GameObject;
			float cablePieceLength = _cablePiece.GetComponent<BoxCollider2D>().size.x;
			if (!_cablePieceParent)
			{
				_cablePieceParent = PrefabUtility.InstantiatePrefab(_cableLineRenderer, script.transform) as GameObject;
				_cablePieceParent.transform.name = "CablePieces";
			}
			_cablePieceParent.transform.SetPositionAndRotation(script.transform.position, script.transform.rotation);
			LineRenderer lineRenderer = _cablePieceParent.GetComponent<LineRenderer>();
			lineRenderer.startWidth = script.cableWidth;
			lineRenderer.endWidth = script.cableWidth;
			lineRenderer.numCornerVertices = script._vertsPerConnection;
			lineRenderer.numCapVertices = script._vertsPerCap;
			lineRenderer.alignment = LineAlignment.TransformZ;
			Utility.LogicUtility.SetSortingLayerRelativeToOther(script._anchorASpriteRenderer, lineRenderer, -1);
			_cablePieceParent.transform.parent = _anchorA.transform.parent;
			SerializedProperty _cablePiecePositions = serializedObject.FindProperty("_cablePiecePositions");
			SerializedProperty _spawnedPieceHingeJoints = serializedObject.FindProperty("_spawnedPieceHingeJoints");
			SerializedProperty _spawnCurvePoints = serializedObject.FindProperty("_spawnCurvePoints");
			SerializedProperty _cableLength = serializedObject.FindProperty("_cableLength");
			_cableLength.floatValue = 0;
			_cablePiecePositions.arraySize = 1;
			_spawnedPieceHingeJoints.arraySize = 0;
			SerializedProperty targetCablePosition = _cablePiecePositions.GetArrayElementAtIndex(0);
			targetCablePosition.vector3Value = _spawnCurvePoints.GetArrayElementAtIndex(0).FindPropertyRelative("position").vector3Value;
			for (int i = 0; i < _spawnCurvePoints.arraySize - 1; ++i)
			{
				Vector3 p0 = _spawnCurvePoints.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value;
				Vector3 p1 = _spawnCurvePoints.GetArrayElementAtIndex(i).FindPropertyRelative("outgoingBezierPoint").vector3Value;
				Vector3 p2 = _spawnCurvePoints.GetArrayElementAtIndex(i + 1).FindPropertyRelative("incomingBezierPoint").vector3Value;
				Vector3 p3 = _spawnCurvePoints.GetArrayElementAtIndex(i + 1).FindPropertyRelative("position").vector3Value;
				List<Vector3> currentBezierPoints = Utility.LogicUtility.GetBezierCurvePoints(p0, p1, p2, p3, cablePieceLength);
				_cablePiecePositions.AddRange(currentBezierPoints);
			}
			SerializedProperty _cablePieces = serializedObject.FindProperty("_cablePieces");
			for (int i = _cablePieces.arraySize - 1; i >= _cablePiecePositions.arraySize - 1; --i)
			{
				if (_cablePieces.GetArrayElementAtIndex(i).objectReferenceValue != null)
				{
					Undo.DestroyObjectImmediate((_cablePieces.GetArrayElementAtIndex(i).objectReferenceValue as CablePiece).gameObject);
				}
				_cablePieces.DeleteArrayElementAtIndex(i);
			}
			for (int i = 0; i < _cablePiecePositions.arraySize - 1; ++i)
			{
				EnsureMatchingCablePieceAt(i);
				GameObject currentPiece = (_cablePieces.GetArrayElementAtIndex(i).objectReferenceValue as CablePiece).gameObject;
				currentPiece.name = $"CablePiece_{i}";
				Vector3 targetPosition = i == 0 ? _anchorA.transform.position : _cablePiecePositions.GetArrayElementAtIndex(i).vector3Value;
				Vector3 nextPosition = i < _cablePiecePositions.arraySize - 2 ? _cablePiecePositions.GetArrayElementAtIndex(i + 1).vector3Value : _anchorB.transform.position;

				currentPiece.GetComponent<BoxCollider2D>().size = Vector2.right * (nextPosition - targetPosition).magnitude + Vector2.up * currentPiece.GetComponent<BoxCollider2D>().size.y;
				_cableLength.floatValue += (nextPosition - targetPosition).magnitude;

				currentPiece.transform.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(Vector3.forward, Vector3.Cross(nextPosition - targetPosition, Vector3.forward)));
				HingeJoint2D jointConnection;
				if (i == 0)
				{
					_spawnedPieceHingeJoints.Add(_anchorA.GetComponent<HingeJoint2D>());
					_spawnedPieceHingeJoints.Add(currentPiece.GetComponent<HingeJoint2D>());
					currentPiece.GetComponent<HingeJoint2D>().anchor = Vector2.zero;
					jointConnection = _anchorA.GetComponent<HingeJoint2D>();
					jointConnection.connectedBody = (_spawnedPieceHingeJoints.GetArrayElementAtIndex(i + 1).objectReferenceValue as HingeJoint2D).gameObject.GetComponent<Rigidbody2D>();
					jointConnection.connectedAnchor = -(_spawnedPieceHingeJoints.GetArrayElementAtIndex(i + 1).objectReferenceValue as HingeJoint2D).anchor;
					Debug.DrawRay(currentPiece.transform.position, nextPosition - targetPosition, Color.red);
					continue;
				}

				_spawnedPieceHingeJoints.Add(currentPiece.GetComponent<HingeJoint2D>());
				jointConnection = _spawnedPieceHingeJoints.GetArrayElementAtIndex(i).objectReferenceValue as HingeJoint2D;
				jointConnection.connectedBody = currentPiece.GetComponent<Rigidbody2D>();
				Debug.DrawRay(currentPiece.transform.position + (Vector3)currentPiece.GetComponent<HingeJoint2D>().anchor, nextPosition - targetPosition, Color.red);
				if (i == _cablePiecePositions.arraySize - 2)
				{
					jointConnection = currentPiece.GetComponent<HingeJoint2D>();
					jointConnection.connectedBody = _anchorB.GetComponent<Rigidbody2D>();
					jointConnection = _anchorB.GetComponent<HingeJoint2D>();
					jointConnection.connectedBody = currentPiece.GetComponent<Rigidbody2D>();
					_spawnedPieceHingeJoints.Add(_anchorB.GetComponent<HingeJoint2D>());
					Debug.DrawRay(_anchorB.transform.position,targetPosition - nextPosition, Color.red);
				}
			}
			_cableLength.floatValue += (_anchorA.GetComponent<BoxCollider2D>().size.x / 2f)*_anchorA.transform.localScale.x;
			_cableLength.floatValue += (_anchorB.GetComponent<BoxCollider2D>().size.x / 2f) * _anchorB.transform.localScale.x;
			_anchorA.GetComponent<DistanceJoint2D>().distance = _cableLength.floatValue;
			_anchorB.GetComponent<DistanceJoint2D>().distance = _cableLength.floatValue;
			script.SetCableColor();
		}

		private void EnsureMatchingCablePieceAt(int index)
		{
			SerializedProperty _cablePieces = serializedObject.FindProperty("_cablePieces");
			GameObject _cablePiece = serializedObject.FindProperty("_cablePiece").objectReferenceValue as GameObject;
			GameObject _cablePieceParent = serializedObject.FindProperty("_cablePieceParent").objectReferenceValue as GameObject;
			Cable script = (Cable)target;
			Undo.RecordObject(script, "ensuring cablepieces are properly set up");
			if (index > _cablePieces.arraySize - 1)
			{
				_cablePieces.Add(null);
			}
			if (_cablePieces.GetArrayElementAtIndex(index).objectReferenceValue == null)
			{
				GameObject newCablePiece = PrefabUtility.InstantiatePrefab(_cablePiece, _cablePieceParent.transform) as GameObject;
				Undo.RegisterCreatedObjectUndo(newCablePiece, "Created new cable piece");
				_cablePieces.GetArrayElementAtIndex(index).objectReferenceValue = newCablePiece.GetComponent<CablePiece>();
			}
		}

		private void InsertBezier(SerializedProperty bezierList, int index, CubicBezierPoint bezierPoint)
		{
			bezierList.InsertArrayElementAtIndex(index);
			var newPoint = bezierList.GetArrayElementAtIndex(index);
			SerializedProperty position = newPoint.FindPropertyRelative("position");
			position.vector3Value = bezierPoint.position;
			newPoint.FindPropertyRelative("incomingBezierPoint").vector3Value = bezierPoint.incomingBezierPoint;
			newPoint.FindPropertyRelative("outgoingBezierPoint").vector3Value = bezierPoint.outgoingBezierPoint;
		}
	}
}
#endif //UNITY_EDITOR