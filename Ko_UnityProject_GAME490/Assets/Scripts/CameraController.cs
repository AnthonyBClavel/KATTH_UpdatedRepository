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

    Transform currentView;
    private AudioSource audioSource;
    private AmbientLoopingSFXManager theALSM;
    private SaveManagerScript saveManagerScript;

    void Awake()
    {
        //saveManagerScript = FindObjectOfType<SaveManagerScript>();
        //saveManagerScript.LoadCameraPosition();       
    }

    // Start is called before the first frame update
    void Start()
    {       
        //currentView = levelViews[currentIndex];
        gameObject.transform.position = levelViews[currentIndex].transform.position;

        instance = this;

        audioSource = GetComponent<AudioSource>();                        

        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
    }

    void Update()
    {
        currentView = levelViews[currentIndex];

        /*** For Debugging purposes ***/
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Switch Puzzle View");                  
            if (currentIndex >= levelViews.Length)           
            {
                Debug.Log("Reset to Frist Puzzle View");
                currentIndex = 0;                    
            }
            
            if(loopingClips != null)
            {
                theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);       
            }

            WindGush();                                                                                               
            currentView = levelViews[currentIndex++];                                                               
        }
        /*** End Debugging ***/
    }

    /**
    * Called once per frame
    * Moves the camera's current position to the new position via linear interpolation
    **/
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed); 
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

        if (SceneManager.GetActiveScene().name == "FifthMap" || SceneManager.GetActiveScene().name == "FourthMap")
        {
            FindObjectOfType<GeneratorScript>().resetEmissiveTextures();
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
        audioSource.PlayOneShot(clips);                                                                          
    }

    // Gets a random audio clip from its respective array
    private AudioClip GetRandomClip()                                                                                
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }


}
