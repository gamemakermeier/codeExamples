using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this singleton handles all game states, including the inventory
namespace StillWaters
{
	public class GameState
	{
		public string name;
		public float value;
		public List<GameStateDependable> dependables = new List<GameStateDependable>();

		public GameState(string _name, float _value)
		{
			name = _name;
			value = _value;
		}
	}

	public class InventorySlot
	{
		public Transform myTrans;
		public string mySlotName;
		public Grabable myItem;
	}

	public class GameStateManager : MonoBehaviour
	{
		public static GameStateManager instance;

		public int safeSlot = 0;
		List<GameState> gameStateList = new List<GameState>();

		public GameState flowerInMortar = new GameState("flowerInMortar", 0);
		public GameState lureFlurescent = new GameState("lureFlurescent", 0);
		public GameState fishCaught = new GameState("fishCaught", 0);

		public List<InventorySlot> inventorySlots = new List<InventorySlot>();
		public List<InventorySlot> freeInventorySlots = new List<InventorySlot>();

		// Start is called before the first frame update
		private void Awake()
		{
			//adding all defined Gamestates;
			gameStateList.Add(flowerInMortar);
			gameStateList.Add(lureFlurescent);

			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(this);
			}
		}
		private void Start()
		{
			//register all dependables in scenes
			GameStateDependable[] allDependables = Resources.FindObjectsOfTypeAll<GameStateDependable>();
			foreach (GameStateDependable _dependable in allDependables)
			{
				//Debug.Log(" Current Dependable name is " + _dependable.name);
				foreach (TargetState _targetState in _dependable.targetStates)
				{
					if (_targetState.stateName != "")
					{
						//Debug.Log("For " + _targetState.stateName + " I add " + _dependable.GetType().ToString() + " from " + _dependable.name);
						AddDependableToGameState(_targetState.stateName, _dependable);
					}
				}
				//Debug.Log("Next Dependable");
			}
			//now that we have everything connected, we adjust the dependables accordingly to initiate the start game state of the game
			foreach (GameState _gameState in gameStateList)
			{
				/*Debug.Log("Adjusting all " + _gameState.dependables.Count + " dependables for " + _gameState.name);
				Debug.Log("Dependebales are:");
				foreach (GameStateDependable _dependable in _gameState.dependables)
				{
					Debug.Log(_dependable.GetType().ToString() + " on " + _dependable.name);
				}
				*/
				AdjustDependables(_gameState);
			}

			foreach (InventorySlotRegisterer inventoryRegisterer in FindObjectsOfType<InventorySlotRegisterer>())
			{
				inventoryRegisterer.Register(inventorySlots);
			}
			foreach (InventorySlot inventorySlot in inventorySlots)
			{
				freeInventorySlots.Add(inventorySlot);
			}
		}

		public void RemoveItemFromInventory(Grabable item)
		{
			foreach (InventorySlot inventorySlot in inventorySlots)
			{
				if (inventorySlot.myItem == item)
				{
					inventorySlot.myItem = null;
					freeInventorySlots.Add(inventorySlot);
				}
			}
		}

		public void AddItemToInventorySlot(Grabable item, InventorySlot inventorySlot)
		{
			//check if the target inventory slot is still free, otherwise assign a random free one

			if (!freeInventorySlots.Contains(inventorySlot))
			{
				int newIndex = Random.Range(0, freeInventorySlots.Count);
				inventorySlot = freeInventorySlots[newIndex];
				freeInventorySlots.Remove(inventorySlot);
			}

			//set it to kinematic CHANGE in the future we might want physics behaviour within the inventory
			Rigidbody targetRig = item.myRig;
			if (targetRig)
			{
				targetRig.isKinematic = true;
			}
			item.grabAnchor.parent = inventorySlot.myTrans;
			item.grabAnchor.position = inventorySlot.myTrans.position;
			//we rotate so the items fit into the inventory slot
			item.grabAnchor.rotation = Quaternion.Euler(inventorySlot.myTrans.rotation.eulerAngles + item.inventoryRotationOffset);
			//and assign the currently grabbed item to that inventorySlot
			item.myInventorySlot = inventorySlot;
			inventorySlot.myItem = item;
			freeInventorySlots.Remove(inventorySlot);
		}


		public void ChangeGameState(string targetStateName, float value, bool addInsteadSet)
		{
			//Debug.Log("Changing gamestate " + targetStateName + " to " + value.ToString());
			foreach (GameState _gameState in gameStateList)
			{
				if (_gameState.name == targetStateName)
				{
					if (!addInsteadSet)
					{
						_gameState.value = value;
						AdjustDependables(_gameState);
					}
					else
					{
						_gameState.value += value;
						AdjustDependables(_gameState);
					}
					return;
				}
			}
			Debug.LogWarning("State you were trying to change didn't exist. adding the state " + targetStateName + " to the list");
			GameState newState = new GameState(targetStateName, value);
			gameStateList.Add(newState);
		}

		public bool CheckGameState(string _stateName, Vector2 _valueRange)
		{
			foreach (GameState _gameState in gameStateList)
			{
				if (_gameState.name == _stateName)
				{
					if (_gameState.value <= _valueRange.y && _gameState.value >= _valueRange.x)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			Debug.LogError("targeted gamestate " + _stateName + "was not found, but requested to be checked. Returning false by default");
			return false;
		}

		private bool CheckGameState(GameState _gameState, Vector2 _valueRange)
		{
			if (_gameState.value <= _valueRange.y && _gameState.value >= _valueRange.x)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddDependableToGameState(string _gameStateName, GameStateDependable _dependable)
		{
			foreach (GameState _gameState in gameStateList)
			{
				if (_gameState.name == _gameStateName)
				{
					_gameState.dependables.Add(_dependable);
					return;
				}
			}
			Debug.LogWarning("State you were trying to change didn't exist. adding the state " + _gameStateName + " to the list");
			GameState newState = new GameState(_gameStateName, 0);
			newState.dependables.Add(_dependable);
			gameStateList.Add(newState);

		}

		private void AdjustDependables(GameState _gameState)
		{
			foreach (GameStateDependable _dependable in _gameState.dependables)
			{
				foreach (TargetState _targetState in _dependable.targetStates)
				{
					if (_targetState.stateName == _gameState.name)
					{
						_dependable.enabled = (CheckGameState(_gameState,_targetState.activeValueRange));
						//Debug.Log("Setting " + _dependable.GetType().ToString() + " on " + _dependable.gameObject.name + " to " + _dependable.enabled);
					}
				}
			}
		}
	}
}
