using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class AmbientLoopingSFXManager : MonoBehaviour
{
    private AudioSource AmbientLoopingSFX;                                                                               

    // Start is called before the first frame update
    void Start()
    {
        AmbientLoopingSFX = GetComponent<AudioSource>();                   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Plays a new audio clip with a different name from the one currenlty playing
    public void ChangeAmbientLoopingSFX(AudioClip newAudioClip)            
    {
        if (AmbientLoopingSFX.clip.name == newAudioClip.name)              
            return;                                                               

        AmbientLoopingSFX.Stop();                                          
        AmbientLoopingSFX.clip = newAudioClip;                             
        AmbientLoopingSFX.Play();                                          
    }

}
