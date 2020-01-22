using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateThrowees : MonoBehaviour {
	public GameObject sampleThrowee;
	public int throweeNumber;
	public List<GameObject> throweeList;
	GameObject currentThrowee;
	// Use this for initialization
	void Awake () {
		throweeList = new List<GameObject>();
		for (int i=0;i<throweeNumber;i++) {
			currentThrowee = Instantiate (sampleThrowee,this.transform);
			throweeList.Add(currentThrowee);
			throweeList [i].SetActive (false);
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
