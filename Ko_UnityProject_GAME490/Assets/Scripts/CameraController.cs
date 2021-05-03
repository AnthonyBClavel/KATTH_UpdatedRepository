using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    //public static CameraController instance;

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
    private SaveManagerScript saveManagerScript;
    private GameHUD gameHUDScript;
    private GeneratorScript generatorScript;
    private TileMovementController playerScript;
    private DialogueCameraViews dialogueCameraViewsScript;

    void Awake()
    {
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        saveManagerScript.LoadCameraPosition();

        gameHUDScript = FindObjectOfType<GameHUD>();
        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();

        CheckForGenScript();
    }

    // Start is called before the first frame update
    void Start()
    {
        //instance = this;
        //gameObject.transform.position = levelViews[currentIndex].transform.position;
         
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

    public void SetCameraPosition()
    {
        if (canMoveCamera)
            gameObject.transform.position = levelViews[currentIndex].transform.position;
    }

    public void NextPuzzleView02()
    {
        //Debug.Log("Switch Puzzle View");
        currentView = levelViews[currentIndex++];
        SetPuzzleNumber();

        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            generatorScript.TurnOffEmisionAndVolume();
        }
       
        if (currentIndex >= levelViews.Length)
        {
            Debug.Log("Reset to Frist Puzzle View");
            currentIndex = 0;
        }
    }

    // Shifts the camera to the next puzzle view
    public void NextPuzzleView(Transform newView)
    {
        //Debug.Log("Switch Puzzle View");
        currentView = newView;
    }

    // We'll use this function below in the future when we want to travel between puzzles
    public void PreviousPuzzleView02()
    {
        //Debug.Log("Switch To Previous Puzzle View");
        currentView = levelViews[currentIndex--];
    }

    // Plays a random wind gush sfx from an audio clip array
    public void WindGush()                                                                                            
    {
        theALSM.ChangeAmbientLoopingSFX();
        PlayWindGushSFX();
    }

    // Checks to see what dialogue view the camera should move to
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

    // Plays a new wind gush sfx that different from the one that was previously played
    private void PlayWindGushSFX()
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

    // Updates the puzzle number in the GameHUD via Camera Script
    private void SetPuzzleNumber()
    {
        /*if (SceneManager.GetActiveScene().name != "TutorialMap")
            gameHUDScript.puzzleNumber.text = "Puzzle: " + (currentIndex + 1) + "/10";
        else
            gameHUDScript.puzzleNumber.text = "Puzzle: " + (currentIndex + 1) + "/7";*/

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/8");
        else
            gameHUDScript.UpdatePuzzleBubbleText((currentIndex + 1) + "/10");

        gameHUDScript.PlayPuzzleNotificationCheck();
    }

    // For pausing the WindGushSFX when you pause the game on the bridge
    private void CheckIfPaused()
    {
        if (FindObjectOfType<PauseMenu>().isPaused && !hasPaused)
        {
            Debug.Log("Wind Gush SFX has been paused");
            audioSource.Pause();
            hasPaused = true;
        }
        else if (!FindObjectOfType<PauseMenu>().isPaused && hasPaused)
        {
            Debug.Log("Wind Gush SFX has resumed");
            audioSource.UnPause();
            hasPaused = false;
        }
    }

    // Checks to see if it can find the generator script
    private void CheckForGenScript()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
            generatorScript = FindObjectOfType<GeneratorScript>();
    }

}
