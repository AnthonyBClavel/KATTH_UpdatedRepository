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
        if (PlayerPrefs.HasKey("MasterVol"))                                                    
        {
            //theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", masterSlider.value);         
        }

        if (PlayerPrefs.HasKey("MusicVol"))                                                     
        {
            //theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", musicSlider.value);            
        }

        if (PlayerPrefs.HasKey("SFXVol"))                                                       
        {
            //theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", sfxSlider.value);                  
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
