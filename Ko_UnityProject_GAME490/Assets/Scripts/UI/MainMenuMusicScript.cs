using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicScript : MonoBehaviour
{
    public GameObject initialLoopBGM;
    public GameObject cleanLoopBGM;

    private float duration;

    // Start is called before the first frame update
    void Start()
    {
        //sets the duration to the length of the audio clip
        duration = initialLoopBGM.GetComponent<AudioSource>().clip.length;

        StartCoroutine("PlayCleanLoopBGM");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //plays the clean looping bgm after the intitial looping bgm (the initial one's volume fades in; woudln't be ideal to loop that)
    private IEnumerator PlayCleanLoopBGM()
    {
        yield return new WaitForSecondsRealtime(duration);
        initialLoopBGM.SetActive(false);
        cleanLoopBGM.SetActive(true);
    }
}
