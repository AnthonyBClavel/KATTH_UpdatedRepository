using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]                                                        
    public AudioClip[] clips;

    [SerializeField]                                                            //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    public AudioClip[] loopingClips;                                       

    private AudioSource audioSource;          

    public static CameraController instance;               

    private AmbientLoopingSFXManager theALSM; 

    public Transform[] levelViews;                            
                                                                                 
    public float transitonSpeed; // Camera transition speed

    Transform currentView; // The variable that is used to determine which view the camera is currenlty at

    int currentIndex = 0; 

    // Start is called before the first frame update
    void Start()
    {       
        currentView = levelViews[currentIndex++]; 

        instance = this;

        audioSource = GetComponent<AudioSource>();                        

        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();
    }

    void Update()
    {
        // For Debugging purposes
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

    // Plays the wind gush audio clip
    public void WindGush()                                                                                            
    {
        AudioClip clips = GetRandomClip();                                                                
        audioSource.PlayOneShot(clips);                                                                          
    }

    private AudioClip GetRandomClip()                                                                                
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];                                                      

    }

}
