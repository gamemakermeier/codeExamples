using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThroweeScript : MonoBehaviour
{
	public Vector3 myPosition;
	Vector3 playerPosition;
	GameObject player;
	movement playerMoveScript;
	Throw playerThrowScript;
	PlayerStatus playerStatus;
	public bool thrown;
	Color splashColor;
	RaycastHit splashPosition;
	Texture2D targetTexture;
	public Color[] color;
	public bool child;
	int fragNumber;
	GameObject frag;
	public Vector3 originalSize;
	public Texture2D tex;
	int startX;
	int startY;
	int spaceY;
	int spaceX;
	int splatX;
	int splatY;
	Vector3 origPos;
	ThroweeScript fragThrowScript;
	Vector3 parentSpeed;
	Rigidbody myRig;
	Rigidbody fragRig;
	Vector2 colorSize;
	bool splashPointFound;
	Color myColor;
	public SoundList splashSounds;
	AudioSource splashSound;
	public Vector2 randomPitchMinMax;
	public Vector2 randomVolumeParChil;
	float splashSoundVolOrig;
	float waitTime;
	MeshRenderer myMesh;
	SphereCollider myCollider;
	// Use this for initialization
	void Awake ()
	{
		myCollider = GetComponent<SphereCollider> ();
		myMesh = GetComponent<MeshRenderer> ();
		player = GameObject.Find ("Player");
		myRig = GetComponent<Rigidbody> ();
		playerMoveScript = player.GetComponent<movement> ();
		playerThrowScript = player.GetComponent<Throw> ();
		playerStatus = player.GetComponent<PlayerStatus> ();
		origPos = new Vector3 (0, 500, 0);
		originalSize = transform.localScale;

	}

	void OnCollisionEnter (Collision _col)
	{   
		
		splashSound = splashSounds.audioSources [Random.Range (0, 2)];
		splashSoundVolOrig = splashSound.volume;

		if (_col.gameObject.CompareTag ("Platform")) {
			splashColor = GetComponent<MeshRenderer> ().material.color;
			splashPointFound = Physics.Raycast (transform.position, (_col.contacts [0].point - transform.position), out splashPosition, 3f, playerThrowScript.targetLayer);
			if (!splashPointFound) {
				Reset ();
			} else {
				if (tex.name == "slidetex6") {
					color = _col.gameObject.GetComponent<MySplash> ().slideSplash;
				}
				targetTexture = _col.gameObject.GetComponent<MeshRenderer> ().material.mainTexture as Texture2D;
				startX = (int)Mathf.Clamp (((splashPosition.textureCoord.x * targetTexture.width)), 0, targetTexture.width);
				startY = (int)Mathf.Clamp (((splashPosition.textureCoord.y * targetTexture.height)), 0, targetTexture.height);
				splatX = (int)(Mathf.Clamp ((playerThrowScript.splatSize / splashPosition.transform.localScale.x), 0, playerThrowScript.splatSize));
				splatY = (int)(Mathf.Clamp ((playerThrowScript.splatSize / splashPosition.transform.localScale.z), 0, playerThrowScript.splatSize));
				startX = Mathf.Max (startX, splatX / 2);
				startY = Mathf.Max (startY, splatY / 2);
				spaceX = Mathf.Clamp (startX - splatX / 2, 0, targetTexture.width - splatX);
				spaceY = Mathf.Clamp (startY - splatY / 2, 0, targetTexture.height - splatY);
				targetTexture.SetPixels (spaceX, spaceY, splatX, splatY, color);
				if (!playerThrowScript.textureUpdater.textureList.Contains (targetTexture)) {
					playerThrowScript.textureUpdater.textureList.Add (targetTexture);
				}
				if (!child) {
					fragNumber = Random.Range (2, playerThrowScript.maxFrags);
					for (int i = 0; i <= fragNumber; i++) {
						frag = playerThrowScript.throweeList [playerThrowScript.currentThroweeNumber];
						fragRig = frag.GetComponent<Rigidbody> ();
						fragThrowScript = frag.GetComponent<ThroweeScript> ();
						frag.transform.position = transform.position + Random.Range (35, 65) / 100f * _col.transform.up + Random.Range (-100, 100) / 100f * transform.right;
						frag.GetComponent<MeshRenderer> ().material = GetComponent<MeshRenderer> ().material;

						fragRig.isKinematic = false;
						fragThrowScript.child = true;
						frag.transform.localScale /= fragNumber;
						fragThrowScript.tex = this.tex;
						fragThrowScript.color = this.color;
						fragThrowScript.thrown = true;
						frag.SetActive (true);
						parentSpeed = myRig.velocity;
						fragRig.velocity = (Mathf.Clamp (parentSpeed.magnitude, 0, 60) / Mathf.Clamp ((fragNumber * playerThrowScript.fragDistanceDamp), 1, playerThrowScript.fragDistanceDamp * fragNumber)) * (frag.transform.position - _col.contacts [0].point).normalized;
						fragRig.AddForce (_col.transform.up * parentSpeed.magnitude / (fragNumber * 0.5f));
						playerThrowScript.Reload ();

					}

					splashSound.pitch = Random.Range (randomPitchMinMax.x * 100 / randomPitchMinMax.x, randomPitchMinMax.y * 100 / randomPitchMinMax.y) / 100;
					splashSound.volume *= Random.Range (randomVolumeParChil.x * 50, randomVolumeParChil.x * 100) / 100;
				} else {
					
					splashSound.pitch = Random.Range (randomPitchMinMax.x * 100, randomPitchMinMax.y * 100) / 100;
					splashSound.volume *= Random.Range (0, randomVolumeParChil.y * 100) / 100;


				}
				splashSound.Play ();
				waitTime = splashSound.clip.length;
			}
		}
		if (_col.gameObject.CompareTag ("Enemy") && tex.name == "FieryTexture") {
			_col.gameObject.GetComponent<EnemyMove> ().agitated = true;
		}

		if (_col.gameObject.name == "Inlet" || _col.gameObject.name == "Outlet" || _col.gameObject.name == "FountainPiece") {
			
			splashSound = splashSounds.audioSources [Random.Range (2, 4)];
			splashSound.pitch = Random.Range (200 , 300)/100;
			splashSound.Play ();
			waitTime = splashSound.clip.length;
		
		}
		myCollider.enabled = false;
		myMesh.enabled = false;
		Invoke("Reset",waitTime);


	}

	void Reset(){
		
		transform.position = origPos;
		myRig.isKinematic = true;
		splashSound.volume = splashSoundVolOrig;
		waitTime = 0;
		myCollider.enabled = true;
		myMesh.enabled = true;
		transform.localScale = originalSize;
		this.enabled = false;
		this.gameObject.SetActive (false);
	}
}