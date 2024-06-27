using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace Ampere
{
    [CustomEditor(typeof(TrapDoor))]
    public class TrapDoorEditor : Editor
    {
        private void OnSceneGUI()
        {
            serializedObject.Update();
            TrapDoor script = serializedObject.targetObject as TrapDoor;
            SerializedProperty targetPoint = serializedObject.FindProperty("movePoint");
            targetPoint.vector2Value = Handles.PositionHandle(script.transform.position + (Vector3)targetPoint.vector2Value, Quaternion.identity) - script.transform.position;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif //UNITY_EDITOR