using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private float originalVolumeBGM; // BGM = background music
    private float originalVolumeDM; // DM = dialogue music
    private float originalVolumeECM; // ECM = end credits music
    private float originalVolumeLASFX; // LASFX = looping ambience sfx
    private float originalVolumeCNSFX; // CNSFX = char noise sfx
    private float originalVolumeGLSFX; // GLSFX = generator loop sfx
    private bool isDialogueMusic = false;
    private bool isEndCreditsMusic = false;

    private GameObject musicHolder;
    private GameObject audioLoopsHolder;
    private GameObject sfxHolder;

    private IEnumerator backgroundMusicCoroutine;
    private IEnumerator dialogueMusicCoroutine;
    private IEnumerator endCreditsMusicCoroutine;
    private IEnumerator loopingAmbienceCoroutine;
    private IEnumerator charNoiseCoroutine;

    [Header("Audio Loops")]
    public AudioSource loopingFireAS;
    public AudioSource charNoiseAS;
    private AudioSource loopingAmbienceAS;
    private AudioSource generatorLoopAS;


    [Header("Sound Effects")]
    public SoundEffect_SO[] dialogueBubbleSFX;
    public SoundEffect_SO[] userInterfaceSFX;
    public SoundEffect_SO[] footstepSFX;
    public SoundEffect_SO[] hitSFX;
    public SoundEffect_SO[] crateSFX;
    public SoundEffect_SO[] chestSFX;
    public SoundEffect_SO[] torchSFX;
    public SoundEffect_SO[] loopSFX;
    public SoundEffect_SO[] otherSFX;

    [Header("Music")]
    public SoundEffect_SO[] zoneMusic;
    public SoundEffect_SO[] otherMusic;

    //Review
    private AudioClip lastLoopingAmbienceClip;
    private AudioClip lastDialogueMusicTrack;

    private TileMovementController playerScript;
    private TransitionFade transitionFadeScript;
    private Generator generatorScript;

    void Awake()
    {
        SetScripts();
        //SetElements();
        //SetAudioLoopsForTutorial();
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayMainMenuMusicCheck();
        SetAudioLoopsActiveCheck();
    }

    // Checks if you're in the tutorial scene - fades in ONLY the music IF SO - ONLY gets called/checked during the tutorial scene
    /*private void SetAudioLoopsForTutorial()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            FadeInBackgroundMusic();
    }*/

    // Returns or sets the value of fadeAudioLength
    /*public float FadeAudioLength
    {
        get
        {
            return fadeAudioLength;
        }
        set
        {
            fadeAudioLength = value;
        }
    }*/

    public AudioSource TurnOnGeneratorAS
    {
        get
        {
            return generatorLoopAS;
            //return turnOnGeneratorAS; // Correct Audio Source
        }
    }

    public AudioSource GeneratorLoopAS
    {
        get
        {
            return generatorLoopAS;
        }
    }

    // Stops playing all of the lengthy puzzle-related sfx
    public void StopAllPuzzleSFX()
    {
        /*if (torchFireIgniteAS.isPlaying)
            torchFireIgniteAS.Stop();
        if (crateSlideAS.isPlaying)
            crateSlideAS.Stop();*/
    }

    // Pauses all of the playing audio sources in the scene
    public void PauseAllAudio()
    {
        // Pauses all of the music
        /*for (int i = 0; i < musicHolder.transform.childCount; i++)
        {
            GameObject child = musicHolder.transform.GetChild(i).gameObject;
            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.Pause();
        }*/
        // Pauses all of the audio loops
        for (int i = 0; i < audioLoopsHolder.transform.childCount; i++)
        {
            GameObject child = audioLoopsHolder.transform.GetChild(i).gameObject;
            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.Pause();
        }
        // Pauses all of the sfx
        for (int i = 0; i < sfxHolder.transform.childCount; i++)
        {
            GameObject child = sfxHolder.transform.GetChild(i).gameObject;

            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.Pause();
        }
    }

    // Resumes all of the paused audio sources in the scene
    public void UnPauseAllAudio()
    {
        // Unpauses all of the music
        /*for (int i = 0; i < musicHolder.transform.childCount; i++)
        {
            GameObject child = musicHolder.transform.GetChild(i).gameObject;
            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (!childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.UnPause();
        }*/
        // Unpauses all of the audio loops
        for (int i = 0; i < audioLoopsHolder.transform.childCount; i++)
        {
            GameObject child = audioLoopsHolder.transform.GetChild(i).gameObject;
            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (!childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.UnPause();
        }
        // Unpauses all of the sfx
        for (int i = 0; i < sfxHolder.transform.childCount; i++)
        {
            GameObject child = sfxHolder.transform.GetChild(i).gameObject;
            AudioSource childAudioSource = child.GetComponent<AudioSource>();

            if (!childAudioSource.isPlaying && child.activeSelf)
                childAudioSource.UnPause();
        }
    }

    // Assigns a new script to the generator script
    public void SetGeneratorScript(Generator newScript)
    {
        generatorScript = newScript;
    }

    // Fades in the generator loop sfx - ONLY USED when transitioning out of a dialogue view
    public void FadeInGeneratorLoopCheck()
    {
        if (generatorLoopAS.isPlaying && generatorScript != null)
            generatorScript.FadeInGeneratorAudio();
    }

    // Fades out the generator loop sfx - ONLY USED when transitioning to a dialogue view
    public void FadeOutGeneratorLoopCheck()
    {
        if (generatorLoopAS.isPlaying && generatorScript != null)
            generatorScript.FadeOutGeneratorAudio(0.25f);
    }

    // Fades in the background music
    public void FadeInBackgroundMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeIn;

        //if (!backgroundMusic.gameObject.activeSelf)
        //    backgroundMusic.gameObject.SetActive(true);

        //if (backgroundMusicCoroutine != null)
        //    StopCoroutine(backgroundMusicCoroutine);

        //backgroundMusicCoroutine = LerpAudio(backgroundMusic, originalVolumeBGM, fadeAudioLength);

        //backgroundMusic.volume = 0f;
        //StartCoroutine(backgroundMusicCoroutine);
    }

    // Fades out the background music
    public void FadeOutBackgroundMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeOut;

        //if (backgroundMusicCoroutine != null)
        //    StopCoroutine(backgroundMusicCoroutine);

        //backgroundMusicCoroutine = LerpAudio(backgroundMusic, 0f, fadeAudioLength);

        //backgroundMusic.volume = originalVolumeBGM;
        //StartCoroutine(backgroundMusicCoroutine);
    }

    // Fades in the looping ambient sfx
    public void FadeInLoopingAmbientSFX()
    {
        float fadeAudioLength = transitionFadeScript.gameFadeIn;

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
        float fadeAudioLength = transitionFadeScript.gameFadeOut;

        if (loopingAmbienceCoroutine != null)
            StopCoroutine(loopingAmbienceCoroutine);

        loopingAmbienceCoroutine = LerpAudio(loopingAmbienceAS, 0f, fadeAudioLength);

        loopingAmbienceAS.volume = originalVolumeLASFX;
        StartCoroutine(loopingAmbienceCoroutine);
    }

    // Fades in the dilaogue music
    public void FadeInDialogueMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeIn;

        //if (!dialogueMusic.gameObject.activeSelf)
        //    dialogueMusic.gameObject.SetActive(true);

        //if (dialogueMusicCoroutine != null)
        //    StopCoroutine(dialogueMusicCoroutine);

        //dialogueMusicCoroutine = LerpAudio(dialogueMusic, originalVolumeDM, fadeAudioLength);
        //isDialogueMusic = true;

        //dialogueMusic.volume = 0f;
        //ChangeDialogueMusicTrack();
        //StartCoroutine(dialogueMusicCoroutine);
    }

    // Fades out the dialogue music
    public void FadeOutDialogueMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeOut;

        //if (dialogueMusicCoroutine != null)
        //    StopCoroutine(dialogueMusicCoroutine);

        //dialogueMusicCoroutine = LerpAudio(dialogueMusic, 0f, fadeAudioLength);
        //isDialogueMusic = false;

        //dialogueMusic.volume = originalVolumeDM;
        //StartCoroutine(dialogueMusicCoroutine);
    }

    // Fades in the end credits music
    public void FadeInEndCreditsMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeIn; // original credits faded for 1 second

        //if (!endCreditsMusic.gameObject.activeSelf)
        //    endCreditsMusic.gameObject.SetActive(true);

        //if (endCreditsMusicCoroutine != null)
        //    StopCoroutine(endCreditsMusicCoroutine);

        //endCreditsMusicCoroutine = LerpAudio(endCreditsMusic, originalVolumeECM, fadeAudioLength);
        //isEndCreditsMusic = true;

        //endCreditsMusic.volume = 0f;
        //endCreditsMusic.Play();
        //StartCoroutine(endCreditsMusicCoroutine);
    }

    // Fades out the end credits music
    public void FadeOutEndCreditsMusic()
    {
        //float fadeAudioLength = transitionFadeScript.gameFadeOut; // original credits faded for 1 second

        //if (endCreditsMusicCoroutine != null)
        //    StopCoroutine(endCreditsMusicCoroutine);

        //endCreditsMusicCoroutine = LerpAudio(endCreditsMusic, 0f, fadeAudioLength);
        //isEndCreditsMusic = false;

        //endCreditsMusic.volume = originalVolumeECM;
        //StartCoroutine(endCreditsMusicCoroutine);
    }

    // Fades out the char noise sfx
    public void FadeOutCharNoiseSFX()
    {
        float fadeAudioLength = transitionFadeScript.gameFadeOut;

        if (charNoiseCoroutine != null)
            StopCoroutine(charNoiseCoroutine);

        charNoiseCoroutine = LerpAudio(charNoiseAS, 0f, fadeAudioLength);

        charNoiseAS.volume = originalVolumeCNSFX;
        StartCoroutine(charNoiseCoroutine);
    }





    
    // Checks to play the sfx within the array ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void PlaySFX(SoundEffect_SO[] array, string nameOfSFX)
    {
        foreach (SoundEffect_SO sFX in array)
        {
            if (sFX.name != nameOfSFX) continue;

            sFX.Play();
            return;
        }

        //Debug.Log($"{nameOfSFX} does NOT exist");
    }

    // Checks to play the music within the array
    private void PlayMusic(SoundEffect_SO[] array, string nameOfMusic)
    {
        foreach (SoundEffect_SO sFX in array)
        {
            if (!sFX.name.Contains(nameOfMusic)) continue;

            sFX.Play();
            return;
        }

        array[1].Play();
        //Debug.Log($"{nameOfMusic} does NOT exist");
    }

    // Dialogue Bubble SFX
    public void PlayPlayerDialogueBubbleSFX() => PlaySFX(dialogueBubbleSFX, "PlayerDialogueBubbleSFX");
    public void PlayNPCDialogueBubbleSFX() => PlaySFX(dialogueBubbleSFX, "NPCDialogueBubbleSFX");

    // User Interface SFX
    public void PlayMenuButtonSelectSFX() => PlaySFX(userInterfaceSFX, "MenuButtonSelectSFX");
    public void PlayMenuButtonClickSFX() => PlaySFX(userInterfaceSFX, "MenuButtonClickSFX");
    public void PlayTipButtonClickSFX() => PlaySFX(userInterfaceSFX, "TipButtonClickSFX");
    public void PlayArrowSelectSFX() => PlaySFX(userInterfaceSFX, "ArrowSelectSFX");
    public void PlayDeathScreenSFX() => PlaySFX(userInterfaceSFX, "DeathScreenSFX");
    public void PlayPopUpSFX() => PlaySFX(userInterfaceSFX, "PopUpSFX");

    // Foostep SFX
    public void PlayGrassFootstepSFX() => PlaySFX(footstepSFX, "GrassFootstepSFX");
    public void PlaySnowFootstepSFX() => PlaySFX(footstepSFX, "SnowFootstepSFX");
    public void PlayStoneFootstepSFX() => PlaySFX(footstepSFX, "StoneFootstepSFX");
    public void PlayMetalFootstepSFX() => PlaySFX(footstepSFX, "MetalFootstepSFX");
    public void PlayWoodFootstepSFX() => PlaySFX(footstepSFX, "WoodFootstepSFX");
    public void PlayCrateFootstepSFX() => PlaySFX(footstepSFX, "CrateFootstepSFX");

    // Hit SFX
    public void PlayHitTreeSFX() => PlaySFX(hitSFX, "HitTreeSFX");
    public void PlayHitSnowTreeSFX() => PlaySFX(hitSFX, "HitSnowTreeSFX");
    public void PlayHitCrystalSFX() => PlaySFX(hitSFX, "HitCrystalSFX");
    public void PlayHitBarrelSFX() => PlaySFX(hitSFX, "HitBarrelSFX");
    public void PlayHitRockSFX() => PlaySFX(hitSFX, "HitRockSFX");

    // Crate SFX
    public void PlayCantPushCrateSFX() => PlaySFX(crateSFX, "CantPushCrateSFX");
    public void PlayPushCrateSFX() => PlaySFX(crateSFX, "PushCrateSFX");
    public void PlayCrateSlideSFX() => PlaySFX(crateSFX, "CrateSlideSFX");
    public void PlayCrateThumpSFX() => PlaySFX(crateSFX, "CrateThumpSFX");

    // Chest SFX
    public void PlayCloseChestSFX() => PlaySFX(chestSFX, "CloseChestSFX");
    public void PlayOpenChestSFX() => PlaySFX(chestSFX, "OpenChestSFX");

    // Torch SFX
    public void PlayExtinguishFireSFX() => PlaySFX(torchSFX, "ExtinguishFireSFX");
    public void PlayIgniteFireSFX() => PlaySFX(torchSFX, "IgniteFireSFX");

    // Loops SFX
    public void PlayAmbientWindSFX() => PlaySFX(loopSFX, "AmbientWindSFX");
    public void PlayTorchFireSFX() => PlaySFX(loopSFX, "TorchFireSFX");
    public void PlayGeneratorSFX() => PlaySFX(loopSFX, "GeneratorSFX");
    public void PlayCharNoiseSFX() => PlaySFX(loopSFX, "CharNoiseSFX");

    // Other SFX
    public void PlaySkippedSceneSFX() => PlaySFX(otherSFX, "SkippedSceneSFX");
    public void PlayBreakRockSFX() => PlaySFX(otherSFX, "BreakRockSFX");
    public void PlayWindGushSFX() => PlaySFX(otherSFX, "WindGushSFX");
    public void PlayFreezeingSFX() => PlaySFX(otherSFX, "FreezeSFX");
    public void PlaySwooshSFX() => PlaySFX(otherSFX, "SwooshSFX");
    public void PlayChimeSFX() => PlaySFX(otherSFX, "ChimeSFX");


    // Checks which audio loops to play at the begining of the scene
    private void SetAudioLoopsActiveCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || !playerScript.OnCheckpoint()) return;

        PlayMusic(zoneMusic, sceneName);
        PlayAmbientWindSFX();
        PlayTorchFireSFX();
    }

    // Checks to play the main menu music
    private void PlayMainMenuMusicCheck()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        PlayMusic(otherMusic, "MainMenuMusic");
    }

    // Sets the scrpts to use
    private void SetScripts() ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        playerScript = (SceneManager.GetActiveScene().name != "MainMenu") ? FindObjectOfType<TileMovementController>() : null;
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }














    // Plays a new looping ambient sfx - different from the one currenlty playing
    public void ChangeLoopingAmbienceSFX()
    {
        //int attempts = 3;
        //AudioClip newLoopingAmbienceClip = loopingAmbienceClips[Random.Range(0, loopingAmbienceClips.Length)];

        //while (newLoopingAmbienceClip == lastLoopingAmbienceClip && attempts > 0)
        //{
        //    newLoopingAmbienceClip = loopingAmbienceClips[Random.Range(0, loopingAmbienceClips.Length)];
        //    attempts--;
        //}

        //lastLoopingAmbienceClip = newLoopingAmbienceClip;

        //loopingAmbienceAS.Stop();
        //loopingAmbienceAS.clip = newLoopingAmbienceClip;
        //loopingAmbienceAS.Play();
    }

    // Selects and plays a new track for the dialogue music - different from the one previously played
    /*public void ChangeDialogueMusicTrack()
    {
        //dialogueMusic.volume = 0.4f;
        //dialogueMusic.pitch = 1f;

        int attempts = 3;
        AudioClip newDialogueMusicTrack = dialogueMusicTracks[Random.Range(0, dialogueMusicTracks.Length)];

        while (newDialogueMusicTrack == lastDialogueMusicTrack && attempts > 0)
        {
            newDialogueMusicTrack = dialogueMusicTracks[Random.Range(0, dialogueMusicTracks.Length)];
            attempts--;
        }

        lastDialogueMusicTrack = newDialogueMusicTrack;

        //dialogueMusic.Stop();
        //dialogueMusic.clip = newDialogueMusicTrack;
        //dialogueMusic.Play();
    }*/

    // Starts the coroutine that lerps the audio
    public void StartLerpAudioCoroutine(IEnumerator corouitne, AudioSource audioSource, float endVolume, float duration)
    {
        if (corouitne != null) StopCoroutine(corouitne);

        corouitne = LerpAudio(audioSource, endVolume, duration);
        StartCoroutine(corouitne);
    }

    // Lerps the volume of the audio source to another over a specific duration (endVolume = volume to lerp to, duration = seconds)
    private IEnumerator LerpAudio(AudioSource audioSource, float endVolume, float duration)
    {
        float time = 0;
        float startVolume = audioSource.volume;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            //time += Time.deltaTime;
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;

        //if (audioSource == dialogueMusic && !isDialogueMusic)
        //{
        //    //dialogueMusic.Stop();
        //    //dialogueMusic.gameObject.SetActive(false);
        //}
        //else if (audioSource == endCreditsMusic && !isEndCreditsMusic)
        //{
        //    //endCreditsMusic.Stop();
        //    //endCreditsMusic.gameObject.SetActive(false);
        //}
        //else if (audioSource == charNoiseAS)
        //{
        //    charNoiseAS.Stop();
        //    charNoiseAS.volume = originalVolumeCNSFX;
        //}
    }



    // Sets private variables, objects, and components
    /*private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "Music")
            {
                musicHolder = child;

                for (int j = 0; j < musicHolder.transform.childCount; j++)
                {
                    AudioSource childAudioSource = musicHolder.transform.GetChild(j).GetComponent<AudioSource>();

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
                audioLoopsHolder = child;

                for (int k = 0; k < audioLoopsHolder.transform.childCount; k++)
                {
                    AudioSource childAudioSource = audioLoopsHolder.transform.GetChild(k).GetComponent<AudioSource>();

                    if (childAudioSource.name == "LoopingAmbienceSFX")
                        loopingAmbienceAS = childAudioSource;
                    if (childAudioSource.name == "GeneratorLoopSFX")
                        generatorLoopAS = childAudioSource;
                }
            }        
        }

        //originalVolumeBGM = backgroundMusic.volume;
        //originalVolumeDM = dialogueMusic.volume;
        //originalVolumeECM = endCreditsMusic.volume;
        originalVolumeLASFX = loopingAmbienceAS.volume;
        originalVolumeCNSFX = charNoiseAS.volume;
        originalVolumeGLSFX = generatorLoopAS.volume;
        SetMusic();
    }*/

}
