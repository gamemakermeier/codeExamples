using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
	Rigidbody playerRig;
	public float moveSpeed;
	public float maxSpeed;
	public float rotationSpeed;
	public float sloRotIncrease;
	[Range (0f, 1f)]
	public float velocityRotFollowFactor;
	[Range (0f, 1f)]
	public float verticalThreshhold;
	[Range (0f, 1f)]
	public float horizontalThreshhold;
	bool jumpIntend;
	bool airJumpIntend;
	public float jumpForce;
	public float airJumpForce;
	public float airJumpHeight;
	public bool constantAirJump;
	public float jumpAngleFactor;
	public float extraGrav;
	Vector3 currentVelocity;
	[HideInInspector]
	public Vector3 throwVelocity;
	public float idleStopFactor;
	float idleTime;
	PlayerStatus playerStatus;
	int chance;
	public SoundList soundList;
	AudioSource currentSound;
	public Vector2 jumpSoundRange;
	public Vector2 randomPitchJump;
	public float jumpSoundChance;

	Vector3 airJumpVector;

	// Use this for initialization
	void Start ()
	{
		playerStatus = GetComponent<PlayerStatus> ();
		playerRig = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (InputManager.instance.jumpInputDown) {
			if (playerStatus.grounded) {
				jumpIntend = true;
				playerStatus.playerAnimator.SetTrigger ("jump");
			}
			if (!playerStatus.grounded && !playerStatus.airJumped && !playerStatus.throwing) {
				airJumpIntend = true;
				playerStatus.playerAnimator.SetTrigger ("jump");
			}
		}
	}

	void FixedUpdate ()
	{
		Move ();
		Jump ();
		AirJump ();
		ThrowRotate ();
	}

	void Move ()
	{
		if (playerRig.velocity.magnitude < maxSpeed && Mathf.Abs (InputManager.instance.verticalInput) >= verticalThreshhold && !playerStatus.throwing) {
			playerRig.AddForce (InputManager.instance.verticalInput * moveSpeed * transform.forward * playerStatus.sloMoFactor, ForceMode.Acceleration);

		}

		if (Mathf.Abs (InputManager.instance.horizontalInput) >= horizontalThreshhold && !playerStatus.throwing) {
			transform.RotateAround (transform.position, transform.up, InputManager.instance.horizontalInput * rotationSpeed * playerStatus.sloMoFactor * Time.deltaTime);
			playerRig.velocity = Quaternion.AngleAxis (InputManager.instance.horizontalInput * rotationSpeed * playerStatus.sloMoFactor * Time.deltaTime * velocityRotFollowFactor, transform.up) * playerRig.velocity;
		}


		if (!playerStatus.grounded && !playerStatus.throwing) {
			playerRig.velocity += Physics.gravity*extraGrav*Time.deltaTime;
		}
		if (!playerStatus.grounded && playerStatus.throwing) {
			playerRig.velocity += Physics.gravity*extraGrav*Time.deltaTime*playerStatus.sloMoFactor/4;
		}

		if ((InputManager.instance.verticalInput == 0 || playerRig.velocity.magnitude > maxSpeed) && playerStatus.groundColor == Color.white && playerStatus.grounded && playerStatus.fullColor) {
			idleTime += idleStopFactor*Time.deltaTime;
			playerRig.velocity = new Vector3 (playerRig.velocity.x / (1 +  idleTime), playerRig.velocity.y, playerRig.velocity.z / (1 + idleTime));
		} else {
			idleTime = 0;
		}

	}

	void ThrowRotate ()
	{
		if (playerStatus.throwing) {
			transform.RotateAround (transform.position, transform.up, InputManager.instance.horizontalInput * rotationSpeed * playerStatus.sloMoFactor * Time.deltaTime * sloRotIncrease);
			transform.RotateAround (transform.position, -transform.right, InputManager.instance.verticalInput * rotationSpeed * playerStatus.sloMoFactor * Time.deltaTime * sloRotIncrease);
			playerRig.velocity -=  Physics.gravity * Mathf.Clamp(1-playerStatus.sloMoFactor,0,1) * Time.deltaTime;
		}
	}

	void Jump ()
	{
		if (jumpIntend && playerStatus.grounded) {
			playerRig.velocity = new Vector3 (playerRig.velocity.x, 0, playerRig.velocity.z);
			playerRig.AddForce (new Vector3 (Mathf.Pow (playerStatus.groundCastCenter.normal.x, jumpAngleFactor) * jumpForce * playerStatus.sloMoFactor,
				playerStatus.groundCastCenter.normal.y * jumpForce * playerStatus.sloMoFactor,
				Mathf.Pow (playerStatus.groundCastCenter.normal.z, jumpAngleFactor) * jumpAngleFactor * jumpForce * playerStatus.sloMoFactor), ForceMode.Impulse);
			jumpIntend = false;
			playerStatus.airJumped = false;
			chance = Random.Range (0, 11);
			if (chance < jumpSoundChance*10){
			currentSound = soundList.audioSources [Random.Range ((int)jumpSoundRange.x,(int)jumpSoundRange.y + 1)];
			currentSound.pitch = Random.Range (randomPitchJump.x * 100, randomPitchJump.y * 100) / 100;
			currentSound.Play ();
			}
		}
	}

	void AirJump ()
	{
		if (airJumpIntend) {
			
			playerStatus.airJumped = true;
			airJumpVector = Vector3.up*airJumpHeight;
			if (constantAirJump) {
				playerRig.velocity = Vector3.zero;
			}
			if (Mathf.Abs (InputManager.instance.verticalInput) >= verticalThreshhold) {
				airJumpVector += (InputManager.instance.verticalInput *new Vector3(transform.forward.x,0,transform.forward.z).normalized).normalized/2;
			}
			if (Mathf.Abs (InputManager.instance.horizontalInput) >= horizontalThreshhold) {
				airJumpVector += (InputManager.instance.horizontalInput * new Vector3(transform.right.x,0,transform.right.z).normalized).normalized/2;
			}
			playerRig.AddForce (airJumpVector * airJumpForce, ForceMode.Impulse);

			chance = Random.Range (0, 11);
			if (chance < jumpSoundChance*10){
				currentSound = soundList.audioSources [Random.Range ((int)jumpSoundRange.x,(int)jumpSoundRange.y + 1)];
				currentSound.pitch = Random.Range (randomPitchJump.x * 100, randomPitchJump.y * 100) / 100;
				currentSound.Play ();
			}
		}
		airJumpIntend = false;
	}
}
