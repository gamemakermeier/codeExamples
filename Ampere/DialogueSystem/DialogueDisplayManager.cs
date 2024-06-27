using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

namespace Ampere
{
	public class DialogueDisplayManager : MonoBehaviour
	{
		[SerializeField]
		private GameObject _dialogueGO;
		[SerializeField]
		private TextMeshProUGUI _dialogueTMP;
		[SerializeField]
		DialogueChoiceParent _dialogueChoiceParent;
		public static DialogueDisplayManager _instance;
		private bool isTalking = false;
		int playerChoice = -1;

		public void InitializeThis()
		{
			if (_instance)
			{
				if (_instance != this)
				{
					Destroy(this);
				}
			}
			else
			{
				_instance = this;
			}
			_dialogueGO.gameObject.SetActive(false);
		}

		public void DisplayDialogue(DialogueContainer targetDialogueContainer, Transform dialogueTrigger, PlayerState interactingPlayerState, Color playerColor, Color NPCColor, bool blockPlayerMovement)
		{
			StartCoroutine(DisplayDialogueContainer(targetDialogueContainer, dialogueTrigger, interactingPlayerState, playerColor, NPCColor, blockPlayerMovement));
		}
		private IEnumerator DisplayDialogueContainer(DialogueContainer targetContainer, Transform dialogueTrigger, PlayerState interactingPlayerState, Color playerColor, Color NPCColor, bool blockPlayerMovement)
		{
			bool originalPlayerMoveState = interactingPlayerState.CantMove;
			if (blockPlayerMovement)
			{
				interactingPlayerState.CantMove = true;
			}
			const float DISPLAY_SPEED = 25;
			string targetGUID = "";
			for (int i = 0; i < targetContainer.linkData.Count; ++i)
			{
				if (targetContainer.linkData[i].portname == "Start")
				{
					targetGUID = targetContainer.linkData[i].targetNodeGUID;
					break;
				}
			}
			if (string.IsNullOrEmpty(targetGUID))
			{
				Debug.LogError($"Couldn't find the start node for dialogue container {targetContainer.name}");
				yield return null;
			}
			else
			{

				while (!string.IsNullOrEmpty(targetGUID))
				{
					//Debug.Log("Going to next GUID thingy");
					DialogueNodeData targetNode = targetContainer.nodeData.First(x => x.GUID == targetGUID);
					if (targetNode == null)
					{
						Debug.LogError($"Couldn't find the node with the targetGUID {targetGUID}. Aborting playing DialogueContainer {targetContainer.name}");
						break;
					}
					float fullDisplayTime = targetNode.dialogueText.Length / DISPLAY_SPEED;
					float lingerTime = Mathf.Max(fullDisplayTime / 3f, 1);
					Transform targetTransform = targetNode.dialogueText.Contains("Player: ") ? interactingPlayerState.transform : targetNode.dialogueText.Contains("Other: ") ? dialogueTrigger : null;
					List<NodeLinkData> continueLinks = targetContainer.linkData.Where(x => x.baseNodeGUID == targetGUID).ToList();

					Color targetColor = targetNode.dialogueText.Contains("Player: ") ? playerColor : targetNode.dialogueText.Contains("Other: ") ? NPCColor : Color.white;
					yield return DisplayDialogueRoutine(targetNode.dialogueText.Replace("Player: ", "").Replace("Other: ", ""), fullDisplayTime, lingerTime, targetColor, targetTransform);
					if (continueLinks.Count > 1)
					{
						_dialogueChoiceParent.ShowChoices(continueLinks);
						AdjustDisplayPosition(_dialogueChoiceParent.GetComponent<RectTransform>(), interactingPlayerState.transform);
						yield return WaitForPlayerInput(interactingPlayerState.Input, continueLinks.Count);
						yield return DisplayDialogueRoutine($"{continueLinks[playerChoice].portname}", fullDisplayTime, lingerTime, playerColor, interactingPlayerState.transform);
					}
					else if (continueLinks.Count == 1)
					{
						playerChoice = 0;
						if (continueLinks[playerChoice].portname != "Choice 0")
						{
							yield return DisplayDialogueRoutine($"{continueLinks[playerChoice].portname}", fullDisplayTime, lingerTime, playerColor, interactingPlayerState.transform);
						}
					}
					else
					{
						playerChoice = -1;
					}
					targetGUID = playerChoice != -1 ? continueLinks[playerChoice].targetNodeGUID : "";
				}

				if (blockPlayerMovement)
				{
					interactingPlayerState.CantMove = false; //TODO quickfix, need to ensure this doesn't enable the player to move when they shouldn't be able to
				}
			}
		}

		private IEnumerator WaitForPlayerInput(InputManagerChannelSO playerInputChannel, int choiceCount)
		{
			switch (choiceCount)
			{
				case 3:
					{
						while (!playerInputChannel.action4Down && !playerInputChannel.action5Down && !playerInputChannel.action6Down)
						{
							yield return new WaitForEndOfFrame();
						}
						playerChoice = playerInputChannel.action4Down ? 0 : playerInputChannel.action5Down ? 1 : 2;
						_dialogueChoiceParent.ShowChoices(null);
						break;
					}
				case 2:
					{
						while (!playerInputChannel.action4Down && !playerInputChannel.action5Down)
						{
							yield return new WaitForEndOfFrame();
						}
						playerChoice = playerInputChannel.action4Down ? 0 : 1;
						_dialogueChoiceParent.ShowChoices(null);
						break;
					}
				case 1:
					{
						while (!playerInputChannel.action4Down)
						{
							yield return new WaitForEndOfFrame();
						}
						playerChoice = 0;
						_dialogueChoiceParent.ShowChoices(null);
						break;
					}
				default:
					_dialogueChoiceParent.ShowChoices(null);
					break;
			}

		}
		private IEnumerator DisplayDialogueRoutine(string text, float timeForFullDisplay, float lingerDuration, Color textColor, Transform targetTransform)
		{
			while (isTalking)
			{
				yield return new WaitForSeconds(0.1f);
			}
			isTalking = true;
			_dialogueTMP.text = "";
			_dialogueTMP.color = textColor;
			_dialogueGO.SetActive(true);
			(_dialogueGO.transform as RectTransform).position = new Vector2(Screen.width * 0.5f, Screen.height * 0.75f);
			float timer = 0;
			float timePerCharacter = timeForFullDisplay / text.Length;
			char[] textArray = text.ToCharArray();
			string currentDisplay = "";
			float charTimer = 0;
			int currentIndex = 0;
			while (_dialogueTMP.text.Length < textArray.Length)
			{
				yield return new WaitForEndOfFrame();
				if (targetTransform)
				{
					AdjustDisplayPosition(_dialogueGO.transform as RectTransform, targetTransform);
				}
				charTimer += Time.deltaTime;
				timer += Time.deltaTime;
				if (charTimer >= timePerCharacter)
				{
					currentDisplay += textArray[currentIndex];
					_dialogueTMP.text = currentDisplay;
					currentIndex++;
					charTimer = 0;
				}
			}
			timer = 0;
			while (timer < lingerDuration)
			{
				yield return new WaitForEndOfFrame();
				timer += Time.deltaTime;
			}
			_dialogueTMP.text = "";
			_dialogueGO.SetActive(false);
			isTalking = false;
		}

		private void AdjustDisplayPosition(RectTransform UITrans, Transform transformToFollow)
		{
			Vector3 followTransScreenPosition = Camera.main.WorldToScreenPoint(transformToFollow.position);
			UITrans.position = followTransScreenPosition + transformToFollow.lossyScale.y * Vector3.up;
		}
	}
}
