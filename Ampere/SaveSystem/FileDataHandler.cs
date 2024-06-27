using Ampere;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class FileDataHandler
{
	private string dataDirPath;
	private string dataFileName = "BYESave_01";

	public FileDataHandler(string dataDirPath, string dataFileName)
	{
		this.dataDirPath = dataDirPath;
		this.dataFileName = dataFileName;
	}

	public GameData Load()
	{
		string fullpath = Path.Combine(dataDirPath, dataFileName);
		GameData loadedData = null;
		if (!File.Exists(fullpath))
		{
			Debug.Log("No file found at path, creating a new one");
			Save(new GameData(new List<SceneData>(), new List<int>()));
		}
		try
		{
			string dataToLoad = "";
			using (FileStream stream = new(fullpath, FileMode.Open))
			{
				using (StreamReader reader = new(stream))
				{
					dataToLoad = reader.ReadToEnd();
				}
			}
			loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
		}
		catch (Exception e)
		{
			Debug.LogError($"Error when trying to load data from file {fullpath} {e}");
		}
		return loadedData;
	}

	public void Save(GameData data)
	{
		string fullpath = Path.Combine(dataDirPath, dataFileName);
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
			string dataToStore = JsonUtility.ToJson(data, true);
			using (FileStream stream = new(fullpath, FileMode.Create))
			{
				using (StreamWriter writer = new(stream))
				{
					writer.Write(dataToStore);
					Debug.Log($"Saved to {dataDirPath}");
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"Error when trying to save data to file {fullpath} {e}");
		}
	}

	public void DeleteSaveFile()
	{
		string fullpath = Path.Combine(dataDirPath, dataFileName);
		if (File.Exists(fullpath))
		{
			File.Delete(fullpath);
		}
	}
}
