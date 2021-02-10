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

    public Transform[] levelViews;

    public static CameraController instance;
    public float transitonSpeed; // The camera's transition speed
    public int currentIndex = 0;

    Transform currentView; // The variable that is used to determine which view the camera is currenlty at
    private AudioSource audioSource;
    private AmbientLoopingSFXManager theALSM;

    // Start is called before the first frame update
    void Start()
    {       
        currentView = levelViews[currentIndex]; 

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
    * Move the camera's current position to the new position via linear interpolation
    **/
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed); 
    }

    /**
     * Function for shifting the camera to the next puzzle view
     **/
    public void NextPuzzleView(Transform newView)                                                                              
    {
        //Debug.Log("Switch Puzzle View");
        currentView = newView;
    }

    public void NextPuzzleView02()
    {
        //Debug.Log("Switch Puzzle View");
        currentView = levelViews[currentIndex++];

        if(SceneManager.GetActiveScene().name == "FithMap")
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

    // Plays the wind gush audio clip
    public void WindGush()                                                                                            
    {
        if (loopingClips != null)
        {
            // Play a new looping ambient sfx whose index is not equal to the one playing right now
            theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);
        }

        AudioClip clips = GetRandomClip();                                                                
        audioSource.PlayOneShot(clips);                                                                          
    }

    private AudioClip GetRandomClip()                                                                                
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }


}
