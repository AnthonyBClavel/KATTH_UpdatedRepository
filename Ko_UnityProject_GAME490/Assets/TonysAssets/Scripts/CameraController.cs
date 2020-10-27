using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]                                                            //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    public AudioClip[] clips;                                                   //creates an array of audio clips 

    [SerializeField]                                                            //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    public AudioClip[] loopingClips;                                            //creates an array of audio clips 

    private AudioSource audioSource;                                            //variable for the AudioSource component 

    public static CameraController instance;                                    //create an instance variable for this script

    private AmbientLoopingSFXManager theALSM;                                   //variable for the specified script

    public Transform[] levelViews;                                              //creates a tranform array
                                                                                 
    public float transitonSpeed;                                                //the speed at which the camera will transition, value set in Unity inspector

    Transform currentView;                                                      //the variable that is used to determine which view the camera is currenlty at

    int currentIndex = 0;                                                       //the integer we'll use/manipulate to idicate the indexes within the array

    // Start is called before the first frame update
    void Start()
    {       
        currentView = levelViews[currentIndex++];                               //the initial camera view when the scene loads

        instance = this;                                                        //sets the instance variable to this script

        audioSource = GetComponent<AudioSource>();                              //sets the audio source variable to the object's audio source component (sets instance)

        theALSM = FindObjectOfType<AmbientLoopingSFXManager>();                 //finds the object attached with the specified script and sets it to the variable
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))                                    //if the space bar is pressed... (this is just for debugging purposes)
        {
            Debug.Log("Switch Puzzle View");                                    //sends a debug message to the console (just for debugging purposes)
            if (currentIndex >= levelViews.Length)                              //if the current index exceeds the length/bounds of the array 
            {
                Debug.Log("Reset to Frist Puzzle View");                        //sends a debug message to the console (just for debugging purposes)
                currentIndex = 0;                                               //set the camera view back to the first puzzle view
            }
            
            if(loopingClips != null)
            {
                theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);        //calls the function from the specified/external script and selects a random clip within the loopingClips array for it to use
            }

            WindGush();                                                                                                 //calls the function stated below (bottom of this script)
            currentView = levelViews[currentIndex++];                                                                   //add one to the current index within the levelViews array, the camera will shift to the new index's corresponding positon (set in the Unity inspector)
        }

    }


    // Update is called once per frame
    void LateUpdate()
    {
        //move the camera's current position to the new position via linear interpolation
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitonSpeed);   //moves the object to the new position based on linear interpolation
    }

    public void NextPuzzleView()                                                                                        //the function for shifting the camera to the next puzzle view
    {
        Debug.Log("Switch Puzzle View");                                                                                //sends a debug message to the console (just for debugging purposes)
        if (currentIndex >= levelViews.Length)                                                                          //if the current index exceeds the length/bounds of the array 
        {
            Debug.Log("Reset to Frist Puzzle View");                                                                    //sends a debug message to the console (just for debugging purposes)
            currentIndex = 0;                                                                                           //set the camera view back to the first puzzle view
        }

        if (loopingClips != null)                                                                                       //as long as there are clips within the array (set in Unity inspector)
        {
            theALSM.ChangeAmbientLoopingSFX(loopingClips[UnityEngine.Random.Range(0, loopingClips.Length)]);            //calls the function from the specified/external script and selects a random clip within the loopingClips array for it to use
        }

        WindGush();                                                                                                     //calls the function stated below (bottom of this script)
        currentView = levelViews [currentIndex++];                                                                      //add one to the current index within the levelViews array, the camera will shift to the new index's corresponding positon (set in the Unity inspector)
    }
    private void WindGush()                                                                                             //the function that plays the audio clip
    {
        AudioClip clips = GetRandomClip();                                                                              //calls the random audio clip function below
        audioSource.PlayOneShot(clips);                                                                                 //plays the audio clip (from start to end - without intturuption) through the object's audio source component
    }
    private AudioClip GetRandomClip()                                                                                   //the function for getting a random clip for the footstep sfx
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];                                                        //selects a random audio clip based on the size of the array (its length)

    }

}
