using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    private Queue<Sentence> sentences;
    [HideInInspector]
    public DialogueTrigger currentDialogueTrigger;
    public float inputCD;
    float inputTimer;
    float lastSentenceTime = 0;
    float thisTryTime = 0;
    public float letterDelay;
    public Text nameText;
    public Text dialogueText;

    public Animator[] panelAnims;
    public Animator nameAnim;
    public Animator dialogueAnim;

    public Text[] choiceTexts;
    public Animator[] choiceFrameAnimators;
    public Animator[] choiceTextAnimators;
    public Animator[] choiceSpriteAnimators;
    public Image[] choiceSprites;
    public Sprite[] xBoxSprites;
    public Sprite[] PS4Sprites;
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

    // Use this for initialization
    void Start()
    {
        sentences = new Queue<Sentence>();
    }

    public void StartDialogue(DialogueTrigger dialogueTrigger, Dialogue dialogue)
    {
        foreach (Animator anim in panelAnims)
        {
            anim.SetBool("panelActive", true);
        }
        nameAnim.SetBool("panelActive", true);
        dialogueAnim.SetBool("dialogueActive", true);
        currentDialogueTrigger = dialogueTrigger;
        currentDialogueTrigger.dialogueActive = true;
        sentences.Clear();
        foreach (Sentence sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        thisTryTime = Time.time;
        inputTimer -= (thisTryTime - lastSentenceTime);
        if (inputTimer > 0)
        {
            return;
        }
        else
        {
            if (sentences.Count == 0)
            {
                EndDialogue();

                return;
            }
            Sentence sentence = sentences.Dequeue();
            nameText.text = sentence.name;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence.text, dialogueText));
            lastSentenceTime = Time.time;
            inputTimer = inputCD;
        }
    }
    public void DisplayNextSentence(float effectDelayTime)
    {
        thisTryTime = Time.time;
        inputTimer -= (thisTryTime - lastSentenceTime);
        if (inputTimer > 0)
        {
            return;
        }
        else
        {
            if (sentences.Count == 0)
            {
                Invoke("EndDialogue", effectDelayTime);

                return;
            }
            Sentence sentence = sentences.Dequeue();
            nameText.text = sentence.name;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence.text, dialogueText));
            lastSentenceTime = Time.time;
            inputTimer = inputCD;
        }
    }

    IEnumerator TypeSentence(string sentenceText, Text targetText)
    {
        targetText.text = "";
        foreach (char letter in sentenceText.ToCharArray())
        {
            targetText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }
    }

    public void EndDialogue()
    {
        InputManager.instance.disabled = false;
        if (currentDialogueTrigger.triggerOnEnter) //we only want the dialogues to automatically activate once, so the player doesn't get stuck in a dialogue loop
        {
            currentDialogueTrigger.triggerOnEnter = false;
            currentDialogueTrigger.triggerOnButtonPress = true;
        }
        InputManager.instance.actionInputDown = false;
        currentDialogueTrigger.dialogueActive = false;
        foreach (Animator anim in panelAnims)
        {
            anim.SetBool("panelActive", false);
        }
        nameAnim.SetBool("panelActive", false);
        dialogueAnim.SetBool("dialogueActive", false);

        if (currentDialogueTrigger.choicesAfterDialogue)
        {
            StartChoice(currentDialogueTrigger, currentDialogueTrigger.choices);
            currentDialogueTrigger.TriggerOutcome(currentDialogueTrigger.myEffects);
            currentDialogueTrigger = null;
        }
        else
        {
            currentDialogueTrigger.TriggerOutcome(currentDialogueTrigger.myEffects);
            currentDialogueTrigger = null;
        }
    }

    public void StartChoice(DialogueTrigger dialogueTrigger, Choice[] choices)
    {
        currentDialogueTrigger = dialogueTrigger;
        currentDialogueTrigger.choicesActive = true;
        if (choices.Length > choiceTexts.Length)
        {
            Debug.Log("too many choices");
            return;
        }
        for (int i = choices.Length; i > 0; i--)
        {
            if (choices[i - 1].choiceLabel != "")
            {
                if (InputManager.instance.activeInputScript == InputManager.instance.XboxControles)
                {
                    choiceSprites[i - 1].sprite = xBoxSprites[i - 1];
                }
                else
                {
                    choiceSprites[i - 1].sprite = PS4Sprites[i - 1];
                }
                choiceFrameAnimators[i - 1].SetBool("panelActive", true);
                choiceTextAnimators[(i - 1)].SetBool("panelActive", true);
                choiceSpriteAnimators[(i - 1)].SetBool("panelActive", true);


                StartCoroutine(TypeSentence(choices[i - 1].choiceLabel, choiceTexts[i - 1]));
            }
        }
        StartCoroutine(MakeAChoice(currentDialogueTrigger));
    }

    IEnumerator MakeAChoice(DialogueTrigger currentChoice)
    {
        InputManager.instance.Reset();
        InputManager.instance.disabled = true;
        while (!(InputManager.instance.choiceOne && currentChoice.choices[0].choiceLabel != "" ||
            InputManager.instance.choiceTwo && currentChoice.choices[1].choiceLabel != "" ||
            InputManager.instance.choiceThree && currentChoice.choices[2].choiceLabel != "" ||
            InputManager.instance.choiceFour && currentChoice.choices[3].choiceLabel != ""))
        {
            yield return null;
        }
        if (InputManager.instance.choiceOne)
        {
            if (currentChoice.choices[0].choiceEffects.Length > 0)
            {
                currentChoice.TriggerOutcome(currentChoice.choices[0].choiceEffects);
            }
            if (currentChoice.choices[0].continuesDialogue)
            {
                if (!currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = true;
                    yield return new WaitForEndOfFrame();
                }
                if (currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = false;
                }
            }
        }
        if (InputManager.instance.choiceTwo)
        {
            if (currentChoice.choices[1].choiceEffects.Length > 0)
            {
                currentChoice.TriggerOutcome(currentChoice.choices[1].choiceEffects);
            }
            if (currentChoice.choices[1].continuesDialogue)
            {


                if (!currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = true;
                    yield return new WaitForEndOfFrame();
                }
                if (currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = false;
                }
            }
        }
        if (InputManager.instance.choiceThree)
        {
            if (currentChoice.choices[2].choiceEffects.Length > 0)
            {
                currentChoice.TriggerOutcome(currentChoice.choices[2].choiceEffects);
            }
            if (currentChoice.choices[2].continuesDialogue)
            {
                if (!currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = true;
                    yield return new WaitForEndOfFrame();
                }
                if (currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = false;
                }

            }
        }
        if (InputManager.instance.choiceFour)
        {
            if (currentChoice.choices[3].choiceEffects.Length > 0)
            {
                currentChoice.TriggerOutcome(currentChoice.choices[3].choiceEffects);
            }
            if (currentChoice.choices[3].continuesDialogue)
            {

                if (!currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = true;
                    yield return new WaitForEndOfFrame();
                }
                if (currentChoice.dialogueActive)
                {
                    InputManager.instance.actionInputDown = false;
                }
            }
        }
        InputManager.instance.disabled = false;
        foreach (Animator animator in choiceFrameAnimators)
        {
            animator.SetBool("panelActive", false);
        }
        foreach (Animator animator in choiceTextAnimators)
        {
            animator.SetBool("panelActive", false);
        }
        foreach (Animator animator in choiceSpriteAnimators)
        {
            animator.SetBool("panelActive", false);
        }

    }
}
