﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;
    private float rayLength = 1f;

    private GameObject player;
    private GameObject bridgeTileCheck;
    private GameObject savedInvisibleBlock;

    private Animator playerAnimator;
    private Vector3 checkpointPosition;
    private IEnumerator resetPlayerCoroutine;

    private GameHUD gameHUDScript;
    private PauseMenu pauseMenuScript;
    private TorchMeter torchMeterScript;
    private SaveManager saveManagerScript;
    private GameManager gameManagerScript;
    private FreezeEffect freezeEffectScript;
    private AudioManager audioManagerScript;
    private TileMovementController playerScript;
    private TutorialDialogue tutorialDialogueScript;

    public int MaxTileMoves
    {
        get { return numMovements; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Checks for the last/closest bridge tile - sets the savedInvisibleBlock's position and sets/saves the player's rotation
    public void SetSavedBlockPosition()
    {
        for (int i = 0; i < 360; i += 90)
        {
            bridgeTileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

            Ray myRay = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            // If the ray doesn't hit anything or if it doesn't hit a bridge tile, then CONTINUE the loop
            if (!Physics.Raycast(myRay, out hit, rayLength) || !hit.collider.CompareTag("BridgeTile") && !hit.collider.name.Contains("BridgeTile")) continue;

            float newPlayerRot = bridgeTileCheck.transform.eulerAngles.y - 180;
            Vector3 bridgeTilePos = hit.collider.transform.position;

            if (playerScript.OnCheckpoint()) player.transform.eulerAngles = new Vector3(0, newPlayerRot, 0);
            savedInvisibleBlock.transform.position = new Vector3(bridgeTilePos.x, 1, bridgeTilePos.z);
            saveManagerScript.SavePlayerRotation(newPlayerRot);

            //Debug.Log("The SavedInvisibleBlock's position has been set");
            break;
        }
    }

    // Checks to reset the player after/without a delay
    // Note: no delay will be added if the duration parameter is not set
    public void ResetPlayer(float duration = 0f)
    {
        // Resets after a delay - for when the torch meter runs out (player freezes)
        if (duration > 0f) StartResetPlayerCoroutine(duration);

        // Resets immediately - for manually restarting a puzzle
        else if (duration == 0f) ResetPlayerElements();
    }

    // Resets the player's coroutines, position, rotation, and torch meter
    private void ResetPlayerElements()
    {
        playerScript.StopPlayerCoroutines();
        audioManagerScript.StopAllPuzzleSFX();
        freezeEffectScript.ResetAlphas();

        player.transform.position = checkpointPosition;
        playerScript.Destination = checkpointPosition;
        saveManagerScript.LoadPlayerRotation();

        torchMeterScript.ResetTorchMeterElements();
        playerScript.WriteToGrassMaterial();
        playerScript.AlertBubbleCheck();

        // Note: resets the animator to its entry state
        playerAnimator.Rebind();
        playerAnimator.Update(0f);

        pauseMenuScript.CanPause = true;
        playerAnimator.enabled = true;

        // Note: the player bools are set to true AFTER the death dialogue has finished playing
        if (tutorialDialogueScript != null && !playerScript.CanRestartPuzzle)
            tutorialDialogueScript.PlayDeathDialogue();

        else playerScript.SetPlayerBoolsTrue();
    }

    // Starts the coroutine that resets the player after a delay
    private void StartResetPlayerCoroutine(float duration)
    {
        if (resetPlayerCoroutine != null) StopCoroutine(resetPlayerCoroutine);

        resetPlayerCoroutine = ResetPlayerCoroutine(duration);
        StartCoroutine(resetPlayerCoroutine);
    }

    // Resets the player after a delay (duration = seconds)
    private IEnumerator ResetPlayerCoroutine(float duration)
    {
        playerScript.SetPlayerBoolsFalse();
        freezeEffectScript.LerpAlphas();

        pauseMenuScript.CanPause = false;
        playerAnimator.enabled = false;
        
        yield return new WaitForSeconds(duration);
        bool canDeathScreen = gameManagerScript.canDeathScreen;
        if (!canDeathScreen) ResetPlayerElements();
        else gameHUDScript.SetDeathScreenActive();
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
        // Sets them by looking at the names of children
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

        playerAnimator = playerScript.GetComponentInChildren<Animator>();
        checkpointPosition = transform.position;
        player = playerScript.gameObject;
    }

}
