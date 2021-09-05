using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public AudioMixer theMixer;

    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    [Header("SFX")]
    public AudioSource loopingFireSFX;
    public AudioSource charNoiseSFX;
    public AudioSource swooshSFX;
    public AudioSource torchFireIgniteSFX;
    public AudioSource torchFireExtinguishSFX;
    public AudioSource freezingSFX;

    [Header("SFX Arrays")]
    public AudioClip[] swooshClips;

    private OptionsMenu optionsMenuScript;

    void Awake()
    {
        SetScripts();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetVolumeSliders();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Sets the scrpts to use
    private void SetScripts()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
            optionsMenuScript = FindObjectOfType<PauseMenu>().optionsScreen.GetComponent<OptionsMenu>();
        else
            optionsMenuScript = FindObjectOfType<MainMenu>().optionsScreen.GetComponent<OptionsMenu>();
    }

    // Sets the volume sliders in the options menu to the last saved value - MUST be called in start
    private void SetVolumeSliders()
    {
        masterSlider = optionsMenuScript.masterSlider;
        musicSlider = optionsMenuScript.musicSlider;
        sfxSlider = optionsMenuScript.sfxSlider;

        if (PlayerPrefs.HasKey("MasterVol"))
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", masterSlider.value);

        if (PlayerPrefs.HasKey("MusicVol"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", musicSlider.value);

        if (PlayerPrefs.HasKey("SFXVol"))
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", sfxSlider.value);
    }


}
