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

    [Header("Audio Loops")]
    public AudioSource dialogueMusic;
    public AudioSource loopingFireSFX;
    public AudioSource charNoiseSFX;

    [Header("SFX")]
    public AudioClip torchFireIgniteSFX;
    public AudioClip torchFireExtinguishSFX;
    public AudioClip freezingSFX;
    public AudioClip chimeSFX; // For popping up tutorial button
    public AudioClip chime02SFX; // For skipping tutorial
    public AudioClip popUpSFX; 
    public AudioClip buttonClickSFX;  
    public AudioClip buttonSelectSFX;
    public AudioClip deathScreenSFX;
    public AudioClip dialoguePopUpSFX;
    public AudioClip dialogueArrowSFX;
    public AudioClip openingChestSFX;
    public AudioClip closingChestSFX;

    private AudioSource swooshAS;
    private AudioSource torchFireIgniteAS;
    private AudioSource torchFireExtinguishAS;
    private AudioSource freezingAS;
    private AudioSource chimeAS;
    private AudioSource chime02AS;
    private AudioSource popUpAS;
    private AudioSource buttonClickAS;
    private AudioSource buttonSelectAS;
    private AudioSource winsGushAS;
    private AudioSource deathScreenAS;
    private AudioSource dialoguePopUpAS;
    private AudioSource dialogueArrowAS;
    private AudioSource openingChestAS;
    private AudioSource closingChestAS;

    [Header("SFX Arrays")]
    public AudioClip[] swooshClips;
    public AudioClip[] windGushClips;
    public AudioClip[] dialogueMusicTracks;

    private AudioClip lastWindGushClip;
    private AudioClip lastSwooshClip;
    private AudioClip lastDialogueMusicTrack;

    private OptionsMenu optionsMenuScript;

    void Awake()
    {
        SetScripts();
        SetElements();
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

    // Sets the scrpts to use
    private void SetScripts()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            PauseMenu pauseMenuScript = FindObjectOfType<PauseMenu>();

            // Sets the game objects by looking at names of children
            for (int i = 0; i < pauseMenuScript.transform.childCount; i++)
            {
                GameObject child = pauseMenuScript.transform.GetChild(i).gameObject;

                if (child.name == "OptionsMenuHolder")
                {
                    GameObject optionsMenuHolder = child;

                    for (int j = 0; j < optionsMenuHolder.transform.childCount; j++)
                    {
                        GameObject child02 = optionsMenuHolder.transform.GetChild(j).gameObject;

                        if (child02.name == "OptionsMenu")
                            optionsMenuScript = child02.GetComponent<OptionsMenu>();
                    }
                }
            }
        }
        else
            optionsMenuScript = FindObjectOfType<MainMenu>().optionsScreen.GetComponent<OptionsMenu>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            AudioSource childAudioSource = transform.GetChild(i).GetComponent<AudioSource>();

            if (childAudioSource.name == "SwooshSFX")
                swooshAS = childAudioSource;
            if (childAudioSource.name == "TorchFireIgniteSFX")
                torchFireIgniteAS = childAudioSource;
            if (childAudioSource.name == "TorchFireExtinguishSFX")
                torchFireExtinguishAS = childAudioSource;
            if (childAudioSource.name == "FreezingSFX")
                freezingAS = childAudioSource;
            if (childAudioSource.name == "ChimeSFX")
                chimeAS = childAudioSource;
            if (childAudioSource.name == "Chime02SFX")
                chime02AS = childAudioSource;
            if (childAudioSource.name == "PopUpSFX")
                popUpAS = childAudioSource;
            if (childAudioSource.name == "ButtonClickSFX")
                buttonClickAS = childAudioSource;
            if (childAudioSource.name == "ButtonSelectSFX")
                buttonSelectAS = childAudioSource;
            if (childAudioSource.name == "WindGushSFX")
                winsGushAS = childAudioSource;
            if (childAudioSource.name == "DeathScreenSFX")
                deathScreenAS = childAudioSource;
            if (childAudioSource.name == "DialoguePopUpSFX")
                dialoguePopUpAS = childAudioSource;
            if (childAudioSource.name == "DialogueArrowSFX")
                dialogueArrowAS = childAudioSource;
            if (childAudioSource.name == "DialogueArrowSFX")
                dialogueArrowAS = childAudioSource;
            if (childAudioSource.name == "OpeningChestSFX")
                openingChestAS = childAudioSource;
            if (childAudioSource.name == "ClosingChestSFX")
                closingChestAS = childAudioSource;
        }
    }

    public void PlayTorchFireIgniteSFX()
    {
        //torchFireIgniteAS.volume = 0.8f;
        //torchFireIgniteAS.pitch = 1f;
        torchFireIgniteAS.PlayOneShot(torchFireIgniteSFX);
    }

    public void PlayTorchFireExtinguishSFX()
    {
        //torchFireExtinguishAS.volume = 0.3f;
        //torchFireExtinguishAS.pitch = 1f;
        torchFireExtinguishAS.PlayOneShot(torchFireExtinguishSFX);
    }

    public void PlayFreezeingSFX()
    {
        //freezingAS.volume = 0.3f;
        //freezingAS.pitch = 1f;
        freezingAS.PlayOneShot(freezingSFX);
    }

    public void PlayChimeSFX()
    {
        //chimeAS.volume = 0.2f;
        //chimeAS.pitch = 2.8f;
        chimeAS.PlayOneShot(chimeSFX);
    }

    public void PlayChime02SFX()
    {
        //chime02AS.volume = 0.2f;
        //chime02AS.pitch = 3.0f;
        chime02AS.PlayOneShot(chime02SFX);
    }

    public void PlayPopUpSFX()
    {
        //popUpAS.volume = 0.3f;
        //popUpAS.pitch = 1f;
        popUpAS.PlayOneShot(popUpSFX);
    }

    public void PlayButtonClickSFX()
    {
        //buttonClickAS.volume = 1f;
        //buttonClickAS.pitch = 1f;
        buttonClickAS.PlayOneShot(buttonClickSFX);
    }
    public void PlayButtonSelectSFX()
    {
        //buttonSelectAS.volume = 1f;
        //buttonSelectAS.pitch = 1f;
        buttonSelectAS.PlayOneShot(buttonSelectSFX);
    }

    public void PlayDeathScreenSFX()
    {
        //buttonSelectAS.volume = 0.5f;
        //buttonSelectAS.pitch = 1f;
        deathScreenAS.PlayOneShot(deathScreenSFX);
    }

    public void PlayDialoguePopUpSFX01()
    {
        // For the player's dialogue bubbles
        dialoguePopUpAS.volume = 0.24f;
        dialoguePopUpAS.pitch = 0.5f;
        dialoguePopUpAS.PlayOneShot(dialoguePopUpSFX);
    }

    public void PlayDialoguePopUpSFX02()
    {
        // For the NPC's dialogue bubbles
        dialoguePopUpAS.volume = 0.24f;
        dialoguePopUpAS.pitch = 0.6f;
        dialoguePopUpAS.PlayOneShot(dialoguePopUpSFX);
    }

    public void PlayDialogueArrowSFX()
    {
        //dialogueArrowAS.volume = 1f;
        //dialogueArrowAS.pitch = 3f;
        dialogueArrowAS.PlayOneShot(dialogueArrowSFX);
    }

    public void PlayOpeningChestSFX()
    {
        //openingChestAS.volume = 0.65f;
        //openingChestAS.pitch = 1f;
        openingChestAS.PlayOneShot(openingChestSFX);
    }

    public void PlayClosingChestSFX()
    {
        //closingChestAS.volume = 0.65f;
        //closingChestAS.pitch = 1f;
        closingChestAS.PlayOneShot(closingChestSFX);
    }

    // Plays a new SwooshSFX - different from the one previously played
    public void PlaySwooshSFX()
    {
        //swooshAS.volume = 0.36f;
        //swooshAS.pitch = 1f;

        int attempts = 3;
        AudioClip newSwooshClip = swooshClips[Random.Range(0, swooshClips.Length)];

        while (newSwooshClip == lastSwooshClip && attempts > 0)
        {
            newSwooshClip = swooshClips[Random.Range(0, swooshClips.Length)];
            attempts--;
        }

        lastSwooshClip = newSwooshClip;
        swooshAS.PlayOneShot(newSwooshClip);
    }

    // Plays a new WindGushSFX - different from the one previously played
    public void PlayWindGushSFX()
    {
        //winsGushAS.volume = 0.85f;
        //winsGushAS.pitch = 0.85f;

        int attempts = 3;
        AudioClip newWindGushClip = windGushClips[Random.Range(0, windGushClips.Length)];

        while (newWindGushClip == lastWindGushClip && attempts > 0)
        {
            newWindGushClip = windGushClips[Random.Range(0, windGushClips.Length)];
            attempts--;
        }

        lastWindGushClip = newWindGushClip;
        winsGushAS.PlayOneShot(newWindGushClip);
    }

    // Selects and plays a new track for the dialogue music - different from the one previously played
    public void ChangeDialogueMusic()
    {
        //dialogueMusic.volume = 0.4f;
        //dialogueMusic.pitch = 1f;

        int attempts = 3;
        AudioClip newDialogueMusicTrack = dialogueMusicTracks[UnityEngine.Random.Range(0, dialogueMusicTracks.Length)];

        while (newDialogueMusicTrack == lastDialogueMusicTrack && attempts > 0)
        {
            newDialogueMusicTrack = dialogueMusicTracks[UnityEngine.Random.Range(0, dialogueMusicTracks.Length)];
            attempts--;
        }

        lastDialogueMusicTrack = newDialogueMusicTrack;

        //dialogueMusic.Stop();
        dialogueMusic.clip = newDialogueMusicTrack;
        dialogueMusic.Play();
    }




}
