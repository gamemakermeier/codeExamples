using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : ObjectState
{
    public bool hideMeshOnDeactivated = true;
    public bool triggerOnButtonPress;
    public bool triggerOnEnter;
    public bool accessedByOtherScriptOnly;
    [HideInInspector]
    public bool dialogueActive;
    [HideInInspector]
    public bool choicesActive;
    public Dialogue dialogue;
    public Effector[] myEffects;
    public bool choicesAfterDialogue;
    public Choice[] choices;
    string currentSceneName;
    MeshRenderer myMesh;
    public bool activated;
    // Use this for initialization

    private void OnDisable()
    {
        this.activated = false;
    }
    private void OnEnable()
    {
        this.activated = true;
    }
    private void Start()
    {
        if (choices != null)
        {
            if (choices.Length != 4 && choices.Length > 0)
            {
                Choice[] oldChoices = choices;
                choices = new Choice[4];
                for (int i = oldChoices.Length; i > 0; i--)
                {
                    choices[i - 1] = oldChoices[i - 1];
                }
                for (int i = choices.Length; i > oldChoices.Length; i--)
                {
                    choices[i - 1] = new Choice { choiceLabel = "", choiceEffects = new Effector[0] };
                }
            }
        }
        if (triggerOnEnter)
        {
            this.gameObject.GetComponent<Collider>().isTrigger = true;
            triggerOnButtonPress = false;
        }
        if (triggerOnButtonPress)
        {
            this.gameObject.GetComponent<Collider>().isTrigger = true;
            triggerOnEnter = false;
        }
        if (accessedByOtherScriptOnly)
        {
            this.enabled = false;
            activated = false;
        }
        myMesh = GetComponent<MeshRenderer>();
        if (hideMeshOnDeactivated && myMesh != null)
        {

            myMesh.enabled = false;
        }
        activated = true;
        Register();
    }

    public void TriggerDialogue()
    {
        if (DialogueManager.instance.currentDialogueTrigger == null)
        {
            DialogueManager.instance.StartDialogue(this, dialogue);
        }
    }

    public void TriggerOutcome(Effector[] effects) // this method is used within the dialoguemanager which calls it upon user input when the end of the dialogue is reached. the effector/s is/are declared in inspector for each dialogue trigger
    {


        currentGameState = GameStateManager.instance.gameState;
        foreach (Effector effect in effects)
        {
            if (effect.stateName != "")
            {
                foreach (StateConnection stateConnection in currentGameState.stateConnections)
                {
                    if (stateConnection.stateLabel == effect.stateName.ToLowerInvariant())
                    {
                        if (!effect.addInsteadOfSet)
                        {
                            stateConnection.currentValue = effect.setTo; // we set the state connection to the new value
                        }
                        else
                        {
                            stateConnection.currentValue += effect.setTo;
                        }
                        GameStateManager.instance.Refresh(stateConnection); // and refresh all affected scripts through the game state mananger
                    }
                }
                if (effect.global)
                {
                    foreach (GameState gameState in GlobalGameStateManager.instance.savedStates)
                    {
                        if (gameState.areaName == "global")
                        {
                            currentGameState = gameState;
                        }
                    }
                    bool found = false;
                    if (currentGameState.stateConnections == null)
                    {
                        currentGameState.stateConnections = new List<StateConnection>();
                    }
                    foreach (StateConnection stateConnection in currentGameState.stateConnections)
                    {
                        if (stateConnection.stateLabel == effect.stateName.ToLowerInvariant())
                        {
                            found = true;
                            if (!effect.addInsteadOfSet)
                            {
                                stateConnection.currentValue = effect.setTo; // we set the state connection to the new value
                            }
                            else
                            {
                                stateConnection.currentValue += effect.setTo;
                            }
                            GameStateManager.instance.Refresh(stateConnection); // and refresh all affected scripts through the game state mananger
                        }
                    }
                    if (!found)
                    {
                        currentGameState.stateConnections.Add(new StateConnection { stateLabel = effect.stateName.ToLowerInvariant(), currentValue = effect.setTo });
                    }
                }
            }
            if (effect.scriptToFire != null)
            {
                effect.scriptToFire.Invoke(effect.targetMethod, 0f);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (activated)// we use the bool activated since script.enabled=false does not affect trigger methods (they would fire regardless)
        {
            if (other.gameObject.CompareTag("Player") && triggerOnButtonPress && InputManager.instance.actionInputDown)
            {
                if (!dialogueActive) //the dialogueActive bool tells us if we already started the dialogue. if no we start it, if yes we skip to our next sentence
                {
                    TriggerDialogue();
                }
                else
                {
                    DialogueManager.instance.DisplayNextSentence();
                }
            }
            if (other.gameObject.CompareTag("Player") && triggerOnEnter)
            {
                if (!dialogueActive)
                {
                    TriggerDialogue();
                }
                else if (InputManager.instance.actionInputDown)
                {
                    DialogueManager.instance.DisplayNextSentence();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (dialogueActive)
        {
            DialogueManager.instance.EndDialogue();
        }
    }
}
