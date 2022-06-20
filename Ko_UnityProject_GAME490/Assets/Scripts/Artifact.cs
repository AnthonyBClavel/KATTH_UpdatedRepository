using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Artifact : MonoBehaviour
{
    public Artifact_SO artifact;

    [Range(0.01f, 100f)]
    private float zoomSpeed = 20f; // 20f = Original Value
    [Range(0.01f, 100f)]
    private float rotationSpeedWithKeys = 26f; // 26f = Original Value
    [Range(0.01f, 100f)]
    private float rotationSpeedWithMouse = 20f; // 20f =  Original Value
    private float horizontalAxis;
    private float verticalAxis;
    private float closeAnimLength;

    private string artifactName;
    private bool hasInspectedArtifact = false;
    private bool hasCollectedArtifact = false;

    private GameObject woodenChestHolder;
    private GameObject artifactHolder;
    private GameObject artifactButtons;
    private GameObject player;
    private GameObject mainCamera;
    private Animator woodenChestAnim;

    private Vector3 wchOriginalRotation;
    private Vector3 ahOriginalRotation;
    private Vector3 ahInspectingRotation;
    private Vector3 ahOriginalPosition;
    private Vector3 ahZoomPosition;

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
    private NotificationBubbles notificationBubblesScript;
    private TransitionFade transitionFadeScript;

    public bool HasInspectedArtifact
    {
        get { return hasInspectedArtifact; }
        set { hasInspectedArtifact = value; }
    }

    public GameObject ArtifactHolder
    {
        get { return artifactHolder; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Saves the name of the collected artifact via PlayerPrefs and updates the artifact notification bubble
    public void CollectArtifact()
    {
        int artifactCount = PlayerPrefs.GetInt("numberOfArtifactsCollected");
        string artifactsCollected = PlayerPrefs.GetString("listOfArtifacts");

        if (artifactCount >= 15 || hasCollectedArtifact || !enabled) return;

        int totalArtifacts = (SceneManager.GetActiveScene().name != "TutorialMap") ? 15 : 1;
        gameHUDScript.UpdateArtifactBubbleText($"{artifactCount + 1}/{totalArtifacts}");
        notificationBubblesScript.PlayArtifactNotificationCheck();

        saveManagerScript.SaveCollectedArtifact(artifactsCollected + artifactName);
        saveManagerScript.SaveNumberOfArtifactsCollected(artifactCount + 1);
        SetArtifactInactiveCheck();
        //Debug.Log("Collected artifact");
    }

    // Sets the camera and artifact to appropiate position/rotation for inspecting
    private void SetArtifactView()
    {
        cameraScript.StopAllCoroutines();

        // Rotates the camera towards the player's direction and positions it slightly above the player
        mainCamera.transform.eulerAngles = mainCamera.transform.eulerAngles + new Vector3(0, player.transform.eulerAngles.y, 0);
        mainCamera.transform.position = player.transform.position + new Vector3(0, 2.15f, 0);

        // Rotates the chest to look towards the player
        woodenChestHolder.transform.LookAt(player.transform);

        // Rotates the artifact to look at the camera
        artifactHolder.transform.LookAt(mainCamera.transform);
        artifactHolder.transform.Rotate(artifactHolder.transform.up, 180, Space.World);

        ahZoomPosition = mainCamera.transform.TransformPoint(Vector3.forward);
        ahInspectingRotation = artifactHolder.transform.eulerAngles;
    }

    // Sets the camera and artifact to their previous position/rotation
    private void SetPreviousView()
    {
        cameraScript.SetToDialogueView();

        artifactHolder.transform.position = ahOriginalPosition;
        artifactHolder.transform.eulerAngles = ahOriginalRotation;
        woodenChestHolder.transform.eulerAngles = wchOriginalRotation;
    }

    // Checks to set the artifact/script inactive - if the artifact was already collected
    private void SetArtifactInactiveCheck()
    {
        if (!PlayerPrefs.GetString("listOfArtifacts").Contains(artifactName)) return;

        artifactHolder.SetActive(false);
        hasCollectedArtifact = true;
        //enabled = false;
    }

    // Opens the artifact chest
    public void OpenChest()
    {
        if (!enabled) return;
    
        ahOriginalPosition = artifactHolder.transform.position;
        audioManagerScript.PlayOpeningChestSFX();
        woodenChestAnim.Play("Open");
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
        playerScript.ChangeAnimationState("Interacting");
        woodenChestAnim.Play("Close");

        yield return new WaitForSeconds(closeAnimLength * 0.75f);
        audioManagerScript.PlayClosingChestSFX();
    }

    // Transitions to the camera's artifact view
    private IEnumerator TransitionToArtifactView() /////////////////////////////////////////////////////
    {
        bool inTutorialDialogue = false;
        float duration = transitionFadeScript.fadeOutAndIn * 0.5f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetArtifactView();

        if (tutorialDialogueScript != null && tutorialDialogueScript.PlayDialogueCheck("Artifacts"))
            inTutorialDialogue = true;
        else
            artifactButtons.SetActive(true);

        yield return new WaitForSeconds(duration);
        if (!inTutorialDialogue) StartInputCoroutine();
    }

    // Transitions to the camera's previous view
    private IEnumerator TransitionToPreviousView()
    {
        float duration = transitionFadeScript.fadeOutAndIn * 0.5f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetPreviousView();

        yield return new WaitForSeconds(duration * 0.5f);
        characterDialogueScript.OpenDialogueOptions();
    }

    // Checks for the artifact input (rotate/reset-rotation/zoom/stop-inspecting)
    private IEnumerator ArtifactInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (artifactButtons.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                StopInputCheck();
                ResetInputCheck();
                RotateInputCheck();
                ZoomInputCheck();
            }
            yield return null;
        }

        // Lerps the artifact to its original position if applicable
        while (artifactHolder.transform.position != ahOriginalPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahOriginalPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahOriginalPosition, zoomSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahOriginalPosition;

            yield return null;
        }

        //Debug.Log("Artifact input check has ended");
    }

    // Checks to stop inspecting the artifact
    private void StopInputCheck()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StopInspectingArtifact();
            artifactButtons.SetActive(false);
        }
    }

    // Checks to reset the artifact's rotation
    private void ResetInputCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
            artifactHolder.transform.eulerAngles = ahInspectingRotation;
    }

    // Checks to rotate the artifact
    private void RotateInputCheck()
    {
        // Rotates the artifact via keys
        if (!Input.GetMouseButton(0))
        {
            horizontalAxis = Input.GetAxisRaw("Horizontal") * rotationSpeedWithKeys * 10 * Time.deltaTime;
            verticalAxis = Input.GetAxisRaw("Vertical") * rotationSpeedWithKeys * 10 * Time.deltaTime;

            artifactHolder.transform.Rotate(mainCamera.transform.up, -horizontalAxis, Space.World);
            artifactHolder.transform.Rotate(mainCamera.transform.right, verticalAxis, Space.World);
        }

        // Rotates the artifact via mouse
        else if (Input.GetMouseButton(0))
        {
            horizontalAxis = Input.GetAxisRaw("Mouse X") * rotationSpeedWithMouse * 100 * Time.deltaTime;
            verticalAxis = Input.GetAxisRaw("Mouse Y") * rotationSpeedWithMouse * 100 * Time.deltaTime;

            artifactHolder.transform.Rotate(mainCamera.transform.up, -horizontalAxis, Space.World);
            artifactHolder.transform.Rotate(mainCamera.transform.right, verticalAxis, Space.World);
        }
    }

    // Checks to zoom the artifact in and out
    private void ZoomInputCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift) && artifactHolder.transform.position != ahZoomPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahZoomPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahZoomPosition, zoomSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahZoomPosition;
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && artifactHolder.transform.position != ahOriginalPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahOriginalPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahOriginalPosition, zoomSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahOriginalPosition;
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

            if (child.name == "WoodenChestHolder")
            {
                woodenChestAnim = child.GetComponentInChildren<Animator>();
                woodenChestHolder = child;
            }             
            if (child.name == "ArtifactHolder")
                artifactHolder = child;         
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "KeybindButtons")
            {
                GameObject keybindButtons = child;

                for (int j = 0; j < keybindButtons.transform.childCount; j++)
                {
                    GameObject child02 = keybindButtons.transform.GetChild(j).gameObject;

                    if (child02.name == "ArtifactButtons")
                        artifactButtons = child02;
                }
            }
        }

        artifactName = artifact.artifactName;
        mainCamera = cameraScript.gameObject;
        player = playerScript.gameObject;

        wchOriginalRotation = woodenChestHolder.transform.eulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;
        ahOriginalPosition = artifactHolder.transform.position;

        closeAnimLength = woodenChestAnim.ReturnClipLength("Close");
        SetArtifactInactiveCheck();
    }

}
