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

    private FreezeEffect freezeEffectScript;
    private TileMovementController playerScript;
    private SaveManager saveManagerScript;
    private GameManager gameManagerScript;
    private PauseMenu pauseMenuScript;
    private GameHUD gameHUDScript;
    private AudioManager audioManagerScript;
    private TutorialDialogue tutorialDialogueScript;
    private TorchMeter torchMeterScript;

    public int MaxTileMoves
    {
        get { return numMovements; }
    }

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
        freezeEffectScript.ResetAlphas();
        audioManagerScript.StopAllPuzzleSFX();

        // Resets all animator layers to their entry state (Idle)
        playerAnimator.Rebind();
        playerAnimator.Update(0f);

        playerScript.StopPlayerCoroutines();
        playerAnimator.enabled = true;
        pauseMenuScript.CanPause = true;

        player.transform.position = checkpointPosition;
        saveManagerScript.LoadPlayerRotation();
        playerScript.Destination = checkpointPosition;
        playerScript.AlertBubbleCheck();
        playerScript.WriteToGrassMaterial();
        torchMeterScript.ResetTorchMeterElements();

        // The player bools are not set to true until the death dialogue has finished playing - ONLY called in tutorial zone
        if (tutorialDialogueScript != null && !playerScript.CanRestartPuzzle)
            tutorialDialogueScript.PlayDeathDialogue();
        else
            playerScript.SetPlayerBoolsTrue();
    }

    // Resets all player elements elements after a delay
    private IEnumerator ResetPlayerCoroutine(float seconds)
    {
        freezeEffectScript.LerpAlphas();
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
        freezeEffectScript = FindObjectOfType<FreezeEffect>();
        playerScript = FindObjectOfType<TileMovementController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        tutorialDialogueScript = (SceneManager.GetActiveScene().name == "TutorialMap") ? FindObjectOfType<TutorialDialogue>() : null;
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
