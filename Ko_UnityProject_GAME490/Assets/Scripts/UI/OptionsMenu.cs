using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Xml.Serialization;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public Toggle fullScreenTog, vsyncTog;

    public ResItem[] resolutions;

    public int selectedResolution;

    public TextMeshProUGUI resolutionLabel;

    public AudioMixer theMixer;

    public Slider masterSlider, musicSlider, sfxSlider;

    public AudioSource sfxLoop;

    // Start is called before the first frame update
    void Start()
    {
        fullScreenTog.isOn = Screen.fullScreen;

        if(QualitySettings.vSyncCount == 0)
        {
            vsyncTog.isOn = false;
        }
        else
        {
            vsyncTog.isOn = true;
        }

        //Search for resolution in the list
        bool foundRes = false;
        for(int i = 0; i < resolutions.Length; i++)
        {
            if(Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                foundRes = true;
                selectedResolution = i;
                UpdateResLabel();
            }
        }

        if(!foundRes)
            resolutionLabel.text = Screen.width.ToString() + " x " + Screen.height.ToString();

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            //theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol");
        }
            
        if (PlayerPrefs.HasKey("MusicVol"))
        {
            //theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol");
        }          

        if (PlayerPrefs.HasKey("SFXVol"))
        {
            //theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol");
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResLeft()
    {
        selectedResolution--;
        if(selectedResolution < 0)
            selectedResolution = 0;

        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Length - 1) 
            selectedResolution = resolutions.Length - 1;

        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
    }

    public void ApplyGraphics()
    {
        if (vsyncTog.isOn)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }

        //Set resolution
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullScreenTog.isOn);

        Debug.Log("Graphic Settings Applied");
    }

    public void SetMasterVol()
    {
        float mastersliderValue = masterSlider.value;

        theMixer.SetFloat("MasterVol", Mathf.Log10(mastersliderValue) * 22);

        PlayerPrefs.SetFloat("MasterVol", mastersliderValue);

    }

    public void SetMusicVol()
    {
        float musicVolumeSlider = musicSlider.value;

        theMixer.SetFloat("MusicVol", Mathf.Log10(musicVolumeSlider) * 22);

        PlayerPrefs.SetFloat("MusicVol", musicVolumeSlider);
    }

    public void SetSFXVol()
    {
        float sfxVolumeSlider = sfxSlider.value;

        theMixer.SetFloat("SFXVol", Mathf.Log10(sfxVolumeSlider) * 22);

        PlayerPrefs.SetFloat("SFXVol", sfxVolumeSlider);
    }

    public void PlaySFXLoop()
    {
        sfxLoop.Play();
    }

    public void StopSFXLoop()
    {
        sfxLoop.Stop();
    }

}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}