using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private int cameraIndex;
    private float cameraSpeed; // 3f

    [Header("Bools")]
    public bool canMoveCamera = true;
    public bool canMoveToDialogueViews = false;
    public bool canMoveToArtifactView = false;
    public bool hasCheckedDialogueViews = false;
    private bool hasPaused = false;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);
    
    private Transform[] puzzleViews;
    private Transform currentView;

    private AmbientLoopingSFXManager theALSM;
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
        SetCameraPosition();
        SetPuzzleNumber();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckIfPaused();
        PuzzleViewsDebuggingCheck();
    }

    void LateUpdate()
    {
        if (!canMoveToArtifactView)
        {
            if (canMoveCamera && !canMoveToDialogueViews)
            {
                if (currentView != puzzleViews[cameraIndex])
                    currentView = puzzleViews[cameraIndex];

                if (transform.position != currentView.position)
                    transform.position = Vector3.Lerp(transform.position, currentView.position, cameraSpeed * Time.deltaTime);

            }
            if (canMoveToDialogueViews && !canMoveCamera)
            {
                DialogueViewsCheck();

                if (transform.position != currentView.position)
                    transform.position = Vector3.Lerp(transform.position, currentView.position, cameraSpeed * Time.deltaTime);
            }
        }
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
            //Debug.Log("Next Puzzle View Activated");
            currentView = puzzleViews[cameraIndex++];
            SetPuzzleNumber();
            PlayWindGushSFX();
            audioManagerScript.PlayWindGushSFX();

            // Turns off the generator's light (if applicable)
            if (SceneManager.GetActiveScene().name == "FifthMap")
                generatorScript.TurnOffEmisionAndVolume();
        }         
    }

    // Sets the camera's position to a puzzle view - determined by currentIndex
    private void SetCameraPosition()
    {
        if (canMoveCamera)
            transform.position = puzzleViews[cameraIndex].transform.position;
    }

    // Checks which dialogue view the camera should move to - for character/artifact dialogue
    private void DialogueViewsCheck()
    {
        if (!hasCheckedDialogueViews)
        { 
            if (playerScript.playerDirection == left)
                currentView = dialogueCameraViewsScript.dialogueCameraViews[0];

            if (playerScript.playerDirection == right)
                currentView = dialogueCameraViewsScript.dialogueCameraViews[1];

            if (playerScript.playerDirection == up)
                currentView = dialogueCameraViewsScript.dialogueCameraViews[2];

            if (playerScript.playerDirection == down)
                currentView = dialogueCameraViewsScript.dialogueCameraViews[3];

            hasCheckedDialogueViews = true;
        }
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

    // Plays a WindGushSFX and changes the ambient looping SFX
    private void PlayWindGushSFX()
    {
        theALSM.ChangeAmbientLoopingSFX();
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
        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        gameManagerScript = FindObjectOfType<GameManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();

        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<GeneratorScript>();
    }

    // Enables debugging for the puzzle views - For Debugging Purposes ONLY
    private void PuzzleViewsDebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                PlayWindGushSFX();
                audioManagerScript.PlayWindGushSFX();
                currentView = puzzleViews[cameraIndex++];

                // Sets the camera back to the first puzzle view
                if (cameraIndex > puzzleViews.Length - 1)
                    cameraIndex = 0;
            }

            if (cameraSpeed != gameManagerScript.cameraSpeed)
                cameraSpeed = gameManagerScript.cameraSpeed;
        }
    }

}
