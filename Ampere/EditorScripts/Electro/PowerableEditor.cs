using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
namespace Ampere
{
    public class Detector
    {
        public Vector3 _position;
        public int _originalIndex;

        public Detector(Vector3 position, int originalIndex)
        {
            _position = position;
            _originalIndex = originalIndex;
        }
    }

    public class DetectedObject
    {
        public int _targetIndex;
        public GameObject _gameObject;

        public DetectedObject(GameObject gameObject, int targetIndex)
        {
            _targetIndex = targetIndex;
            _gameObject = gameObject;
        }
    }
    [CustomEditor(typeof(Powerable),true)]
    public class PowerableEditor : Editor
    {
        List<Vector3> connectionPositions = new();
        List<Vector3> receiverPositions = new();
        List<Detector> connectionDetectors = new();
        List<Detector> receiverDetectors = new();
        List<int> detectedIndexes = new();
        Color orange = new(.9f, .5f, .2f);

        Powerable[] allPowerables;
        Connection[] allConnections;
        readonly float detectionRange = .5f;

        bool wasFocused;
        bool currentlyFocused;
        bool mouseDown = false;
        bool skipNextFrame;
        GameObject highlightObject;
        List<GameObject> highLightObjectAndAllChildren;

        int oldPowerablesCount = -1;
        int oldConnectionsCount = -1;

        public void OnSceneGUI()
        {
            Powerable script = target as Powerable;
            if (Event.current.clickCount > 0 && !skipNextFrame)
            {

                mouseDown = !mouseDown;
                if (!mouseDown)
                {
                    AddHitConnections(script, connectionDetectors, allConnections);
                    AddHitPowerables(script, receiverDetectors, allPowerables);
                }
                skipNextFrame = true;
            }
            else
            {
                skipNextFrame = false;
            }
            currentlyFocused = InternalEditorUtility.isApplicationActive;
            if (currentlyFocused)
            {
                if (!wasFocused)
                {
                    allConnections = FindObjectsOfType<Connection>();
                    allPowerables = FindObjectsOfType<Powerable>();
                }
            }
            wasFocused = currentlyFocused;

            connectionPositions = GetNonNullPositions(script._connections);
            DrawLines(script.transform.position, connectionPositions, Color.blue);
            receiverPositions = GetNonNullPositions(script._energyReceivers);
            DrawLines(script.transform.position, receiverPositions, Color.yellow);


            if (!DoesListHaveEmptyEntry(script._connections))
            {
                script._connections.Add(null);
            }
            if (!DoesListHaveEmptyEntry(script._energyReceivers))
            {
                script._energyReceivers.Add(null);
            }

            Vector3 defaultPosition = script.transform.position + Vector3.up * .5f;

            if (oldConnectionsCount != script._connections.Count)
            {
                CreateDetectors(script._connections, ref connectionDetectors, defaultPosition + Vector3.right * .25f);
            }
            oldConnectionsCount = script._connections.Count;
            if (oldPowerablesCount != script._energyReceivers.Count)
            {
                CreateDetectors(script._energyReceivers, ref receiverDetectors, defaultPosition - Vector3.right * .25f);
            }
            oldPowerablesCount = script._energyReceivers.Count;

            DrawHandleAndLine(ref receiverDetectors, script.transform.position, orange);
            DrawHandleAndLine(ref connectionDetectors, script.transform.position, Color.cyan);

            if (!mouseDown)
            {
                return;
            }

            highlightObject = null;

            AddHitConnections(script, connectionDetectors, allConnections);
            AddHitPowerables(script, receiverDetectors, allPowerables);

            if (highlightObject)
            {
                Handles.DrawOutline(highLightObjectAndAllChildren, orange);
            }
        }
        private void DrawLines(Vector3 origin, List<Vector3> targets, Color color)
        {
            Color originalColor = Handles.color;
            Handles.color = color;
            foreach (Vector3 target in targets)
            {
                Handles.DrawLine(origin, target, 3);
            }
            Handles.color = originalColor;
        }
        private bool DoesListHaveEmptyEntry<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    return true;
                }
                else if (list[i].ToString() == "null")
                {
                    return true;
                }
            }
            return false;
        }

        private List<Vector3> GetNonNullPositions(List<LineRenderer> list)
        {
            List<Vector3> positions = new();
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != null)
                {
                    positions.Add(list[i].transform.position);
                }
            }
            return positions;
        }
        private List<Vector3> GetNonNullPositions(List<Powerable> list)
        {
            List<Vector3> positions = new();
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != null)
                {
                    positions.Add(list[i].GetTransform().position);
                }
            }
            return positions;
        }

        private void AddHitConnections(Powerable script, List<Detector> detectors, Connection[] objectsToDetect)
        {
            detectedIndexes.Clear();
            List<DetectedObject> detectedLinerenderers = DetectAllObjects(detectors, objectsToDetect);
            for (int i = 0; i < detectedLinerenderers.Count; i++)
            {
                if (detectedLinerenderers[i]._gameObject.TryGetComponent(out LineRenderer targetLineRenderer))
                {
                    if (script._connections.Contains(targetLineRenderer))
                    {
                        continue;
                    }
                    var targetSlot = serializedObject.FindProperty("_connections").GetArrayElementAtIndex(detectedLinerenderers[i]._targetIndex);
                    targetSlot.objectReferenceValue = targetLineRenderer;
                    serializedObject.ApplyModifiedProperties();
                    if (highlightObject == targetLineRenderer.gameObject)
                    {
                        highlightObject = null;
                    }
                }
            }
        }
        private void AddHitPowerables(Powerable script, List<Detector> detectorPositions, Powerable[] objectsToDetect)
        {
            detectedIndexes.Clear();
            List<DetectedObject> detectedReceivers = DetectAllObjects(detectorPositions, objectsToDetect);
            for (int i = 0; i < detectedReceivers.Count; i++)
            {
                Transform targetTransform = detectedReceivers[i]._gameObject.transform;
                if (targetTransform.TryGetComponent(out Powerable powerable))
                {
                    if (script._energyReceivers.Contains(powerable))
                    {
                        continue;
                    }
                }
                var targetSlot = serializedObject.FindProperty("_energyReceivers").GetArrayElementAtIndex(detectedReceivers[i]._targetIndex);
                targetSlot.objectReferenceValue = powerable;
                serializedObject.ApplyModifiedProperties();
                if (highlightObject == targetTransform.gameObject)
                {
                    highlightObject = null;
                }
            }
        }

        private List<DetectedObject> DetectAllObjects<T>(List<Detector> detectors, T[] objectsToDetect) where T : Component
        {
            List<DetectedObject> detectedObjects = new();
            for (int i = 0; i < detectors.Count; i++)
            {
                DetectedObject detectedObject = DetectComponent(detectors[i], objectsToDetect);
                if (detectedObject != null)
                {
                    detectedIndexes.Add(i);
                    detectedObjects.Add(detectedObject);
                }
            }
            return detectedObjects;
        }
        private DetectedObject DetectComponent<T>(Detector detector, T[] objectsToDetect) where T:Component
        {
            foreach (T component in objectsToDetect)
            {
                if (Vector2.Distance((Vector2)component.transform.position, (Vector2)detector._position) < detectionRange)
                {
                    if (mouseDown)
                    {
                        if (!highlightObject == component)
                        {
                            highlightObject = component.gameObject;
                            highLightObjectAndAllChildren = GetAllChildGOs(component.gameObject);
                            highLightObjectAndAllChildren.Add(highlightObject);
                        }
                        return null;
                    }
                    else
                    {
                        return new DetectedObject(component.gameObject, detector._originalIndex);
                    }
                }
            }
            return null;
        }

        private void DrawHandleAndLine(ref List<Detector> detectorList, Vector3 origin, Color color)
        {
            Color originalColor = Handles.color;
            Handles.color = color;
            for (int i = detectorList.Count - 1; i >= 0; i--)
            {
                Handles.DrawLine(origin, detectorList[i]._position, 3f);
                detectorList[i]._position = Handles.PositionHandle(detectorList[i]._position, Quaternion.identity);
            }
            Handles.color = originalColor;
        }

        private void CreateDetectors<T>(List<T> list, ref List<Detector> detectors, Vector3 defaultPosition)
        {
            detectors.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    detectors.Add(new Detector(defaultPosition, i));
                }
                else if (list[i].ToString() == "null")
                {
                    detectors.Add(new Detector(defaultPosition, i));
                }
            }
        }

        private List<GameObject> GetAllChildGOs(GameObject parentGO)
        {
            List<GameObject> childGOs = new();
            for (int i = 0; i < parentGO.transform.childCount; i++)
            {
                GameObject childGO = parentGO.transform.GetChild(i).gameObject;
                childGOs.Add(childGO);
                childGOs.AddRange(GetAllChildGOs(childGO));
            }
            return childGOs;
        }
    }
}
#endif //UNITY_EDITOR
