using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//this script is used to load the target scene and set up the render texture for the portal, once it is opened
public class LoadPortalScene : ObjectState
{
    public GameObject door;
    public float doorOpenTime;
    public GameObject playerBlocker;
    public int nextScene;
    public bool random;
    public int totalSceneCount;
    [Tooltip("Add all scene names BUT essentials in the same order as in build index")]
    public List<string> sceneNames;
    public Scene oldScene;
    bool loadingDone = true;
    public GameObject showstopper;
    public GameObject portal;
    public Animator myAnim;
    public float minWaitTime;
    public GameObject portalCam;
    public Material portalCamMat;
    bool minWaitOver;
    [HideInInspector]
    public Transform targetPoint;
    bool portalsActivated;
    public AudioSource portalSound;
    public AudioSource doorSound;
    private void Start()
    {

        portalsActivated = true;
        portal.SetActive(portalsActivated);
        oldScene = this.gameObject.scene;
        Register();
    }

    private void OnEnable()
    {
        portalsActivated = true;
        portal.SetActive(portalsActivated);
    }
    private void OnDisable()
    {
        portalsActivated = false;
        portal.SetActive(portalsActivated);
    }
    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        loadingDone = false;
        GlobalGameStateManager.instance.SaveState(GameStateManager.instance.gameState);
        GameStateManager.instance = null;
        if (random)
        {
            sceneIndex = Random.Range(1, totalSceneCount + 1);
            while (sceneNames[sceneIndex - 1].ToLowerInvariant() == oldScene.name.ToLowerInvariant())
            {
                sceneIndex = Random.Range(1, totalSceneCount + 1);
            }
        }
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        loading.allowSceneActivation = false;
        while (loading.progress < 0.9f)
        {
            yield return null;
        }

        //Fade the loading screen out here


        loading.allowSceneActivation = true;

        while (!loading.isDone)
        {
            yield return null;
        }
        loadingDone = true;
        SetupPortal();
        if (minWaitOver && showstopper.activeSelf)
        {
            showstopper.SetActive(false);
        }

    }

    IEnumerator BlockSight()
    {
        //this guarantees that the view blocker blocks for the entire time it needs to load, but also for a minimum amount of time, even if the loading is faster, to not make it look awkward on fast loads
        minWaitOver = false;
        showstopper.SetActive(true);
        yield return new WaitForSeconds(minWaitTime);
        if (loadingDone)
        {
            showstopper.SetActive(false);
        }
        minWaitOver = true;
    }

    IEnumerator OpenDoor()
    {
        //we open the door and make it not collide with the player, while the animation is playing
        playerBlocker.SetActive(false);
        door.GetComponent<BoxCollider>().enabled = false;
        myAnim.SetTrigger("playerAction");
        doorSound.pitch = 1 + Random.Range(-1, 2) * 0.1f;
        doorSound.Play();
        yield return new WaitForSeconds(doorOpenTime);
        //if it is a normal door and not a portal, we close the door after a set amount of time and make it collide with the player again
        if (!portalsActivated)
        {
            myAnim.SetTrigger("playerAction");
            yield return new WaitForSeconds(myAnim.GetCurrentAnimatorClipInfo(0).Length);
            door.GetComponent<BoxCollider>().enabled = true;
            playerBlocker.SetActive(true);
        }

    }

    void SetupPortal()
    {
        //we target the last loaded scene and get the landing point for that scene (currently hard coded vial GO name CHANGE)
        Scene targetScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        foreach (GameObject rootObject in targetScene.GetRootGameObjects())
        {
            if (rootObject.name == "Portal-Main")
            {
                foreach (Transform child in rootObject.GetComponentInChildren<Transform>())
                {
                    if (child.gameObject.name == "LandingPoint")
                    {
                        targetPoint = child.transform;
                        Debug.DrawRay(targetPoint.position, Vector3.up * 3f, Color.red, 5f);
                    }
                }
            }
        }
        Camera portalCamCam = portalCam.GetComponent<Camera>();
        if (portalCamCam.targetTexture != null)
        {
            portalCamCam.targetTexture.Release();
        }
        portalCamCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        portalCamMat.mainTexture = portalCamCam.targetTexture;
        portalCam.SetActive(true);
        portalSound.Play();
    }

    //if the player is in the activatable area and presses the action input, we set up the portal
    private void OnTriggerStay(Collider other)
    {
        if (SceneManager.sceneCount < 3 && InputManager.instance.actionInputDown && other.CompareTag("Player") && loadingDone)
        {
            StartCoroutine(OpenDoor());
            //if the portal isn't flagged as active, it is a normal door
            if (portalsActivated)
            {
                //else we activate the sight blocker and load the new scene that is needed for the portal
                StartCoroutine(BlockSight());
                StartCoroutine(LoadSceneAsync(nextScene));
            }
        }
    }
}
