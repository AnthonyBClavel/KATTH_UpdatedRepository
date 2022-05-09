﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Artifact : MonoBehaviour
{
    private bool hasInspectedArtifact = false;

    private string artifactName;
    private float rotateWithKeysSpeed = 200f; // Original Value = 200f
    private float rotateWithMouseSpeed = 500f; // Original Value = 500f
    private float horizontalAxis;
    private float verticalAxis;
    private int currentDialogueIndex;

    private Animator woodenChestAnim;
    private GameObject woodenChestHolder;
    private GameObject artifactHolder;
    private GameObject continueButton;
    private GameObject player;
    private GameObject mainCamera;
    private TextMeshProUGUI continueButtonText;

    private Vector3 wchOriginalRotation;
    private Vector3 ahOriginalRotation;
    private Vector3 ahInspectingRotation;

    public ARTIFACT artifact;
    private TextAsset dialogueOptions;
    private TextAsset[] artifactDialogue;

    private IEnumerator artifactViewCoroutine;
    private IEnumerator previousViewCoroutine;
    private IEnumerator closeChestCoroutine;
    private IEnumerator inputCoroutine;

    private CameraController cameraScript;
    private CharacterDialogue characterDialogueScript;
    private TileMovementController playerScript;
    private GameHUD gameHUDScript;
    private SaveManager saveManagerScript;
    private TutorialDialogue tutorialDialogueScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private TransitionFade transitionFadeScript;

    public bool HasInspectedArtifact
    {
        get { return hasInspectedArtifact; }
        set { hasInspectedArtifact = value; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Checks if the artifact has already been collected
        if (PlayerPrefs.GetString("listOfArtifacts").Contains(artifactName))
        {
            artifactHolder.SetActive(false);
            enabled = false;
        }
    }

    // Saves the name of the collected artifact via PlayerPrefs and updates the artifact notification bubble
    public void CollectArtifact()
    {
        int artifactsCount = PlayerPrefs.GetInt("numberOfArtifactsCollected");
        string artifactsCollected = PlayerPrefs.GetString("listOfArtifacts");

        if (artifactsCount < 15 && artifactHolder.activeSelf && enabled)
        {
            int totalArtifacts = (SceneManager.GetActiveScene().name == "TutorialMap") ? 1 : 15;
            gameHUDScript.UpdateArtifactBubbleText($"{artifactsCount + 1}/{totalArtifacts}");

            notificationBubblesScript.PlayArtifactNotificationCheck();
            saveManagerScript.SaveCollectedArtifact(artifactsCollected + artifactName);
            saveManagerScript.SaveNumberOfArtifactsCollected(artifactsCount + 1);

            artifactHolder.SetActive(false);
            enabled = false;
        }
    }

    // Sets and starts the dialogue for the artifact
    private void StartArtifactDialogue()
    {
        characterDialogueScript.isInteractingWithArtifact = true;
        characterDialogueScript.UpdateArtifactScript(this);
        characterDialogueScript.setPlayerDialogue(ReturnRandomArtifactDialogue());
        characterDialogueScript.setDialogueQuestions(dialogueOptions);
        characterDialogueScript.StartDialogue();
    }

    // Returns a randomly selected text asset for the artifact dialogue
    private TextAsset ReturnRandomArtifactDialogue()
    {
        int newDialogueIndex = Random.Range(0, artifactDialogue.Length);
        int attempts = 3;

        // Attempts to set a text asset that's different from the one previously played
        while (newDialogueIndex == currentDialogueIndex && attempts > 0)
        {
            newDialogueIndex = Random.Range(0, artifactDialogue.Length);
            attempts--;
        }

        currentDialogueIndex = newDialogueIndex;
        return artifactDialogue[newDialogueIndex];
    }

    // Sets the camera view to a new position and rotation (up-close view of the artifact)
    private void SetArtifactView()
    {
        float playerRotationY = player.transform.eulerAngles.y;
        cameraScript.StopAllCoroutines();

        // Positions the camera slightly above the player and sets its y rotation to the player's y rotation
        mainCamera.transform.position = player.transform.position + new Vector3(0, 2.15f, 0);
        mainCamera.transform.eulerAngles = mainCamera.transform.eulerAngles + new Vector3(0, playerRotationY, 0);

        // Rotates the artifact and the chest to look towards the player
        artifactHolder.transform.eulerAngles += new Vector3(0, playerRotationY, 0);
        woodenChestHolder.transform.eulerAngles = new Vector3(0, playerRotationY + 180, 0);
        ahInspectingRotation = artifactHolder.transform.eulerAngles;
    }

    // Sets the camera view back to it's previous position and rotation
    private void SetPreviousView()
    {
        cameraScript.SetToDialogueView();

        // Resets the artifact and the chest to their default rotations
        artifactHolder.transform.eulerAngles = ahOriginalRotation;
        woodenChestHolder.transform.eulerAngles = wchOriginalRotation;
    }

    // Opens the artifact chest
    public void OpenChest()
    {
        if (artifactHolder.activeSelf && enabled)
        {
            woodenChestAnim.Play("Open");
            audioManagerScript.PlayOpeningChestSFX();
            StartArtifactDialogue();
        }
    }

    // Closes the artifact chest
    public void CloseChest()
    {
        if (closeChestCoroutine != null)
            StopCoroutine(closeChestCoroutine);

        closeChestCoroutine = CloseArtifactChest();
        StartCoroutine(closeChestCoroutine);
    }

    // Starts the coroutine that transitions to the artifact view
    public void InspectArtifact()
    {
        if (artifactViewCoroutine != null)
            StopCoroutine(artifactViewCoroutine);

        artifactViewCoroutine = TransitionToArtifactView();
        StartCoroutine(artifactViewCoroutine);
    }

    // Starts the coroutine that transitions to the previous view
    public void StopInspectingArtifact()
    {
        if (previousViewCoroutine != null)
            StopCoroutine(previousViewCoroutine);

        previousViewCoroutine = TransitionToPreviousView();
        StartCoroutine(previousViewCoroutine);
    }

    // Starts the coroutine that checks for the artifact input
    public void StartInputCoroutine()
    {
        if (inputCoroutine != null)
            StopCoroutine(inputCoroutine);

        inputCoroutine = ArtifactInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // The coroutine for closing the artifact chest
    private IEnumerator CloseArtifactChest()
    {
        woodenChestAnim.Play("Close");

        // Plays the sfx at the end of the chest's closing animation
        yield return new WaitForSeconds(1f/6f); // 0.166f
        audioManagerScript.PlayClosingChestSFX();
    }

    // Transitions to the camera's artifact view
    private IEnumerator TransitionToArtifactView()
    {
        bool inTutorialDialogue = false;
        float duration = transitionFadeScript.fadeOutAndIn / 2f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        continueButton.SetActive(true);
        SetArtifactView();

        if (tutorialDialogueScript != null && tutorialDialogueScript.PlayArtifactDialogueCheck())
            inTutorialDialogue = true;

        yield return new WaitForSeconds(duration);
        characterDialogueScript.hasTransitionedToArtifactView = true;

        if (!inTutorialDialogue)
            StartInputCoroutine();
            
        // Coroutine doesnt work when ending tutorial dialogue
        // maybe try finding the opacity of a image - use the tutorial null thing to set a variable to null or somehting in awake
    }

    // Transitions to the camera's previous view
    private IEnumerator TransitionToPreviousView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetPreviousView();

        yield return new WaitForSeconds(duration / 2f);
        characterDialogueScript.OpenDialogueOptionsBubble();
    }

    // Checks for the input that rotates or resets the artifact
    private IEnumerator ArtifactInputCheck()
    {
        yield return new WaitForSeconds(0.01f);
        while (continueButton.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime <= 0)
                yield return null;

            // Resets the artifact's rotation
            if (Input.GetKeyDown(KeyCode.R))
                artifactHolder.transform.eulerAngles = ahInspectingRotation;

            // Stops inspecting the artifact
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StopInspectingArtifact();
                characterDialogueScript.hasTransitionedToArtifactView = false;
                continueButton.SetActive(false);
            }

            // Rotates the artifact via keys
            if (!Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Horizontal") * rotateWithKeysSpeed * Time.deltaTime;
                verticalAxis = Input.GetAxis("Vertical") * rotateWithKeysSpeed * Time.deltaTime;

                artifactHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactHolder.transform.Rotate(Vector3.right, verticalAxis);
            }

            // Rotates the artifact via mouse
            if (Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Mouse X") * rotateWithMouseSpeed * Time.deltaTime;
                verticalAxis = Input.GetAxis("Mouse Y") * rotateWithMouseSpeed * Time.deltaTime;

                artifactHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
            yield return null;
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        cameraScript = FindObjectOfType<CameraController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        tutorialDialogueScript = (SceneManager.GetActiveScene().name == "TutorialMap") ? FindObjectOfType<TutorialDialogue>() : null;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName01 = child.name;

            if (childName01 == "WoodenChestHolder")
            {
                woodenChestAnim = child.GetComponentInChildren<Animator>();
                woodenChestHolder = child;
            }             
            if (childName01 == "ArtifactHolder")
                artifactHolder = child;         
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ContinueButton")
            {
                continueButtonText = child.GetComponent<TextMeshProUGUI>();
                continueButton = child;
            }
        }

        artifactName = artifact.artifactName;
        dialogueOptions = artifact.dialogueOptions;
        artifactDialogue = artifact.artifactDialogue;

        mainCamera = cameraScript.gameObject;
        player = playerScript.gameObject;

        wchOriginalRotation = woodenChestHolder.transform.eulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;

        DebuggingCheck();
    }

    // Updates the rotation speeds if their value changes
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (rotateWithKeysSpeed != gameManagerScript.rotateWithKeysSpeed)
                rotateWithKeysSpeed = gameManagerScript.rotateWithKeysSpeed;

            if (rotateWithMouseSpeed != gameManagerScript.rotateWithMouseSpeed)
                rotateWithMouseSpeed = gameManagerScript.rotateWithMouseSpeed;
        }
    }

}
