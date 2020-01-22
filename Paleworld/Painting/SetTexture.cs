using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTexture : MonoBehaviour {
	public GameObject textureSetter;
	public MySplash splashScript;
	Vector2 colorSize;
	GameObject player;
	Throw playerThrowScript;
	Texture myTex;
	public Texture2D slideTexture;
	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player");
		playerThrowScript = player.GetComponent<Throw> ();
		myTex = Instantiate(textureSetter.GetComponent<CreateTexture>().levelTexture);
		GetComponent<MeshRenderer> ().material.mainTexture = myTex;
		colorSize.x = Mathf.Clamp ((playerThrowScript.splatSize / transform.localScale.x), 0, myTex.width);
		colorSize.y = Mathf.Clamp ((playerThrowScript.splatSize / transform.localScale.z), 0, myTex.height);
		splashScript.slideSplash = slideTexture.GetPixels (0, 0, (int)colorSize.x, (int)colorSize.y);
		Destroy (this);
	}

}
