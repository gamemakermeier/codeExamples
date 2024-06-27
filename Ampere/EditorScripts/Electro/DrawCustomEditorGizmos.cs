using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace Ampere
{
    public static class DrawCustomEditorGizmos
    {
        private static GameObject gizmoParent;
        public static bool drawGizmos;
        public static bool customSceneGuiInitiated = false;
        #region ElectroSocket
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawElectroSocketGizmoNotSelected(ElectroSocket socketScript, GizmoType gizmoType)
        {
            DoFauxGizmo(socketScript.gameObject, socketScript.transform, ref socketScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawElectroSocketGizmoSelected(ElectroSocket socketScript, GizmoType gizmoType)
        {
            DoFauxGizmo(socketScript.gameObject, socketScript.transform, ref socketScript.gizmo, false);
        }
        #endregion

        #region ElectroCable
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawElectroPlugGizmoNotSelected(ElectroCable cableScript, GizmoType gizmoType)
        {
            DoFauxGizmo(cableScript.gameObject, cableScript._anchorA.transform, ref cableScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawElectroPlugGizmoSelected(ElectroCable cableScript, GizmoType gizmoType)
        {
            DoFauxGizmo(cableScript.gameObject, cableScript._anchorA.transform, ref cableScript.gizmo, false);
        }
        #endregion

        #region HydraulicRamp
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawHydraulicRampGizmoNotSelected(HydraulicRampController hydraulicRampScript, GizmoType gizmoType)
        {
            DoFauxGizmo(hydraulicRampScript.gameObject, hydraulicRampScript.platform.transform, ref hydraulicRampScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawHydraulicRampGizmoSelected(HydraulicRampController hydraulicRampScript, GizmoType gizmoType)
        {
            DoFauxGizmo(hydraulicRampScript.gameObject, hydraulicRampScript.platform.transform, ref hydraulicRampScript.gizmo, false);
        }
        #endregion

        #region TrapDoor
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawTrapDoorGizmoNotSelected(TrapDoor trapDoorScript, GizmoType gizmoType)
        {
            DoFauxGizmo(trapDoorScript.gameObject, trapDoorScript.transform, ref trapDoorScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawTrapDoorGizmoSelected(TrapDoor trapDoorScript, GizmoType gizmoType)
        {
            DoFauxGizmo(trapDoorScript.gameObject, trapDoorScript.transform, ref trapDoorScript.gizmo, false);
        }
        #endregion

        #region LightSocket
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawLightSocketGizmoNotSelected(LightSocket lightSocketScript, GizmoType gizmoType)
        {
            DoFauxGizmo(lightSocketScript.gameObject, lightSocketScript.transform, ref lightSocketScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawLightSocketGizmoSelected(LightSocket lightSocketScript, GizmoType gizmoType)
        {
            DoFauxGizmo(lightSocketScript.gameObject, lightSocketScript.transform, ref lightSocketScript.gizmo, false);
        }
        #endregion

        #region LightBulb
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawLightBulbGizmoNotSelected(LightBulb lightBulbScript, GizmoType gizmoType)
        {
            DoFauxGizmo(lightBulbScript.gameObject, lightBulbScript.transform, ref lightBulbScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawLightBulbGizmoSelected(LightBulb lightBulbScript, GizmoType gizmoType)
        {
            DoFauxGizmo(lightBulbScript.gameObject, lightBulbScript.transform, ref lightBulbScript.gizmo, false);
        }
        #endregion

        #region Connection
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawConnectionGizmoNotSelected(Connection connectionScript, GizmoType gizmoType)
        {
            DoFauxGizmo(connectionScript.gameObject, connectionScript.transform, ref connectionScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawConnectionGizmoSelected(Connection connectionScript, GizmoType gizmoType)
        {
            DoFauxGizmo(connectionScript.gameObject, connectionScript.transform, ref connectionScript.gizmo, false);
        }
        #endregion

        #region Platform
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawPlatformGizmoNotSelected(PlatformScript platformScript, GizmoType gizmoType)
        {
            DoFauxGizmo(platformScript.gameObject, platformScript.transform, ref platformScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawPlatformGizmoSelected(PlatformScript platformScript, GizmoType gizmoType)
        {
            DoFauxGizmo(platformScript.gameObject, platformScript.transform, ref platformScript.gizmo, false);
        }
        #endregion

        #region Door
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawDoorGizmoNotSelected(DoorScript doorScript, GizmoType gizmoType)
        {
            DoFauxGizmo(doorScript.gameObject, doorScript.transform, ref doorScript.gizmo, true);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawDoorGizmoSelected(DoorScript doorScript, GizmoType gizmoType)
        {
            DoFauxGizmo(doorScript.gameObject, doorScript.transform, ref doorScript.gizmo, false);
        }
        #endregion

        [DrawGizmo(GizmoType.Selected)]
        static void OnFauxGizmoSelected(FauxGizmoScript fauxGizmoScript, GizmoType gizmoType)
        {
            if (fauxGizmoScript.myGO)
            {
                Selection.activeGameObject = fauxGizmoScript.myGO;
            }
        }

        [DrawGizmo(GizmoType.NonSelected)]
        static void OnGameManagerExists(GameManager gameManagerScript, GizmoType gizmoType)
        {
            if (customSceneGuiInitiated)
            {
                return;
            }
            customSceneGuiInitiated = true;
            CustomSceneGUI.ToggleCustomSceneGUI();
        }
        [MenuItem("BehindUnyieldingEyes/Utility/Toggle Gizmos")]
        private static void ToggleGizmos()
        {
            drawGizmos = !drawGizmos;
        }

        static void DoFauxGizmo(GameObject gizmoHolder, Transform positionTransform, ref GameObject gizmoRef, bool targetState)
        {
            if (Application.isPlaying)
            {
                return;
            }
            if (!drawGizmos)
            {
                if (gizmoRef != null)
                {
                    gizmoRef?.SetActive(false);
                }
                return;
            }
            if (!gizmoRef)
            {
                if (!gizmoParent)
                {
                    gizmoParent = GameObject.Find("gizmoParent");
                    if (!gizmoParent)
                    {
                        gizmoParent = new GameObject("gizmoParent");
                    }
                }
                bool duplicate = false;
                FauxGizmoScript[] otherGizmos = gizmoParent.GetComponentsInChildren<FauxGizmoScript>();
                foreach (FauxGizmoScript gizmoScript in otherGizmos)
                {
                    if (gizmoScript.myGO == gizmoHolder)
                    {
                        gizmoRef = gizmoScript.gameObject;
                        duplicate = true;
                    }
                }
                if (!duplicate)
                {
                    gizmoRef = Object.Instantiate(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SelectionCircle")[0]), typeof(GameObject))) as GameObject;
                    gizmoRef.transform.parent = gizmoParent.transform;
                    FauxGizmoScript fauxGizmoScript = gizmoRef.GetComponent<FauxGizmoScript>();
                    fauxGizmoScript.myGO = gizmoHolder;
                }
            }
            gizmoRef.SetActive(targetState);
            gizmoRef.transform.position = positionTransform.position - Vector3.forward * 1f;
        }
    }
}
#endif
