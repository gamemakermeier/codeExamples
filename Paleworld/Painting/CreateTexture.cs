using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTexture : MonoBehaviour {
	public Texture texture;
	public Texture2D levelTexture;
	// Use this for initialization
	void Awake () {
		levelTexture = Instantiate (texture) as Texture2D;

	}

	// Update is called once per frame
	void Update () {
		
	}
	void FixedUpdate(){
		//levelTexture.Apply ();
	}
}
