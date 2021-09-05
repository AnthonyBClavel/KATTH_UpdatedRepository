using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public float transitonSpeed;
    public int currentIndex = 0;

    public bool canMoveCamera = true;
    public bool canMoveToDialogueViews = false;
    public bool canMoveToArtifactView = false;
    public bool hasCheckedDialogueViews = false;
    private bool hasPaused = false;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);
                                                     
    public AudioClip[] windGushClips;
    private Transform[] puzzleViews;

    Transform currentView;
    private AudioSource audioSource;
    private AudioClip lastWindGushClip;

    private AmbientLoopingSFXManager theALSM;
    private GameHUD gameHUDScript;
    private GeneratorScript generatorScript;
    private TileMovementController playerScript;
    private DialogueCameraViews dialogueCameraViewsScript;
    private SaveManagerScript saveManagerScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        SetScripts();
        SetPuzzleViews();
    }

    // Start is called before the first frame update
    void Start()
    {       
        audioSource = GetComponent<AudioSource>();
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
                currentView = puzzleViews[currentIndex];

                if (transform.position != currentView.position)
                    transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed);

            }
            if (canMoveToDialogueViews && !canMoveCamera)
            {
                DialogueViewsCheck();

                if (transform.position != currentView.position)
                    transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed);
            }
        }
    }

    // Sets the camera's position to a puzzle view - determined by currentIndex
    public void SetCameraPosition()
    {
        if (canMoveCamera)
            transform.position = puzzleViews[currentIndex].transform.position;
    }

    // Moves the camera to the next puzzle view
    public void NextPuzzleView()
    {
        if (currentIndex < puzzleViews.Length - 1)
        {
            //Debug.Log("Next Puzzle View Activated");
            currentView = puzzleViews[currentIndex++];
            SetPuzzleNumber();
            PlayWindGushSFX();

            // Turns off the generator's light (if applicable)
            if (SceneManager.GetActiveScene().name == "FifthMap")
                generatorScript.TurnOffEmisionAndVolume();
        }         
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
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/8");
        else
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/10");

        gameHUDScript.PlayPuzzleNotificationCheck();
    }

    // Plays a WindGushSFX and changes the ambient looping SFX
    public void PlayWindGushSFX()
    {
        PlayNewWindGushSFX();      
        theALSM.ChangeAmbientLoopingSFX();
    }

    // Plays a new WindGushSFX - different from the one previously played
    private void PlayNewWindGushSFX()
    {
        int attempts = 3;
        AudioClip newWindGushClip = windGushClips[Random.Range(0, windGushClips.Length)];

        while (newWindGushClip == lastWindGushClip && attempts > 0)
        {
            newWindGushClip = windGushClips[Random.Range(0, windGushClips.Length)];
            attempts--;
        }

        lastWindGushClip = newWindGushClip;
        audioSource.PlayOneShot(newWindGushClip);
    }

    // Pauses the WindGushSFX if you pause the game on a bridge - OPTIONAL (not currenlty used)
    private void CheckIfPaused()
    {
        PauseMenu pauseMenuScript = FindObjectOfType<PauseMenu>();

        if (pauseMenuScript.isPaused && !hasPaused)
        {
            Debug.Log("Wind Gush SFX has been paused");
            audioSource.Pause();
            hasPaused = true;
        }
        else if (pauseMenuScript.isPaused && hasPaused)
        {
            Debug.Log("Wind Gush SFX has resumed");
            audioSource.UnPause();
            hasPaused = false;
        }
    }

    // Sets the puzzle views array
    private void SetPuzzleViews()
    {
        puzzleViews = gameManagerScript.puzzleViews;
    }

    // Determines which scripts to find
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        gameManagerScript = FindObjectOfType<GameManager>();

        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<GeneratorScript>();
    }

    // Enables debugging for the puzzle views - For Debugging Purposes ONLY
    private void PuzzleViewsDebuggingCheck()
    {
        if (saveManagerScript.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                PlayWindGushSFX();
                currentView = puzzleViews[currentIndex++];

                // Sets the camera back to the first puzzle view
                if (currentIndex > puzzleViews.Length - 1)
                    currentIndex = 0;
            }
        }
    }

}
