using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private bool hasPaused = false;

    private int cameraIndex;
    private float cameraSpeed; // 3f

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);
    
    private Transform[] puzzleViews;
    private Vector3 currentView;
    private Vector3 currentDialogueView;
    private Vector3 originalCameraRotation;

    private GameHUD gameHUDScript;
    private GeneratorScript generatorScript;
    private TileMovementController playerScript;
    private DialogueCameraViews dialogueCameraViewsScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;

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

    // Update is called once per frame
    void Update()
    {
        //CheckIfPaused();
        DebuggingCheck();
    }

    /*** Functions for save manager START here ***/
    // Sets the cameraIndex to the saved cameraIndex
    public void LoadSavedCameraIndex()
    {
        cameraIndex = PlayerPrefs.GetInt("cameraIndex");
    }

    // Sets the cameraIndex - for save manager script ONLY
    public void SetCameraIndex(int index)
    {
        cameraIndex = index;
    }

    // returns the value of the cameraIndex
    public int ReturnCameraIndex()
    {
        return cameraIndex;
    }
    /*** Functions for save manager END here ***/

    // Moves the camera to the next puzzle view
    public void NextPuzzleView()
    {
        if (cameraIndex < puzzleViews.Length - 1)
        {
            //Debug.Log("Moved to next puzzle view");
            audioManagerScript.ChangeLoopingAmbientSFX();
            audioManagerScript.PlayWindGushSFX();
            LerpCameraToNextPuzzleView();
            SetPuzzleNumber(); // Set puzzle number must come after lerping camera


            // Turns off the generator's light (if applicable)
            if (SceneManager.GetActiveScene().name == "FifthMap")
                generatorScript.TurnOffEmisionAndVolume();
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
        if (playerScript.playerDirection == left)
            currentDialogueView = dialogueCameraViewsScript.dialogueCameraViews[0].position;

        if (playerScript.playerDirection == right)
            currentDialogueView = dialogueCameraViewsScript.dialogueCameraViews[1].position;

        if (playerScript.playerDirection == up)
            currentDialogueView = dialogueCameraViewsScript.dialogueCameraViews[2].position;

        if (playerScript.playerDirection == down)
            currentDialogueView = dialogueCameraViewsScript.dialogueCameraViews[3].position;

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
        puzzleViews = gameManagerScript.puzzleViews;
        cameraSpeed = gameManagerScript.cameraSpeed;
    }

    // Determines which scripts to find
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        gameManagerScript = FindObjectOfType<GameManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();

        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<GeneratorScript>();
    }

    // Enables debugging for the puzzle views and cameraSpeed - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                audioManagerScript.ChangeLoopingAmbientSFX();
                audioManagerScript.PlayWindGushSFX();
                LerpCameraToNextPuzzleView();
            }

            if (cameraSpeed != gameManagerScript.cameraSpeed)
                cameraSpeed = gameManagerScript.cameraSpeed;
        }
    }

}
