using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCredits : MonoBehaviour
{
    public GameObject[] credits;
    public string levelToLoad;

    public Animator creditsAnimator;

    //private int currentIndex = 0;
    private GameObject currentCredit;

    // Start is called before the first frame update
    void Start()
    {
        // Sync framerate to monitors refresh rate
        QualitySettings.vSyncCount = 1;

        StartCoroutine("PlayIntroCredits");
        StartCoroutine(LoadSceneAsync(levelToLoad));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator PlayIntroCredits()
    {
        yield return new WaitForSeconds(2f);

        currentCredit = credits[0];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[1];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[2];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[3];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[4];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[5];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[6];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);


        currentCredit = credits[7];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);

        currentCredit = credits[8];
        currentCredit.SetActive(true);
        yield return new WaitForSeconds(2f);
        currentCredit.SetActive(false);

        creditsAnimator.SetTrigger("FadeOut_IC");
        yield return new WaitForSeconds(2f);

    }

    // Loads the next scene asynchronously until the credits finish
    IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return new WaitForSeconds(2);
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        var hasWaitLoad = true;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress == 0.9f)
            {
                if (hasWaitLoad)
                {
                    hasWaitLoad = false;
                    yield return WaitLoad(asyncLoad);
                }
            }

            yield return null;
        }
    }

    IEnumerator WaitLoad(AsyncOperation asyncLoad)
    {
        yield return new WaitForSeconds(19.6f);
        asyncLoad.allowSceneActivation = true;
    }

}
