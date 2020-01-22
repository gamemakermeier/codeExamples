using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 this script enables a dialoguetrigger to fire on contact rather than player input
 I used onTriggerStay to guarantee that the dialogue gets fired again, if it gets interrupted by some weird interaction the first time around (which would be changed if there was more time for polishing)
 */
public class AutoDialogueOnEnter : MonoBehaviour
{
    public DialogueTrigger targetDialogueTrigger;
    //the time it takes for the dialogue to continue automatically
    public float dialogueProgressionTime;
    float dialogueProgressionTimer;
    public bool disableAfterDialogue;
    public Animator targetAnimator;
    public string AnimationTrigger;
    bool inProgress;
    // Use this for initialization
    IEnumerator FireDialogue()
    {
        inProgress = true;
        InputManager.instance.disabled = true; //this sets the inputmanager to only take dialogue relevant inputs
        InputManager.instance.Reset(); //we reset the input manager to prevent interaction with lingering inputs
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger(AnimationTrigger);
        }
        //starting the dialogue
        targetDialogueTrigger.TriggerDialogue();
        //and setting the autoprogressing timer
        dialogueProgressionTimer = dialogueProgressionTime;
        //as long as the player doesn't leave the dialogue area
        while (DialogueManager.instance.currentDialogueTrigger != null)
        {
            //we tick the timer down
            while (dialogueProgressionTimer > 0)
            {
                dialogueProgressionTimer -= Time.deltaTime;
                yield return null;
            }
            //and display the next sentence (sets currentdialoguetrigger to null if no more sentences are available)
            DialogueManager.instance.DisplayNextSentence();
            //and finally reset the timer
            dialogueProgressionTimer = dialogueProgressionTime;
            yield return null;

        }
        //the inputmanager starts taking all default inputs again
        InputManager.instance.disabled = false;
        if (disableAfterDialogue)
        {
            this.gameObject.SetActive(false);
        }
        inProgress = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && DialogueManager.instance.currentDialogueTrigger == null && !inProgress)
        {
            StartCoroutine(FireDialogue());
        }
    }
}
