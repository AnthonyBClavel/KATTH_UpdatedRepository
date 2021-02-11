using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicScript : MonoBehaviour
{
    public GameObject mainMenuBGM;
    private float loopingMainMenuBGM;

    // Start is called before the first frame update
    void Awake()
    {
        loopingMainMenuBGM = 0.0f;

        StartCoroutine("FadeInVolume");
    }

    // Update is called once per frame
    void Update()
    {
        mainMenuBGM.GetComponent<AudioSource>().volume = loopingMainMenuBGM;
    }

    // Increases the volume over time until it reaches its max value
    public IEnumerator FadeInVolume()
    {
        for (float i = 0f; i <= 0.8; i += 0.01f)
        {
            i = loopingMainMenuBGM;
            loopingMainMenuBGM += 0.01f;
            yield return new WaitForSeconds(0.025f);
        }
    }

}
