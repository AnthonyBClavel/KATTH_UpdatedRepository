using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCredits : MonoBehaviour
{
    [Range(0.5f, 5)]
    public float fadeDuration = 2f;
    [Range(0.5f, 5)]
    public float displayNameDuration = 2f;

    private string levelToLoad;
    private bool hasFinishIntroCredits = false;

    private GameObject introCreditsHolder;
    private Image fadeOverlay;
    private Color zeroAlpha = new Color(0, 0, 0, 0);
    private Color fullAlpha = new Color(0, 0, 0, 1);

    void Awake()
    {
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Syncs the framerate to the monitor's refresh rate
        QualitySettings.vSyncCount = 1;

        StartCoroutine(FadeAlpha(zeroAlpha, fadeDuration));
        StartCoroutine(LoadNextLevelAsync());
    }

    // Fades the alpha of the overlay to another over a specific duration (duration = seconds)
    private IEnumerator FadeAlpha(Color endValue, float duration)
    {
        float time = 0f;
        Color startValue = fadeOverlay.color;

        while (time < duration)
        {
            fadeOverlay.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        fadeOverlay.color = endValue;

        if (endValue == zeroAlpha)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PlayCredits());
        }
        else if (endValue == fullAlpha)
        {
            yield return new WaitForSeconds(0.5f);
            hasFinishIntroCredits = true;
        }          
    }

    // Plays the main intro credits sequence
    private IEnumerator PlayCredits()
    {
        //Debug.Log("Intro credits have started");

        for (int i = 0; i < introCreditsHolder.transform.childCount; i++)
        {
            GameObject child = introCreditsHolder.transform.GetChild(i).gameObject;

            child.SetActive(true);
            yield return new WaitForSeconds(2f);
            child.SetActive(false);
        }

        StartCoroutine(FadeAlpha(fullAlpha, fadeDuration));
    }

    // Loads the next scene asynchronously until the credits finish
    private IEnumerator LoadNextLevelAsync()
    {
        yield return new WaitForSeconds(0.1f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                if (hasFinishIntroCredits)
                {
                    //Debug.Log("Intro credits have finished");
                    CreateNewSaveFile();
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName = child.name;

            if (childName == "IntroCreditsHolder")
                introCreditsHolder = child;
            if (childName == "FadeOverlay")
                fadeOverlay = child.GetComponent<Image>();
        }

        levelToLoad = "FirstMap";
    }

    // Creates a new save file
    private void CreateNewSaveFile()
    {
        Debug.Log("Created New Save File");

        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.DeleteKey("cameraIndex");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");
        PlayerPrefs.DeleteKey("savedScene");

        PlayerPrefs.DeleteKey("listOfArtifacts");
        PlayerPrefs.DeleteKey("numberOfArtifactsCollected");

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

}
