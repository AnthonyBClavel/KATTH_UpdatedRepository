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

    [SerializeField]                                                        
    public AudioClip[] clips;

    [SerializeField]
    public Transform[] levelViews;

    Transform currentView;
    private AudioSource audioSource;
    private AudioClip lastWindGushClip;

    private AmbientLoopingSFXManager theALSM;
    private GameHUD gameHUDScript;
    private GeneratorScript generatorScript;
    private TileMovementController playerScript;
    private DialogueCameraViews dialogueCameraViewsScript;

    void Awake()
    {
        SetScripts();
    }

    // Start is called before the first frame update
    void Start()
    {       
        audioSource = GetComponent<AudioSource>();
        SetCameraPosition();
        SetPuzzleNumber();
    }

    void Update()
    {
        //CheckIfPaused();

        /*if (canMoveCamera)
            currentView = levelViews[currentIndex];*/

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Switch Puzzle View");                  
            WindGush();                                                                                               
            currentView = levelViews[currentIndex++];                                                               
        }
        if (currentIndex > levelViews.Length - 1)
        {
            Debug.Log("Reset to Frist Puzzle View");
            currentIndex = 0;
        }
        /*** End Debugging ***/
    }

    /**
    * Called once per frame
    * Moves the camera's current position to the new position via linear interpolation
    **/
    void LateUpdate()
    {
        if (!canMoveToArtifactView)
        {
            if (canMoveCamera && !canMoveToDialogueViews)
            {
                currentView = levelViews[currentIndex];

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

    // Sets the camera's position to the position of an object in the levelViews array
    public void SetCameraPosition()
    {
        if (canMoveCamera)
            gameObject.transform.position = levelViews[currentIndex].transform.position;
    }

    // Moves the camera to the next puzzle view and updates the puzzle bubble notification
    public void NextPuzzleView02()
    {
        //Debug.Log("Next Puzzle View Activated");
        currentView = levelViews[currentIndex++];
        SetPuzzleNumber();

        // Turns of the generator's light (if applicable)
        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            generatorScript.TurnOffEmisionAndVolume();
        }

        // Resets the camera to the first puzzle view (if applicable)
        if (currentIndex >= levelViews.Length)
        {
            Debug.Log("Reset to First Puzzle View");
            currentIndex = 0;
        }
    }

    // Checks to see which dialogue view the camera should move to
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

    // Updates the puzzle number in the puzzle bubble notification
    private void SetPuzzleNumber()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/8");
        else
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/10");

        gameHUDScript.PlayPuzzleNotificationCheck();
    }

    // Plays a random WindGushSFX and changes the looping ambient sfx
    public void PlayWindGushSFX()
    {
        PlayNewWindGushSFX();      
        theALSM.ChangeAmbientLoopingSFX();
    }

    // Plays a new WindGushSFX that's different from the one that was previously played
    private void PlayNewWindGushSFX()
    {
        int attempts = 3;
        AudioClip newWindGushClip = clips[Random.Range(0, clips.Length)];

        while (newWindGushClip == lastWindGushClip && attempts > 0)
        {
            newWindGushClip = clips[Random.Range(0, clips.Length)];
            attempts--;
        }

        lastWindGushClip = newWindGushClip;
        audioSource.PlayOneShot(newWindGushClip);
    }

    // Pauses the WindGushSFX when you pause the game on the bridge - Optional
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

    // Checks which scripts to set
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        
        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<GeneratorScript>();
    }

}
