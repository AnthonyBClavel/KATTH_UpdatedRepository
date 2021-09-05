using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtifactScript : MonoBehaviour
{
    public string artifactName;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);
    private Vector3 originalChestRotation;

    [Header("Bools")]
    public bool hasFinsihedTransition = false;
    public bool canRotateArtifact = false;
    public bool canTransitionFade = true;
    public bool hasInspectedArtifact = false;
    public bool hasCollectedArtifact = false;
    public bool isInspectingArtifact = false;

    [Header("Transforms")]
    public Transform artifactHolder;
    public Transform artifactTransform;
    private Transform playerTransform;
    private Transform pixelatedCamera;
    private GameObject artifactObject;

    [Header("Variables")]
    public float rotationSpeedWithKeys;
    public float rotationSpeedWithMouse;
    private float horizontalAxis;
    private float verticalAxis;
    private int lastArtifactIndex;

    private Vector3 originalArtifactHolderRotation;
    private Vector3 originalArtifactRotation;
    private Vector3 inspectingArtifactRotation;
    private Vector3 cameraOrigPosition;
    private Vector3 cameraOrigrotation;

    [Header("Animator And GameObject")]
    public Animator woodenChestAnim;
    public GameObject WoodenCrateHolder;

    [Header("Audio")]
    public AudioClip chestOpeningThumpSFX;
    public AudioClip chestClosingThumpSFX;
    private AudioSource audioSource;

    [Header("Artifact Dialogue Array")]
    public TextAsset[] artifactDialogueFiles;

    private CameraController cameraScript;
    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private GameHUD gameHUDScript;
    private SaveManagerScript saveManagerScript;
    private TutorialDialogueManager tutorialDialogueManager;

    void Awake()
    {
        SetScripts();
    }

    // Start is called before the first frame update
    void Start()
    {
        CollectedArtifactsCheck();

        pixelatedCamera = cameraScript.gameObject.GetComponent<Transform>();
        originalArtifactHolderRotation = artifactHolder.eulerAngles;

        originalArtifactRotation = artifactTransform.localEulerAngles;
        inspectingArtifactRotation = new Vector3(-20, -180, 0);

        audioSource = GetComponent<AudioSource>();
        artifactObject = artifactTransform.gameObject;
        originalChestRotation = WoodenCrateHolder.transform.localEulerAngles;
    }


    // Update is called once per frame
    void Update()
    {
        RotateArtifactCheck();
    }

    void FixedUpdate()
    {
        /*if (Input.GetKey(KeyCode.W))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x + rotationSpeed, artifact.eulerAngles.y, 0);
        if (Input.GetKey(KeyCode.S))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x - rotationSpeed, artifact.eulerAngles.y, 0);
        if (Input.GetKey(KeyCode.A))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x, artifact.eulerAngles.y + rotationSpeed, 0);
        if (Input.GetKey(KeyCode.D))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x, artifact.eulerAngles.y - rotationSpeed, 0);*/

        //float horizontalAxis = Input.GetAxis("Horizontal");
        //float verticalAxis = Input.GetAxis("Vertical");
        //Vector3 currEulerAngles = transform.localEulerAngles;

        //currEulerAngles.y += -horizontalAxis * rotationSpeed * Time.deltaTime;
        //currEulerAngles.x += verticalAxis * rotationSpeed * Time.deltaTime;

        //transform.localRotation = Quaternion.Euler(currEulerAngles);

        MoveObjectWithKeysCheck();
        MoveObjectWithMouseCheck();
    }

    // Checks to see if the player has already collected the artifact
    public void CollectedArtifactsCheck()
    {
        string listOfArtifactNames = PlayerPrefs.GetString("listOfArtifacts");

        if (listOfArtifactNames.Contains(artifactName) && hasCollectedArtifact != true)
            hasCollectedArtifact = true;
    }

    // Saves the name of the artifact within the large string, and updates/plays the artifact notification
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

            gameHUDScript.PlayArtifactNotificationCheck();
            saveManagerScript.SaveCollectedArtifact(listOfArtifactNames + artifactName);
            saveManagerScript.SaveNumberOfArtifactsCollected(PlayerPrefs.GetInt("numberOfArtifactsCollected") + 1);
        }
    }

    // Interacts with the artifact - transitions to the close up camera view of the artifact
    public void InspectArtifact()
    {
        if (canTransitionFade)
        {
            StartCoroutine("TransitionToArtifactView");
            canTransitionFade = false;
        }
    }

    // Stops inspecting the artifact - transitions back to the previous camera view
    public void StopInspectingArtifact()
    {
        if (canTransitionFade)
        {
            StartCoroutine("TransitionOutOfArtifactView");
            canTransitionFade = false;
        }
    }

    // Opens the wooden chest
    public void OpenChest()
    {
        PlayOpenChestSFX();
        woodenChestAnim.SetTrigger("Open");
    }

    // closes the wooden chest
    public void CloseChest()
    {
        StartCoroutine("CloseWoodenChestSequence");
    }

    // Checks when the artifact can be rotated/reseted 
    private void RotateArtifactCheck()
    {
        if (Input.GetKeyDown(KeyCode.R) && canRotateArtifact && !pauseMenuScript.isChangingScenes)
        {
            if (isInspectingArtifact)
                SetInspectingRotation();
            else
                SetDefaultRotation();

            //transform.rotation = Quaternion.identity;
        }
    }

    // Sets the rotation of the wooden chest so that it faces the player
    private void SetChestRotation()
    {
        if (playerScript.playerDirection == up)
            WoodenCrateHolder.transform.localEulerAngles = down;

        if (playerScript.playerDirection == left)
            WoodenCrateHolder.transform.localEulerAngles = right;

        if (playerScript.playerDirection == down)
            WoodenCrateHolder.transform.localEulerAngles = up;

        if (playerScript.playerDirection == right)
            WoodenCrateHolder.transform.localEulerAngles = left;
    }

    // Sets the new set of dialogue for the artifact
    public void SetArtifactDialogue()
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

    // Sets the camera to look at the artifact up close
    public void SetNewCameraView()
    {
        cameraOrigPosition = pixelatedCamera.position;
        cameraOrigrotation = pixelatedCamera.eulerAngles;

        isInspectingArtifact = true;
        cameraScript.canMoveToArtifactView = true;

        pixelatedCamera.position = playerTransform.localPosition + new Vector3(0, 2.15f, 0);
        pixelatedCamera.eulerAngles = new Vector3(pixelatedCamera.transform.eulerAngles.x, playerTransform.localEulerAngles.y, pixelatedCamera.transform.eulerAngles.z);

        SetInspectingRotation();
        canRotateArtifact = true;
    }
    
    // Sets the camera back to it's previous position and rotation
    public void ResetCameraView()
    {
        Debug.Log("Has Reseted Camera View");
        pixelatedCamera.position = cameraOrigPosition;
        pixelatedCamera.eulerAngles = cameraOrigrotation;

        isInspectingArtifact = false;
        cameraScript.canMoveToArtifactView = false;

        SetDefaultRotation();
        canRotateArtifact = false;
    }

    // Rotates the artifact to look at the new camera view
    private void SetInspectingRotation()
    {
        artifactHolder.LookAt(pixelatedCamera);
        artifactTransform.localRotation = Quaternion.Euler(inspectingArtifactRotation);
    }

    // Resets the artifact to its default rotation
    private void SetDefaultRotation()
    {
        if (artifactHolder.eulerAngles != originalArtifactHolderRotation)
            artifactHolder.eulerAngles = originalArtifactHolderRotation;

        artifactTransform.localRotation = Quaternion.Euler(originalArtifactRotation);
    }

    // Plays the sfx for opening the chest
    private void PlayOpenChestSFX()
    {
        //audioSource.volume = 0.65f;
        //audioSource.pitch = 1f;
        audioSource.PlayOneShot(chestOpeningThumpSFX);
    }

    // Plays the sfx for closing the chest
    private void PlayCloseChestSFX()
    {
        //audioSource.volume = 0.65f;
        //audioSource.pitch = 1f;
        audioSource.PlayOneShot(chestClosingThumpSFX);
    }

    // Checks to see if the artifact can move with the arrow keys
    private void MoveObjectWithKeysCheck()
    {
        if (canRotateArtifact && !Input.GetMouseButton(0) && !pauseMenuScript.isChangingScenes)
        {
            horizontalAxis = Input.GetAxis("Horizontal") * rotationSpeedWithKeys * Time.deltaTime;
            verticalAxis = Input.GetAxis("Vertical") * rotationSpeedWithKeys * Time.deltaTime;

            artifactTransform.Rotate(Vector3.up, -horizontalAxis);
            artifactTransform.Rotate(Vector3.right, verticalAxis);
        }
    }

    // Checks to see if the artifact can move with the mouse
    private void MoveObjectWithMouseCheck()
    {
        if (canRotateArtifact && Input.GetMouseButton(0) && !pauseMenuScript.isChangingScenes)
        {
            horizontalAxis = Input.GetAxis("Mouse X") * rotationSpeedWithMouse * Time.deltaTime;
            verticalAxis = Input.GetAxis("Mouse Y") * rotationSpeedWithMouse * Time.deltaTime;

            artifactTransform.Rotate(Vector3.up, -horizontalAxis);
            artifactTransform.Rotate(Vector3.right, verticalAxis);
        }
    }

    // Sets the artifact inactive - for when the player collects it
    private void HasCollectArtifactCheck()
    {
        if (hasCollectedArtifact)
            artifactObject.SetActive(false);
    }

    // Determines which scripts to find
    private void SetScripts()
    {
        cameraScript = FindObjectOfType<CameraController>();
        playerTransform = FindObjectOfType<TileMovementController>().gameObject.transform;
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            tutorialDialogueManager = FindObjectOfType<TutorialDialogueManager>();
    }

    // Triggers the sequence that closes the wooden chest
    private IEnumerator CloseWoodenChestSequence()
    {
        // Checks to see if the artifact was in the tutorial - ends the tutorial dialogue properly if so
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            tutorialDialogueManager.EndTutorialDialogueManager();

        HasCollectArtifactCheck();
        woodenChestAnim.SetTrigger("Close");
        yield return new WaitForSeconds(0.166f);
        PlayCloseChestSFX();
    }

    // Transitions the the close up camera view of the artifact
    private IEnumerator TransitionToArtifactView()
    {
        characterDialogueScript.dialogueArrow.SetActive(false);
        characterDialogueScript.fadeTransition.SetActive(true);
        pauseMenuScript.canPause = false;

        yield return new WaitForSeconds(1f);
        SetNewCameraView();
        SetChestRotation();
        characterDialogueScript.ChangeContinueButtonText("Go Back");
        characterDialogueScript.continueButton.SetActive(true);

        yield return new WaitForSeconds(1f);
        pauseMenuScript.canPause = true;
        characterDialogueScript.fadeTransition.SetActive(false);
        characterDialogueScript.hasTransitionedToArtifactView = true;
        canTransitionFade = true;
    }

    // Transitions back to the previous camera view
    private IEnumerator TransitionOutOfArtifactView()
    {
        characterDialogueScript.fadeTransition.SetActive(true);
        pauseMenuScript.canPause = false;

        yield return new WaitForSeconds(1f);
        ResetCameraView();
        WoodenCrateHolder.transform.localEulerAngles = originalChestRotation;

        yield return new WaitForSeconds(0.5f);
        if (!characterDialogueScript.dialogueOptionsBubble.activeSelf)
            characterDialogueScript.OpenDialogueOptionsBubble();

        yield return new WaitForSeconds(0.5f);
        pauseMenuScript.canPause = true;
        characterDialogueScript.fadeTransition.SetActive(false);
        canTransitionFade = true;
    }

}
