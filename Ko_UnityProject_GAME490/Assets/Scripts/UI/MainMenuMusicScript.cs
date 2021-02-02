using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicScript : MonoBehaviour
{
    /*public GameObject initialLoopBGM;
    public GameObject cleanLoopBGM;
    private float duration;*/

    public GameObject mainMenuBGM;
    private float loopingMainMenuBGM;

    // Start is called before the first frame update
    void Awake()
    {
        /* Sets the duration to the length of the audio clip
        duration = initialLoopBGM.GetComponent<AudioSource>().clip.length;
        StartCoroutine("PlayCleanLoopBGM"); */

        loopingMainMenuBGM = 0.0f;

        StartCoroutine("FadeInVolume");
    }

    // Update is called once per frame
    void Update()
    {
        mainMenuBGM.GetComponent<AudioSource>().volume = loopingMainMenuBGM;
    }

    public IEnumerator FadeInVolume()
    {
        for (float i = 0f; i <= 0.8; i += 0.01f)
        {
            i = loopingMainMenuBGM;
            loopingMainMenuBGM += 0.01f;
            yield return new WaitForSeconds(0.025f);
        }
    }

    /* Plays the clean looping bgm after the intitial looping bgm (the initial one's volume fades in via the file itself; woudln't be ideal to loop that)
    private IEnumerator PlayCleanLoopBGM()
    {
        yield return new WaitForSecondsRealtime(duration);
        initialLoopBGM.SetActive(false);
        cleanLoopBGM.SetActive(true);
    }*/

}
