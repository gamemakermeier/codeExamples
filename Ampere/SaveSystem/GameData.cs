using Ampere.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Ampere
{
	[System.Serializable]
	public class GameData
	{
		public List<SceneData> AllSceneData = new();
		public List<int> AllGameIDs;
		public static int IDMaxRange = 16777216;
		public GameData(List<SceneData> baseSceneData, List<int> baseIDList)
		{
			AllSceneData = baseSceneData;
			AllGameIDs = baseIDList;
		}
		public SceneData GetOrCreateSceneData(string sceneName)
		{
			for (int i = 0; i < AllSceneData.Count; ++i)
			{
				if (AllSceneData[i].SceneName == sceneName)
				{
					return AllSceneData[i];
				}
			}
			SceneData newSceneData = new(sceneName);
			AllSceneData.Add(newSceneData);
			return newSceneData;
		}

		public (int sceneDataIndex, int dataListIndex) GetSaveDataIndizes<T>(int uniqueID, bool targetAddedPickupsList = false) where T : SaveData
		{
			for (int i = 0; i < AllSceneData.Count; ++i)
			{
				int dataIndex = -1;
				if (targetAddedPickupsList)
				{
					if (typeof(T).Equals(typeof(PickupableSaveData)))
					{
						dataIndex = GetSaveDataIndex(uniqueID, AllSceneData[i].AddedPickups);
					}
					if (typeof(T).Equals(typeof(CableSaveData)))
					{
						dataIndex = GetSaveDataIndex(uniqueID, AllSceneData[i].AddedCables);
					}
				}
				else if (typeof(T).Equals(typeof(PickupableSaveData)))
				{
					dataIndex = GetSaveDataIndex(uniqueID, AllSceneData[i].PickupableData);
				}
				else if (typeof(T).Equals(typeof(CableSaveData)))
				{
					dataIndex = GetSaveDataIndex(uniqueID, AllSceneData[i].CableData);
				}
				else if (typeof(T).Equals(typeof(PickupReceiverSaveData)))
				{
					dataIndex = GetSaveDataIndex(uniqueID, AllSceneData[i].ReceiverData);
				}
				if (dataIndex == -1)
				{
					continue;
				}
				return (i, dataIndex);
			}
			return (-1, -1);
		}
		private int GetSaveDataIndex<T>(int uniqueID, List<T> targetList) where T : SaveData
		{
			for (int i = 0; i < targetList.Count; ++i)
			{
				if (targetList[i].UniqueID.Equals(uniqueID))
				{
					return i;
				}
			}
			return -1;
		}
		public void InsertDataIntoCorrectList(SaveData saveData, bool fromOtherScene = false)
		{

			(int sceneIndex, int dataListIndex) = (-1, -1);
			if (fromOtherScene || saveData as PickupableSaveData != null || saveData as CableSaveData != null)
			{
				if (saveData as PickupableSaveData != null)
				{
					(sceneIndex, dataListIndex) = GetSaveDataIndizes<PickupableSaveData>(saveData.UniqueID, fromOtherScene);
				}
				if (saveData as CableSaveData != null)
				{
					(sceneIndex, dataListIndex) = GetSaveDataIndizes<CableSaveData>(saveData.UniqueID, fromOtherScene);
				}
			}
			if (saveData as PickupReceiverSaveData != null)
			{
				(sceneIndex, dataListIndex) = GetSaveDataIndizes<PickupReceiverSaveData>(saveData.UniqueID);
			}
			if (dataListIndex < 0)
			{
				SceneData targetSceneData = GetOrCreateSceneData(saveData.OriginalSceneName);
				if (fromOtherScene)
				{
					if (saveData as PickupableSaveData != null)
					{
						targetSceneData.AddedPickups.Add(saveData as PickupableSaveData);
						return;
					}
					if (saveData as CableSaveData != null)
					{
						targetSceneData.AddedCables.Add(saveData as CableSaveData);
						return;
					}
				}
				if (saveData as PickupableSaveData != null)
				{
					targetSceneData.PickupableData.Add(saveData as PickupableSaveData);
					return;
				}
				if (saveData as CableSaveData != null)
				{
					targetSceneData.CableData.Add(saveData as CableSaveData);
					return;
				}
				if (saveData as PickupReceiverSaveData != null)
				{
					targetSceneData.ReceiverData.Add(saveData as PickupReceiverSaveData);
					return;
				}
			}
			else
			{
				if (fromOtherScene)
				{
					if (saveData as PickupableSaveData != null)
					{
						saveData.OriginalSceneName = AllSceneData[sceneIndex].AddedPickups[dataListIndex].OriginalSceneName;
						AllSceneData[sceneIndex].AddedPickups[dataListIndex] = saveData as PickupableSaveData;
						return;
					}
					if (saveData as CableSaveData != null)
					{
						saveData.OriginalSceneName = AllSceneData[sceneIndex].AddedCables[dataListIndex].OriginalSceneName;
						AllSceneData[sceneIndex].AddedCables[dataListIndex] = saveData as CableSaveData;
						return;
					}
				}
				if (saveData as PickupableSaveData != null)
				{
					AllSceneData[sceneIndex].PickupableData[dataListIndex] = saveData as PickupableSaveData;
					return;
				}
				if (saveData as CableSaveData != null)
				{
					AllSceneData[sceneIndex].CableData[dataListIndex] = saveData as CableSaveData;
					return;
				}
				if (saveData as PickupReceiverSaveData != null)
				{
					AllSceneData[sceneIndex].ReceiverData[dataListIndex] = saveData as PickupReceiverSaveData;
					return;
				}
			}
		}

		public int ValidateIDUniqueness(int ID, bool freshID)
		{
			int originalID = ID;
			List<int> idsThatWereAlreadyTried = new();
			while (LogicUtility.GetDuplicateCount(ID, AllGameIDs) >= (freshID ? 1 : 2) || ID.Equals(0))
			{
				idsThatWereAlreadyTried.Add(ID);
				if (idsThatWereAlreadyTried.Count >= GameData.IDMaxRange - 2)
				{
					Debug.LogError("Tried all possible IDs without finding a free one");
					ID = -1;
					break;
				}
				ID = Random.Range(1, GameData.IDMaxRange);
			}

			if ((ID != originalID || !AllGameIDs.Contains(ID)) && !ID.Equals(-1))
			{
				AllGameIDs.Add(ID);
			}
			return ID;
		}
	}

	[System.Serializable]
	public class SceneData
	{
		public string SceneName;
		public List<PickupableSaveData> PickupableData = new();
		public List<SaveData> TriggerData = new();
		public List<CableSaveData> CableData = new();
		public List<PickupReceiverSaveData> ReceiverData = new();
		public List<PickupableSaveData> AddedPickups = new();
		public List<CableSaveData> AddedCables = new();

		public SceneData(string sceneName)
		{
			SceneName = sceneName;
			PickupableData = new();
			TriggerData = new();
			CableData = new();
			ReceiverData = new();
			AddedPickups = new();
			AddedCables = new();
		}
	}

	[System.Serializable]
	public class PickupableSaveData : MoveAbleSaveData
	{
		public SelectableType Type;
		public bool RemovedFromScene = false;
		public PickupableSaveData(SelectableType type = SelectableType.None, int uniqueID = 0, bool isActive = false, string originalSceneName = "", Vector3 position = default, Quaternion rotation = new Quaternion(), bool removedFromScene = false) : base(uniqueID, isActive, originalSceneName, position, rotation)
		{
			Type = type;
			UniqueID = uniqueID;
			IsActive = isActive;
			OriginalSceneName = originalSceneName;
			Position = position;
			Rotation = rotation;
			RemovedFromScene = removedFromScene;
		}
		public PickupableSaveData(PickupableSaveData originalToCopyFrom) : base(originalToCopyFrom.UniqueID, originalToCopyFrom.IsActive, originalToCopyFrom.OriginalSceneName, originalToCopyFrom.Position, originalToCopyFrom.Rotation)
		{
			Type = originalToCopyFrom.Type;
			UniqueID = originalToCopyFrom.UniqueID;
			IsActive = originalToCopyFrom.IsActive;
			OriginalSceneName = originalToCopyFrom.OriginalSceneName;
			Position = originalToCopyFrom.Position;
			RemovedFromScene = originalToCopyFrom.RemovedFromScene;
		}
	}
	[System.Serializable]
	public class CableSaveData : SaveData
	{
		public CablePieceData[] CablePiecesData;
		public PickupableSaveData AnchorA;
		public PickupableSaveData AnchorB;
		public float Length;
		public bool RemovedFromScene = false;
		public CableSaveData(int uniqueID, bool isActive, string originalSceneName, CablePieceData[] cablePiecesData = null, PickupableSaveData anchorA = null, PickupableSaveData anchorB = null, float length = 0, bool removedFromScene = false) : base(uniqueID, isActive, originalSceneName)
		{
			UniqueID = uniqueID;
			IsActive = isActive;
			OriginalSceneName = originalSceneName;
			CablePiecesData = cablePiecesData;
			AnchorA = anchorA;
			AnchorB = anchorB;
			Length = length;
			RemovedFromScene = removedFromScene;
		}
		public CableSaveData(CableSaveData dataToCopy) : base(dataToCopy.UniqueID, dataToCopy.IsActive, dataToCopy.OriginalSceneName)
		{
			UniqueID = dataToCopy.UniqueID;
			IsActive = dataToCopy.IsActive;
			OriginalSceneName = dataToCopy.OriginalSceneName;
			CablePiecesData = LogicUtility.DeepCopyArray(dataToCopy.CablePiecesData);
			AnchorA = dataToCopy.AnchorA;
			AnchorB = dataToCopy.AnchorB;
			Length = dataToCopy.Length;
			RemovedFromScene = dataToCopy.RemovedFromScene;
		}
	}
	[System.Serializable]
	public class CablePieceData
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public float Length;
		public CablePieceData(Vector3 position, Quaternion rotation, float length)
		{
			Position = position;
			Rotation = rotation;
			Length = length;
		}
	}

	[System.Serializable]
	public class PickupReceiverSaveData : SaveData
	{
		public PickupableSaveData PluggedInPickupData;
		public PickupReceiverSaveData(int uniqueID, bool isActive, string originalSceneName, PickupableSaveData pluggedInPickupData) : base(uniqueID, isActive, originalSceneName)
		{
			UniqueID = uniqueID;
			IsActive = isActive;
			OriginalSceneName = originalSceneName;
			PluggedInPickupData = pluggedInPickupData;
		}
	}
	[System.Serializable]
	public class SaveData
	{
		public int UniqueID;
		public bool IsActive;
		public string OriginalSceneName;

		public SaveData(int uniqueID, bool isActive, string originalSceneName)
		{
			UniqueID = uniqueID;
			IsActive = isActive;
			OriginalSceneName = originalSceneName;
		}
	}

	[System.Serializable]
	public class MoveAbleSaveData : SaveData
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public MoveAbleSaveData(int uniqueID, bool isActive, string originalSceneName, Vector3 position, Quaternion rotation) : base(uniqueID, isActive, originalSceneName)
		{
			UniqueID = uniqueID;
			IsActive = isActive;
			OriginalSceneName = originalSceneName;
			Position = position;
			Rotation = rotation;
		}
	}
}