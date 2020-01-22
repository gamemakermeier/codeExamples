using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamMove : MonoBehaviour
{
    Transform playerCam;
    public Transform portal;
    public GameObject myLandingPoint;
    Transform targetPoint;
    public LoadPortalScene loadingScript;
    // Use this for initialization
    void Start()
    {
        this.gameObject.SetActive(false);
        playerCam = GlobalGameStateManager.instance.playerCam.transform;
    }
    private void OnEnable()
    {
        targetPoint = loadingScript.targetPoint;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (targetPoint != null)
        {
            Vector3 playerOffSetFromPortal = playerCam.position - portal.position;
            float localOffSetForward = Vector3.Dot(myLandingPoint.transform.forward, playerOffSetFromPortal);
            float localOffSetRight = Vector3.Dot(myLandingPoint.transform.right, playerOffSetFromPortal);
            transform.position = targetPoint.position - localOffSetForward * targetPoint.forward - localOffSetRight * targetPoint.right + (portal.transform.position.y - GlobalGameStateManager.instance.player.transform.position.y + GlobalGameStateManager.instance.player.transform.localScale.y / 2) * Vector3.up;
            float angularDifference = targetPoint.parent.rotation.eulerAngles.y - portal.parent.rotation.eulerAngles.y + 180;
            Quaternion rotationalDifference = Quaternion.AngleAxis(angularDifference, Vector3.up);
            Vector3 newCamDir = rotationalDifference * playerCam.forward;
            transform.rotation = Quaternion.LookRotation(newCamDir, Vector3.up);
        }
    }
}
