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
    private TutorialDialogue tutorialDialogueScript;

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

    // Returns the number of movements for the puzzle - determined within the checkpoint's script component in unity inspector
    public int GetNumMovements() 
    {
        return numMovements; 
    }

    // Checks for the closet bridge tile and sets the savedInvisibleBlock's position to that bridge tile
    public bool LastBridgeTileCheck()
    {
        for (int i = 0; i < 360; i += 90)
        {
            bridgeTileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

            Ray myRay = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            if (Physics.Raycast(myRay, out hit, rayLength))
            {
                GameObject lastBridgeTile = hit.collider.gameObject;

                if (lastBridgeTile.name == "BridgeTile" || lastBridgeTile.tag == "BridgeTile")
                {
                    //Debug.Log("Bridge tile was found");
                    float playerRotation = bridgeTileCheck.transform.localEulerAngles.y - 180; 
 
                    // Sets the player's rotation (ONLY when a puzzle is loaded while debugging)
                    if (player.transform.position == transform.position && gameManagerScript.isDebugging)
                        player.transform.localEulerAngles = new Vector3(0, playerRotation, 0);

                    // Saves the player's rotataion as the opposite angle from which the last bridge tile was found
                    saveManagerScript.SavePlayerRotation(playerRotation);
                    savedInvisibleBlock.transform.position = new Vector3(lastBridgeTile.transform.position.x, 1, lastBridgeTile.transform.position.z);

                    return true;
                }
            }
            //Debug.Log("Bridge tile was NOT found");
        }

        return false;
    }

    // Starts the coroutine to reset all player elements (delay = seconds, set float to zero for instant reset)
    public void ResetPlayer(float delay)
    {
        // Reset the player after the delay
        if (delay > 0f)
        {
            if (resetPlayerCoroutine != null)
                StopCoroutine(resetPlayerCoroutine);

            resetPlayerCoroutine = ResetPlayerCoroutine(delay);
            StartCoroutine(resetPlayerCoroutine);
        }
        // Instantly reset the player if there's no delay
        else if (delay <= 0f)
            ResetPlayerElements();
    }

    // Resets all player elements
    private void ResetPlayerElements()
    {
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
        playerScript.SetPlayerBoolsTrue();
        playerScript.AlertBubbleCheck();

        if (tutorialDialogueScript != null)
        {
            if (tutorialDialogueScript.CanPlayDeathDialogue)
            {
                playerScript.SetPlayerBoolsFalse();
                tutorialDialogueScript.PlayDeathDialogue();
            }          
            else
                tutorialDialogueScript.CanPlayDeathDialogue = false;
        }
            
        /*if (tutorialDialogueScript != null)
            playerScript.SetPlayerBoolsTrue();
        else
            tutorialDialogueScript.HasPlayedDeathDialogue = false;*/

        // Only sets the player bools to true while not in the tutorial zone - OLD VERSION
        /*if (sceneName != "TutorialMap")
            playerScript.SetPlayerBoolsTrue();*/
    }

    // Resets all player elements elements after a delay
    private IEnumerator ResetPlayerCoroutine(float seconds)
    {
        iceMaterialScript.LerpIceAlphas();
        playerScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.CanPause = false;

        yield return new WaitForSeconds(seconds);
        if (!gameManagerScript.canDeathScreen)
            ResetPlayerElements();
        else if (gameManagerScript.canDeathScreen)
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
        tutorialDialogueScript = null;
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

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "TutorialDialogueHolder")
                tutorialDialogueScript = child.GetComponent<TutorialDialogue>();
        }

        player = playerScript.gameObject;
        playerAnimator = playerScript.GetComponentInChildren<Animator>();
    }

}
