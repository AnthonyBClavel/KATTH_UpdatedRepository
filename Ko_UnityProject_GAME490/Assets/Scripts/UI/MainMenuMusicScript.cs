﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicScript : MonoBehaviour
{
    public GameObject mainMenuBGM;
    private float loopingMainMenuBGM;

    void Awake()
    {
        loopingMainMenuBGM = 0.0f;
        mainMenuBGM.GetComponent<AudioSource>().volume = loopingMainMenuBGM;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("FadeInVolume");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FadeOutMusicVolume()
    {
        loopingMainMenuBGM = 0.8f;
        StartCoroutine("FadeOutVolume");
    }

    public void FadeInMusicVolume()
    {
        loopingMainMenuBGM = 0.0f;
        StartCoroutine("FadeInVolume");
    }

    // Increases the volume over time until it reaches its max value
    public IEnumerator FadeInVolume()
    {
        for (float i = 0f; i <= 0.8; i += 0.01f)
        {
            i = loopingMainMenuBGM;
            loopingMainMenuBGM += 0.01f;
            mainMenuBGM.GetComponent<AudioSource>().volume = loopingMainMenuBGM;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Increases the volume over time until it reaches its max value
    private IEnumerator FadeOutVolume()
    {
        for (float i = 0.8f; i >= 0; i -= 0.01f)
        {
            i = loopingMainMenuBGM;
            loopingMainMenuBGM -= 0.01f;
            mainMenuBGM.GetComponent<AudioSource>().volume = loopingMainMenuBGM;
            yield return new WaitForSeconds(0.04f);
        }
    }

}
