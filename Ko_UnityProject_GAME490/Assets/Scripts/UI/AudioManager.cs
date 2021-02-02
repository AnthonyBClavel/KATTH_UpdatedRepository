using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer theMixer;

    public Slider masterSlider, musicSlider, sfxSlider;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("MasterVol"))                                                    //if the music volume is stored...
        {
            //theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", masterSlider.value);         //set float value
        }

        if (PlayerPrefs.HasKey("MusicVol"))                                                     //if the music volume is stored...
        {
            //theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", musicSlider.value);            //set float value
        }

        if (PlayerPrefs.HasKey("SFXVol"))                                                       //if the sfx volume is stored...
        {
            //theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", sfxSlider.value);                  //set float value
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
