using Ampere.Utility;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ampere
{
	public class SaveDataManager : MonoBehaviour
	{
		[Header("File Storage Config")]
		[SerializeField]
		private string fileName = "base";
		public GameData currentGameData;
		private FileDataHandler fileDataHandler;
		private GameDataCache gameDataCache;

		public GameObject bulbPrefab;
		public GameObject cablePrefab;

		public static SaveDataManager INSTANCE { get; private set; }
		public void InitializeThis()
		{

			if (INSTANCE != null)
			{
				if (INSTANCE != this)
				{
					Destroy(this);
				}
			}
			else
			{
				INSTANCE = this;
			}
			this.fileDataHandler = new(Application.persistentDataPath, fileName);
#if UNITY_EDITOR
			gameDataCache = (GameDataCache)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/GameDataCache.asset", typeof(GameDataCache));
#endif
			currentGameData = FetchGameData();
		}

		public void SaveSceneData(Scene targetScene)
		{
			List<ISaveData> allSaveableSceneObjects = LogicUtility.GetAllLoadedObjectsInScene<ISaveData>(targetScene);
			List<Pickupable> allPickupAbleSceneObjects = LogicUtility.GetAllLoadedObjectsInScene<Pickupable>(targetScene); //TODO: put this list global so that objects that already saved can dequeue

			for (int i = 0; i < allPickupAbleSceneObjects.Count; ++i)
			{
				if (!allPickupAbleSceneObjects[i])
				{
					continue;
				}
				(allPickupAbleSceneObjects[i] as ISaveData).WriteIntoGameData(ref currentGameData);
			}

			for (int i = 0; i < allSaveableSceneObjects.Count; ++i)
			{
				if (allSaveableSceneObjects[i] == null)
				{
					continue;
				}
				allSaveableSceneObjects[i].WriteIntoGameData(ref currentGameData);
			}
			SaveGameData(currentGameData);
		}
		public void LoadSceneData(Scene targetScene)
		{
			List<ISaveData> allSaveDataObjects = LogicUtility.GetAllLoadedObjectsInScene<ISaveData>(targetScene);
			SceneData targetSceneData = currentGameData.GetOrCreateSceneData(targetScene.name);
			for (int i = 0; i < targetSceneData.AddedPickups.Count; ++i)
			{
				SpawnPickupable(targetSceneData.AddedPickups[i], targetScene);
			}
			for (int i = 0; i < targetSceneData.AddedCables.Count; ++i)
			{
				SpawnCable(targetSceneData.AddedCables[i], targetScene);
			}
			for (int i = 0; i < allSaveDataObjects.Count; ++i)
			{
				allSaveDataObjects[i].LoadData(currentGameData);
			}
		}

		public GameData FetchGameData()
		{
#if UNITY_EDITOR
			gameDataCache = (GameDataCache)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/GameDataCache.asset", typeof(GameDataCache));
#endif
			return fileDataHandler.Load();
		}

		public void SaveGameData(GameData gameData)
		{
#if UNITY_EDITOR
			Undo.RecordObject(gameDataCache, "Adding new data");
			gameDataCache.GameData = gameData;
			AssetDatabase.SaveAssets();
#endif
			currentGameData = gameData;
			fileDataHandler.Save(currentGameData);
		}

		private void OnApplicationQuit()
		{
			for (int i = 0; i < SceneManager.sceneCount; ++i)
			{
				if (SceneManager.GetSceneAt(i).buildIndex == 0)
				{
					continue;
				}
				SaveSceneData(SceneManager.GetSceneAt(i));
			}
		}

		private void SpawnPickupable(PickupableSaveData data, Scene targetScene)
		{
			if (data.Type.Equals(SelectableType.LightBulb))
			{
				SpawnSimplePickup(bulbPrefab, data, targetScene);
			}
		}

		private void SpawnSimplePickup(GameObject originalGO, PickupableSaveData saveData, Scene targetScene)
		{
			GameObject newPickup = GameObject.Instantiate(originalGO);
			SceneManager.MoveGameObjectToScene(newPickup, targetScene);
			newPickup.transform.parent = LogicUtility.FindGameObjectInTargetScene("AllProps", targetScene)?.transform;
			SetSimplePickupData(newPickup.GetComponent<Pickupable>(), saveData);
		}
		private void SetSimplePickupData(Pickupable targetPickupable, PickupableSaveData saveData)
		{
			targetPickupable.GetComponent<Pickupable>().SetUniqueID(saveData.UniqueID);
			targetPickupable.gameObject.SetActive(saveData.IsActive);
			targetPickupable.transform.position = saveData.Position;
		}

		private void SpawnCable(CableSaveData data, Scene targetScene)
		{
			GameObject newCable = GameObject.Instantiate(cablePrefab);
			SceneManager.MoveGameObjectToScene(newCable, targetScene);
			newCable.transform.parent = LogicUtility.FindGameObjectInTargetScene("AllElectroCables", targetScene)?.transform;
			Cable cable = newCable.GetComponent<Cable>();
			SetSimplePickupData(cable._anchorA.GetComponent<Pickupable>(), data.AnchorA);
			SetSimplePickupData(cable._anchorB.GetComponent<Pickupable>(), data.AnchorB);
			cable.SetUniqueID(data.UniqueID);
			cable.gameObject.SetActive(data.IsActive);
			cable.CreateCable(data);
		}

		public void RemoveByID<T>(ref List<T> targetList, int ID) where T : SaveData
		{
			for (int i = targetList.Count - 1; i >= 0; --i)
			{
				if (targetList[i].UniqueID.Equals(ID))
				{
					targetList.RemoveAt(i);
				}
			}
		}

		public void DeleteSaveFile()
		{
			if (fileDataHandler == null)
			{
				this.fileDataHandler = new(Application.persistentDataPath, fileName);
			}
			fileDataHandler.DeleteSaveFile();
		}
	}
}