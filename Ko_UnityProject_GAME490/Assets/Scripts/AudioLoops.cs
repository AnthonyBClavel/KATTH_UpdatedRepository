using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AudioLoops : MonoBehaviour
{
    public GameObject BGM;
    public GameObject AmbienceLoop;
    public AudioSource chimeSFX;

    private float loopingBGM;
    private float loopingAmbience;

    void Awake()
    {
        SetAudioLoopsForTutorial();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Fades in the volumes for both the BGM and Ambience loop (used after world intros)
    public void FadeInAudioLoops()
    {
        FadeInBGMLoop();
        FadeInAmbienceLoop();
    }

    // Sets the loop volumes to their defaults (used for when the player loads a saved game)
    public void SetAudioLoopsToDefault()
    {     
        BGM.SetActive(true);
        loopingBGM = 0.4f;
        loopingAmbience = 0.4f;
    }

    // Sets the audio loops to 0 volume to start the fade coroutines properly
    public void SetAudioLoopsToZero()
    {
        BGM.GetComponent<AudioSource>().volume = loopingBGM;
        AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
        loopingBGM = 0.0f;
        loopingAmbience = 0.0f;
    }

    // Fades out both the bgm and ambient sfx loop
    public void FadeOutAudioLoops()
    {
        loopingBGM = 0.4f;
        loopingAmbience = 0.4f;
        StartCoroutine("FadeOutMusicLoop");
        StartCoroutine("FadeOutAmbienceLoop");
        StartCoroutine("DisableAudioLoops");
    }

    // Fades in the bgm loop
    public void FadeInBGMLoop()
    {
        StopCoroutine("FadeOutMusicLoop");
        BGM.GetComponent<AudioSource>().volume = loopingBGM;
        loopingBGM = 0.0f;
        BGM.SetActive(true);
        StartCoroutine("FadeInMusicLoop");
    }

    // Fades out the bgm loop
    public void FadeOutBGMLoop()
    {
        StopCoroutine("FadeInMusicLoop");
        BGM.GetComponent<AudioSource>().volume = loopingBGM;
        loopingBGM = 0.4f;
        BGM.SetActive(true);
        StartCoroutine("FadeOutMusicLoop");
    }

    // Fades in the ambient sfx loop
    private void FadeInAmbienceLoop()
    {
        AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
        loopingAmbience = 0.0f;
        StartCoroutine("FadeInAmbienceSFXLoop");
    }

    // Checks if you're in the tutorial scene - fades in ONLY the music IF SO - ONLY gets called/checked during the tutorial scene
    private void SetAudioLoopsForTutorial()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            FadeInBGMLoop();
    }

    // Increases the bgm volume over time until it reaches its max value
    private IEnumerator FadeInMusicLoop()
    {
        for (float i = 0f; i <= 0.4f; i += 0.01f)
        {
            i = loopingBGM;
            loopingBGM += 0.01f;
            BGM.GetComponent<AudioSource>().volume = loopingBGM;
            yield return new WaitForSeconds(0.025f);
        }
    }
    // Increases the ambience loop volume over time until it reaches its max value
    private IEnumerator FadeInAmbienceSFXLoop()
    {
        for (float j = 0f; j <= 0.4f; j += 0.01f)
        {
            j = loopingAmbience;
            loopingAmbience += 0.01f;
            AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Decreases the bgm volume over time until it reaches its min value
    private IEnumerator FadeOutMusicLoop()
    {
        for (float i = 0.4f; i >= 0f; i -= 0.01f)
        {
            i = loopingBGM;
            loopingBGM -= 0.01f;
            BGM.GetComponent<AudioSource>().volume = loopingBGM;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Decreases the ambience loop volume over time until it reaches its min value
    private IEnumerator FadeOutAmbienceLoop()
    {
        for (float j = 0.4f; j >= 0f; j -= 0.01f)
        {
            j = loopingAmbience;
            loopingAmbience -= 0.01f;
            AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Sets the BGM and Ambience loop GameObjects to inactive after specified time
    private IEnumerator DisableAudioLoops()
    {
        yield return new WaitForSeconds(1.6f);
        loopingBGM = 0f;
        loopingAmbience = 0f;       
        BGM.SetActive(false);
        AmbienceLoop.SetActive(false);
    }

}
