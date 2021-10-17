using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private bool hasPaused = false;
    private float cameraSpeed; // 3f
    private int cameraIndex;

    private GameObject northDialogueView;
    private GameObject eastDialogueView;
    private GameObject southDialogueView;
    private GameObject westDialogueView;

    private Transform[] puzzleViews;
    private Vector3 currentView;
    private Vector3 currentDialogueView;
    private Vector3 originalCameraRotation;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    private GameHUD gameHUDScript;
    private Generator generatorScript;
    private TileMovementController playerScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private PuzzleManager puzzleManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalCameraRotation = transform.eulerAngles;
        SetCameraPosition();
        SetPuzzleNumber();
    }

    // Returns or sets the cameraSpeed
    public float CameraSpeed
    {
        get
        {
            return cameraSpeed;
        }
        set
        {
            cameraSpeed = value;
        }
    }

    // Returns or sets the cameraIndex
    public int CameraIndex
    {
        get
        {
            return cameraIndex;
        }
        set
        {
            cameraIndex = value;
        }
    }

    // Moves the camera to the next puzzle view
    public void NextPuzzleView()
    {
        if (cameraIndex < puzzleViews.Length - 1)
        {
            //Debug.Log("Moved to next puzzle view");
            puzzleManagerScript.ResetGeneratorCheck();
            audioManagerScript.ChangeLoopingAmbienceSFX();
            audioManagerScript.PlayWindGushSFX();
            LerpCameraToNextPuzzleView();
            SetPuzzleNumber(); // Set puzzle number must come AFTER lerping camera
        }         
    }

    // Moves the camera to the previous puzzle view
    public void PreviousPuzzleView()
    {
        if (cameraIndex > 0)
        {
            //Debug.Log("Moved to next puzzle view");
            puzzleManagerScript.ResetGeneratorCheck();
            audioManagerScript.ChangeLoopingAmbienceSFX();
            audioManagerScript.PlayWindGushSFX();
            LerpCameraToPreviousPuzzleView();
            SetPuzzleNumber(); // Set puzzle number must come AFTER lerping camera
        }
    }

    // Sets the camera to the current dialogue view
    public void SetCameraToCurrentDialogueView()
    {
        transform.position = currentDialogueView;
        transform.eulerAngles = originalCameraRotation;
    }

    // Lerps the camera's position to the current puzzle view
    public void LerpCameraToCurrentPuzzleView()
    {
        currentView = puzzleViews[cameraIndex].position;
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));
    }

    // Checks which dialogue view the camera should move to - for character/artifact dialogue
    public void LerpCameraToDialogueView()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        if (playerDirection == down)
            currentDialogueView = northDialogueView.transform.position;

        if (playerDirection == left)
            currentDialogueView = eastDialogueView.transform.position;

        if (playerDirection == up)
            currentDialogueView = southDialogueView.transform.position;

        if (playerDirection == right)
            currentDialogueView = westDialogueView.transform.position;

        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentDialogueView));
    }

    // Lerps the camera to the next puzzle view
    private void LerpCameraToNextPuzzleView()
    {
        cameraIndex++;

        if (cameraIndex > puzzleViews.Length - 1)
            cameraIndex = 0;

        currentView = puzzleViews[cameraIndex].position;
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));      
    }

    // Lerps the camera to the previous puzzle view
    private void LerpCameraToPreviousPuzzleView()
    {
        cameraIndex--;

        if (cameraIndex < 0)
            cameraIndex = puzzleViews.Length - 1;

        currentView = puzzleViews[cameraIndex].position;
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));
    }

    // Sets the camera's initial position - determined by currentIndex (does not set during intro or if on first puzzle)
    private void SetCameraPosition()
    {
        if (transform.position != puzzleViews[cameraIndex].position)
            transform.position = puzzleViews[cameraIndex].position;           
    }

    // Updates and checks to play the puzzle notification bubble
    private void SetPuzzleNumber()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/8");
        else
            gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/10");

        notificationBubblesScript.PlayPuzzleNotificationCheck();
    }

    // Pauses the WindGushSFX if you pause the game on a bridge - OPTIONAL (not currenlty used)
    private void CheckIfPaused()
    {
        PauseMenu pauseMenuScript = FindObjectOfType<PauseMenu>();

        if (pauseMenuScript.isPaused && !hasPaused)
        {
            Debug.Log("Wind Gush SFX has been paused");
            //audioSource.Pause();
            hasPaused = true;
        }
        else if (pauseMenuScript.isPaused && hasPaused)
        {
            Debug.Log("Wind Gush SFX has resumed");
            //audioSource.UnPause();
            hasPaused = false;
        }
    }

    // Lerps the position of the camera to a new position (endPosition = position to lerp to)
    private IEnumerator LerpCamera(Vector3 endPosition)
    {
        gameManagerScript.CheckForCameraScriptDebug();

        while (transform.position != endPosition)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = endPosition;
    }

    // Sets the puzzle views array and camera transition speed
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            GameObject child = transform.parent.GetChild(i).gameObject;

            if (child.name == "DialogueViewsHolder")
            {
                GameObject dialogueViewsHolder = child;

                for (int j = 0; j < dialogueViewsHolder.transform.childCount; j++)
                {
                    GameObject child02 = dialogueViewsHolder.transform.GetChild(j).gameObject;

                    if (child02.name == "NorthDV")
                        northDialogueView = child02;
                    if (child02.name == "EastDV")
                        eastDialogueView = child02;
                    if (child02.name == "SouthDV")
                        southDialogueView = child02;
                    if (child02.name == "WestDV")
                        westDialogueView = child02;
                }
            }
        }

        puzzleViews = gameManagerScript.puzzleViews;
        cameraSpeed = gameManagerScript.cameraSpeed;
    }

    // Determines which scripts to find
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameManagerScript = FindObjectOfType<GameManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();

        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<Generator>();
    }

    // Enables debugging for the puzzle views and cameraSpeed - For Debugging Purposes ONLY
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Equals)) // =
            {
                audioManagerScript.ChangeLoopingAmbienceSFX();
                audioManagerScript.PlayWindGushSFX();
                LerpCameraToNextPuzzleView();
                Debug.Log("Debugging: Moved Camera To Puzzle " + (cameraIndex + 1));
            }

            if (Input.GetKeyDown(KeyCode.Minus)) // -
            {
                audioManagerScript.ChangeLoopingAmbienceSFX();
                audioManagerScript.PlayWindGushSFX();
                LerpCameraToPreviousPuzzleView();
                Debug.Log("Debugging: Moved Camera To Puzzle " + (cameraIndex + 1));
            }
        }
    }

}
