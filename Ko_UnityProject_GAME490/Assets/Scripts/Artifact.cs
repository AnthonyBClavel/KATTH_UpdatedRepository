using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Artifact : MonoBehaviour
{
    public Artifact_SO artifact;
    private bool hasInspectedArtifact = false;

    private string artifactName;
    private float scrollSpeed = 20f; // Original Value = 20f
    private float rotationSpeedWithKeys = 26f; // Original Value = 26f
    private float rotationSpeedWithMouse = 20f; // Original Value = 20f
    private float horizontalAxis;
    private float verticalAxis;

    private Animator woodenChestAnim;
    private GameObject woodenChestHolder;
    private GameObject artifactHolder;
    private GameObject artifactButtons;
    private GameObject player;
    private GameObject mainCamera;

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
    private GameManager gameManagerScript;
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
        Debug.Log("AYOO?");
        int artifactsCount = PlayerPrefs.GetInt("numberOfArtifactsCollected");
        string artifactsCollected = PlayerPrefs.GetString("listOfArtifacts");

        if (artifactsCount < 15 && artifactHolder.activeSelf && enabled)
        {
            Debug.Log("IT WORKED");
            int totalArtifacts = (SceneManager.GetActiveScene().name == "TutorialMap") ? 1 : 15;
            gameHUDScript.UpdateArtifactBubbleText($"{artifactsCount + 1}/{totalArtifacts}");

            notificationBubblesScript.PlayArtifactNotificationCheck();
            saveManagerScript.SaveCollectedArtifact(artifactsCollected + artifactName);
            saveManagerScript.SaveNumberOfArtifactsCollected(artifactsCount + 1);

            artifactHolder.SetActive(false);
            enabled = false;
        }
    }

    // Sets the camera view to a new position and rotation (up-close view of the artifact)
    private void SetArtifactView()
    {
        float playerRotationY = player.transform.eulerAngles.y;
        cameraScript.StopAllCoroutines();

        // Positions the camera slightly above the player and sets its y rotation to the player's y rotation
        mainCamera.transform.position = player.transform.position + new Vector3(0, 2.15f, 0);
        mainCamera.transform.eulerAngles = mainCamera.transform.eulerAngles + new Vector3(0, playerRotationY, 0);
        ahZoomPosition = mainCamera.transform.TransformPoint(Vector3.forward);

        // Rotates the artifact and the chest to look towards the player
        woodenChestHolder.transform.LookAt(player.transform.position);
        artifactHolder.transform.LookAt(mainCamera.transform.position);
        artifactHolder.transform.Rotate(artifactHolder.transform.up, 180, Space.World);
        ahInspectingRotation = artifactHolder.transform.eulerAngles;     

        // Alternative rotation methods - For Reference
        //artifactHolder.transform.eulerAngles += new Vector3(0, playerRotationY, 0);
        //woodenChestHolder.transform.eulerAngles = new Vector3(0, playerRotationY + 180, 0);
    }

    // Sets the camera view back to it's previous position and rotation (dialogue view)
    private void SetPreviousView()
    {
        cameraScript.SetToDialogueView();

        // Resets the artifact and the chest to their default rotations
        artifactHolder.transform.position = ahOriginalPosition;
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
        SetArtifactView();

        if (tutorialDialogueScript != null && tutorialDialogueScript.PlayArtifactDialogueCheck())
            inTutorialDialogue = true;
        else
            artifactButtons.SetActive(true);

        yield return new WaitForSeconds(duration);
        if (!inTutorialDialogue)
            StartInputCoroutine();
    }

    // Transitions to the camera's previous view
    private IEnumerator TransitionToPreviousView()
    {
        float duration = transitionFadeScript.fadeOutAndIn / 2f;
        transitionFadeScript.PlayTransitionFade();

        yield return new WaitForSeconds(duration);
        SetPreviousView();

        yield return new WaitForSeconds(duration / 2f);
        characterDialogueScript.OpenDialogueOptions();
    }

    // Checks for the input that rotates or resets the artifact
    private IEnumerator ArtifactInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (artifactButtons.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                //DebuggingCheck();
                StopInputCheck();
                ResetInputCheck();
                RotateInputCheck();
                ZoomInputCheck();
            }
            yield return null;
        }

        // Checks to reset the artifact to its original position
        while (artifactHolder.transform.position != ahOriginalPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahOriginalPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahOriginalPosition, scrollSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahOriginalPosition;

            yield return null;
        }

        //Debug.Log("Artifact input check has ended");
    }

    // Checks when to stop inspecting the artifact
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

    // Checks to zoom the artifact in/out
    private void ZoomInputCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift) && artifactHolder.transform.position != ahZoomPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahZoomPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahZoomPosition, scrollSpeed * Time.deltaTime);
            else
                artifactHolder.transform.position = ahZoomPosition;
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && artifactHolder.transform.position != ahOriginalPosition)
        {
            if (Vector3.Distance(artifactHolder.transform.position, ahOriginalPosition) > 0.01f)
                artifactHolder.transform.position = Vector3.Lerp(artifactHolder.transform.position, ahOriginalPosition, scrollSpeed * Time.deltaTime);
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

        DebuggingCheck();
    }

    // Updates the rotation speeds if applicable
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (rotationSpeedWithKeys != gameManagerScript.rotationSpeedWithKeys)
                rotationSpeedWithKeys = gameManagerScript.rotationSpeedWithKeys;

            if (rotationSpeedWithMouse != gameManagerScript.rotationSpeedWithMouse)
                rotationSpeedWithMouse = gameManagerScript.rotationSpeedWithMouse;
        }
    }

}
