using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private List<(AudioSource, IEnumerator)> audioCoroutines = new List<(AudioSource, IEnumerator)>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    private string tutorialZone = "TutorialMap";
    private string mainMenu = "MainMenu";
    private string sceneName;

    [Header("Audio Loops")]
    private AudioSource torchFireAS;
    private AudioSource generatorAS;

    [Header("Sound Effects")]
    public AudioClip_SO[] dialogueBubbleSFX;
    public AudioClip_SO[] userInterfaceSFX;
    public AudioClip_SO[] footstepSFX;
    public AudioClip_SO[] hitSFX;
    public AudioClip_SO[] crateSFX;
    public AudioClip_SO[] chestSFX;
    public AudioClip_SO[] torchSFX;
    public AudioClip_SO[] loopSFX;
    public AudioClip_SO[] otherSFX;

    private AudioClip_SO generatorSFX;
    private AudioClip_SO ambientWindSFX;
    private AudioClip_SO torchFireSFX;
    private AudioClip_SO charNoiseSFX;

    [Header("Music")]
    public AudioClip_SO[] zoneMusic;
    public AudioClip_SO[] otherMusic;

    private AudioClip_SO backgroundMusic;
    private AudioClip_SO dialogueMusic;
    private AudioClip_SO endCreditsMusic;

    public static AudioManager instance;
    private TorchMeter torchMeterScript;
    private TileMovementController playerScript;

    public AudioClip_SO TorchFireSFX
    {
        get { return torchFireSFX; }
    }

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetMusic();
        SetAudioLoops();
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayMainMenuMusicCheck();
        SetAudioLoopsActiveCheck();
    }

    // Stops playing all of the puzzle-related sfx (does not stop music or audio loops)
    public void StopAllPuzzleAudio()
    {
        foreach ((AudioSource, IEnumerator) tuple in audioCoroutines)
        {
            AudioSource audioSource = tuple.Item1;
            if (audioSource.name.Contains("Music") || audioSource.loop) continue;

            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
            //Debug.Log($"STOPPED playing {audioSource.name}");
        }

        // Checks to stop playing the generator sfx
        if (generatorAS == null || !generatorAS.isPlaying) return;
        generatorAS.Stop();
        generatorAS.gameObject.SetActive(false);
    }

    // Pauses all of the sfx (does not pause music)
    public void PauseAllAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.gameObject.activeInHierarchy || audioSource.name.Contains("Music") || !audioSource.isPlaying) continue;

            audioSource.Pause();
            //Debug.Log($"PAUSED {audioSource.name}");
        }
    }

    // Unpauses all of the sfx (does not unpause anything that is already playing)
    public void UnPauseAllAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.gameObject.activeInHierarchy || audioSource.name.Contains("Music") || audioSource.isPlaying) continue;

            audioSource.UnPause();
            //Debug.Log($"UNPAUSED {audioSource.name}");
        }
    }

    // Checks to set the volume for the torch fire sfx
    public void SetTorchFireVolume(float newVolume)
    {
        if (torchMeterScript == null || torchFireAS == null) return;
        torchFireAS.volume = newVolume;
    }

    // Checks to set the max volume for the torch fire sfx
    public void ResetTorchFireVolume()
    {
        if (torchMeterScript == null || torchFireAS == null) return;
        torchFireAS.volume = torchFireSFX.volume;
    }

    // Checks to fade in the generator sfx
    public void FadeInGeneratorSFX()
    {
        //if (generatorAS == null) return;
        generatorSFX.FadeInAudio(d: 1f);
    }

    // Checks to fade out the generator sfx
    public void FadeOutGeneratorSFX()
    {
        if (generatorAS == null || !generatorAS.isPlaying) return;
        generatorSFX.FadeOutAudio(d: 1f);
    }

    // Cross fades in the dialogue music - For character dialogue script ONLY
    public void CrossFadeInDialogueMusic()
    {
        dialogueMusic.FadeInAudio();
        backgroundMusic.FadeOutAudio();

        if (generatorAS == null || !generatorAS.isPlaying) return;
        generatorSFX.FadeOutAudio(ev: 0.25f, d: 1f);
    }

    // Cross fades out the dialogue music - For character dialogue script ONLY
    public void CrossFadeOutDialogueMusic()
    {
        dialogueMusic.FadeOutAudio();
        backgroundMusic.FadeInAudio();

        if (generatorAS == null || !generatorAS.isPlaying) return;
        generatorSFX.FadeInAudio(sv: 0.25f, d: 1f);
    }

    // Fades the looping audio in/out
    public void FadeInBackgroundMusic() => backgroundMusic.FadeInAudio();
    public void FadeOutBackgroundMusic() => backgroundMusic.FadeOutAudio();
    public void FadeInEndCreditsMusic() => endCreditsMusic.FadeInAudio();
    public void FadeOutEndCreditsMusic() => endCreditsMusic.FadeOutAudio();
    public void FadeInAmbientWindSFX() => ambientWindSFX.FadeInAudio();
    public void FadeOutAmbientWindSFX() => ambientWindSFX.FadeOutAudio();

    // SFX for the dialogue bubbles
    public void PlayPlayerDialogueBubbleSFX() => PlaySFX(dialogueBubbleSFX, "PlayerDialogueBubbleSFX");
    public void PlayNPCDialogueBubbleSFX() => PlaySFX(dialogueBubbleSFX, "NPCDialogueBubbleSFX");

    // SFX for the user interface
    public void PlayMenuButtonSelectSFX() => PlaySFX(userInterfaceSFX, "MenuButtonSelectSFX");
    public void PlayMenuButtonClickSFX() => PlaySFX(userInterfaceSFX, "MenuButtonClickSFX");
    public void PlayTipButtonClickSFX() => PlaySFX(userInterfaceSFX, "TipButtonClickSFX");
    public void PlayArrowSelectSFX() => PlaySFX(userInterfaceSFX, "ArrowSelectSFX");
    public void PlayDeathScreenSFX() => PlaySFX(userInterfaceSFX, "DeathScreenSFX");
    public void PlayPressEnterSFX() => PlaySFX(userInterfaceSFX, "PressEnterSFX");
    public void PlayPopUpSFX() => PlaySFX(userInterfaceSFX, "PopUpSFX");

    // SFX for the player's footsteps
    public void PlayGrassFootstepSFX() => PlaySFX(footstepSFX, "GrassFootstepSFX");
    public void PlaySnowFootstepSFX() => PlaySFX(footstepSFX, "SnowFootstepSFX");
    public void PlayStoneFootstepSFX() => PlaySFX(footstepSFX, "StoneFootstepSFX");
    public void PlayMetalFootstepSFX() => PlaySFX(footstepSFX, "MetalFootstepSFX");
    public void PlayWoodFootstepSFX() => PlaySFX(footstepSFX, "WoodFootstepSFX");
    public void PlayCrateFootstepSFX() => PlaySFX(footstepSFX, "CrateFootstepSFX");

    // SFX for hitting/shaking a static object
    public void PlayHitTreeSFX() => PlaySFX(hitSFX, "HitTreeSFX");
    public void PlayHitSnowTreeSFX() => PlaySFX(hitSFX, "HitSnowTreeSFX");
    public void PlayHitCrystalSFX() => PlaySFX(hitSFX, "HitCrystalSFX");
    public void PlayHitBarrelSFX() => PlaySFX(hitSFX, "HitBarrelSFX");
    public void PlayHitRockSFX() => PlaySFX(hitSFX, "HitRockSFX");

    // SFX for the crate
    public void PlayCantPushCrateSFX() => PlaySFX(crateSFX, "CantPushCrateSFX");
    public void PlayPushCrateSFX() => PlaySFX(crateSFX, "PushCrateSFX");
    public void PlayCrateSlideSFX() => PlaySFX(crateSFX, "CrateSlideSFX");
    public void PlayCrateThumpSFX() => PlaySFX(crateSFX, "CrateThumpSFX");

    // SFX for the artifact chest
    public void PlayCloseChestSFX() => PlaySFX(chestSFX, "CloseChestSFX");
    public void PlayOpenChestSFX() => PlaySFX(chestSFX, "OpenChestSFX");

    // SFX for the player's torch
    public void PlayExtinguishFireSFX() => PlaySFX(torchSFX, "ExtinguishFireSFX");
    public void PlayIgniteFireSFX() => PlaySFX(torchSFX, "IgniteFireSFX");

    // SFX for other elements/events
    public void PlayTurnOnGeneratorSFX() => PlaySFX(otherSFX, "TurnOnGeneratorSFX");
    public void PlayShortWindGushSFX() => PlaySFX(otherSFX, "ShortWindGushSFX");
    public void PlayLongWindGushSFX() => PlaySFX(otherSFX, "LongWindGushSFX");
    public void PlaySkippedSceneSFX() => PlaySFX(otherSFX, "SkippedSceneSFX");
    public void PlayBreakRockSFX() => PlaySFX(otherSFX, "BreakRockSFX");
    public void PlayFreezeingSFX() => PlaySFX(otherSFX, "FreezeSFX");
    public void PlaySwooshSFX() => PlaySFX(otherSFX, "SwooshSFX");
    public void PlayChimeSFX() => PlaySFX(otherSFX, "ChimeSFX");


    // SFX that loop
    public void PlayAmbientWindSFX() => ambientWindSFX.Play();
    public void PlayTorchFireSFX() => torchFireSFX.Play();
    public void PlayGeneratorSFX() => generatorSFX.Play();
    public void PlayCharNoiseSFX() => charNoiseSFX.Play();

    // Plays the appropriate sfx within the scriptable object array
    private void PlaySFX(AudioClip_SO[] array, string nameOfSFX)
    {
        foreach (AudioClip_SO sFX in array)
        {
            if (sFX.name != nameOfSFX) continue;
            sFX.Play();
            return;
        }
        //Debug.Log($"{nameOfSFX} does NOT exist");
    }

    // Returns the appropiate scriptable object
    private AudioClip_SO ReturnAudioSO(AudioClip_SO[] array, string nameOfSO)
    {
        foreach (AudioClip_SO audioSO in array)
        {
            if (!audioSO.name.Contains(nameOfSO)) continue;
            return audioSO;
        }
        return null;
    }

    // Checks to add the audio source to the list of audio sources
    public void AddAudioSourceCheck(AudioSource audioSource)
    {
        if (audioSources.Contains(audioSource)) return;
        audioSources.Add(audioSource);

        switch (audioSource.name)
        {
            case "TorchFireSFX":
                torchFireAS = audioSource;
                break;
            case "GeneratorSFX":
                generatorAS = audioSource;
                break;
            default:
                break;
        }
    }

    // Checks which audio loops to play at the begining of the scene
    private void SetAudioLoopsActiveCheck()
    {
        if (sceneName == mainMenu || sceneName != tutorialZone && !playerScript.OnCheckpoint()) return;

        backgroundMusic.FadeInAudio();
        ambientWindSFX.FadeInAudio();
        torchFireSFX.Play();
    }

    // Checks to play the main menu music
    private void PlayMainMenuMusicCheck()
    {
        if (sceneName != mainMenu) return;

        backgroundMusic.FadeInAudio();
    }

    // Checks to play or resume the audio
    private void PlayAudioCheck(AudioSource audioSource)
    {
        if (audioSource.isPlaying) return;
        string name = audioSource.name;

        if (name.Contains("MapMusic") || name.Contains("MenuMusic"))
            audioSource.UnPause();
        else
            audioSource.Play();
    }

    // Checks to stop or pause the audio
    private void StopAudioCheck(AudioSource audioSource)
    {
        if (!audioSource.isPlaying || audioSource.volume != 0) return;
        string name = audioSource.name;

        if (name.Contains("MapMusic") || name.Contains("MenuMusic"))
            audioSource.Pause();
        else
            audioSource.Stop();
    }

    // Checks to stop the appropiate coroutine - stops the coroutine and removes the tuple from the list
    private void StopCorouitne(AudioSource audioSource)
    {
        foreach ((AudioSource, IEnumerator) tuple in audioCoroutines)
        {
            if (audioSource != tuple.Item1) continue;

            if (tuple.Item2 != null) StopCoroutine(tuple.Item2);
            audioCoroutines.Remove(tuple);
            break;
        }
    }

    // Starts the coroutine that lerps the volume of an audio source
    public void StartLerpAudioCoroutine(AudioSource audioSource, float startVolume, float endVolume, float duration)
    {
        StopCorouitne(audioSource);
        IEnumerator lerpAudioCoroutine = LerpAudio(audioSource, startVolume, endVolume, duration);

        audioCoroutines.Add((audioSource, lerpAudioCoroutine));
        StartCoroutine(lerpAudioCoroutine);
    }

    // Starts the coroutine that sets the audio object inactive
    public void StartSetInactiveCoroutine(AudioSource audioSource, float duration)
    {
        StopCorouitne(audioSource);
        IEnumerator setInactiveCoroutine = SetObjectInactive(audioSource, duration);

        audioCoroutines.Add((audioSource, setInactiveCoroutine));
        StartCoroutine(setInactiveCoroutine);
    }

    // Lerps the volume of an audio source to another over a duration (duration = seconds)
    private IEnumerator LerpAudio(AudioSource audioSource, float startVolume, float endVolume, float duration)
    {
        PlayAudioCheck(audioSource);
        audioSource.volume = startVolume;
        float time = 0;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;
        StopAudioCheck(audioSource);
        StopCorouitne(audioSource);
    }

    // Sets the audio object inactive after a delay (duration = seconds)
    private IEnumerator SetObjectInactive(AudioSource audioSource, float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.gameObject.SetActive(false);
        StopCorouitne(audioSource);
    }

    // Sets the audio loops to use
    private void SetAudioLoops()
    {
        foreach (AudioClip_SO sFX in loopSFX)
        {
            switch (sFX.name)
            {
                case "TorchFireSFX":
                    torchFireSFX = sFX;
                    break;
                case "AmbientWindSFX":
                    ambientWindSFX = sFX;
                    break;
                case "CharNoiseSFX":
                    charNoiseSFX = sFX;
                    break;
                case "GeneratorSFX":
                    generatorSFX = sFX;
                    break;
                default:
                    break;
            }
        }     
    }

    // Sets the music to use
    private void SetMusic()
    {
        backgroundMusic = (sceneName == mainMenu) ? ReturnAudioSO(otherMusic, sceneName) :
        ReturnAudioSO(zoneMusic, sceneName) ?? ReturnAudioSO(zoneMusic, "FirstMapMusic");

        foreach (AudioClip_SO music in otherMusic)
        {
            switch (music.name)
            {
                case "EndCreditsMusic":
                    endCreditsMusic = music;
                    break;
                case "DialogueMusic":
                    dialogueMusic = music;
                    break;
                default:
                    break;
            }
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>() : null;
        torchMeterScript = (sceneName != mainMenu) ? FindObjectOfType<TorchMeter>() : null;
        instance = this;
    }

}
