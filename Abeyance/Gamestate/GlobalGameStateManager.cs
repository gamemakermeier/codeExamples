using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameStateManager : MonoBehaviour
{
    public List<GameState> savedStates;
    public static GlobalGameStateManager instance;
    public Camera playerCam;
    public GameObject player;
    GameState globalGameState;
    GameState savedAreaState;
    public int startSceneIndex;
    public string introName;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        savedStates = new List<GameState>();
        savedStates.Add(new GameState { areaName = "global" });
        SceneManager.LoadScene(startSceneIndex, LoadSceneMode.Additive);
    }
    private void Start()
    {
        MovieManager.instance.StartMovie(introName);
    }

    public void SaveState(GameState currentRoomState)
    {
        for (int i = savedStates.Count; i > 0; i--)
        {
            if (savedStates[i - 1].areaName == currentRoomState.areaName)
            {
                savedStates[i - 1] = currentRoomState;
                return;
            }
        }
        savedStates.Add(currentRoomState);
    }

    public void LoadState(GameState targetGameState)
    {
        savedAreaState = null;
        globalGameState = null;
        for (int i = savedStates.Count; i > 0; i--)
        {
            if (savedStates[i - 1].areaName == "global")
            {
                globalGameState = savedStates[i - 1];
                if (globalGameState.stateConnections == null)
                {
                    globalGameState.stateConnections = new List<StateConnection>();
                }
            }
            if (savedStates[i - 1].areaName == targetGameState.areaName)
            {
                savedAreaState = savedStates[i - 1];
                if (savedAreaState.stateConnections == null)
                {
                    savedAreaState.stateConnections = new List<StateConnection>();
                }
            }
        }
        if (savedAreaState == null)
        {
            savedStates.Add(targetGameState);
            savedAreaState = savedStates[savedStates.Count - 1];
        }
        for (int a = globalGameState.stateConnections.Count; a > 0; a--)
        {
            for (int b = savedAreaState.stateConnections.Count; b > 0; b--)
            {
                if (savedAreaState.stateConnections[b - 1].stateLabel == globalGameState.stateConnections[a - 1].stateLabel)
                {

                    savedAreaState.stateConnections[b - 1].currentValue = globalGameState.stateConnections[a - 1].currentValue;
                }
            }
        }
        foreach (StateConnection stateConnection in savedAreaState.stateConnections)
        {

            for (int j = targetGameState.stateConnections.Count; j > 0; j--)
            {
                for (int k = savedAreaState.stateConnections.Count; k > 0; k--)
                {
                    if (targetGameState.stateConnections[j - 1].stateLabel == savedAreaState.stateConnections[k - 1].stateLabel)
                    {
                        targetGameState.stateConnections[j - 1].currentValue = savedAreaState.stateConnections[k - 1].currentValue;
                    }
                }
            }
        }
    }
}
