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

    static readonly string tutorialZone = "TutorialMap";
    private string sceneName;
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

    private NotificationBubbles notificationBubblesScript;
    private CharacterDialogue characterDialogueScript;
    private TutorialDialogue tutorialDialogueScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private CameraController cameraScript;
    private SaveManager saveManagerScript;
    private HUD headsUpDisplayScript;

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
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetArtifactInactive(); // Must be called in Start()!
    }

    // Saves the name of the collected artifact and updates the artifact notification bubble
    public void CollectArtifact()
    {
        if (hasCollectedArtifact || !enabled) return;

        int artifactCount = saveManagerScript.ArtifactCount += 1;
        saveManagerScript.ArtifactsCollected += $"{artifactName}, ";
        notificationBubblesScript.SetArtifactNotificationText(artifactCount);
        SetArtifactInactive();

        //Debug.Log("Collected artifact");
    }

    // Checks to set the artifact/script inactive - if the artifact was collected
    private void SetArtifactInactive()
    {
        if (!saveManagerScript.ArtifactsCollected.Contains(artifactName)) return;

        artifactHolder.SetActive(false);
        hasCollectedArtifact = true;
        enabled = false;
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

        woodenChestHolder.transform.eulerAngles = wchOriginalRotation;
        artifactHolder.transform.eulerAngles = ahOriginalRotation;
        artifactHolder.transform.position = ahOriginalPosition;
    }

    // Opens the artifact chest
    public void OpenChest()
    {
        if (!enabled) return;
    
        wchOriginalRotation = woodenChestHolder.transform.eulerAngles;
        ahOriginalPosition = artifactHolder.transform.position;

        audioManagerScript.PlayOpenChestSFX();
        woodenChestAnim.Play("Open");
    }

    // Closes the artifact chest
    public void CloseChest()
    {
        if (closeChestCoroutine != null) StopCoroutine(closeChestCoroutine);

        closeChestCoroutine = CloseArtifactChest();
        StartCoroutine(closeChestCoroutine);
    }

    // Starts the coroutine that transitions to the artifact view
    public void InspectArtifact()
    {
        if (artifactViewCoroutine != null) StopCoroutine(artifactViewCoroutine);

        artifactViewCoroutine = TransitionToArtifactView();
        StartCoroutine(artifactViewCoroutine);
    }

    // Starts the coroutine that transitions to the previous view
    public void StopInspectingArtifact()
    {
        if (previousViewCoroutine != null) StopCoroutine(previousViewCoroutine);

        previousViewCoroutine = TransitionToPreviousView();
        StartCoroutine(previousViewCoroutine);
    }

    // Starts the coroutine that checks for the artifact input
    public void StartInputCoroutine()
    {
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        inputCoroutine = ArtifactInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // Plays an sfx after the artifact chest closes
    private IEnumerator CloseArtifactChest()
    {
        playerScript.ChangeAnimationState("Interacting");
        woodenChestAnim.Play("Close");

        yield return new WaitForSeconds(closeAnimLength * 0.75f);
        audioManagerScript.PlayCloseChestSFX();
    }

    // Transitions to the camera's artifact view
    private IEnumerator TransitionToArtifactView()
    {
        blackOverlayScript.StartTransitionFadeCoroutine();
        float duration = blackOverlayScript.TransitionFadeDuration;   
        bool inTutorialDialogue = false;

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
        float duration = blackOverlayScript.TransitionFadeDuration;
        blackOverlayScript.StartTransitionFadeCoroutine();

        yield return new WaitForSeconds(duration);
        SetPreviousView();

        yield return new WaitForSeconds(duration * 0.5f);
        characterDialogueScript.OpenDialogueOptions();
    }

    // Checks for the artifact input
    private IEnumerator ArtifactInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (artifactButtons.activeSelf && !blackOverlayScript.IsChangingScenes)
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
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        StopInspectingArtifact();
        artifactButtons.SetActive(false);
    }

    // Checks to reset the artifact's rotation
    private void ResetInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return;

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

    // Checks to zoom into the artifact
    private void ZoomInputCheck()
    {
        // Zoom in
        if (Input.GetKey(KeyCode.LeftShift) && artifactHolder.transform.position != ahZoomPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahZoomPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahZoomPosition, zoomSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahZoomPosition;
        }
        // Zoom out
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
        tutorialDialogueScript = (sceneName == tutorialZone) ? FindObjectOfType<TutorialDialogue>() : null;
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        headsUpDisplayScript = FindObjectOfType<HUD>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "WoodenChestHolder":
                    woodenChestHolder = child.gameObject;
                    woodenChestAnim = woodenChestHolder.GetComponentInChildren<Animator>();
                    closeAnimLength = woodenChestAnim.ReturnClipLength("Close");
                    break;
                case "ArtifactHolder":
                    artifactHolder = child.gameObject;
                    break;
                case "ArtifactButtons":
                    artifactButtons = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.parent.name == "WoodenChestHolder" || child.parent.name == "ArtifactHolder") continue;
            if (child.name == "NotificationBubbles" || child.parent.name == "ArtifactButtons") continue;
            if (child.name == "DialogueOptionButtons" || child.name == "CharacterDialogue") continue;

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(headsUpDisplayScript.transform);
        SetVariables(transform);

        artifactName = artifact.artifactName;
        mainCamera = cameraScript.gameObject;
        player = playerScript.gameObject;

        wchOriginalRotation = woodenChestHolder.transform.eulerAngles;
        ahOriginalRotation = artifactHolder.transform.eulerAngles;
        ahOriginalPosition = artifactHolder.transform.position;
    }

}
