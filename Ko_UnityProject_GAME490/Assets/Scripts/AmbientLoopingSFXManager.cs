using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class AmbientLoopingSFXManager : MonoBehaviour
{
    private AudioSource AmbientLoopingSFX;                                 //variable for the AudioSource component                                              

    // Start is called before the first frame update
    void Start()
    {
        AmbientLoopingSFX = GetComponent<AudioSource>();                   //sets the audio source variable to the object's audio source component (sets instance)
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeAmbientLoopingSFX(AudioClip newAudioClip)            //the function that changes the ambient looping sfx
    {
        if (AmbientLoopingSFX.clip.name == newAudioClip.name)              //if the audio clip is the same as the one currently being played...
        {
            return;                                                        //return the function (prevents the clip from restarting)
        }
        AmbientLoopingSFX.Stop();                                          //stop playing the clip in the AudioSource component
        AmbientLoopingSFX.clip = newAudioClip;                             //set the clip equal to the 
        AmbientLoopingSFX.Play();                                          //play the clip in the AudioSource component
    }

}
