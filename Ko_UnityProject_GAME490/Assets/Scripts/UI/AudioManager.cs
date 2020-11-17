using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer theMixer;                                                     //variable for the AudioMixer

    public Slider masterSlider, musicSlider, sfxSlider;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("MusicVol"))                                         //if the music volume is stored...
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);          //set float value
        }

        if (PlayerPrefs.HasKey("MusicVol"))                                         //if the music volume is stored...
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);            //set float value
        }

        if (PlayerPrefs.HasKey("SFXVol"))                                           //if the sfx volume is stored...
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);                //set float value
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
