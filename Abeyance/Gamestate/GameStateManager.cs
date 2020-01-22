using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public float initiationWaitTime;
    public GameState gameState;
    public static GameStateManager instance;
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
    }
    private void Start()
    {
        Invoke("InitiateGameState", initiationWaitTime);
    }

    public void InitiateGameState()
    {
        for (int i = gameState.stateConnections.Count; i > 0; i--)
        {
            Refresh(gameState.stateConnections[i - 1]);
        }
    }

    public void Refresh(StateConnection targetStateConnection)
    {
        if (targetStateConnection.affectedScripts != null)
        {
            foreach (ObjectState stateScript in targetStateConnection.affectedScripts)
            {

                stateScript.Refresh();

            }
        }
    }
}
