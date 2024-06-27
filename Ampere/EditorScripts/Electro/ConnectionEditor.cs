using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Ampere
{
    [CustomEditor(typeof(Connection))]
    public class ConnectionEditor : Editor
    {
        List<Vector3> positionList = new List<Vector3>();
        private void OnSceneGUI()
        {
            Connection connectionScript = serializedObject.targetObject as Connection;
            LineRenderer lineRenderer = connectionScript.gameObject.GetComponent<LineRenderer>();
            Undo.RecordObject(lineRenderer, "changed connection spline positions");
            Transform connectionTransform = connectionScript.transform;
            Vector3[] connectionPositions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(connectionPositions);
            Vector3 center = Vector3.zero;
            for (int i = connectionPositions.Length - 1; i >= 0; i--)
            {
                Vector3 newPosition = Handles.PositionHandle(connectionPositions[i], Quaternion.identity);
                lineRenderer.SetPosition(i, newPosition);
                center += newPosition;

            }
            connectionTransform.position = center / connectionPositions.Length;

            Handles.BeginGUI();
            Rect currentSceneViewSize = SceneView.lastActiveSceneView.camera.pixelRect;
            Camera currentScieneViewCamera = SceneView.lastActiveSceneView.camera;
            Vector2 buttonSize = 0.05f * currentSceneViewSize.width * Vector2.right + 0.04f * currentSceneViewSize.height * Vector2.up;
            Vector3 connectionScreenPos = currentScieneViewCamera.WorldToScreenPoint(connectionTransform.position);
            float windowPosY = currentSceneViewSize.height - (connectionScreenPos.y + 2f * buttonSize.y);
            float windowPosX = connectionScreenPos.x - buttonSize.x / 2;
            if (GUI.Button(new Rect(windowPosX * Vector2.right + windowPosY * Vector2.up, buttonSize), "Add Point"))
            {
                lineRenderer.GetPositions(connectionPositions);
                positionList.Clear();
                positionList.AddRange(connectionPositions);
                Vector3 lastPosition = connectionPositions[connectionPositions.Length - 1];
                Vector3 secondToLastPosition = connectionPositions[connectionPositions.Length - 2];
                Vector3 lastDirection = lastPosition - secondToLastPosition;
                positionList.Add(lastPosition + lastDirection.normalized);
                lineRenderer.positionCount = positionList.Count;
                lineRenderer.SetPositions(positionList.ToArray());
            }
            windowPosY = currentSceneViewSize.height - (connectionScreenPos.y - buttonSize.y);
            if (GUI.Button(new Rect(windowPosX * Vector2.right + windowPosY * Vector2.up, buttonSize), "Remove"))
            {
                if (lineRenderer.positionCount > 2)
                {
                    lineRenderer.GetPositions(connectionPositions);
                    positionList.Clear();
                    positionList.AddRange(connectionPositions);
                    positionList.RemoveAt(positionList.Count - 1);
                    lineRenderer.positionCount = positionList.Count;
                    lineRenderer.SetPositions(positionList.ToArray());
                }
            }
            Handles.EndGUI();
        }
    }
}
#endif //UNITY_EDITOR
