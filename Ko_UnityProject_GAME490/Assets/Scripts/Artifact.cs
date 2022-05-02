using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    private string artifactName;
    private int currentDialogueIndex;

    private Animator woodenChestAnim;
    private GameObject woodenChestHolder;
    private GameObject artifactHolder;
    private GameObject continueButton;
    private GameObject player;
    private GameObject mainCamera;

    private TextMeshProUGUI continueButtonText; 

    private Vector3 wchOriginalRotation; // wch = wooden chest holder
    private Vector3 ahOriginalRotation; // ah = artifact holder
    private Vector3 ahInspectingRotation;

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

    public bool CanRotateArtifact
    {
        get { return canRotateArtifact; }
        set { canRotateArtifact = value; }
    }

    public bool HasInspectedArtifact
    {
        get { return hasInspectedArtifact; }
        set { hasInspectedArtifact = value; }
    }

    public bool HasCollectedArtifact
    {
        get { return hasCollectedArtifact; }
        set { hasCollectedArtifact = value; }
    }

    public bool IsInspectingArtifact
    {
        get { return isInspectingArtifact; }
        set { isInspectingArtifact = value; }
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
            hasCollectedArtifact = true;
    }

    // Update is called once per frame
    void Update()
    {
        ResetArtifactRotationCheck();
        CanRotateArtifactCheck();
    }

    // Opens the artifact chest
    public void OpenChest()
    {
        woodenChestAnim.Play("Open");
        audioManagerScript.PlayOpeningChestSFX();
    }

    // Closes the artifact chest
    public void CloseChest()
    {
        if (closeChestCoroutine != null)
            StopCoroutine(closeChestCoroutine);

        closeChestCoroutine = CloseArtifactChest();
        StartCoroutine(closeChestCoroutine);
    }

    // Sets and starts the dialogue for the artifact
    public void StartArtifactDialogue()
    {
        characterDialogueScript.isInteractingWithArtifact = true;
        characterDialogueScript.UpdateArtifactScript(this);
        characterDialogueScript.setPlayerDialogue(ReturnRandomArtifactDialogue());
        characterDialogueScript.setDialogueQuestions(dialogueOptionsFile);
        characterDialogueScript.StartDialogue();
    }

    // Saves the name of the collected artifact via PlayerPrefs and updates the artifact notification bubble
    public void CollectArtifact()
    {
        int artifactsCount = PlayerPrefs.GetInt("numberOfArtifactsCollected");
        string artifactsCollected = PlayerPrefs.GetString("listOfArtifacts");

        if (artifactsCount < 15 && !hasCollectedArtifact)
        {
            int totalArtifacts = (SceneManager.GetActiveScene().name == "TutorialMap") ? 1 : 15;
            gameHUDScript.UpdateArtifactBubbleText($"{artifactsCount + 1}/{totalArtifacts}");
            artifactHolder.SetActive(false);

            notificationBubblesScript.PlayArtifactNotificationCheck();
            saveManagerScript.SaveCollectedArtifact(artifactsCollected + artifactName);
            saveManagerScript.SaveNumberOfArtifactsCollected(artifactsCount + 1);
            hasCollectedArtifact = true;
        }
    }

    // Returns a randomly selected text asset for the dialogue
    private TextAsset ReturnRandomArtifactDialogue()
    {
        int attempts = 3;
        int newDialogueIndex = Random.Range(0, artifactDialogueFiles.Length);

        // Attempts to set a text asset that different from the one previously played
        while (newDialogueIndex == currentDialogueIndex && attempts > 0)
        {
            newDialogueIndex = Random.Range(0, artifactDialogueFiles.Length);
            attempts--;
        }

        currentDialogueIndex = newDialogueIndex;
        return artifactDialogueFiles[newDialogueIndex];
    }

    // Checks if the player can reset the artifact's rotation
    private void ResetArtifactRotationCheck()
    {
        if (Input.GetKeyDown(KeyCode.R) && canRotateArtifact && !transitionFadeScript.IsChangingScenes && pauseMenuScript.CanPause)
        {
            if (isInspectingArtifact)
                artifactHolder.transform.eulerAngles = ahInspectingRotation;

            //else
               //SetDefaultRotation();
        }
    }

    // Sets the camera view for inspecting the artifact
    private void SetArtifactView()
    {
        float playerRotationY = player.transform.eulerAngles.y;
        cameraScript.StopAllCoroutines();

        // Positions the camera slightly above the player and sets its y rotation to the player's y rotation
        mainCamera.transform.position = player.transform.position + new Vector3(0, 2.15f, 0);
        mainCamera.transform.eulerAngles = mainCamera.transform.eulerAngles + new Vector3(0, playerRotationY, 0);

        // Rotates the artifact and chest to look at the player
        artifactHolder.transform.eulerAngles += new Vector3(0, playerRotationY, 0);
        woodenChestHolder.transform.eulerAngles = new Vector3(0, playerRotationY + 180, 0);
        ahInspectingRotation = artifactHolder.transform.eulerAngles;

        isInspectingArtifact = true;
        canRotateArtifact = true;
    }

    // Sets the camera back to it's previous position and rotation
    private void SetPreviousView()
    {
        cameraScript.SetToDialogueView();

        // Resets the artifact and chest to their default rotations
        artifactHolder.transform.eulerAngles = ahOriginalRotation;
        woodenChestHolder.transform.eulerAngles = wchOriginalRotation;

        isInspectingArtifact = false;
        canRotateArtifact = false;
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

                artifactHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
            // Rotation via mouse
            if (Input.GetMouseButton(0))
            {
                horizontalAxis = Input.GetAxis("Mouse X") * rotateWithMouseSpeed * Time.deltaTime;
                verticalAxis = Input.GetAxis("Mouse Y") * rotateWithMouseSpeed * Time.deltaTime;

                artifactHolder.transform.Rotate(Vector3.up, -horizontalAxis);
                artifactHolder.transform.Rotate(Vector3.right, verticalAxis);
            }
        }
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

    // The coroutine for closing the artifact chest
    private IEnumerator CloseArtifactChest()
    {
        woodenChestAnim.Play("Close");

        // Plays the sfx at the end of the chest's closing animation
        yield return new WaitForSeconds(1f/6f); // 0.166f
        audioManagerScript.PlayClosingChestSFX();
    }

    // Transitions the artifact camera view
    private IEnumerator TransitionToArtifactView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        continueButtonText.text = "Go Back";
        continueButton.SetActive(true);
        SetArtifactView();

        if (tutorialDialogueScript != null) 
            tutorialDialogueScript.PlayArtifactDialogueCheck();

        yield return new WaitForSeconds(duration);
        characterDialogueScript.hasTransitionedToArtifactView = true;
    }

    // Transitions to the camera's previous position and rotation
    private IEnumerator TransitionToPreviousView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetPreviousView();

        yield return new WaitForSeconds(duration / 2f);
        //if (!dialogueOptionsBubble.activeSelf)
        characterDialogueScript.OpenDialogueOptionsBubble();
    }

    // Sets the scripts to use
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
                continueButton = child;
                continueButtonText = child.GetComponent<TextMeshProUGUI>();
            }
        }

        artifactName = gameObject.name.Replace("_AC", "");
        mainCamera = cameraScript.gameObject;
        player = playerScript.gameObject;

        wchOriginalRotation = woodenChestHolder.transform.eulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;

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
