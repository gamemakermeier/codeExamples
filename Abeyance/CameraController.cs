using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public Camera currentCam;
    public GameObject player;
    public float horizontalRotSpeed = 3;
    public float verticalRotSpeed = 1;
    public Vector3 camOffSet;
    public float camBackwardsCheckRange = 0.5f;
    public LayerMask camPushMask;
    public float camPushSpeed;
    public float camMoveSmooth = 0.5f;
    public float minCamWallDist = 0.3f;
    RaycastHit camBackWall;
    float origXRotation;
    Vector3 origForward;
    public float camUpInputMin = 0.3f;
    public Vector2 maxXRotation;
    [Range(0f, 1f)]
    public float XRotReturnSpeed = 0.1f;
    playerMovement playerMoveScript;
    Rigidbody playerRig;
    [HideInInspector]
    public Vector3 playerHead;
    [HideInInspector]
    public Vector3 pushHead;
    Transform camTrans;
    float distanceVectorDelta;
    float cameraHeadVectorDelta;
    [HideInInspector]
    public Vector3 camPointer;
    Collider[] colls;
    float maxDistance;
    float camDistance;
    public float camSideCheckDist = 0.5f;
    Vector3 playerPos;
    List<float> rayHitDist;
    float centerDist;
    float lowestDist;
    public float pentaRayMoreThanLowest;
    public LayerMask normalView;
    public LayerMask playerTooCloseView;
    public float playerIgnoreDistance;
    public float pushCamRotSpeed;
    public float pushCamMinRotDif;

    GameObject cameraGhost;
    [HideInInspector]
    public bool playerControlledCam;
    // Use this for initialization
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        playerControlledCam = true;
        rayHitDist = new List<float>();
        playerMoveScript = player.GetComponent<playerMovement>();
        playerRig = player.GetComponent<Rigidbody>();
        camTrans = currentCam.transform;
        origXRotation = camTrans.rotation.eulerAngles.x;
        camTrans.position = player.transform.position + camOffSet.x * camTrans.forward + camOffSet.y * camTrans.up + camOffSet.z * camTrans.right;
        playerHead = player.transform.position + player.transform.up * player.transform.localScale.y;
        camPointer = (camTrans.position - playerHead).normalized;
        maxDistance = (camTrans.position - playerHead).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PlayerStatus.instance.pushing && playerControlledCam)
        {
            Rotate();
            Translate();
            CheckIfPlayerTooClose();
        }
        if (PlayerStatus.instance.pushing)
        {
            PushCam();
            CheckIfPlayerTooClose();
        }
    }

    void Rotate()
    {
        playerPos = player.transform.position;
        playerHead = playerPos + player.transform.up * player.transform.localScale.y;
        camTrans.position = playerHead + camPointer * maxDistance;
        if (Mathf.Abs(InputManager.instance.cameraHorizontal) > 0) // horizontal rotation
        {
            camTrans.RotateAround(playerHead, Vector3.up, horizontalRotSpeed * InputManager.instance.cameraHorizontal);
            playerMoveScript.currentForward = new Vector3(camTrans.forward.x, 0, camTrans.forward.z).normalized;
            playerMoveScript.currentRight = new Vector3(camTrans.right.x, 0, camTrans.right.z).normalized;
        }
        if (Mathf.Abs(InputManager.instance.cameraVertical) > camUpInputMin) // vertical rotation
        {
            if (InputManager.instance.cameraVertical > 0 &&
                camTrans.rotation.eulerAngles.x + InputManager.instance.cameraVertical * verticalRotSpeed < origXRotation + maxXRotation.y)
            {
                camTrans.RotateAround(playerHead, camTrans.right, InputManager.instance.cameraVertical * verticalRotSpeed);
            }
            if ((InputManager.instance.cameraVertical < 0 && (
                camTrans.rotation.eulerAngles.x + InputManager.instance.cameraVertical * verticalRotSpeed > origXRotation + maxXRotation.x &&
                camTrans.rotation.eulerAngles.x + InputManager.instance.cameraVertical * verticalRotSpeed < origXRotation + maxXRotation.y) ||
                   camTrans.rotation.eulerAngles.x + InputManager.instance.cameraVertical * verticalRotSpeed > 360 + maxXRotation.x + origXRotation))
            {
                camTrans.RotateAround(playerHead, camTrans.right, InputManager.instance.cameraVertical * horizontalRotSpeed);
            }
        }
        else if (origXRotation != camTrans.rotation.eulerAngles.x && playerRig.velocity.magnitude > 0.1)
        {
            if (camTrans.rotation.eulerAngles.x < origXRotation + maxXRotation.y)
            {
                camTrans.RotateAround(playerHead, camTrans.right, (origXRotation - camTrans.rotation.eulerAngles.x) * XRotReturnSpeed);
            }
            else
            {
                camTrans.RotateAround(playerHead, camTrans.right, (360 - camTrans.rotation.eulerAngles.x + origXRotation) * XRotReturnSpeed);
            }
        }
        camPointer = (camTrans.position - playerHead).normalized;
    }

    void Translate()
    {
        rayHitDist.Clear();
        centerDist = maxDistance;
        lowestDist = maxDistance;
        //new
        if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance, out camBackWall, camPushMask))
        {
            rayHitDist.Add(camBackWall.distance - minCamWallDist);
            centerDist = camBackWall.distance - minCamWallDist;
        }
        if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance + camTrans.right * camSideCheckDist, out camBackWall, camPushMask))
        {
            rayHitDist.Add(Mathf.Clamp(camBackWall.distance - minCamWallDist, 0, centerDist));
        }
        if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance - camTrans.right * camSideCheckDist, out camBackWall, camPushMask))
        {
            rayHitDist.Add(Mathf.Clamp(camBackWall.distance - minCamWallDist, 0, centerDist));
        }
        if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance + camTrans.up * camSideCheckDist, out camBackWall, camPushMask))
        {
            rayHitDist.Add(Mathf.Clamp(camBackWall.distance - minCamWallDist, 0, centerDist));
        }
        if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance - camTrans.up * camSideCheckDist, out camBackWall, camPushMask))
        {
            rayHitDist.Add(Mathf.Clamp(camBackWall.distance - minCamWallDist, 0, centerDist));
        }

        if (rayHitDist.Count > 0)
        {
            camDistance = 0;
            for (int i = rayHitDist.Count; i > 0; i--)
            {
                lowestDist = Mathf.Min(lowestDist, rayHitDist[i - 1]);
                camDistance += rayHitDist[i - 1];
            }
            camDistance /= rayHitDist.Count;
            camDistance = Mathf.Clamp(camDistance, 0, lowestDist * pentaRayMoreThanLowest - minCamWallDist);
        }
        else
        {
            camDistance = Mathf.Lerp(camDistance, maxDistance, camMoveSmooth / 5f);
        }
        //alt
        /*if (Physics.Linecast(playerHead, playerHead + camPointer * maxDistance, out camBackWall, camPushMask))
        {
            camDistance = Mathf.Clamp(camBackWall.distance - minCamWallDist, 0, maxDistance);
        }
        else
        {
            camDistance = maxDistance;
        }*/
        camTrans.position = Vector3.Lerp(camTrans.position, playerHead + camPointer * camDistance, camMoveSmooth);
    }
    public void ResetToPlayer()
    {
        playerPos = player.transform.position;
        playerHead = playerPos + player.transform.up * player.transform.localScale.y;
        camPointer = (camTrans.position - playerHead).normalized;
        playerMoveScript.currentForward = new Vector3(camTrans.forward.x, 0, camTrans.forward.z).normalized;
        playerMoveScript.currentRight = new Vector3(camTrans.right.x, 0, camTrans.right.z).normalized;
    }

    public void CheckIfPlayerTooClose()
    {
        Vector3 distCheck = camTrans.position - playerHead;
        if (distCheck.magnitude < playerIgnoreDistance)
        {
            currentCam.cullingMask = playerTooCloseView;
        }
        else
        {
            currentCam.cullingMask = normalView;
        }
    }

    public void CamGhostSetup()
    {
        playerControlledCam = false;
        cameraGhost = new GameObject("cameraGhost");
        cameraGhost.transform.position = currentCam.transform.position;
        cameraGhost.transform.rotation = currentCam.transform.rotation;
        cameraGhost.transform.parent = player.transform;
    }

    void PushCam()
    {

        Vector3 playerForward = player.transform.forward;
        Vector3 playerRight = player.transform.right;
        Transform camTrans = currentCam.transform;
        Vector3 targetVector = camOffSet.x * playerForward + camOffSet.z * playerRight;
        Vector3 flattener = Vector3.forward + Vector3.right;
        if (Vector3.Dot((currentCam.transform.position - playerHead), targetVector) != 1)
        {
            Debug.DrawRay(player.transform.position, targetVector * targetVector.magnitude, Color.red, 0.5f);
            Debug.DrawRay(player.transform.position, Vector3.Scale((currentCam.transform.position - playerHead), flattener) * Vector3.Scale((currentCam.transform.position - playerHead), flattener).magnitude, Color.blue, 0.5f);
            float rotDif = Vector3.SignedAngle(targetVector, Vector3.Scale((currentCam.transform.position - playerHead), flattener), Vector3.up);
            if (Mathf.Abs(rotDif) >= pushCamMinRotDif)
            {
                if (Mathf.Abs(rotDif) > pushCamRotSpeed * Time.deltaTime)
                {
                    rotDif = Mathf.Sign(rotDif) * pushCamRotSpeed * Time.deltaTime;
                }

                camTrans.RotateAround(playerHead, Vector3.up, -rotDif);
                playerMoveScript.currentForward = new Vector3(camTrans.forward.x, 0, camTrans.forward.z).normalized;
                playerMoveScript.currentRight = new Vector3(camTrans.right.x, 0, camTrans.right.z).normalized;
            }
        }
    }

    public void ReturnFromPushing()
    {
        /* pushHead = playerHead;
         Debug.DrawRay(pushHead, Vector3.up*10f,Color.red,5f);
         playerHead = playerPos + player.transform.up * player.transform.localScale.y;
         */
        //StartCoroutine(CameraDrive(pushHead, playerHead));
        //not working properly atm 
        StartCoroutine(CameraDrive(cameraGhost.transform));
    }
    IEnumerator CameraDrive(Transform targetTransform)
    {
        while (currentCam.transform.position != targetTransform.position || currentCam.transform.rotation != targetTransform.rotation)
        {
            currentCam.transform.position = Vector3.Lerp(currentCam.transform.position, targetTransform.position, pushCamRotSpeed * Time.deltaTime);
            currentCam.transform.rotation = Quaternion.Lerp(currentCam.transform.rotation, targetTransform.rotation, pushCamRotSpeed * Time.deltaTime);
            if ((targetTransform.position - currentCam.transform.position).magnitude > 0.1f)
            {
                currentCam.transform.position = targetTransform.position;
            }
            yield return null;
        }
        playerPos = player.transform.position;
        playerHead = playerPos + player.transform.up * player.transform.localScale.y;
        camPointer = (camTrans.position - playerHead).normalized;
        playerMoveScript.currentForward = new Vector3(camTrans.forward.x, 0, camTrans.forward.z).normalized;
        playerMoveScript.currentRight = new Vector3(camTrans.right.x, 0, camTrans.right.z).normalized;
        playerControlledCam = true;
        /*bool breaker = true;
        if (breaker)
        {
            Debug.Break();
            breaker = false;
            yield return null;

        }*/
        if (targetTransform.gameObject == cameraGhost)
        {
            Destroy(targetTransform.gameObject);
        }
    }

    IEnumerator CameraDrive(Vector3 currentPoint, Vector3 targetPoint)
    {
        GameObject anchor = new GameObject("anchor");
        anchor.transform.position = currentPoint;
        currentCam.transform.parent = anchor.transform;
        print("camDrive");
        while (anchor.transform.position != targetPoint)
        {

            anchor.transform.position = Vector3.Lerp(anchor.transform.position, targetPoint, camMoveSmooth * Time.deltaTime);
            Debug.DrawRay(anchor.transform.position, anchor.transform.right * 5f, Color.green, 5f);
            yield return null;
        }
        currentCam.transform.parent = null;
        Destroy(anchor);
        pushHead = Vector3.zero;
        playerControlledCam = true;
    }
}
