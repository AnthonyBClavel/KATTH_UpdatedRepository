﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Artifact : MonoBehaviour
{
    private bool canRotateArtifact = false;
    private bool hasInspectedArtifact = false;
    private bool hasCollectedArtifact = false;
    private bool isInspectingArtifact = false;

    private float rotateWithKeysSpeed; // 200f
    private float rotateWithMouseSpeed; // 500f
    private float horizontalAxis;
    private float verticalAxis;
    private int lastArtifactIndex;
    private string artifactName;

    private Animator woodenChestAnim;
    private GameObject WoodenChestHolder;
    private GameObject artifactHolder;
    private GameObject artifactRotationHolder;
    private GameObject continueButton;
    private GameObject dialogueOptionsBubble;
    private GameObject player;
    private GameObject pixelatedCamera;

    private Vector3 wchOriginalRotation; // wch = wooden chest holder
    private Vector3 ahOriginalRotation; // ah = artifact holder
    private Vector3 arhOriginalRotation; // arh = artifact rotation holder
    private Vector3 inspectingArtifactRotation;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    private IEnumerator artifactViewCoroutine;
    private IEnumerator previousViewCoroutine;
    private IEnumerator closeChestCoroutine;

    public TextAsset dialogueOptionsFile;
    public TextAsset[] artifactDialogueFiles;

    private CameraController cameraScript;
    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private GameHUD gameHUDScript;
    private SaveManager saveManagerScript;
    private TutorialDialogue tutorialDialogueScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        HasCollectedArtifactCheck();
    }

    // Update is called once per frame
    void Update()
    {
        ResetArtifactRotationCheck();
        CanRotateArtifactCheck();
    }

    // Return or sets the value of the bool canRotateArtifact
    public bool CanRotateArtifact
    {
        get
        {
            return canRotateArtifact;
        }
        set
        {
            canRotateArtifact = value;
        }
    }

    // Return or sets the value of the bool hasInspectedArtifact
    public bool HasInspectedArtifact
    {
        get
        {
            return hasInspectedArtifact;
        }
        set
        {
            hasInspectedArtifact = value;
        }
    }

    // Return or sets the value of the bool HasCollectedArtifact
    public bool HasCollectedArtifact
    {
        get
        {
            return hasCollectedArtifact;
        }
        set
        {
            hasCollectedArtifact = value;
        }
    }

    // Returns or sets the value of rotateWithKeysSpeed
    public float RotateWithKeysSpeed
    {
        get
        {
            return rotateWithKeysSpeed;
        }
        set
        {
            rotateWithKeysSpeed = value;
        }
    }

    // Return or sets the value of the bool isInspectingArtifact
    public bool IsInspectingArtifact
    {
        get
        {
            return isInspectingArtifact;
        }
        set
        {
            isInspectingArtifact = value;
        }
    }

    // Returns or sets the value of rotateWithMouseSpeed
    public float RotateWithMouseSpeed
    {
        get
        {
            return rotateWithMouseSpeed;
        }
        set
        {
            rotateWithMouseSpeed = value;
        }
    }

    // Updates all artifact elements in the character dialogue script - also starts dialogue
    public void SetVariablesForCharacterDialogueScript()
    {
        characterDialogueScript.UpdateScriptForArtifact(this);
        SetArtifactDialogue();
        OpenChest();
        characterDialogueScript.isInteractingWithArtifact = true;
        characterDialogueScript.setDialogueQuestions(dialogueOptionsFile);
        characterDialogueScript.StartDialogue();
    }

    // Saves/adds the name of the artifact to a string and updates/plays the artifact notification
    public void SaveCollectedArtifact()
    {
        int numberOfArtifactsCollected = PlayerPrefs.GetInt("numberOfArtifactsCollected");
        string listOfArtifactNames = PlayerPrefs.GetString("listOfArtifacts");

        if (numberOfArtifactsCollected < 15 && hasCollectedArtifact)
        {
            if (SceneManager.GetActiveScene().name == "TutorialMap")
                gameHUDScript.UpdateArtifactBubbleText((numberOfArtifactsCollected + 1) + "/1");
            else
                gameHUDScript.UpdateArtifactBubbleText((numberOfArtifactsCollected + 1) + "/15");

            notificationBubblesScript.PlayArtifactNotificationCheck();
            saveManagerScript.SaveCollectedArtifact(listOfArtifactNames + artifactName); // Adds the name of the artifact to the saved string
            saveManagerScript.SaveNumberOfArtifactsCollected(PlayerPrefs.GetInt("numberOfArtifactsCollected") + 1); // Adds 1 to the saved int
        }
    }

    // Transitions to the close up camera view of the artifact
    public void InspectArtifact()
    {
        if (artifactViewCoroutine != null)
            StopCoroutine(artifactViewCoroutine);

        artifactViewCoroutine = TransitionToArtifactView();
        StartCoroutine(artifactViewCoroutine);
    }

    // Transitions back to the previous camera view
    public void StopInspectingArtifact()
    {
        if (previousViewCoroutine != null)
            StopCoroutine(previousViewCoroutine);

        previousViewCoroutine = TransitionToPreviousView();
        StartCoroutine(previousViewCoroutine);
    }

    // Closes the wooden chest
    public void CloseChest()
    {
        if (closeChestCoroutine != null)
            StopCoroutine(closeChestCoroutine);

        closeChestCoroutine = CloseArtifactChest();
        StartCoroutine(closeChestCoroutine);
    }

    // Opens the wooden chest
    private void OpenChest()
    {
        audioManagerScript.PlayOpeningChestSFX();
        woodenChestAnim.SetTrigger("Open");
    }

    // Checks if the artifact has been collected
    private void HasCollectedArtifactCheck()
    {
        string listOfArtifactNames = PlayerPrefs.GetString("listOfArtifacts");

        // If the string doesn't contain the name of the artifact...
        if (listOfArtifactNames.Contains(artifactName) && hasCollectedArtifact != true)
            hasCollectedArtifact = true;
    }

    // Checks if the player can reset the artifact's rotation
    private void ResetArtifactRotationCheck()
    {
        if (Input.GetKeyDown(KeyCode.R) && canRotateArtifact && !transitionFadeScript.IsChangingScenes && pauseMenuScript.CanPause)
        {
            if (isInspectingArtifact)
                SetInspectingRotation();
            else
                SetDefaultRotation();
        }
    }

    // Rotates the wooden chest towards the player
    private void SetChestRotation()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        if (playerDirection == up)
            WoodenChestHolder.transform.localEulerAngles = down;

        else if (playerDirection == right)
            WoodenChestHolder.transform.localEulerAngles = left;

        else if (playerDirection == down)
            WoodenChestHolder.transform.localEulerAngles = up;

        else if (playerDirection == left)
            WoodenChestHolder.transform.localEulerAngles = right;
    }

    // Sets the artifact camera view - sets the camera's position and rotation to the player's
    private void SetArtifactCameraView()
    {
        cameraScript.StopAllCoroutines(); // Stops camera from lerping if its coroutine is still active

        pixelatedCamera.transform.position = player.transform.localPosition + new Vector3(0, 2.15f, 0); // Sets the camera slighty above the player
        pixelatedCamera.transform.eulerAngles = pixelatedCamera.transform.eulerAngles + new Vector3(0, player.transform.localEulerAngles.y, 0);

        SetInspectingRotation();
        isInspectingArtifact = true;
        canRotateArtifact = true;
    }

    // Sets the camera back to it's previous position and rotation
    private void ResetCameraView()
    {
        //Debug.Log("Has Reset Camera View");
        cameraScript.SetToDialogueView();

        SetDefaultRotation();
        isInspectingArtifact = false;
        canRotateArtifact = false;
    }

    // Rotates the artifact to look at the camera
    private void SetInspectingRotation()
    {
        artifactHolder.transform.LookAt(pixelatedCamera.transform);
        artifactRotationHolder.transform.localRotation = Quaternion.Euler(inspectingArtifactRotation);
    }

    // Resets the artifact to its default rotation
    private void SetDefaultRotation()
    {
        if (artifactHolder.transform.eulerAngles != ahOriginalRotation)
            artifactHolder.transform.eulerAngles = ahOriginalRotation;

        artifactRotationHolder.transform.localRotation = Quaternion.Euler(arhOriginalRotation);
    }

    // Sets the artifact dialogue - randomly slected and different from the one previously played
    private void SetArtifactDialogue()
    {
        int attempts = 3;
        int newArtifactIndex = UnityEngine.Random.Range(0, artifactDialogueFiles.Length);

        while (newArtifactIndex == lastArtifactIndex && attempts > 0)
        {
            newArtifactIndex = UnityEngine.Random.Range(0, artifactDialogueFiles.Length);
            attempts--;
        }

        lastArtifactIndex = newArtifactIndex;
        characterDialogueScript.setPlayerDialogue(artifactDialogueFiles[newArtifactIndex]);
    }

    // Checks if the artifact can be rotated
    private void CanRotateArtifactCheck()
    {
        if (canRotateArtifact && !transitionFadeScript.IsChangingScenes && pauseMenuScript.CanPause)
        {
            //DebuggingCheck();

            // Rotation via keys
            if (!Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Horizontal") * rotateWithKeysSpeed * Time.deltaTime;
                verticalAxis = Input.GetAxis("Vertical") * rotateWithKeysSpeed * Time.deltaTime;

                artifactRotationHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactRotationHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
            // Rotation via mouse
            if (Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Mouse X") * rotateWithMouseSpeed * Time.deltaTime;
                verticalAxis = Input.GetAxis("Mouse Y") * rotateWithMouseSpeed * Time.deltaTime;

                artifactRotationHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactRotationHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
        }
    }

    // Triggers the sequence that closes the wooden chest
    private IEnumerator CloseArtifactChest()
    {      
        // Sets the artifact inactive if the player has collected it
        if (hasCollectedArtifact)
            artifactRotationHolder.SetActive(false);

        woodenChestAnim.SetTrigger("Close");
        yield return new WaitForSeconds(0.166f);
        audioManagerScript.PlayClosingChestSFX();
    }

    // Transitions the artifact camera view
    private IEnumerator TransitionToArtifactView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetArtifactCameraView();
        SetChestRotation();
        characterDialogueScript.ChangeContinueButtonText("Go Back");
        continueButton.SetActive(true);

        if (tutorialDialogueScript != null)
            tutorialDialogueScript.PlayArtifactDialogueCheck();

        yield return new WaitForSeconds(duration);
        characterDialogueScript.hasTransitionedToArtifactView = true;
    }

    // Transitions to the camera's previous position and rotation
    private IEnumerator TransitionToPreviousView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        ResetCameraView();
        WoodenChestHolder.transform.localEulerAngles = wchOriginalRotation;

        yield return new WaitForSeconds(duration / 2);
        if (!dialogueOptionsBubble.activeSelf)
            characterDialogueScript.OpenDialogueOptionsBubble();
    }

    // Sets the scripst to use
    private void SetScripts()
    {
        cameraScript = FindObjectOfType<CameraController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            tutorialDialogueScript = FindObjectOfType<TutorialDialogue>();
        else
            tutorialDialogueScript = null;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "WoodenChestHolder")
            {
                WoodenChestHolder = child;
                woodenChestAnim = child.GetComponentInChildren<Animator>();
            }
                
            if (child.name == "ArtifactHolder")
            {
                artifactHolder = child;

                for (int k = 0; k < artifactHolder.transform.childCount; k++)
                {
                    GameObject child02 = artifactHolder.transform.GetChild(k).gameObject;

                    if (child02.name == "ArtifactRotationHolder")
                        artifactRotationHolder = child02;
                }
            }           
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ContinueButton")
                continueButton = child;
        }

        for (int i = 0; i < characterDialogueScript.transform.childCount; i++)
        {
            GameObject child = characterDialogueScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueOptionsBubble")
                dialogueOptionsBubble = child;
        }

        string objectName = gameObject.name;
        artifactName = objectName.Substring(0, objectName.Length - 3); // Subtracts the 3 characters at the end of the game object's name ("_AC");

        player = playerScript.gameObject;
        pixelatedCamera = cameraScript.gameObject;

        wchOriginalRotation = WoodenChestHolder.transform.localEulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;
        arhOriginalRotation = artifactRotationHolder.transform.localEulerAngles;
        inspectingArtifactRotation = new Vector3(-20, -180, 0);

        rotateWithKeysSpeed = gameManagerScript.rotateWithKeysSpeed;
        rotateWithMouseSpeed = gameManagerScript.rotateWithMouseSpeed;
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
