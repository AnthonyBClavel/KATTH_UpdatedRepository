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
        BGM.GetComponent<AudioSource>().volume = loopingBGM;
        AmbienceLoop.GetComponent<AudioSource>().volume = loopingAmbience;

        if (SceneManager.GetActiveScene().name == "TutorialMap")
        {
            loopingBGM = 0.0f;
            AmbienceLoop.GetComponent<AudioSource>().volume = 0.4f;
            SetAudioLoopsActive();
        }
        else
        {
            loopingBGM = 0.0f;
            loopingAmbience = 0.0f;
        }

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetAudioLoopsActive()
    {
        BGM.SetActive(true);
        StartCoroutine(FadeInMusicLoop());

        if (SceneManager.GetActiveScene().name != "TutorialMap")
            StartCoroutine(FadeInAmbienceLoop());
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
