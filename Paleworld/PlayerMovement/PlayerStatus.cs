using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
	public bool grounded;
	bool groundedCenter;
	bool groundedRight;
	bool groundedLeft;
	bool groundedFront;
	bool groundedBack;
	[HideInInspector]
	public RaycastHit groundCastCenter;
	RaycastHit groundCastRight;
	RaycastHit groundCastLeft;
	RaycastHit groundCastFront;
	RaycastHit groundCastBack;
	RaycastHit rotateToGroundCast;
	RaycastHit rotateToGroundCastAlt;
	RaycastHit rotateToGroundCastUp;
	public Vector2 groundCheckOffSets;
	public bool airJumped;
	public bool throwing;
	public bool walking;
	public float sloMoFactor;
	Rigidbody rig;
	public float bumpForce;
	public float slideForce;
	public float maxSlideVelocity;
	public float downFactor;
	public float groundCheckDistance;
	public LayerMask groundLayer;
	public float groundAlignmentSpeed;

	Texture2D checkTexture;
	public Color groundColor;
	public bool fullColor;
	public Texture2D slideyTex;
	public Texture2D bouncyTex;
	public float rotationAdjustSpeed;
	Vector3 newUp = Vector3.up;
	Vector3 newRight;
	Vector3 normalGrav;

	Color slideyColor;
	Color bouncyColor;
	Color downColor;
	Color fieryColor;

	public Animator playerAnimator;

	Quaternion aspiredRotation;

	bool hitNewRight;
	bool hitNewRightAlt;
	bool hitNewUp;
	bool oldBool;
	public Vector3 savePosition;
	// Use this for initialization
	void Start ()
	{
		savePosition = this.transform.position;
		normalGrav = Physics.gravity;
		rig = GetComponent<Rigidbody> ();
		walking = true;
		slideyColor = slideyTex.GetPixel (0, 0);
		bouncyColor = bouncyTex.GetPixel (0, 0);
	}

	// Update is called once per frame
	void Update ()
	{
		groundedRight = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up + transform.right * groundCheckOffSets.x, -transform.up, out groundCastRight, groundCheckDistance, groundLayer);
		groundedLeft = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up - transform.right * groundCheckOffSets.x, -transform.up, out groundCastLeft, groundCheckDistance, groundLayer);
		groundedFront = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up + transform.forward * groundCheckOffSets.y, -transform.up, out groundCastFront, groundCheckDistance, groundLayer);
		groundedBack = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up - transform.forward * groundCheckOffSets.y, -transform.up, out groundCastBack, groundCheckDistance, groundLayer);
		groundedCenter = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up, -transform.up, out groundCastCenter, groundCheckDistance, groundLayer);
		grounded = groundedCenter || groundedRight || groundedLeft || groundedFront || groundedBack;
		if (grounded) {
			if (groundedCenter) {
				checkTexture = groundCastCenter.transform.gameObject.GetComponent<MeshRenderer> ().material.mainTexture as Texture2D;
				groundColor = checkTexture.GetPixel ((int)(groundCastCenter.textureCoord.x * checkTexture.width), (int)(groundCastCenter.textureCoord.y * checkTexture.height));
				fullColor = checkTexture.GetPixel ((int)(groundCastBack.textureCoord.x * checkTexture.width), (int)(groundCastBack.textureCoord.y * checkTexture.height)) ==
				checkTexture.GetPixel ((int)(groundCastFront.textureCoord.x * checkTexture.width), (int)(groundCastFront.textureCoord.y * checkTexture.height));
				newUp = Vector3.RotateTowards (transform.up, groundCastCenter.normal, rotationAdjustSpeed * Mathf.Deg2Rad, 0).normalized;
			}

		} else {
			groundColor = Color.white;
			newUp = Vector3.up;
		}
	}

	void FixedUpdate ()
	{

		//AnimationPrimer
		playerAnimator.SetBool ("isMoving", rig.velocity.magnitude > 1f);
		playerAnimator.SetBool ("isShooting", InputManager.instance.shootInputStay);
		playerAnimator.SetBool ("isGrounded", grounded);
		playerAnimator.SetBool ("isFalling", rig.velocity.y < 0);

		//Code
		AddColorEffect (groundColor);
		if ((!grounded && Physics.gravity != normalGrav && !throwing) || groundColor == Color.white) {
			Physics.gravity = normalGrav;
		}

		if (transform.up != newUp.normalized && !throwing /*&& (groundedRight || groundedLeft || groundedFront || groundedBack || !grounded)*/) {
			AdjustRotation ();
		}
	}

	void AddColorEffect (Color _contactedColor)
	{
		if (_contactedColor == slideyColor && rig.velocity.magnitude < maxSlideVelocity) {
			playerAnimator.SetBool ("isOnSlidey", true);
			rig.AddForce (transform.forward * slideForce * sloMoFactor, ForceMode.Force);
			Physics.gravity = -groundCastCenter.normal * 9.81f;
		} else {
			playerAnimator.SetBool ("isOnSlidey", false);
		}
		if (_contactedColor == bouncyColor) {
			rig.velocity = new Vector3 (rig.velocity.x, 0, rig.velocity.z);
			rig.AddForce (groundCastCenter.transform.up * bumpForce * sloMoFactor, ForceMode.Impulse);
			grounded = false;
		}
	}

	void AdjustRotation ()
	{
		groundedCenter = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up, -transform.up, out groundCastCenter, groundCheckDistance, groundLayer);
		hitNewRight = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up + transform.right * 0.25f, -transform.up, out rotateToGroundCast, groundCheckDistance * 2f, groundLayer);
		hitNewRightAlt = Physics.Raycast (transform.position + groundCheckDistance * 0.1f * transform.up - transform.right * 0.25f, -transform.up, out rotateToGroundCastAlt, groundCheckDistance * 2f, groundLayer);
		if (!groundedCenter) {
			newRight = transform.right;
			newUp = Vector3.up;
			aspiredRotation = Quaternion.LookRotation (Vector3.Cross (newRight, newUp), newUp);
			transform.rotation = Quaternion.Lerp (transform.rotation, aspiredRotation, groundAlignmentSpeed/4f);
			Debug.DrawRay (transform.position + groundCheckDistance * 0.1f * transform.up, transform.right * 5f, Color.red, 25f);
		}
		if (hitNewRight && groundedCenter && hitNewRightAlt) {
			
			if (rotateToGroundCast.normal == groundCastCenter.normal) {
				
				//Debug.DrawRay(rotateToGroundCast.point,rotateToGroundCast.normal*5f,Color.blue,100f);
				//Debug.DrawRay (groundCastCenter.point, groundCastCenter.normal * 5f, Color.red, 100f);
				newRight = (rotateToGroundCast.point - groundCastCenter.point).normalized;
				aspiredRotation = Quaternion.LookRotation (Vector3.Cross (newRight, newUp), newUp);
				//Debug.DrawRay (transform.position + groundCheckDistance * 0.1f * transform.up, newRight * 5f, Color.blue, 25f);
				transform.rotation = Quaternion.Lerp (transform.rotation, aspiredRotation, groundAlignmentSpeed);
			} else if (rotateToGroundCastAlt.normal == groundCastCenter.normal) {
				//Debug.DrawRay (groundCastCenter.point, groundCastCenter.normal * 5f, Color.red, 100f);
				//Debug.DrawRay(rotateToGroundCastAlt.point,rotateToGroundCastAlt.normal*5f,Color.green,100f);
				newRight = -(rotateToGroundCastAlt.point - groundCastCenter.point).normalized;
				aspiredRotation = Quaternion.LookRotation (Vector3.Cross (newRight, newUp), newUp);
				//Debug.DrawRay (transform.position + groundCheckDistance * 0.1f * transform.up, newRight * 5f, Color.green, 25f);
				transform.rotation = Quaternion.Lerp (transform.rotation, aspiredRotation, groundAlignmentSpeed);
			}
		}

	}

	void OnCollisionExit (Collision _col)
	{
		if (_col.gameObject.CompareTag ("Platform")) {

			airJumped = false;
		}
	}
}
