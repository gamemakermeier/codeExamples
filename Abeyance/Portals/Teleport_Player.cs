using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//this script is attached to all portals, which teleports the player to the "other side" of said portal
//it manages the player transportation and loading of the game state needed for the new scene
//it is based on the smooth portals in unity tutorial by brackeys, but heavily modifed
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
            //if we chose to change the music (normal scene change, no special event going on that has it's own global theme) we do so
            //each portal also holds a load script that provides the information for the new scene
            //the new scene is already loaded by opening the portal (player couldn't collide with the portal if it wasn't opened
            if (!noNewMusic)
            {
                MusicManager.instance.StopClip(loadScript.oldScene.name, true);
                MusicManager.instance.PlayClip(loadScript.targetPoint.gameObject.scene.name, true);
            }
            playerTrans = GlobalGameStateManager.instance.player.transform;
            //we get the local offSet so we can mirror it to the other portal and seemlessly transition the player
            Vector3 offSet = new Vector3(playerTrans.position.x - transform.position.x, playerTrans.position.y - myLocalLandingPoint.transform.position.y, playerTrans.position.z - transform.position.z);
            float localForward = Vector3.Dot(transform.forward, offSet);
            float localRight = Vector3.Dot(transform.right, offSet);
            //we create a "packing object" to keep all internal relations of the player, camera and such intact
            GameObject playerParent = new GameObject("PlayerParent");
            playerParent.transform.position = playerTrans.position;
            playerParent.transform.rotation = playerTrans.rotation;
            playerTrans.parent = playerParent.transform;
            Transform currentCamTrans = CameraController.instance.currentCam.transform;
            currentCamTrans.parent = playerParent.transform;
            //we calculate the rotational difference from the starting- to the endpoint portal and change the packing object transform accordingly, moving the player and camera along with it
            float angularDifference = loadScript.targetPoint.transform.parent.rotation.eulerAngles.y - this.transform.parent.rotation.eulerAngles.y + 180;
            playerParent.transform.position = loadScript.targetPoint.position + localForward * loadScript.targetPoint.forward - localRight * loadScript.targetPoint.right;
            playerParent.transform.RotateAround(playerTrans.position, Vector3.up, angularDifference);
            //we "unpack" the player and remove the packing object
            playerTrans.parent = null;
            currentCamTrans.parent = null;
            //and refresh the cameracontroller so it knows that the move has happened
            CameraController.instance.ResetToPlayer();
            Destroy(playerParent);
            //we unload the old scene and load the gamestate for the currently active scene
            SceneManager.UnloadSceneAsync(loadScript.oldScene);
            GlobalGameStateManager.instance.LoadState(GameStateManager.instance.gameState);
            GameStateManager.instance.InitiateGameState();
        }
    }
}