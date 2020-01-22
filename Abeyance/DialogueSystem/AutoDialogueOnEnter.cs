using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDialogueOnEnter : MonoBehaviour
{
    public DialogueTrigger targetDialogueTrigger;
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
        InputManager.instance.disabled = true;
        InputManager.instance.Reset();
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger(AnimationTrigger);
        }
        targetDialogueTrigger.TriggerDialogue();
        dialogueProgressionTimer = dialogueProgressionTime;
        while (DialogueManager.instance.currentDialogueTrigger != null)
        {
            while (dialogueProgressionTimer > 0)
            {
                dialogueProgressionTimer -= Time.deltaTime;
                yield return null;
            }
            DialogueManager.instance.DisplayNextSentence();
            dialogueProgressionTimer = dialogueProgressionTime;
            yield return null;

        }
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
