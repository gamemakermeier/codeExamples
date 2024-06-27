using Ampere.Utility;
using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
namespace Ampere
{
	public class GameManager : MonoBehaviour, IBatchUpdate
	{
		bool initiated = false;
		public bool GamePaused;
		public float GameTime = 1f;
		public GameObject ForeGround;

		private bool _resetPressed = false;
		[HideInInspector]
		public bool Loading { get; set; } = false;

		public static GameManager INSTANCE;

		private bool _buttonWasUp = true;

		[SerializeField]
		KeyCode gameResetKey = KeyCode.P;
		[SerializeField]
		private float idleModeTime = 90f;
		private float idleTime = 0f;
		private bool idleMode = false;
		[SerializeField]
		private GameObject _videoPanel;
		[SerializeField]
		private GameObject _videoPanelPrefab;
		[SerializeField]
		private GameObject _musicPlayerPrefab;
		private MusicPlayer _musicPlayer;
		[SerializeField]
		private AudioMixerGroup _levelMusicMixerGroup;
		[SerializeField]
		private UpdateManager _updateManager;
		[SerializeField]
		private SelectionManager _selectionManager;
		[SerializeField]
		private PowerManager _powerManager;
		[SerializeField]
		private SaveDataManager _saveDataManager;
		[SerializeField]
		private InputManager _inputManager;
		[SerializeField]
		private DialogueDisplayManager _dialogueDisplayManager;
		[SerializeField]
		private CameraManager _cameraManager;
		private int _currentSceneIndex;
		public Transform PlayerTrans;
		private PlayerState _playerState;
		private PlayerMovement _playerMovement;
		private PlayerActions _playerActions;
		public SoundParameterSettings SoundConfig;
		public bool DropDownOptionsAvailable = false;
		private int _currentLevelBuildIndex = -1;
		private bool _optionsOpen;
		private bool _optionsStateJustChanged;

		private string _currentPlayerStart = "PlayerStartPosition";
		private string _currentCameraStart = "MainCameraStartPosition";

		public string WasRemovedSuffix = "wasRemoved";
		public bool OptionsOpen
		{
			get { return _optionsOpen; }
			set
			{
				_optionsOpen = value;
				_playerState.Frozen = value;
				_playerState.CantMove = value;
			}
		}

		public GameObject hiddenMenuUI;

		private void Awake()
		{
			InitializeThis();
			_saveDataManager.InitializeThis();
			_updateManager.InitializeThis();
			_inputManager.InitializeThis();
			_powerManager.InitializeThis();
			_selectionManager.InitializeThis();
			_dialogueDisplayManager.InitializeThis();
			_cameraManager.InitializeThis();
			GamePaused = false;
		}
		public void InitializeThis()
		{
			if (initiated)
			{
				return;
			}
			if (INSTANCE == null)
			{
				INSTANCE = this;
			}
			else if (INSTANCE != this)
			{
				Destroy(this);
				return;
			}
			PlayerTrans.gameObject.SetActive(false);
			if (SceneManager.sceneCount == 1)
			{
				SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
			}
			if (!_videoPanel)
			{
				_videoPanel = GameObject.Find("VideoPanel");
				if (!_videoPanel)
				{
					Canvas UICanvas = GameObject.FindObjectOfType<Canvas>();
					if (!UICanvas)
					{
						Debug.LogError("Could find neither Video panel nor the UI canvas. idle video will not work");
					}
					else
					{
						_videoPanel = Instantiate(_videoPanelPrefab, UICanvas.transform);
						_videoPanel.SetActive(false);
					}
				}
				else
				{
					_videoPanel.SetActive(false);
				}
			}
			_currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
			_musicPlayer = MusicPlayer.Instance;
			if (!_musicPlayer)
			{
				_musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();
				if (!_musicPlayer)
				{
					_musicPlayer = Instantiate(_musicPlayerPrefab).GetComponent<MusicPlayer>();
				}
			}

			if (!_playerState)
			{
				_playerState = PlayerTrans.GetComponent<PlayerState>();
			}
			if (!_playerMovement)
			{
				_playerMovement = PlayerTrans.GetComponent<PlayerMovement>();
			}
			if (!_playerActions)
			{
				_playerActions = PlayerTrans.GetComponent<PlayerActions>();
			}
			initiated = true;
		}
		private void OnEnable()
		{
			if (UpdateManager.Instance)
			{
				UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
			}
			else
			{
				GameObject.FindObjectOfType<UpdateManager>().RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
			}
		}
		private void OnDisable()
		{
			UpdateManager.Instance.DeregisterSlicedUpdate(this);
		}
#if UNITY_EDITOR
		private void Update()
		{
			if (!UpdateManager.Instance.debugMode)
			{
				return;
			}
			BatchUpdate();
		}
#endif
		public void BatchUpdate()
		{
			if (idleMode)
			{
				if (Input.anyKey)
				{
					SwitchScene("", "IntroVideo");
				}
			}
			if (!Input.anyKey && _currentSceneIndex != 1)
			{
				idleTime += Time.deltaTime;
				if (idleTime >= idleModeTime)
				{
					VideoPlayer vidPlayer = _videoPanel.GetComponentInChildren<VideoPlayer>();
					if (vidPlayer.clip)
					{
						_musicPlayer.StopMusicImmediate();
						_videoPanel.SetActive(true);
						GamePaused = true;
						idleMode = true;
					}
					else
					{
						SwitchScene("", "IntroVideo");
					}
				}
			}
			else
			{
				idleTime = 0;
			}

			Time.timeScale = GameTime;
			if (Input.GetButtonUp("Start")) _buttonWasUp = true;
			if (Input.GetButtonDown("Start") && _buttonWasUp)
			{
				_buttonWasUp = false;
				TogglePause();
			}

			//    screenText.text = gameTime.ToString();


			else if (Input.anyKeyDown && _resetPressed && !Input.GetButtonDown("Reset"))
			{
				_resetPressed = false;
				Camera.main.transform.Find("Reset").GetComponent<SpriteRenderer>().enabled = false;
			}


			if (Input.GetButtonDown("Reset") && !GameManager.INSTANCE._optionsOpen)
			{
				Reset();
			}
			#region cheatKeys

			//#if UNITY_EDITOR
			//            if (Input.GetKeyDown(KeyCode.KeypadPlus))
			//            {
			//                gameTime *= 2f;
			//            }
			//            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			//            {
			//                gameTime *= 0.5f;
			//            }
			//            else if (Input.GetKeyDown(KeyCode.KeypadEnter))
			//            {
			//                gameTime = 1f;
			//            }
			//            //LEVEL DEBUG
			//            if (Input.GetKeyDown(KeyCode.Alpha1))
			//            {
			//                SceneManager.LoadScene("Tutorial1", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKeyDown(KeyCode.Alpha2))
			//            {
			//                SceneManager.LoadScene("Tutorial2", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKeyDown(KeyCode.Alpha3))
			//            {
			//                SceneManager.LoadScene("Tutorial3", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKeyDown(KeyCode.Alpha4))
			//            {
			//                SceneManager.LoadScene("Tutorial4", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKeyDown(KeyCode.Alpha5))
			//            {
			//                SceneManager.LoadScene("Level1", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKeyDown(KeyCode.Alpha6))
			//            {
			//                SceneManager.LoadScene("Tutorial2", LoadSceneMode.Single);
			//            }
			//#endif
			//            if (Input.GetKeyDown(gameResetKey))
			//            {
			//                SceneManager.LoadScene(0);
			//            }

			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha1))
			//            {
			//                Debug.Log("Change Level");
			//                SceneManager.LoadScene("Tutorial1", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha2))
			//            {
			//                SceneManager.LoadScene("Tutorial2", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha3))
			//            {
			//                SceneManager.LoadScene("Tutorial3", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha4))
			//            {
			//                SceneManager.LoadScene("Tutorial4", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha5))
			//            {
			//                SceneManager.LoadScene("Level1", LoadSceneMode.Single);
			//            }
			//            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Alpha6))
			//            {
			//                //SceneManager.LoadScene("Level2", LoadSceneMode.Single);
			//            }
			if (Input.GetKeyDown(KeyCode.M))
			{
				if (!_musicPlayer._musicPaused)
				{
					_musicPlayer.PauseMusic();
				}
				else
				{
					_musicPlayer.ResumeMusic();
				}
			}
			#endregion
			if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetAxis("Start") > 0) && !_optionsStateJustChanged && SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name != "MainMenu" && !_optionsStateJustChanged && SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name != "Options_MainMenu" && SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name != "IntroVideo" && SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name != "OutroVideo" && SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name != "creditSceneDCP") //open and close menu with escape
			{
				//Debug.Log($"Escape keydown is {Input.GetKeyDown(KeyCode.Escape)} and start axis values is {Input.GetAxis("Start")}");
				if (!_optionsOpen)
				{
					OpenMenu();
					_optionsStateJustChanged = true;
				}
				else
				{
					CloseMenu();
					_optionsStateJustChanged = true;
				}
			}
			if ((Input.GetKeyUp(KeyCode.Escape) || Input.GetAxis("Start") <= 0) && _optionsStateJustChanged)
			{
				_optionsStateJustChanged = false;
			}
		}

		public void Reset()
		{
			if (!_resetPressed)
			{
				_resetPressed = true;
				Camera.main.transform.Find("Reset").GetComponent<SpriteRenderer>().enabled = true;
				return;
			}

			if (_resetPressed)
			{
				_resetPressed = false;
				ReloadLevel();
				return;
			}

		}

		public void SwitchScene(string currentSceneName, string nextSceneName, string playerTarget = "", string cameraTarget = "")
		{
			if (_playerActions._selectionChannel.CurrentPickup)
			{
				Debug.Log($"Scene name is {_playerActions._selectionChannel.CurrentPickup.gameObject.scene.name}");
			}
			_currentPlayerStart = playerTarget;
			_currentCameraStart = cameraTarget;
			StartCoroutine(SwitchSceneCoroutine(currentSceneName, nextSceneName));
		}

		private IEnumerator SwitchSceneCoroutine(string currentSceneName, string nextSceneName)
		{
			DeactivatePlayer();
			if (PowerManager.INSTANCE._globalPowerIsOn)
			{
				PowerManager.INSTANCE.Switch();
			}
			if (!Loading)
			{
				Loading = true;
				Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
				if (!nextScene.isLoaded)
				{
					yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
				}
				nextScene = SceneManager.GetSceneByName(nextSceneName);
				SceneSetup targetSceneSetup = LogicUtility.GetFirstFoundInScene<SceneSetup>(nextScene);
				if (targetSceneSetup.LoadSceneData) //COMMENT OUT TO DISABLE SAVE GAME
				{
					SaveDataManager.INSTANCE.LoadSceneData(nextScene); //loading scene before transferring the current pickup to ensure the pickup does not get loaded in twice
				}//END OF COMMENT OUT TO DISABLE SAVE GAME
				for (int i = 0; i < SceneManager.sceneCount; ++i)
				{
					Scene targetScene = SceneManager.GetSceneAt(i);
					if (targetScene.buildIndex.Equals(0))
					{
						continue;
					}
					if (targetScene.name.Equals(nextSceneName))
					{
						continue;
					}
					SaveDataManager.INSTANCE.SaveSceneData(targetScene); //COMMENT OUT TO DISABLE SAVE GAME
					if (_playerActions._selectionChannel.CurrentPickup) // transfer the current pickup of the player to the next scene, can't store currentpickup bc it might change in the transferToScene method
					{
						if (_playerActions._selectionChannel.CurrentPickup.gameObject.scene == targetScene)
						{
							TransferPickupToScene(_playerActions._selectionChannel.CurrentPickup, currentSceneName, nextScene);
						}
					}//END OF COMMENT OUT TO DISABLE SAVE GAME
					yield return SceneManager.UnloadSceneAsync(targetScene);
				}
				PowerManager.INSTANCE.UpdateLight();
				Loading = false;
				if (_playerActions._selectionChannel.CurrentPickup)
				{
					_playerActions.PickUpItem(_playerActions._selectionChannel.CurrentPickup);
				}
				_currentSceneIndex = nextScene.buildIndex;
				//Debug.Log($"Currently held pickup at end of loading is is {_playerActions._selectionChannel.CurrentPickup}");
			}
		}

		void TogglePause()
		{
			GamePaused = !GamePaused;
			if (GamePaused)
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = GameTime;
			}
		}

		public void SetupScene(SceneSetup sceneSetupData)
		{
			if (!_currentCameraStart.Equals(""))
			{
				if (_currentPlayerStart.Equals(""))
				{
					if (sceneSetupData.mainCamStartPositions.GetItemByName(_currentCameraStart))
					{
						MovePlayerTo(sceneSetupData.mainCamStartPositions.GetItemByName(_currentCameraStart).position);
					}
				}
				CameraManager.INSTANCE.defaultPlayerCam.Follow = sceneSetupData.mainCamStartPositions.GetItemByName(_currentCameraStart);
			}
			else
			{
				CameraManager.INSTANCE.defaultPlayerCam.Follow = sceneSetupData.mainCamStartPositions[0];
			}

			if (sceneSetupData.playerStartPositions.GetItemByName(_currentPlayerStart))
			{
				CameraManager.INSTANCE.defaultPlayerCam.Follow = PlayerTrans;
				MovePlayerTo(sceneSetupData.playerStartPositions.GetItemByName(_currentPlayerStart).position);
				PlayerTrans.gameObject.SetActive(true);
				ActivatePlayer();
			}
			//else
			//{
			//	CameraManager.INSTANCE.defaultPlayerCam.Follow = null;
			//	PlayerTrans.gameObject.SetActive(false);
			//}
			PowerManager.INSTANCE._foreGroundDarkness = sceneSetupData.foreGroundDarkness; //we want to set it null if it's not set so null check isn't necessary
			PowerManager.INSTANCE.CurrentLightMode = sceneSetupData.sceneDefaultLightMode; //TODO also use playerprefs or some kind of safe system to accord for permanent scene changes due to gameplay instead of loading default state all the time
			DropDownOptionsAvailable = sceneSetupData.DropDownOptionsEnabled;

			_currentLevelBuildIndex = sceneSetupData.gameObject.scene.buildIndex;
			SetMouseState(sceneSetupData.LockAndHideCursor);
			_musicPlayer.StartLevelMusic(sceneSetupData.levelMusicIndex);
			CinemachineFramingTransposer transposer = CameraManager.INSTANCE.defaultPlayerCam.GetCinemachineComponent<CinemachineFramingTransposer>();
			if (transposer)
			{
				transposer.m_TrackedObjectOffset.y = sceneSetupData.CamSettings.Elevation;
			}
		}

		private void ReloadLevel()
		{
			if (!Loading)
			{
				StartCoroutine(ReloadLevelCoroutine());
			}
		}

		private IEnumerator ReloadLevelCoroutine()
		{
			InputManager.INSTANCE.blockAllInputs = true;
			Loading = true;

			//yield return MainCamController.FadeOut(); //TODO new fadout with CameraManager
			for (int i = 0; i < SceneManager.sceneCount; ++i)
			{
				Scene targetScene = SceneManager.GetSceneAt(i);
				if (targetScene.buildIndex.Equals(_currentLevelBuildIndex))
				{
					AsyncOperation sceneUnloadProcess = SceneManager.UnloadSceneAsync(targetScene);
					while (!sceneUnloadProcess.isDone)
					{
						yield return new WaitForEndOfFrame();
					}
					break;
				}
			}
			AsyncOperation sceneLoadProcess = SceneManager.LoadSceneAsync(_currentLevelBuildIndex);
			while (!sceneLoadProcess.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			Loading = false;
			//yield return MainCamController.FadeIn(); //TODO new fadein with CameraManager
			InputManager.INSTANCE.blockAllInputs = false;
		}

		void OpenMenu()
		{
			//Debug.Log("loading Options");
			OptionsOpen = true;
			SceneManager.LoadScene("Options", LoadSceneMode.Additive);
			SetMouseState(false);
		}
		void CloseMenu()
		{
			//Debug.Log("unloading Options");
			OptionsOpen = false;
			SceneManager.UnloadSceneAsync("Options");
			SetMouseState(true);
		}

		private void DeactivatePlayer()
		{
			_playerState.Frozen = true;
			_playerMovement.Rb2d.bodyType = RigidbodyType2D.Kinematic;
		}

		private void ActivatePlayer()
		{
			_playerState.Frozen = false;
			_playerMovement.Rb2d.bodyType = RigidbodyType2D.Dynamic;
		}

		private void TransferPickupToScene(Pickupable targetPickup, string currentScene, Scene targetScene)
		{
			GameObject targetGO = targetPickup.gameObject;
			int targetID = targetPickup.GetUniqueID();
			CableAnchor carriedCableAnchor = (targetPickup as CableAnchor);
			if (carriedCableAnchor)
			{
				carriedCableAnchor._otherAnchor._currentSocket?.Eject();
				targetGO = carriedCableAnchor._cableScript.gameObject;
			}
			GameData currentGameData = SaveDataManager.INSTANCE.currentGameData;
			SceneData currentSceneData = currentGameData.GetOrCreateSceneData(currentScene);
			SceneData targetSceneData = currentGameData.GetOrCreateSceneData(targetScene.name);
			targetGO.transform.parent = null;

			PickupableSaveData pickupDataEntry = currentSceneData.AddedPickups.GetByID(targetPickup.GetUniqueID());
			CableSaveData cableDataEntry = null;
			if (carriedCableAnchor)
			{
				cableDataEntry = currentSceneData.AddedCables.GetByID(carriedCableAnchor._cableScript.GetUniqueID());
			}
			bool wasAddedToScene = pickupDataEntry != null || cableDataEntry != null;
			SaveData originalPickupInTargetScene = carriedCableAnchor ? targetSceneData.CableData.GetByID(carriedCableAnchor._cableScript.GetUniqueID()) : targetSceneData.PickupableData.GetByID(targetID);
			if (!wasAddedToScene)
			{
				if (carriedCableAnchor)
				{
					cableDataEntry = currentGameData.GetOrCreateSceneData(carriedCableAnchor.gameObject.scene.name).CableData.GetByID(carriedCableAnchor._cableScript.GetUniqueID());
					cableDataEntry.RemovedFromScene = true;
					carriedCableAnchor._cableScript.OverWriteIntoGameData(ref currentGameData, new CableSaveData(cableDataEntry));
				}
				else
				{
					pickupDataEntry = currentGameData.GetOrCreateSceneData(targetPickup.gameObject.scene.name).PickupableData.GetByID(targetPickup.GetUniqueID());
					pickupDataEntry.RemovedFromScene = true;
					targetPickup.OverWriteIntoGameData(ref currentGameData, new PickupableSaveData(pickupDataEntry));
				}
			}

			if (LogicUtility.IsNullOrDestroyed(originalPickupInTargetScene))
			{
				SceneManager.MoveGameObjectToScene(targetGO, targetScene);
			}
			else
			{
				targetPickup = LogicUtility.GetPickupableByID<Pickupable>(targetScene, targetID);
				carriedCableAnchor = targetPickup as CableAnchor;
				if (carriedCableAnchor)
				{
					carriedCableAnchor._cableScript.transform.position += targetPickup.transform.position - PlayerTrans.position;
					cableDataEntry = originalPickupInTargetScene as CableSaveData;
					cableDataEntry.RemovedFromScene = false;
				}
				else
				{
					pickupDataEntry = originalPickupInTargetScene as PickupableSaveData;
					pickupDataEntry.RemovedFromScene = false;
				}
				_playerActions._selectionChannel.CurrentPickup = targetPickup;
			}

			if (carriedCableAnchor) //writing into the target scene and loading the data afterwards
			{
				SaveDataManager.INSTANCE.RemoveByID(ref currentGameData.AllSceneData[currentGameData.AllSceneData.IndexOf(currentSceneData)].AddedCables, cableDataEntry.UniqueID);
				cableDataEntry.RemovedFromScene = false;
				carriedCableAnchor._cableScript.OverWriteIntoGameData(ref currentGameData, cableDataEntry);
				carriedCableAnchor._cableScript.LoadData(currentGameData);
				carriedCableAnchor._cableScript.ResetCableToPosition(PlayerTrans.position);
			}
			else
			{
				SaveDataManager.INSTANCE.RemoveByID(ref currentGameData.AllSceneData[currentGameData.AllSceneData.IndexOf(currentSceneData)].AddedPickups, pickupDataEntry.UniqueID);
				pickupDataEntry.RemovedFromScene = false;
				targetPickup.OverWriteIntoGameData(ref currentGameData, pickupDataEntry);
				targetPickup.LoadData(currentGameData);
			}
			if (!LogicUtility.IsNullOrDestroyed(originalPickupInTargetScene))
			{
				Destroy(targetGO);
			}
			SaveDataManager.INSTANCE.SaveGameData(currentGameData);
		}

		private void SetMouseState(bool lockedAndHidden)
		{
			Cursor.lockState = lockedAndHidden ? CursorLockMode.Locked : CursorLockMode.Confined;
			Cursor.visible = !lockedAndHidden;
		}

		public void MovePlayerTo(Vector3 targetPosition)
		{
			PlayerActions playerActions = PlayerTrans.GetComponent<PlayerActions>();
			Pickupable carriedPickup = playerActions._selectionChannel.CurrentPickup;
			if (carriedPickup)
			{
				GameObject carriedPickupGO = carriedPickup.gameObject;
				CableAnchor carriedCableAnchor = carriedPickup as CableAnchor;
				if (carriedCableAnchor)
				{
					carriedPickupGO = carriedCableAnchor._cableScript.gameObject;
				}
				Scene originalScene = carriedPickupGO.scene;
				Transform originalParent = carriedPickupGO.transform.parent;
				carriedPickupGO.transform.parent = playerActions.CarryOffset;
				PlayerTrans.position = targetPosition;
				carriedPickupGO.transform.parent = null;
				SceneManager.MoveGameObjectToScene(carriedPickupGO, originalScene);
				carriedPickupGO.transform.parent = originalParent;
			}
			else
			{
				PlayerTrans.position = targetPosition;
			}
		}

		//private void MovePickupToPlayer(GameObject targetGO, Transform transformToMoveOverwrite = null)
		//{
		//	Transform transToMove = transformToMoveOverwrite ? transformToMoveOverwrite : targetGO.transform;
		//	GameObject tempParent = new GameObject("MoveParent");
		//	SceneManager.MoveGameObjectToScene(tempParent, targetGO.scene);
		//	tempParent.transform.position = targetGO.transform.position;
		//	Transform originalParent = transToMove.parent;
		//	transToMove.parent = tempParent.transform;
		//	tempParent.transform.position = PlayerTrans.position;
		//	transToMove.parent = originalParent;
		//	Destroy(tempParent);
		//}
	}
}
