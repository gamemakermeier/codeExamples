using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
	public int splatSize;
	GameObject currentThrowee;
	public GameObject player;
	[HideInInspector]
	public bool thrown;
	public GameObject throweePosition;
	Vector3 origThroweeSize;
	public int maxFrags;
	public float fragDistanceDamp;

	float throwTimer;
	public float throwReload;
	public float walkForceReduction;
	public float walkNumberFactor;



	//List all potion textures

	public Texture2D slideyTex;
	public Texture2D bumpyTex;
	public Texture2D fieryTex;
	public Texture2D downTex;
	Texture2D currentTex;


	movement moveScript;
	PlayerStatus playerStatus;
	Quaternion origRot;
	public float throwForce;
	mainCamControle cameraController;
	public Vector2 tiltFactor;
	public float defaultYSpitTilt;

	public float slowMoThrow;
	float slowTimer;
	public float slowTimeStartMax;
	public float slowTimeMax;
	public float timeSlowReloadFactor;
	[HideInInspector]
	public RaycastHit throwTarget;
	bool targetAquired;
	Vector3 shootDir;
	public LayerMask targetLayer;

	public GameObject throweeCreator;
	[HideInInspector]
	public List<GameObject> throweeList;
	[HideInInspector]
	public int currentThroweeNumber;

	public GameObject sampleThrowee;
	public UpdateTextures textureUpdater;
	public SoundList soundList;
	public Vector2 randomPitchSpit;
	[HideInInspector]
	public int throweeNumber;
	// Use this for initialization
	void Start ()
	{
		currentTex = slideyTex;
		throweeList = throweeCreator.GetComponent<CreateThrowees> ().throweeList;
		currentThrowee = throweeList [0];
		origThroweeSize = currentThrowee.transform.localScale;
		cameraController = GameObject.Find ("CameraController").GetComponent<mainCamControle> ();
		playerStatus = GetComponent<PlayerStatus> ();
		moveScript = GetComponent<movement> ();
		currentThrowee = throweeList [currentThroweeNumber];
	}
	
	// Update is called once per frame
	void Update ()
	{
		SelectPotion ();
		Throwing ();
		WalkThrowing ();

	}

	void FixedUpdate ()
	{
		
	}

	void SelectPotion ()
	{
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			currentThrowee = throweeList [currentThroweeNumber];
			currentTex = slideyTex;
		}

		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			currentThrowee = throweeList [currentThroweeNumber];
			currentTex = bumpyTex;
		}

		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			currentThrowee = throweeList [currentThroweeNumber];
			currentTex = fieryTex;
		}

		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			currentThrowee = throweeList [currentThroweeNumber];
			currentTex = downTex;
		}
	}

	void WalkThrowing ()
	{
		if (playerStatus.walking && InputManager.instance.shootInputStay) {
			if (throwTimer <= 0 && !currentThrowee.activeSelf) {
				soundList.audioSources [0].Stop ();
				ActivateThrowee (currentThrowee);
				soundList.audioSources [0].pitch = Random.Range (randomPitchSpit.x * 100, randomPitchSpit.y * 100) / 100;
				soundList.audioSources [0].Play ();

			} else {
				throwTimer -= Time.deltaTime;
			}
			if (currentThrowee.activeSelf) {
				ThroweeScript currentThroweeScript = currentThrowee.GetComponent<ThroweeScript> ();
				Rigidbody currentThroweeRig = currentThrowee.GetComponent<Rigidbody> ();
				throwTimer = throwReload / walkNumberFactor;
				currentThroweeRig.isKinematic = false;
				currentThroweeScript.enabled = true;
				currentThroweeScript.thrown = true;
				Vector3 playerVelocity = this.GetComponent<Rigidbody> ().velocity;
				currentThroweeRig.velocity = playerVelocity - Vector3.Scale (playerVelocity, transform.up);
				if (InputManager.instance.colorWheelVertical == 0) {
					currentThroweeRig.AddForce ((transform.forward + transform.right * InputManager.instance.colorWheelHorizontal * tiltFactor.x - transform.up * tiltFactor.y * defaultYSpitTilt) * throwForce / walkForceReduction, ForceMode.Impulse);
				} else {
					currentThroweeRig.AddForce ((transform.forward + transform.right * InputManager.instance.colorWheelHorizontal * tiltFactor.x + transform.up * InputManager.instance.colorWheelVertical * tiltFactor.y) * throwForce / walkForceReduction, ForceMode.Impulse);
				}
				Reload ();
			}

		}
	}

	void Throwing ()
	{
		//rightClickThrow
		if (InputManager.instance.aimInputDown && slowTimer <=slowTimeStartMax) {
			playerStatus.walking = false;
			playerStatus.throwing = true;
			playerStatus.sloMoFactor = slowMoThrow;
			player.GetComponent<Rigidbody> ().velocity *= playerStatus.sloMoFactor;
			origRot = transform.rotation;
			cameraController.origRotThrow = Camera.main.transform.rotation;
			cameraController.distance = cameraController.distanceThrow;
			throwTimer = throwReload;
			targetAquired = Physics.Raycast (new Ray (transform.position, transform.forward), out throwTarget, 2000f, targetLayer);
		}

		if (InputManager.instance.aimInputStay && playerStatus.throwing ) { //player holds down button to prepare throw
			
			slowTimer += Time.deltaTime;


			targetAquired = Physics.Raycast (new Ray (transform.position, transform.forward), out throwTarget, 2000f, targetLayer);

			if (throwTimer <= 0 && !currentThrowee.activeSelf) {
				ActivateThrowee (currentThrowee);
				currentThrowee.GetComponent<MeshRenderer> ().enabled = false;
				thrown = false;

			} else {
				throwTimer -= Time.deltaTime;
			}
			if (!thrown && currentThrowee.activeSelf) {
				currentThrowee.transform.position = throweePosition.transform.position;
			}
			
			if (Input.GetKeyDown (KeyCode.Alpha1) && !thrown) { // if player presses selection button, switch to different throwee
				if (currentThrowee.activeSelf) {
					currentThrowee.SetActive (false);
					currentThroweeNumber += 1;
					currentThroweeNumber = currentThroweeNumber % throweeList.Count;
				}
				currentTex = slideyTex;
				Reload ();
				ActivateThrowee (currentThrowee);
				thrown = false;
			}

			if (Input.GetKeyDown (KeyCode.Alpha2) && !thrown) { // if player presses selection button, switch to different throwee
				if (currentThrowee.activeSelf) {
					currentThrowee.SetActive (false);
					Reload ();
					ActivateThrowee (currentThrowee);
				}
				currentTex = bumpyTex;
				Reload ();
				ActivateThrowee (currentThrowee);
				thrown = false;

			}
			if (Input.GetKeyDown (KeyCode.Alpha3) && !thrown) { // if player presses selection button, switch to different throwee
				if (currentThrowee.activeSelf) {
					currentThrowee.SetActive (false);
					Reload ();
					ActivateThrowee (currentThrowee);
				}
				currentTex = fieryTex;
				Reload ();
				ActivateThrowee (currentThrowee);
				thrown = false;

			}
			if (Input.GetKeyDown (KeyCode.Alpha4) && !thrown) { // if player presses selection button, switch to different throwee
				if (currentThrowee.activeSelf) {
					currentThrowee.SetActive (false);
					Reload ();
					ActivateThrowee (currentThrowee);
				}
				currentTex = downTex;
				Reload ();
				ActivateThrowee (currentThrowee);
				thrown = false;

			}

			if (InputManager.instance.shootInputStay && currentThrowee.activeSelf) { // if player presses action button while holding throw prepare button, throwee gets everything to be a real impactful object and gets thrown
				currentThrowee.GetComponent<Rigidbody> ().isKinematic = false;
				currentThrowee.GetComponent<ThroweeScript> ().enabled = true;
				currentThrowee.GetComponent<MeshRenderer> ().enabled = true;
				if (!targetAquired) {
					shootDir = transform.forward;
				} else {
					shootDir = (throwTarget.point - currentThrowee.transform.position).normalized;
				}
				soundList.audioSources [0].Stop ();
				soundList.audioSources [0].pitch = Random.Range (randomPitchSpit.x * 100, randomPitchSpit.y * 100) / 100;
				currentThrowee.GetComponent<Rigidbody> ().AddForce (shootDir * throwForce, ForceMode.Impulse);
				soundList.audioSources [0].Play ();

				thrown = true;
				currentThrowee.GetComponent<ThroweeScript> ().thrown = true;
				Reload ();

				throwTimer = throwReload;
			}
		}
		if (InputManager.instance.aimInputUp && slowTimer < slowTimeMax || slowTimer>slowTimeMax) { // on release of throw prepare button or slowTime is depleted
			if (!thrown) { // if nothing was thrown, destroy throwee
				currentThrowee.SetActive (false);
				Reload ();
			}
			playerStatus.walking = true; //get player into normal state
			playerStatus.throwing = false;
			transform.rotation = new Quaternion (origRot.x, origRot.y, origRot.z, origRot.w);
			thrown = false;
			player.GetComponent<Rigidbody> ().velocity /= playerStatus.sloMoFactor;
			playerStatus.sloMoFactor = 1;

			slowTimer = Mathf.Min(slowTimeMax,slowTimer);
		}
		if (!playerStatus.throwing && slowTimer>0) {
			slowTimer -= Time.deltaTime;
		}
	}

	void ActivateThrowee (GameObject _throwee)
	{	

		_throwee = throweeList [currentThroweeNumber];
		ThroweeScript throwScript = _throwee.GetComponent<ThroweeScript> ();
		throwScript.child = false;
		_throwee.transform.position = throweePosition.transform.position;
		throwScript.tex = currentTex;
		_throwee.transform.localScale = origThroweeSize;
		throwScript.myPosition = _throwee.transform.position - player.transform.position;
		throwScript.thrown = false;
		_throwee.GetComponent<MeshRenderer> ().material.mainTexture = currentTex;

		_throwee.SetActive (true);
	}

	public void Reload ()
	{
		currentThroweeNumber += 1;
		currentThroweeNumber = currentThroweeNumber % throweeList.Count;
		currentThrowee = throweeList [currentThroweeNumber];
	}
}
