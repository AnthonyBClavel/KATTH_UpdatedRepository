using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [SerializeField]                                                        
    public AudioClip[] clips;

    [SerializeField]
    public AudioClip[] loopingClips;

    [SerializeField]
    public Transform[] levelViews;

    public static CameraController instance;
    public float transitonSpeed;
    public int currentIndex = 0;
    public bool canMoveCamera = true;

    Transform currentView;
    private AudioSource audioSource;
    private AmbientLoopingSFXManager theALSM;
    private SaveManagerScript saveManagerScript;
    private GameHUD gameHUDScript;
    private GeneratorScript generatorScript;

    private bool hasPaused;

    void Awake()
    {
        saveManagerScript = FindObjectOfType<SaveManagerScript>(); //
        saveManagerScript.LoadCameraPosition(); //

        gameHUDScript = FindObjectOfType<GameHUD>();
        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();

        CheckForGenScript();
    }

    // Start is called before the first frame update
    void Start()
    {
        hasPaused = false;   
        instance = this;
        audioSource = GetComponent<AudioSource>();

        //gameObject.transform.position = levelViews[currentIndex].transform.position;
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
            if(loopingClips != null)
            {
                theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);       
            }

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
        if (canMoveCamera)
        {
            currentView = levelViews[currentIndex];
            transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed);
        }          
    }

    public void SetCameraPosition()
    {
        if (canMoveCamera)
            gameObject.transform.position = levelViews[currentIndex].transform.position;
    }

    
    // Shifts the camera to the next puzzle view
    public void NextPuzzleView(Transform newView)                                                                              
    {
        //Debug.Log("Switch Puzzle View");
        currentView = newView;
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
    
    // We'll use this function below in the future when we want to travel between puzzles
    public void PreviousPuzzleView02()
    {
        //Debug.Log("Switch Puzzle View");
        currentView = levelViews[currentIndex--];
    }

    // Plays a random wind gush sfx from an audio clip array
    public void WindGush()                                                                                            
    {
        // Plays an audio clip whose index is not equal to the one playing right now
        if (loopingClips != null)
        {           
            theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);
        }

        AudioClip clips = GetRandomClip();
        //audioSource.clip = clips;
        audioSource.PlayOneShot(clips);

    }

    // Gets a random audio clip from its respective array
    private AudioClip GetRandomClip()                                                                                
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    // Updates the puzzle number in the GameHUD via Camera Script
    private void SetPuzzleNumber()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
            gameHUDScript.puzzleNumber.text = "Puzzle: " + (currentIndex + 1) + "/10";
        else
            gameHUDScript.puzzleNumber.text = "Puzzle: " + (currentIndex + 1) + "/7";
    }

    // For pausing the WindGushSFX when you pause the game on the bridge
    private void CheckIfPaused()
    {
        if(FindObjectOfType<PauseMenu>().isPaused && !hasPaused)
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
