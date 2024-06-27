using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
public static class CreateAssets
{
    [MenuItem("BehindUnyieldingEyes/Create new/Electro/Cable")]
    public static void CreateNewCable()
    {
        GameObject newCable = CreateNewAsset("ElectroCable t:prefab");
        GameObject parent = GameObject.Find("AllElectroCables");
        if (!parent)
        {
            parent = new GameObject("AllElectroCables");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newCable.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Electro/Socket/powered")]
    public static void CreateNewPoweredSocket()
    {
        GameObject newPoweredSocket = CreateNewAsset("ElectroSocket_Round_Y t:prefab");
        GameObject parent = GameObject.Find("AllElectroSockets");
        if (!parent)
        {
            parent = new GameObject("AllElectroSockets");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newPoweredSocket.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Electro/Socket/unPowered/round_blue")]
    public static void CreateNewBlueSocket()
    {
        GameObject newBlueRoundSocket = CreateNewAsset("ElectroSocket_Round_B t:prefab");
        GameObject parent = GameObject.Find("AllElectroSockets");
        if (!parent)
        {
            parent = new GameObject("AllElectroSockets");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newBlueRoundSocket.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Electro/Socket/unPowered/square_yellow")]
    public static void CreateNewYellowSquareSocket()
    {
        GameObject newYellowSquareSocket = CreateNewAsset("ElectroSocket_Flat_Y t:prefab");
        GameObject parent = GameObject.Find("AllElectroSockets");
        if (!parent)
        {
            parent = new GameObject("AllElectroSockets");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newYellowSquareSocket.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Electro/Socket/unPowered/square_green")]
    public static void CreateNewGreenSquareSocket()
    {
        GameObject newGreenSquareSocket = CreateNewAsset("ElectroSocket_Flat_G t:prefab");
        GameObject parent = GameObject.Find("AllElectroSockets");
        if (!parent)
        {
            parent = new GameObject("AllElectroSockets");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newGreenSquareSocket.transform.parent = parent.transform;
    }
    [MenuItem("BehindUnyieldingEyes/Create new/Connection")]
    public static void CreateNewConnection()
    {
        GameObject newConnection = CreateNewAsset("Connection t:prefab");
        GameObject parent = GameObject.Find("AllConnections");
        if (!parent)
        {
            parent = new GameObject("AllConnections");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newConnection.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Light/Bulb")]
    public static void CreateNewLightBulb()
    {
        GameObject newBulb = CreateNewAsset("LightBulb t:prefab");
        GameObject parent = GameObject.Find("AllProps");
        if (!parent)
        {
            parent = new GameObject("AllProps");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newBulb.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Light/Socket")]
    public static void CreateNewLightSocket()
    {
        GameObject newLightSocket = CreateNewAsset("Light_Socket t:prefab");
        GameObject parent = GameObject.Find("AllLightSockets");
        if (!parent)
        {
            parent = new GameObject("AllLightSockets");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newLightSocket.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Structure/Platform")]
    public static void CreateNewPlatform()
    {
        GameObject newPlatform = CreateNewAsset("Platform_Static_Metal t:prefab");
        GameObject parent = GameObject.Find("AllPlatforms");
        if (!parent)
        {
            parent = new GameObject("AllPlatforms");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newPlatform.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Structure/Lift")]
    public static void CreateNewLift()
    {
        GameObject newPlatform = CreateNewAsset("HydraulicRamp t:prefab");
        GameObject parent = GameObject.Find("AllHydraulicRamps");
        if (!parent)
        {
            parent = new GameObject("AllHydraulicRamps");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newPlatform.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Structure/TrapDoor")]
    public static void CreateNewTrapdoor()
    {
        GameObject newTrapDoor = CreateNewAsset("Trapdoor t:prefab");
        GameObject parent = GameObject.Find("AllTrapDoors");
        if (!parent)
        {
            parent = new GameObject("AllTrapDoors");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newTrapDoor.transform.parent = parent.transform;
    }

    [MenuItem("BehindUnyieldingEyes/Create new/Structure/Door")]
    public static void CreateNewDoor()
    {
        GameObject newDoor = CreateNewAsset("Door t:prefab");
        GameObject parent = GameObject.Find("Core");
        if (!parent)
        {
            parent = new GameObject("Core");
            Undo.RegisterCreatedObjectUndo(parent, "create a new asset parent");
        }
        newDoor.transform.parent = parent.transform;
    }

    private static GameObject CreateNewAsset(string searchString)
    {
        string[] assetGuids = AssetDatabase.FindAssets(searchString);
        string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        Undo.RegisterCreatedObjectUndo(newGameObject, "create a new asset");
        Camera currentCam = SceneView.lastActiveSceneView.camera;
        newGameObject.transform.position = currentCam.ScreenToWorldPoint(currentCam.pixelWidth / 2f * Vector3.right + currentCam.pixelHeight / 2f * Vector3.up);
		newGameObject.transform.position = Vector3.Scale(newGameObject.transform.position, Vector3.up + Vector3.right);
        Selection.activeObject = newGameObject;
        return newGameObject;
    }
}
#endif //UNITY_EDITOR
