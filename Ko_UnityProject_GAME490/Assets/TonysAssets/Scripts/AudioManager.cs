using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer theMixer;                                                     //variable for the AudioMixer

    // Start is called before the first frame update
    void Start()
    {

        if (PlayerPrefs.HasKey("MasterVol"))                                        //if the master volume is stored...
        {
            theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));      //set it to the AudioMixer
        }

        if (PlayerPrefs.HasKey("MusicVol"))                                         //if the music volume is stored...
        {
            theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));        //set it to the AudioMixer
        }

        if (PlayerPrefs.HasKey("SFXVol"))                                           //if the sfx volume is stored...
        {
            theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));            //set it to the AudioMixer
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
