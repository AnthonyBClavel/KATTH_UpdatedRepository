using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtifactScript : MonoBehaviour
{
    public string artifactName;

    [Header("Bools")]
    public bool canRotateArtifact = false;
    public bool canTransitionFade = true;
    public bool hasInspectedArtifact = false;
    public bool hasCollectedArtifact = false;
    public bool isInspectingArtifact = false;

    private float rotationSpeedWithKeys; // 200f
    private float rotationSpeedWithMouse; // 500f
    private float horizontalAxis;
    private float verticalAxis;
    private int lastArtifactIndex;

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
    private Vector3 cameraOriginalPosition;
    private Vector3 cameraOriginalRotation;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    [Header("Artifact Dialogue Array")]
    public TextAsset[] artifactDialogueFiles;
    public TextAsset dialogueOptionsFile;

    private CameraController cameraScript;
    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private GameHUD gameHUDScript;
    private SaveManagerScript saveManagerScript;
    private TutorialDialogueManager tutorialDialogueManager;
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
        DebuggingCheck();
    }

    void FixedUpdate()
    {
        CanRotateArtifactCheck();
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

    // Transitions to the close up view of the artifact
    public void InspectArtifact()
    {
        if (canTransitionFade)
        {
            StartCoroutine("TransitionToArtifactCameraView");
            canTransitionFade = false;
        }
    }

    // Transitions back to the previous camera view
    public void StopInspectingArtifact()
    {
        if (canTransitionFade)
        {
            StartCoroutine("TransitionToPreviousCameraView");
            canTransitionFade = false;
        }
    }

    // Closes the wooden chest
    public void CloseChest()
    {
        StartCoroutine("CloseWoodenChestSequence");
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
        if (Input.GetKeyDown(KeyCode.R) && canRotateArtifact && !pauseMenuScript.isChangingScenes)
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
        if (playerScript.playerDirection == up)
            WoodenChestHolder.transform.localEulerAngles = down;

        if (playerScript.playerDirection == left)
            WoodenChestHolder.transform.localEulerAngles = right;

        if (playerScript.playerDirection == down)
            WoodenChestHolder.transform.localEulerAngles = up;

        if (playerScript.playerDirection == right)
            WoodenChestHolder.transform.localEulerAngles = left;
    }

    // Sets the artifact camera view - sets the camera's position and rotation to the player's
    private void SetArtifactCameraView()
    {
        cameraOriginalPosition = pixelatedCamera.transform.position;
        cameraOriginalRotation = pixelatedCamera.transform.eulerAngles;

        isInspectingArtifact = true;
        cameraScript.canMoveToArtifactView = true;

        pixelatedCamera.transform.position = player.transform.localPosition + new Vector3(0, 2.15f, 0); // Sets the camera slighty above the player
        pixelatedCamera.transform.eulerAngles = new Vector3(pixelatedCamera.transform.eulerAngles.x, player.transform.localEulerAngles.y, pixelatedCamera.transform.eulerAngles.z);

        SetInspectingRotation();
        canRotateArtifact = true;
    }

    // Sets the camera back to it's previous position and rotation
    private void ResetCameraView()
    {
        //Debug.Log("Has Reset Camera View");
        pixelatedCamera.transform.position = cameraOriginalPosition;
        pixelatedCamera.transform.eulerAngles = cameraOriginalRotation;

        isInspectingArtifact = false;
        cameraScript.canMoveToArtifactView = false;

        SetDefaultRotation();
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
        if (canRotateArtifact && !pauseMenuScript.isChangingScenes)
        {
            // Rotation via keys
            if (!Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Horizontal") * rotationSpeedWithKeys * Time.deltaTime;
                verticalAxis = Input.GetAxis("Vertical") * rotationSpeedWithKeys * Time.deltaTime;

                artifactRotationHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactRotationHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
            // Rotation via mouse
            if (Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Mouse X") * rotationSpeedWithMouse * Time.deltaTime;
                verticalAxis = Input.GetAxis("Mouse Y") * rotationSpeedWithMouse * Time.deltaTime;

                artifactRotationHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactRotationHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
        }
    }

    // Sets the artifact inactive when the player collects it
    private void SetArtifactInactiveCheck()
    {
        if (hasCollectedArtifact)
            artifactRotationHolder.SetActive(false);
    }

    // Triggers the sequence that closes the wooden chest
    private IEnumerator CloseWoodenChestSequence()
    {
        // Checks if the artifact was in the tutorial - ends the tutorial dialogue properly if so
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            tutorialDialogueManager.EndTutorialDialogueManager();

        SetArtifactInactiveCheck();
        woodenChestAnim.SetTrigger("Close");
        yield return new WaitForSeconds(0.166f);
        audioManagerScript.PlayClosingChestSFX();
    }

    // Transitions the artifact camera view
    private IEnumerator TransitionToArtifactCameraView()
    {
        transitionFadeScript.PlayTransitionFade();
        float duration = transitionFadeScript.fadeInAndOut / 2;

        yield return new WaitForSeconds(duration);
        SetArtifactCameraView();
        SetChestRotation();
        characterDialogueScript.ChangeContinueButtonText("Go Back");
        continueButton.SetActive(true);

        yield return new WaitForSeconds(duration);
        characterDialogueScript.hasTransitionedToArtifactView = true;
        canTransitionFade = true;
    }

    // Transitions to the camera's previous position and rotation
    private IEnumerator TransitionToPreviousCameraView()
    {
        transitionFadeScript.PlayTransitionFade();
        float duration = transitionFadeScript.fadeInAndOut / 2;

        yield return new WaitForSeconds(duration);
        ResetCameraView();
        WoodenChestHolder.transform.localEulerAngles = wchOriginalRotation;

        yield return new WaitForSeconds(duration / 2);
        if (!dialogueOptionsBubble.activeSelf)
            characterDialogueScript.OpenDialogueOptionsBubble();

        yield return new WaitForSeconds(duration / 2);
        canTransitionFade = true;
    }

    // Sets the scripst to use
    private void SetScripts()
    {
        cameraScript = FindObjectOfType<CameraController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            tutorialDialogueManager = FindObjectOfType<TutorialDialogueManager>();
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

        player = playerScript.gameObject;
        pixelatedCamera = cameraScript.gameObject;

        wchOriginalRotation = WoodenChestHolder.transform.localEulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;
        arhOriginalRotation = artifactRotationHolder.transform.localEulerAngles;
        inspectingArtifactRotation = new Vector3(-20, -180, 0);

        rotationSpeedWithKeys = gameManagerScript.rotationWithKeys;
        rotationSpeedWithMouse = gameManagerScript.rotationWithMouse;
    }

    // Updates the rotation speeds for the artifact inputs - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (rotationSpeedWithKeys != gameManagerScript.rotationWithKeys)
                rotationSpeedWithKeys = gameManagerScript.rotationWithKeys;

            if (rotationSpeedWithMouse != gameManagerScript.rotationWithMouse)
                rotationSpeedWithMouse = gameManagerScript.rotationWithMouse;
        }
    }
}
