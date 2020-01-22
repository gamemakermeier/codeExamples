using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCamControle : MonoBehaviour
{
	public GameObject player;
	public GameObject cam;
	Rigidbody playerRig;
	[HideInInspector]
	public Quaternion origRotThrow;
	movement playerMoveScript;
	PlayerStatus playerStatus;
	[HideInInspector]
	public Vector3 distance;
	public Vector3 distanceThrow;
	public Vector3 camDistanceNorm;
	public Vector3 camDistance;
	public Vector3 camLookOffSet;
	public float SnapBack;
	[Range (-10f, 1f)]
	public float speedSnapReduce;
	public float speedCamMinSpeed;
	public float throwLookFocusDistance;
	public float noHitLookDistance;
	RaycastHit hit;
	Throw playerThrowScript;
	Vector3 leveledPlayerForward;
	Vector3 leveledCamForward;
	float forwardDifference;
	public float verticalRotSpeed;
	float snapBackFactor;
	Vector3 playerToCamVector;
	bool playerObstructed;
	bool playerClear;
	bool cameraObstructed;
	bool backSpaceReq;
	bool upSpaceReq;
	bool downSpaceReq;
	bool playerUpObstruct;
	bool playerBackObstruct;
	bool playerDownObstruct;
	bool camClear;
	Vector3 playerPos;
	Vector3 playerUp;
	Vector3 playerForward;
	Vector3 playerRight;
	public Vector3 camChangeDistance;
	public LayerMask DistanceCheckLayer;
	RaycastHit obstructor;
	// Use this for initialization
	void Start ()
	{
		Cursor.visible = false;
		camDistance = camDistanceNorm;
		cam.transform.position = player.transform.position + camDistance.x * player.transform.right + camDistance.y * player.transform.up + camDistance.z * player.transform.forward;
		cam.transform.LookAt (playerPos + camLookOffSet.x * playerForward + camLookOffSet.y * playerUp + camLookOffSet.z * player.transform.right);
		playerMoveScript = player.GetComponent<movement> ();
		playerStatus = player.GetComponent<PlayerStatus> ();
		playerThrowScript = player.GetComponent<Throw> ();
		playerRig = player.GetComponent<Rigidbody> ();

	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		playerPos = player.transform.position;
		playerUp = player.transform.up;
		playerForward = player.transform.forward;
		playerRight = player.transform.right;
		if (!playerStatus.throwing) {
			playerToCamVector = (cam.transform.position - playerPos);
			backSpaceReq = Physics.Raycast (cam.transform.position, -cam.transform.forward, camChangeDistance.z, DistanceCheckLayer);
			upSpaceReq = Physics.Raycast (cam.transform.position, cam.transform.up, camChangeDistance.y, DistanceCheckLayer);
			downSpaceReq = Physics.Raycast (cam.transform.position, -cam.transform.up, camChangeDistance.y, DistanceCheckLayer);
			playerObstructed = Physics.Raycast (cam.transform.position, -playerToCamVector,out obstructor, playerToCamVector.magnitude, DistanceCheckLayer);
			playerClear = !Physics.Raycast (cam.transform.position, -playerToCamVector + playerUp * 1.2f, (-playerToCamVector - playerUp * 1.2f).magnitude, DistanceCheckLayer) &&
			!Physics.Raycast (cam.transform.position, -playerToCamVector , (-playerToCamVector).magnitude, DistanceCheckLayer);
			camClear = !Physics.Raycast (playerPos, camDistanceNorm, playerToCamVector.magnitude + 3f, DistanceCheckLayer);
			NormalCam ();
		}
		if (playerStatus.throwing) {
			ThrowCam ();

		}
	}

	void NormalCam ()
	{
		if (!playerObstructed) {
			if (backSpaceReq) {
				camDistance.z += camLookOffSet.z;
			}
			if (upSpaceReq && !downSpaceReq) {
				camDistance.y -= camLookOffSet.y;
			}
			if (downSpaceReq && !upSpaceReq) {
				camDistance.y += camLookOffSet.y;
			}
		}

		playerUpObstruct = Physics.Raycast (playerPos, playerUp, 5f, DistanceCheckLayer);
		playerBackObstruct = Physics.Raycast (playerPos, -playerForward, 5f, DistanceCheckLayer);
		playerDownObstruct = Physics.Raycast (playerPos, -playerUp, 1f, DistanceCheckLayer);
		cameraObstructed = playerObstructed || !playerClear;

		if (!playerClear) {
			if (playerUpObstruct) {
				camDistance = Quaternion.AngleAxis (-verticalRotSpeed, Vector3.right) * camDistance;
			}
			if (playerBackObstruct && !playerUpObstruct) {
				cam.transform.RotateAround (playerPos, playerRight, verticalRotSpeed);
			} else {
				camDistance = Quaternion.AngleAxis (-verticalRotSpeed, Vector3.right) * camDistance;
			}
			if (playerDownObstruct && !playerUpObstruct) {
				camDistance = Quaternion.AngleAxis (verticalRotSpeed, Vector3.right) * camDistance;

			}
		}
		if (playerObstructed) {
			if (camDistance.magnitude > 2) {
				camDistance -= camDistance.normalized * 0.15f;
			}
		}
		if (camClear && !playerObstructed && playerClear) {
			if (camDistance.x < camDistanceNorm.x) {
				camDistance.x += camChangeDistance.x;
			}				
			if (camDistance.y < camDistanceNorm.y) {
				camDistance.y += camChangeDistance.y;
			}
			if (camDistance.z < camDistanceNorm.z) {
				camDistance.z += camChangeDistance.z;
			}
		}
		snapBackFactor = SnapBack - (SnapBack * speedSnapReduce) * Mathf.Clamp (playerRig.velocity.magnitude / speedCamMinSpeed, 0, 1);
		if (!playerUpObstruct && camClear && !playerObstructed) {
			camDistance = camDistanceNorm;
		}
		cam.transform.position = Vector3.Lerp (cam.transform.position, playerPos + camDistance.x * player.transform.right + camDistance.y * player.transform.up + camDistance.z * player.transform.forward, snapBackFactor);

			
		cam.transform.LookAt (playerPos + camLookOffSet.x * playerForward + camLookOffSet.y * playerUp + camLookOffSet.z * player.transform.right);
	}

	void ThrowCam ()
	{
		cam.transform.position = playerPos + distance.x * playerForward + distance.y * playerUp + distance.z * player.transform.right;
		if (Physics.Raycast (new Ray (playerPos, playerForward), out hit, 2000f, playerThrowScript.targetLayer)) {
			cam.transform.LookAt (hit.point);
		} else {
			cam.transform.LookAt (playerForward * noHitLookDistance);
		}
	}
}
