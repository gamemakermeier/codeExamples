#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Ampere.Utility;
using UnityEditor;
using System.IO;
namespace Ampere
{
	public class BYEAssetModificationProcessor : AssetModificationProcessor
	{
		public static string[] OnWillSaveAssets(string[] paths)
		{
			string sceneName = string.Empty;

			foreach (string path in paths)
			{
				if (path.Contains(".unity"))
				{
					sceneName = Path.GetFileNameWithoutExtension(path);
				}
			}

			if (sceneName.Length == 0)
			{
				return paths;
			}
			Scene targetScene = SceneManager.GetSceneByName(sceneName);
			List<FauxGizmoScript> allFauxGizmosInScene = LogicUtility.GetAllLoadedObjectsInScene<FauxGizmoScript>(targetScene);

			for (int i = allFauxGizmosInScene.Count - 1; i >= 0; --i)
			{
				if (!allFauxGizmosInScene[i])
				{
					continue;
				}
				if (allFauxGizmosInScene[i].transform)
				{
					if (allFauxGizmosInScene[i].transform.parent)
					{
						if (allFauxGizmosInScene[i].transform.parent.name.Equals("gizmoParent"))
						{
							Object.DestroyImmediate(allFauxGizmosInScene[i].transform.parent.gameObject);
							continue;
						}
					}
				}
				Object.DestroyImmediate(allFauxGizmosInScene[i].transform.gameObject);
			}

			GameObject foregroundDarkness = LogicUtility.FindGameObjectInTargetScene("ForeGroundDarkness", targetScene);
			if (foregroundDarkness)
			{
				foregroundDarkness.SetActive(true);
			}


			SaveDataManager saveDataManager = new GameObject("savedataManager").AddComponent<SaveDataManager>();
			saveDataManager.InitializeThis();
			List <ISaveData> allSaveableObjects = LogicUtility.GetAllLoadedObjectsInScene<ISaveData>(targetScene);
			for (int i = 0; i < allSaveableObjects.Count; ++i)
			{
				Undo.RecordObject(allSaveableObjects[i] as MonoBehaviour, "validating and if needed changing unique ID");
				allSaveableObjects[i].SetUniqueID(SaveDataManager.INSTANCE.currentGameData.ValidateIDUniqueness(allSaveableObjects[i].GetUniqueID(), allSaveableObjects[i].GetUniqueID() == 0));
			}


			saveDataManager.SaveSceneData(targetScene);
			Object.DestroyImmediate(saveDataManager.gameObject);
			return paths;
		}
	}
}
#endif //UNITY_EDITOR