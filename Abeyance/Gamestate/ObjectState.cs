using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectState : MonoBehaviour
{ //this script behaves just as the dialogue trigger methods with the same name, but instead deactivates the attached game object instead of the very script
    public StateEffects myActiveScenarios;
    StateConnection myStateConnection;
    bool active;
    public bool scriptAffected;
    public bool gameObjectAffected;
    [HideInInspector]
    public GameState currentGameState;
    private void Start()
    {
        Register();
    }
    public void Register()
    {
        foreach (StateEffect activeScenario in myActiveScenarios.activeScenarios)
        {
            foreach (State state in activeScenario.isActiveWhen)
            {
                bool found = false;
                currentGameState = GameStateManager.instance.gameState;
                foreach (StateConnection stateConnection in currentGameState.stateConnections)
                {
                    if (state.stateLabel.ToLowerInvariant() == stateConnection.stateLabel)
                    {
                        found = true;
                        bool existing = false;
                        foreach (MonoBehaviour script in stateConnection.affectedScripts)
                        {
                            if (script == this)
                            {
                                existing = true;
                            }
                        }
                        if (!existing)
                        {
                            stateConnection.affectedScripts.Add(this);
                        }
                    }
                }
                if (!found)
                {
                    myStateConnection = new StateConnection
                    {
                        affectedScripts = new List<MonoBehaviour>()
                        {
                            this
                        },
                        stateLabel = state.stateLabel.ToLowerInvariant()
                    };
                    currentGameState.stateConnections.Add(myStateConnection);
                }
            }
        }
    }
    public void Refresh()
    {

        foreach (StateEffect stateEffect in myActiveScenarios.activeScenarios)
        {
            active = true;
            foreach (State state in stateEffect.isActiveWhen)
            {
                if (GameStateManager.instance != null)
                {
                    foreach (StateConnection stateConnection in GameStateManager.instance.gameState.stateConnections)
                    {
                        if (stateConnection != null)
                        {
                            if (state.stateLabel.ToLowerInvariant() == stateConnection.stateLabel)
                            {
                                if (state.currentValue != stateConnection.currentValue)
                                {
                                    active = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (gameObjectAffected)
        {
            this.gameObject.SetActive(active);
        }
        if (scriptAffected)
        {
            this.enabled = active;
        }
    }
}
