using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTextures : MonoBehaviour {
	public List <Texture2D> textureList;

	// Use this for initialization
	void Start () {
		
	}

	void FixedUpdate(){
		if (textureList.Count > 0) {
			foreach (Texture2D tex in textureList) {
				tex.Apply ();
			}
			textureList.Clear ();
		}
	}
}
