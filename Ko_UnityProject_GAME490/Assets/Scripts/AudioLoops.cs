using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AudioLoops : MonoBehaviour
{
    public GameObject BGM;
    public GameObject AmbienceLoop;

    private float loopingBGM;
    private float loopingAmbience;

    void Awake()
    {
        SetAudioLoopsCheck();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Fades in the volumes for both the BGM and Ambience loop (used after world intros)
    public void SetAudioLoopsActive()
    {
        BGM.SetActive(true);
        StartCoroutine(FadeInMusicLoop());

        if (SceneManager.GetActiveScene().name != "TutorialMap")
            StartCoroutine(FadeInAmbienceLoop());
    }

    // Sets the loop volumes to their defaults (used for when the player loads saved game)
    public void SetAudioLoopsToDefault()
    {
        BGM.SetActive(true);
        loopingBGM = 0.4f;
        loopingAmbience = 0.4f;
    }

    // Checks if you're in the tutorial scene - fades in only the music IF SO, sets volume for loops to zero IF NOT
    private void SetAudioLoopsCheck()
    {
        if (SceneManager.GetActiveScene().name == "TutorialMap")
        {
            BGM.GetComponent<AudioSource>().volume = loopingBGM;
            loopingBGM = 0.0f;
            SetAudioLoopsActive();
        }
        else
        {
            BGM.GetComponent<AudioSource>().volume = loopingBGM;
            AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
            loopingBGM = 0.0f;
            loopingAmbience = 0.0f;
        }
    }

    // Increases the bgm volume over time until it reaches its max value
    private IEnumerator FadeInMusicLoop()
    {
        for (float i = 0f; i <= 0.4; i += 0.01f)
        {
            i = loopingBGM;
            loopingBGM += 0.01f;
            BGM.GetComponent<AudioSource>().volume = loopingBGM;
            yield return new WaitForSeconds(0.025f);
        }
    }
    // Increases the ambience loop volume over time until it reaches its max value
    private IEnumerator FadeInAmbienceLoop()
    {
        for (float j = 0f; j <= 0.4; j += 0.01f)
        {
            j = loopingAmbience;
            loopingAmbience += 0.01f;
            AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;
            yield return new WaitForSeconds(0.025f);
        }
    }

}
