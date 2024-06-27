using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace Ampere
{
    [CustomEditor(typeof(HydraulicRampController))]
    public class HydraulicRampEditor : Editor
    {
        private void OnSceneGUI()
        {
            HydraulicRampController script = target as HydraulicRampController;
            Transform targetHeight = serializedObject.FindProperty("_targetPositions").GetArrayElementAtIndex(serializedObject.FindProperty("_targetPositions").arraySize-1).objectReferenceValue as Transform;
            Undo.RecordObject(targetHeight, "changing target height position");
            targetHeight.localPosition = Vector3.Scale(Handles.PositionHandle(targetHeight.position, Quaternion.identity) - script.transform.position, Vector3.up);
        }
    }
}
#endif //UNITY_EDITOR
