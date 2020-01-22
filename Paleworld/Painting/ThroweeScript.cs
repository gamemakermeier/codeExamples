using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this script is attached to every paint ball in the scene and manages their collision behaviour
//this includes repainting the texture of the object it hit at the correct UV position, creating splashballs and resetting itself to the ball pool
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
	void Awake()
	{
		myCollider = GetComponent<SphereCollider>();
		myMesh = GetComponent<MeshRenderer>();
		player = GameObject.Find("Player");
		myRig = GetComponent<Rigidbody>();
		playerMoveScript = player.GetComponent<movement>();
		playerThrowScript = player.GetComponent<Throw>();
		playerStatus = player.GetComponent<PlayerStatus>();
		origPos = new Vector3(0, 500, 0);
		originalSize = transform.localScale;

	}

	void OnCollisionEnter(Collision _col)
	{
		//no matter where it collides, we set the splashsound up
		splashSound = splashSounds.audioSources[Random.Range(0, 2)];
		splashSoundVolOrig = splashSound.volume;
		//if we hit a platform,which is the tag for our paintable surfaces
		if (_col.gameObject.CompareTag("Platform"))
		{
			//the initial plan was to have different colors that give the surfaces different physics properties, so the splash ball would check here which color it actually has
			splashColor = GetComponent<MeshRenderer>().material.color;
			//the ball checks if could find a valid point of impact. If I remember correctly, this was necessary due to contactpoints not providing UV data
			splashPointFound = Physics.Raycast(transform.position, (_col.contacts[0].point - transform.position), out splashPosition, 3f, playerThrowScript.targetLayer);
			//if no valid point was found, we just reset the ball without anything happening
			if (!splashPointFound)
			{
				Reset();
			}
			else
			{
				//setting the color according to what kind of paintball this is
				//hardcoding due to little time for polishing CHANGE
				if (tex.name == "slidetex6")
				{
					color = _col.gameObject.GetComponent<MySplash>().slideSplash;
				}
				//we get the texture and mark the intended portion for repaint
				targetTexture = _col.gameObject.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
				startX = (int)Mathf.Clamp(((splashPosition.textureCoord.x * targetTexture.width)), 0, targetTexture.width);
				startY = (int)Mathf.Clamp(((splashPosition.textureCoord.y * targetTexture.height)), 0, targetTexture.height);
				splatX = (int)(Mathf.Clamp((playerThrowScript.splatSize / splashPosition.transform.localScale.x), 0, playerThrowScript.splatSize));
				splatY = (int)(Mathf.Clamp((playerThrowScript.splatSize / splashPosition.transform.localScale.z), 0, playerThrowScript.splatSize));
				startX = Mathf.Max(startX, splatX / 2);
				startY = Mathf.Max(startY, splatY / 2);
				spaceX = Mathf.Clamp(startX - splatX / 2, 0, targetTexture.width - splatX);
				spaceY = Mathf.Clamp(startY - splatY / 2, 0, targetTexture.height - splatY);
				targetTexture.SetPixels(spaceX, spaceY, splatX, splatY, color);
				//registering the texture to our textureUpdater to update them centrally for performance gain
				if (!playerThrowScript.textureUpdater.textureList.Contains(targetTexture))
				{
					playerThrowScript.textureUpdater.textureList.Add(targetTexture);
				}
				//child is a bool used to mark activated paintballs as those who are created by a big paintball hitting a surface rather than from the player shooting it
				//these create a more natural feeling painting experience
				//so if this current paintball isn't one of those secondary ones we create some
				if (!child)
				{
					fragNumber = Random.Range(2, playerThrowScript.maxFrags);
					for (int i = 0; i <= fragNumber; i++)
					{
						frag = playerThrowScript.throweeList[playerThrowScript.currentThroweeNumber];
						fragRig = frag.GetComponent<Rigidbody>();
						fragThrowScript = frag.GetComponent<ThroweeScript>();
						//they get spawned around the location of the primary paintball
						frag.transform.position = transform.position + Random.Range(35, 65) / 100f * _col.transform.up + Random.Range(-100, 100) / 100f * transform.right;
						frag.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;

						fragRig.isKinematic = false;
						fragThrowScript.child = true;
						frag.transform.localScale /= fragNumber;
						fragThrowScript.tex = this.tex;
						fragThrowScript.color = this.color;
						fragThrowScript.thrown = true;
						frag.SetActive(true);
						parentSpeed = myRig.velocity;
						//and get a velocity that is influenced by how many secondaries were spawned and the direction from the impactpoint to their spawnpoint and the initial main ball velocity direction and magnitude
						fragRig.velocity = (Mathf.Clamp(parentSpeed.magnitude, 0, 60) / Mathf.Clamp((fragNumber * playerThrowScript.fragDistanceDamp), 1, playerThrowScript.fragDistanceDamp * fragNumber)) * (frag.transform.position - _col.contacts[0].point).normalized;
						fragRig.AddForce(_col.transform.up * parentSpeed.magnitude / (fragNumber * 0.5f));
						//and we set the index for our throwing script so it targets a new ball, since we took one out of the pool
						playerThrowScript.Reload();

					}
					//we reset the splashsound properties so it doesn't sound the same every time
					splashSound.pitch = Random.Range(randomPitchMinMax.x * 100 / randomPitchMinMax.x, randomPitchMinMax.y * 100 / randomPitchMinMax.y) / 100;
					splashSound.volume *= Random.Range(randomVolumeParChil.x * 50, randomVolumeParChil.x * 100) / 100;
				}
				else
				{
					//if it's a secondary, we just reset the splashsound but with slightly different properties
					splashSound.pitch = Random.Range(randomPitchMinMax.x * 100, randomPitchMinMax.y * 100) / 100;
					splashSound.volume *= Random.Range(0, randomVolumeParChil.y * 100) / 100;


				}
				// and finally we play the splash sound
				splashSound.Play();
				waitTime = splashSound.clip.length;
			}
		}
		if (_col.gameObject.CompareTag("Enemy") && tex.name == "FieryTexture")
		{
			_col.gameObject.GetComponent<EnemyMove>().agitated = true;
		}
		//this is a special case where the paintball hits an inlet, which is a mechanic that, instead of directly painting the surface, additionally sprays an area from the outlet
		//to communicate the significance , we use a different sound
		if (_col.gameObject.name == "Inlet" || _col.gameObject.name == "Outlet" || _col.gameObject.name == "FountainPiece")
		{

			splashSound = splashSounds.audioSources[Random.Range(2, 4)];
			splashSound.pitch = Random.Range(200, 300) / 100;
			splashSound.Play();
			waitTime = splashSound.clip.length;

		}
		//we hide the object and reset it as soon as the sound is played
		myCollider.enabled = false;
		myMesh.enabled = false;
		Invoke("Reset", waitTime);


	}

	void Reset()
	{

		transform.position = origPos;
		myRig.isKinematic = true;
		splashSound.volume = splashSoundVolOrig;
		waitTime = 0;
		myCollider.enabled = true;
		myMesh.enabled = true;
		transform.localScale = originalSize;
		this.enabled = false;
		this.gameObject.SetActive(false);
	}
}