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

    private float fadeAudioLength;
    private float originalVolumeBGM; // BGM = background music
    private float originalVolumeDM; // DM = dialogue music
    private float originalVolumeLASFX; // LASFX = looping ambience sfx
    private bool isDialogueMusic = true;

    private IEnumerator backgroundMusicCoroutine;
    private IEnumerator dialogueMusicCoroutine;
    private IEnumerator loopingAmbienceCoroutine;

    [Header("Music")]
    private AudioSource mainMenuMusic;
    private AudioSource dialogueMusic;
    private AudioSource backgroundMusic;
    private AudioSource endCreditsMusic;

    [Header("Audio Loops")]
    public AudioSource loopingFireAS;
    public AudioSource charNoiseAS;
    private AudioSource loopingAmbienceAS;

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
    public AudioClip pushCrateSFX;
    public AudioClip cantPushCrateSFX;
    public AudioClip crateSlideSFX;
    public AudioClip crateThumpSFX;

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
    private AudioSource treeHitAS;
    private AudioSource snowTreeHitAS;
    private AudioSource crystalHitAS;
    private AudioSource metalHitAS;
    private AudioSource rockHitAS;
    private AudioSource rockBreakAS;
    private AudioSource footstepsAS;
    private AudioSource pushCrateAS;
    private AudioSource cantPushCrateAS;
    private AudioSource crateSlideAS;
    private AudioSource crateThumpAS;

    [Header("SFX Arrays")]
    public AudioClip[] swooshClips;
    public AudioClip[] windGushClips;
    public AudioClip[] rockHitClips;
    public AudioClip[] rockBreakClips;
    public AudioClip[] treeHitClips;
    public AudioClip[] snowTreeHitClips;
    public AudioClip[] crystalHitClips;
    public AudioClip[] metalHitClips;

    [Header("Footsteps SFX Arrays")]
    public AudioClip[] grassFootstepClips;
    public AudioClip[] snowFootstepClips;
    public AudioClip[] stoneFootstepClips;
    public AudioClip[] metalFootstepClips;
    public AudioClip[] woodFootstepClips;
    public AudioClip[] crateFootstepClips;

    [Header("Audio Loop Arrays")]
    public AudioClip[] loopingAmbienceClips;
    public AudioClip[] dialogueMusicTracks;
    public AudioClip[] backgroundMusicTracks;

    private AudioClip lastWindGushClip;
    private AudioClip lastSwooshClip;
    private AudioClip lastLoopingAmbienceClip;
    private AudioClip lastDialogueMusicTrack;
    private AudioClip lastTreeHitClip;
    private AudioClip lastSnowTreeHitClip;
    private AudioClip lastCrystalHitClip;
    private AudioClip lastMetalHitClip;
    private AudioClip lastRockHitClip;
    private AudioClip lastRockBreakClip;
    private AudioClip lastGrassFootstepClip;
    private AudioClip lastSnowFootstepClip;
    private AudioClip lastStoneFootstepClip;
    private AudioClip lastMetalFootstepClip;
    private AudioClip lastWoodFootstepClip;
    private AudioClip lastCrateFootstepClip;

    private OptionsMenu optionsMenuScript;
    private TileMovementController playerScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
        //SetAudioLoopsForTutorial();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetVolumeSliders();
        SetAudioLoopsActiveCheck();
    }

    // Update is called once per frame
    void Update()
    {
        DebuggingCheck();
    }

    // Checks if you're in the tutorial scene - fades in ONLY the music IF SO - ONLY gets called/checked during the tutorial scene
    /*private void SetAudioLoopsForTutorial()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            FadeInBackgroundMusic();
    }*/

    // Fades in the background music
    public void FadeInBackgroundMusic()
    {
        if (!backgroundMusic.gameObject.activeSelf)
            backgroundMusic.gameObject.SetActive(true);

        if (backgroundMusicCoroutine != null)
            StopCoroutine(backgroundMusicCoroutine);

        backgroundMusicCoroutine = LerpAudio(backgroundMusic, originalVolumeBGM, fadeAudioLength);

        backgroundMusic.volume = 0f;
        StartCoroutine(backgroundMusicCoroutine);
    }

    // Fades out the background music
    public void FadeOutBackgroundMusic()
    {
        if (backgroundMusicCoroutine != null)
            StopCoroutine(backgroundMusicCoroutine);

        backgroundMusicCoroutine = LerpAudio(backgroundMusic, 0f, fadeAudioLength);

        backgroundMusic.volume = originalVolumeBGM;
        StartCoroutine(backgroundMusicCoroutine);
    }

    // Fades in the looping ambient sfx
    public void FadeInLoopingAmbientSFX()
    {
        if (!loopingAmbienceAS.gameObject.activeSelf)
            loopingAmbienceAS.gameObject.SetActive(true);

        if (loopingAmbienceCoroutine != null)
            StopCoroutine(loopingAmbienceCoroutine);

        loopingAmbienceCoroutine = LerpAudio(loopingAmbienceAS, originalVolumeLASFX, fadeAudioLength);

        loopingAmbienceAS.volume = 0f;
        StartCoroutine(loopingAmbienceCoroutine);
    }

    // Fades out the looping ambient sfx
    public void FadeOutLoopingAmbientSFX()
    {
        if (loopingAmbienceCoroutine != null)
            StopCoroutine(loopingAmbienceCoroutine);

        loopingAmbienceCoroutine = LerpAudio(loopingAmbienceAS, 0f, fadeAudioLength);
        isDialogueMusic = true;

        loopingAmbienceAS.volume = originalVolumeLASFX;
        StartCoroutine(loopingAmbienceCoroutine);
    }

    // Fades in the dilaogue music
    public void FadeInDialogueMusic()
    {
        if (!dialogueMusic.gameObject.activeSelf)
            dialogueMusic.gameObject.SetActive(true);

        if (dialogueMusicCoroutine != null)
            StopCoroutine(dialogueMusicCoroutine);

        dialogueMusicCoroutine = LerpAudio(dialogueMusic, originalVolumeDM, fadeAudioLength);
        isDialogueMusic = true;

        dialogueMusic.volume = 0f;
        ChangeDialogueMusicTrack();
        StartCoroutine(dialogueMusicCoroutine);
    }

    // Fades out the dialogue music
    public void FadeOutDialogueMusic()
    {
        if (dialogueMusicCoroutine != null)
            StopCoroutine(dialogueMusicCoroutine);

        dialogueMusicCoroutine = LerpAudio(dialogueMusic, 0f, fadeAudioLength);
        isDialogueMusic = false;

        dialogueMusic.volume = originalVolumeDM;
        StartCoroutine(dialogueMusicCoroutine);
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

    public void PlayPushCrateSFX()
    {
        //pushCrateAS.volume = 0.72f;
        //pushCrateAS.pitch = 0.9f;
        pushCrateAS.PlayOneShot(pushCrateSFX);
    }

    public void PlayCantPushCrateSFX()
    {
        //cantPushCrateAS.volume = 0.9f;
        //cantPushCrateAS.pitch = 1.0f;
        cantPushCrateAS.PlayOneShot(cantPushCrateSFX);
    }

    public void PlayCrateSlideSFX()
    {
        //crateSlideAS.volume = 0.06f;
        //crateSlideAS.pitch = 1.0f;
        crateSlideAS.PlayOneShot(crateSlideSFX);
    }

    public void PlayCrateThumpSFX()
    {
        //crateThumpAS.volume = 1.0f;
        //crateThumpAS.pitch = 1.0f;
        crateThumpAS.PlayOneShot(crateThumpSFX);
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

    // Plays a new treeHitSFX - different from the one previously played
    public void PlayTreeHitSFX()
    {
        //treeHitAS.volume = 0.9f;
        //treeHitAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newTreeHitClip = treeHitClips[Random.Range(0, treeHitClips.Length)];

        while (newTreeHitClip == lastTreeHitClip && attempts > 0)
        {
            newTreeHitClip = treeHitClips[Random.Range(0, treeHitClips.Length)];
            attempts--;
        }

        lastTreeHitClip = newTreeHitClip;
        treeHitAS.PlayOneShot(newTreeHitClip);
    }

    // Plays a new snowTreeHitSFX - different from the one previously played
    public void PlaySnowTreeHitSFX()
    {
        //snowTreeHitAS.volume = 1.0f;
        //snowTreeHitAS.pitch = 1.1f;

        int attempts = 3;
        AudioClip newSnowTreeHitClip = snowTreeHitClips[Random.Range(0, snowTreeHitClips.Length)];

        while (newSnowTreeHitClip == lastSnowTreeHitClip && attempts > 0)
        {
            newSnowTreeHitClip = snowTreeHitClips[Random.Range(0, snowTreeHitClips.Length)];
            attempts--;
        }

        lastSnowTreeHitClip = newSnowTreeHitClip;
        snowTreeHitAS.PlayOneShot(newSnowTreeHitClip);
    }

    // Plays a new crystalHitSFX - different from the one previously played
    public void PlayCrystalHitSFX()
    {
        //crystalHitAS.volume = 0.28f;
        //crystalHitAS.pitch = 0.9f;

        int attempts = 3;
        AudioClip newCrystalHitClip = crystalHitClips[Random.Range(0, crystalHitClips.Length)];

        while (newCrystalHitClip == lastCrystalHitClip && attempts > 0)
        {
            newCrystalHitClip = crystalHitClips[Random.Range(0, crystalHitClips.Length)];
            attempts--;
        }

        lastCrystalHitClip = newCrystalHitClip;
        crystalHitAS.PlayOneShot(newCrystalHitClip);
    }

    // Plays a new metalHitSFX - different from the one previously played
    public void PlayMetalHitSFX()
    {
        //metalHitSFX.volume = 0.2f;
        //metalHitSFX.pitch = 1.0f;

        int attempts = 3;
        AudioClip newMetalHitClip = metalHitClips[Random.Range(0, metalHitClips.Length)];

        while (newMetalHitClip == lastMetalHitClip && attempts > 0)
        {
            newMetalHitClip = metalHitClips[Random.Range(0, metalHitClips.Length)];
            attempts--;
        }

        lastMetalHitClip = newMetalHitClip;
        metalHitAS.PlayOneShot(newMetalHitClip);
    }

    // Plays a new rockHitSFX - different from the one previously played
    public void PlayRockHitSFX()
    {
        //rockHitAS.volume = 0.4f;
        //rockHitAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newRockHitClip = rockHitClips[Random.Range(0, rockHitClips.Length)];

        while (newRockHitClip == lastRockHitClip && attempts > 0)
        {
            newRockHitClip = rockHitClips[Random.Range(0, rockHitClips.Length)];
            attempts--;
        }

        lastRockHitClip = newRockHitClip;
        rockHitAS.PlayOneShot(newRockHitClip);
    }

    // Plays a new rockBreakSFX - different from the one previously played
    public void PlayRockBreakSFX()
    {
        //rockBreakAS.volume = 1.0f;
        //rockBreakAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newRockBreakClip = rockBreakClips[Random.Range(0, rockBreakClips.Length)];

        while (newRockBreakClip == lastRockBreakClip && attempts > 0)
        {
            newRockBreakClip = rockBreakClips[Random.Range(0, rockBreakClips.Length)];
            attempts--;
        }

        lastRockBreakClip = newRockBreakClip;
        rockBreakAS.PlayOneShot(newRockBreakClip);
    }

    // Plays a new grassFootstepSFX - different from the one previously played
    public void PlayGrassFootstepSFX()
    {
        footstepsAS.volume = 0.7f;
        footstepsAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newGrassFootstepClip = grassFootstepClips[Random.Range(0, grassFootstepClips.Length)];

        while (newGrassFootstepClip == lastGrassFootstepClip && attempts > 0)
        {
            newGrassFootstepClip = grassFootstepClips[Random.Range(0, grassFootstepClips.Length)];
            attempts--;
        }

        lastGrassFootstepClip = newGrassFootstepClip;
        footstepsAS.PlayOneShot(newGrassFootstepClip);
    }

    // Plays a new snowFootstepSFX - different from the one previously played
    public void PlaySnowFootstepSFX()
    {
        footstepsAS.volume = 0.7f;
        footstepsAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newSnowFootstepClip = snowFootstepClips[Random.Range(0, snowFootstepClips.Length)];

        while (newSnowFootstepClip == lastSnowFootstepClip && attempts > 0)
        {
            newSnowFootstepClip = snowFootstepClips[Random.Range(0, snowFootstepClips.Length)];
            attempts--;
        }

        lastSnowFootstepClip = newSnowFootstepClip;
        footstepsAS.PlayOneShot(newSnowFootstepClip);
    }

    // Plays a new stoneFootstepSFX - different from the one previously played
    public void PlayStoneFootstepSFX()
    {
        footstepsAS.volume = 0.55f;
        footstepsAS.pitch = 1.2f;

        int attempts = 3;
        AudioClip newStoneFootstepClip = stoneFootstepClips[Random.Range(0, stoneFootstepClips.Length)];

        while (newStoneFootstepClip == lastStoneFootstepClip && attempts > 0)
        {
            newStoneFootstepClip = stoneFootstepClips[Random.Range(0, stoneFootstepClips.Length)];
            attempts--;
        }

        lastStoneFootstepClip = newStoneFootstepClip;
        footstepsAS.PlayOneShot(newStoneFootstepClip);
    }

    // Plays a new metalFootstepSFX - different from the one previously played
    public void PlayMetalFootstepSFX()
    {
        footstepsAS.volume = 1.0f;
        footstepsAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newMetalFootstepClip = metalFootstepClips[Random.Range(0, metalFootstepClips.Length)];

        while (newMetalFootstepClip == lastMetalFootstepClip && attempts > 0)
        {
            newMetalFootstepClip = metalFootstepClips[Random.Range(0, metalFootstepClips.Length)];
            attempts--;
        }

        lastMetalFootstepClip = newMetalFootstepClip;
        footstepsAS.PlayOneShot(newMetalFootstepClip);
    }

    // Plays a new woodFootstepSFX - different from the one previously played
    public void PlayWoodFootstepSFX()
    {
        footstepsAS.volume = 0.8f;
        footstepsAS.pitch = 0.9f;

        int attempts = 3;
        AudioClip newWoodFootstepClip = woodFootstepClips[Random.Range(0, woodFootstepClips.Length)];

        while (newWoodFootstepClip == lastWoodFootstepClip && attempts > 0)
        {
            newWoodFootstepClip = woodFootstepClips[Random.Range(0, woodFootstepClips.Length)];
            attempts--;
        }

        lastWoodFootstepClip = newWoodFootstepClip;
        footstepsAS.PlayOneShot(newWoodFootstepClip);
    }

    // Plays a new woodFootstepSFX - different from the one previously played
    public void PlayCrateFootstepSFX()
    {
        footstepsAS.volume = 0.4f;
        footstepsAS.pitch = 1.0f;

        int attempts = 3;
        AudioClip newCrateFootsepClip = crateFootstepClips[Random.Range(0, crateFootstepClips.Length)];

        while (newCrateFootsepClip == lastCrateFootstepClip && attempts > 0)
        {
            newCrateFootsepClip = crateFootstepClips[Random.Range(0, crateFootstepClips.Length)];
            attempts--;
        }

        lastCrateFootstepClip = newCrateFootsepClip;
        footstepsAS.PlayOneShot(newCrateFootsepClip);
    }

    // Plays a new looping ambient sfx - different from the one currenlty playing
    public void ChangeLoopingAmbienceSFX()
    {
        int attempts = 3;
        AudioClip newLoopingAmbienceClip = loopingAmbienceClips[Random.Range(0, loopingAmbienceClips.Length)];

        while (newLoopingAmbienceClip == lastLoopingAmbienceClip && attempts > 0)
        {
            newLoopingAmbienceClip = loopingAmbienceClips[Random.Range(0, loopingAmbienceClips.Length)];
            attempts--;
        }

        lastLoopingAmbienceClip = newLoopingAmbienceClip;

        loopingAmbienceAS.Stop();
        loopingAmbienceAS.clip = newLoopingAmbienceClip;
        loopingAmbienceAS.Play();
    }

    // Selects and plays a new track for the dialogue music - different from the one previously played
    public void ChangeDialogueMusicTrack()
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

        dialogueMusic.Stop();
        dialogueMusic.clip = newDialogueMusicTrack;
        dialogueMusic.Play();
    }

    // Checks which audio loops to set active
    private void SetAudioLoopsActiveCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "MainMenu" && playerScript.checkIfOnCheckpoint())
        {
            backgroundMusic.gameObject.SetActive(true);
            loopingAmbienceAS.gameObject.SetActive(true);
        }
    }

    // Lerps the volume of an audio source over a specific duration (endVolume = volume to lerp to, duration = seconds)
    private IEnumerator LerpAudio(AudioSource audioSource, float endVolume, float duration)
    {
        float time = 0;
        float startVolume = audioSource.volume;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;

        if (audioSource == dialogueMusic && !isDialogueMusic)
            dialogueMusic.gameObject.SetActive(false);
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

    // Determines the music to play for each scene
    private void SetMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AudioClip musicTrackToPlay = null;

        if (sceneName == "TutorialMap")
            musicTrackToPlay = backgroundMusicTracks[0];
        else if (sceneName == "FirstMap")
            musicTrackToPlay = backgroundMusicTracks[1];
        else if (sceneName == "SecondMap")
            musicTrackToPlay = backgroundMusicTracks[2];
        else if (sceneName == "ThirdMap")
            musicTrackToPlay = backgroundMusicTracks[3];
        else if (sceneName == "FourthMap")
            musicTrackToPlay = backgroundMusicTracks[4];
        else if (sceneName == "FifthMap")
            musicTrackToPlay = backgroundMusicTracks[5];

        // Note: no need to Stop() and Play() since this is called in awake
        //backgroundMusic.Stop();
        backgroundMusic.clip = musicTrackToPlay;
        //backgroundMusic.Play();
    }

    // Sets the scrpts to use
    private void SetScripts()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "MainMenu")
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

            playerScript = FindObjectOfType<TileMovementController>();
        }
        else
            optionsMenuScript = FindObjectOfType<MainMenu>().optionsScreen.GetComponent<OptionsMenu>();

        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "Music")
            {
                GameObject music = child;

                for (int j = 0; j < music.transform.childCount; j++)
                {
                    AudioSource childAudioSource = music.transform.GetChild(j).GetComponent<AudioSource>();

                    if (childAudioSource.name == "MainMenuMusic")
                        mainMenuMusic = childAudioSource;
                    if (childAudioSource.name == "DialogueMusic")
                        dialogueMusic = childAudioSource;
                    if (childAudioSource.name == "BackgroundMusic")
                        backgroundMusic = childAudioSource;
                    if (childAudioSource.name == "EndCreditsMusic")
                        endCreditsMusic = childAudioSource;
                }
            }

            if (child.name == "AudioLoops")
            {
                GameObject audioLoops = child;

                for (int k = 0; k < audioLoops.transform.childCount; k++)
                {
                    AudioSource childAudioSource = audioLoops.transform.GetChild(k).GetComponent<AudioSource>();

                    if (childAudioSource.name == "LoopingAmbienceSFX")
                        loopingAmbienceAS = childAudioSource;
                }
            }

            if (child.name == "SFX")
            {
                GameObject sfx = child;

                for (int l = 0; l < sfx.transform.childCount; l++)
                {
                    AudioSource childAudioSource = sfx.transform.GetChild(l).GetComponent<AudioSource>();

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
                    if (childAudioSource.name == "TreeHitSFX")
                        treeHitAS = childAudioSource;
                    if (childAudioSource.name == "SnowTreeHitSFX")
                        snowTreeHitAS = childAudioSource;
                    if (childAudioSource.name == "CrystalHitSFX")
                        crystalHitAS = childAudioSource;
                    if (childAudioSource.name == "MetalHitSFX")
                        metalHitAS = childAudioSource;
                    if (childAudioSource.name == "RockHitSFX")
                        rockHitAS = childAudioSource;
                    if (childAudioSource.name == "RockBreakSFX")
                        rockBreakAS = childAudioSource;
                    if (childAudioSource.name == "FootstepsSFX")
                        footstepsAS = childAudioSource;
                    if (childAudioSource.name == "PushCrateSFX")
                        pushCrateAS = childAudioSource;
                    if (childAudioSource.name == "CantPushCrateSFX")
                        cantPushCrateAS = childAudioSource;
                    if (childAudioSource.name == "CrateSlideSFX")
                        crateSlideAS = childAudioSource;
                    if (childAudioSource.name == "CrateThumpSFX")
                        crateThumpAS = childAudioSource;
                }
            }
        }

        originalVolumeBGM = backgroundMusic.volume;
        originalVolumeDM = dialogueMusic.volume;
        originalVolumeLASFX = loopingAmbienceAS.volume;
        fadeAudioLength = gameManagerScript.fadeAudioLength;
        SetMusic();
    }

    // Updates the fadeAudioLength if it changed - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (fadeAudioLength != gameManagerScript.fadeAudioLength)
                fadeAudioLength = gameManagerScript.fadeAudioLength;
        }
    }

}
