using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script is used to mark textures for change, so we only update those who actually got new color information.
//every paint ball (throweescript) registers the texture to this script, if it wasn't already in the texture list
public class UpdateTextures : MonoBehaviour
{
	public List<Texture2D> textureList;

	// Use this for initialization
	void Start()
	{

	}

	//every fixed update (probably better to use OnUpdate with an updatemanager but I didn't know that at the time), we apply the changes done to the textures that are marked for changes and clear the list afterwards
	void FixedUpdate()
	{
		if (textureList.Count > 0)
		{
			foreach (Texture2D tex in textureList)
			{
				tex.Apply();
			}
			textureList.Clear();
		}
	}
}
