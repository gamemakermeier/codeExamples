using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Ampere
{
    public class CustomSceneGUI
    {
        static bool editorMenuOpen;
        static bool assetCreationMenuOpen;
        static bool electroCreationMenuOpen;
        static bool lightCreationMenuOpen;
        static bool socketCreationMenuOpen;
        static bool unpoweredSocketCreationMenuOpen;
        static GameObject darkness;
        public static CustomSceneGUI customSceneUI;
        [MenuItem("BehindUnyieldingEyes/Toggle On Scene Menu")]
        public static void ToggleCustomSceneGUI()
        {
            if (customSceneUI == null)
            {
                customSceneUI = new CustomSceneGUI();
            }
            if (editorMenuOpen)
            {
                editorMenuOpen = false;
                SceneView.duringSceneGui -= customSceneUI.RenderSceneGUI;
            }
            if (!editorMenuOpen)
            {
                editorMenuOpen = true;
                SceneView.duringSceneGui += customSceneUI.RenderSceneGUI;
            }
        }

        public void RenderSceneGUI(SceneView sceneview)
        {
            float buttonWidth = Screen.width / 10f;
            float buttonHeight = Screen.height / 20f;
            Vector2 buttonSize = Vector2.right * buttonWidth + Vector2.up * buttonHeight;
            Vector2 currentButtonPosition = Vector2.right * (buttonWidth / 2f + Screen.width / 200f) + Vector2.up * (buttonHeight / 2f + Screen.height / 100f);
            Vector2 verticalBetweenButtons = Vector2.up * (buttonHeight + Screen.height / 66f);
            Vector2 horizontalBetweenButtons = Vector2.right * (buttonWidth + Screen.width / 100f);
            Handles.BeginGUI();
            if (!editorMenuOpen)
            {
                if (GUI.Button(new Rect(currentButtonPosition, buttonSize), "Open Editor Menu"))
                {
                    editorMenuOpen = true;
                }
            }
            if (editorMenuOpen)
            {
                if (GUI.Button(new Rect(currentButtonPosition, buttonSize), "Toggle Darkness"))
                {
                    ToggleDarkness();
                }
                currentButtonPosition += verticalBetweenButtons;
                if (GUI.Button(new Rect(currentButtonPosition, buttonSize), "Toggle Gizmos"))
                {
                    DrawCustomEditorGizmos.drawGizmos = !DrawCustomEditorGizmos.drawGizmos;
                }
                currentButtonPosition += verticalBetweenButtons;
                if (GUI.Button(new Rect(currentButtonPosition, buttonSize), "Create Asset"))
                {
                    assetCreationMenuOpen = !assetCreationMenuOpen;
                }
                if (assetCreationMenuOpen)
                {
                    Vector2 currentSubMenuButtonPosition = currentButtonPosition + horizontalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Electro"))
                    {
                        electroCreationMenuOpen = !electroCreationMenuOpen;
                        if (electroCreationMenuOpen)
                        {
                            lightCreationMenuOpen = false;
                        }
                    }
                    if (electroCreationMenuOpen)
                    {
                        Vector2 currentElectroMenuButtonPosition = currentSubMenuButtonPosition + horizontalBetweenButtons;
                        if (GUI.Button(new Rect(currentElectroMenuButtonPosition, buttonSize), "Socket"))
                        {
                            socketCreationMenuOpen = !socketCreationMenuOpen;
                        }
                        if (socketCreationMenuOpen)
                        {
                            Vector2 currentSocketMenuButtonPosition = currentElectroMenuButtonPosition + horizontalBetweenButtons;
                            if (GUI.Button(new Rect(currentSocketMenuButtonPosition, buttonSize), "powered"))
                            {
                                CreateAssets.CreateNewPoweredSocket();
                            }
                            currentSocketMenuButtonPosition += verticalBetweenButtons;
                            if (GUI.Button(new Rect(currentSocketMenuButtonPosition, buttonSize), "unpowered"))
                            {
                                unpoweredSocketCreationMenuOpen = !unpoweredSocketCreationMenuOpen;
                            }
                            if (unpoweredSocketCreationMenuOpen)
                            {
                                Vector2 currentUnpoweredMenuButtonPosition = currentSocketMenuButtonPosition + horizontalBetweenButtons;
                                if (GUI.Button(new Rect(currentUnpoweredMenuButtonPosition, buttonSize), "Yellow"))
                                {
                                    CreateAssets.CreateNewYellowSquareSocket();
                                }
                                currentUnpoweredMenuButtonPosition += verticalBetweenButtons;
                                if (GUI.Button(new Rect(currentUnpoweredMenuButtonPosition, buttonSize), "Green"))
                                {
                                    CreateAssets.CreateNewGreenSquareSocket();
                                }
                                currentUnpoweredMenuButtonPosition += verticalBetweenButtons;
                                if (GUI.Button(new Rect(currentUnpoweredMenuButtonPosition, buttonSize), "Blue"))
                                {
                                    CreateAssets.CreateNewBlueSocket();
                                }
                                currentUnpoweredMenuButtonPosition += verticalBetweenButtons;
                            }
                            currentSocketMenuButtonPosition += verticalBetweenButtons;
                        }
                        currentElectroMenuButtonPosition += verticalBetweenButtons;
                        if (GUI.Button(new Rect(currentElectroMenuButtonPosition, buttonSize), "Cable"))
                        {
                            CreateAssets.CreateNewCable();
                        }
                        currentElectroMenuButtonPosition += verticalBetweenButtons;
                        if (GUI.Button(new Rect(currentElectroMenuButtonPosition, buttonSize), "Connection"))
                        {
                            CreateAssets.CreateNewConnection();
                        }
                        currentElectroMenuButtonPosition += verticalBetweenButtons;
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Light"))
                    {
                        lightCreationMenuOpen = !lightCreationMenuOpen;
                        if (lightCreationMenuOpen)
                        {
                            electroCreationMenuOpen = false;
                        }
                    }
                    if (lightCreationMenuOpen)
                    {
                        Vector2 currentLightMenuButtonPosition = currentSubMenuButtonPosition + horizontalBetweenButtons;
                        if (GUI.Button(new Rect(currentLightMenuButtonPosition, buttonSize), "LightBulb"))
                        {
                            CreateAssets.CreateNewLightBulb();
                        }
                        currentLightMenuButtonPosition += verticalBetweenButtons;
                        if (GUI.Button(new Rect(currentLightMenuButtonPosition, buttonSize), "LightSocket"))
                        {
                            CreateAssets.CreateNewLightSocket();
                        }
                        currentLightMenuButtonPosition += verticalBetweenButtons;
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Platform"))
                    {
                        CreateAssets.CreateNewPlatform();
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Lift"))
                    {
                        CreateAssets.CreateNewLift();
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Trapdoor"))
                    {
                        CreateAssets.CreateNewTrapdoor();
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;
                    if (GUI.Button(new Rect(currentSubMenuButtonPosition, buttonSize), "Door"))
                    {
                        CreateAssets.CreateNewDoor();
                    }
                    currentSubMenuButtonPosition += verticalBetweenButtons;


                }
                currentButtonPosition += verticalBetweenButtons;
                if (GUI.Button(new Rect(currentButtonPosition, buttonSize), "Close Menu"))
                {
                    editorMenuOpen = false;
                }
            }
            Handles.EndGUI();
        }

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
                        break;
                    }
                }
            }
            if (darkness)
            {
                darkness.SetActive(!darkness.activeSelf);
            }
        }
    }
}
#endif //UNITY_EDITOR