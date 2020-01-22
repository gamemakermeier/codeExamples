using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Teleport_Player : MonoBehaviour
{
    Transform playerTrans;
    public GameObject myLocalLandingPoint;
    public LoadPortalScene loadScript;
    public bool noNewMusic;
    private void OnCollisionEnter(Collision collision)
    {
        if (SceneManager.sceneCount > 2)
        {
            if (!noNewMusic)
            {
                MusicManager.instance.StopClip(loadScript.oldScene.name, true);
                MusicManager.instance.PlayClip(loadScript.targetPoint.gameObject.scene.name, true);
            }
            playerTrans = GlobalGameStateManager.instance.player.transform;
            Vector3 offSet = new Vector3(playerTrans.position.x - transform.position.x, playerTrans.position.y - myLocalLandingPoint.transform.position.y, playerTrans.position.z - transform.position.z);
            float localForward = Vector3.Dot(transform.forward, offSet);
            float localRight = Vector3.Dot(transform.right, offSet);
            GameObject playerParent = new GameObject("PlayerParent");
            playerParent.transform.position = playerTrans.position;
            playerParent.transform.rotation = playerTrans.rotation;
            playerTrans.parent = playerParent.transform;
            Transform currentCamTrans = CameraController.instance.currentCam.transform;
            currentCamTrans.parent = playerParent.transform;
            float angularDifference = loadScript.targetPoint.transform.parent.rotation.eulerAngles.y - this.transform.parent.rotation.eulerAngles.y + 180;
            playerParent.transform.position = loadScript.targetPoint.position + localForward * loadScript.targetPoint.forward - localRight * loadScript.targetPoint.right;
            playerParent.transform.RotateAround(playerTrans.position, Vector3.up, angularDifference);
            playerTrans.parent = null;
            currentCamTrans.parent = null;
            CameraController.instance.ResetToPlayer();
            Destroy(playerParent);
            SceneManager.UnloadSceneAsync(loadScript.oldScene);
            GlobalGameStateManager.instance.LoadState(GameStateManager.instance.gameState);
            GameStateManager.instance.InitiateGameState();
        }
    }
}