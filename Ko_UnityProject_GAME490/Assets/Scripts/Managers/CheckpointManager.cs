using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;
    private float rayLength = 1f;

    private GameObject bridgeTileCheck;
    private GameObject player;
    private GameObject savedInvisibleBlock;

    private Vector3 checkpointPosition;
    private Animator playerAnimator;
    private IEnumerator resetPlayerCoroutine;

    private IceMaterial iceMaterialScript;
    private TileMovementController playerScript;
    private SaveManager saveManagerScript;
    private GameManager gameManagerScript;
    private PauseMenu pauseMenuScript;
    private GameHUD gameHUDScript;
    private AudioManager audioManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        checkpointPosition = transform.position;
    }

    // Returns the number of movements for the checkpoint - movements determined within the script's component in unity inspector
    public int getNumMovements() { return numMovements; }

    // Checks for the closet bridge tile and sets the savedInvisibleBlock's position to that bridge tile
    public bool LastBridgeTileCheck()
    {
        int rayDirection = 0;

        for (int i = 0; i <= 270; i += 90)
        {
            Ray myRayLBT = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            Debug.DrawRay(myRayLBT.origin, myRayLBT.direction, Color.red);

            if (Physics.Raycast(myRayLBT, out hit, rayLength))
            {
                GameObject lastBridgeTile = hit.collider.gameObject;
                float lastBridgeTileX = lastBridgeTile.transform.position.x;
                float lastBridgeTileZ = lastBridgeTile.transform.position.z;
                string name = lastBridgeTile.name;
                string tag = lastBridgeTile.tag;

                if (name == "BridgeTile" || tag == "BridgeTile")
                {
                    //Debug.Log("BridgeBlock Found");
                    //playerScript.bridge = lastBridgeTile.transform.parent.gameObject;
                    savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);

                    // Saves the player's rotataion as the opposite angle from which the BridgeBlock was found
                    float playerRotation = bridgeTileCheck.transform.localEulerAngles.y - 180;
                    saveManagerScript.SavePlayerRotation(playerRotation);

                    // Sets the player's rotation (ONLY gets called when a puzzle is loaded while debugging)
                    if (player.transform.position == transform.position)
                        player.transform.localEulerAngles = new Vector3(0, playerRotation, 0);                            

                    return true;
                }
            }

            //Debug.Log("BridgeBlock Not Found");
            i = rayDirection;
            rayDirection += 90;
            bridgeTileCheck.transform.localEulerAngles = new Vector3(0, rayDirection, 0);
        }

        return false;
    }

    // Resets all player elements (position, animation, rotation, torch meter)
    public void ResetPlayer()
    {
        bool hasFoundTutorialDialogue = false;

        iceMaterialScript.ResetIceAlphas();
        audioManagerScript.StopAllPuzzleSFX();

        // Resets all animator layers to their entry state (Idle)
        playerAnimator.Rebind();
        playerAnimator.Update(0f);

        playerScript.StopPlayerCoroutines();
        playerAnimator.enabled = true;
        pauseMenuScript.CanPause = true;

        player.transform.position = checkpointPosition;
        saveManagerScript.LoadPlayerRotation();
        playerScript.setDestination(checkpointPosition);
        playerScript.ResetTorchMeter();

        // Checks to see if there's a tutorial dialogue manager in the scene
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (!hasFoundTutorialDialogue && child.name == "TutorialDialogueHolder")
                hasFoundTutorialDialogue = true;
        }
        // Note: Check to set player bools to true MUST come after the for loop above!
        if (!hasFoundTutorialDialogue)
            playerScript.SetPlayerBoolsTrue();

        // Only sets the player bools to true while not in the tutorial zone - OLD VERSION
        /*if (sceneName != "TutorialMap")
            playerScript.SetPlayerBoolsTrue();*/
    }

    // Resets all player elements after a delay (set float to zero for instant reset)
    public void ResetPlayerDelay(float seconds)
    {
        if (resetPlayerCoroutine != null)
            StopCoroutine(resetPlayerCoroutine);

        resetPlayerCoroutine = ResetPlayerPosition(seconds);
        StartCoroutine(resetPlayerCoroutine);
    }

    // Resets the player's position and other elements after a delay
    private IEnumerator ResetPlayerPosition(float seconds)
    {
        iceMaterialScript.LerpIceAlphas();
        playerScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.CanPause = false;

        yield return new WaitForSeconds(seconds);
        if (!gameManagerScript.canDeathScreen)
            ResetPlayer();
        if (gameManagerScript.canDeathScreen)
            gameHUDScript.SetDeathScreenActive();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        iceMaterialScript = FindObjectOfType<IceMaterial>();
        playerScript = FindObjectOfType<TileMovementController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "BridgeTileCheck")
                bridgeTileCheck = child;
        }

        for (int i = 0; i < saveManagerScript.transform.childCount; i++)
        {
            GameObject child = saveManagerScript.transform.GetChild(i).gameObject;

            if (child.name == "SavedInvisibleBlock")
                savedInvisibleBlock = child;
        }

        player = playerScript.gameObject;
        playerAnimator = playerScript.GetComponentInChildren<Animator>();
    }

}
