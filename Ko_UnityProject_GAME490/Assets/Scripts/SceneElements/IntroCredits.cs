using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCredits : MonoBehaviour
{
    [SerializeField] [Range(0.5f, 5)]
    private float lerpDuration = 2f; // Original Value = 2f
    [SerializeField] [Range(0.5f, 5)]
    private float displayNameDuration = 2f; // Original Value = 2f
    private float zeroAlpha = 0f;
    private float fullAlpha = 1f;

    private bool hasFinishIntroCredits = false;
    private string levelToLoad = "TestMap";
    private string sceneName;

    private Transform introCredits;
    private Image blackOverlay;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetElements();
        StartIntroCredits();
    }

    // Fades in the black overlay
    private void FadeInOverlay() => StartCoroutine(LerpOverlay(fullAlpha, zeroAlpha));

    // Fades out the black overlay
    private void FadeOutOverlay() => StartCoroutine(LerpOverlay(zeroAlpha, fullAlpha));

    // Checks to start the intro credits
    private void StartIntroCredits()
    {
        if (sceneName != "IntroCredits") return;

        // Note: syncs framerate to monitor's refresh rate
        QualitySettings.vSyncCount = 1;

        StartCoroutine(LoadNextLevelAsync());
        FadeInOverlay();
    }

    // Determines which methods to call based on the black overlay's alpha
    private void EndAlphaCheck()
    {
        if (blackOverlay.color.a == zeroAlpha)
            StartCoroutine(PlayIntroCredits());

        else if (blackOverlay.color.a == fullAlpha)
            hasFinishIntroCredits = true;
    }

    // Checks to activate the next loaded scene 
    private void ActivateNextScene(AsyncOperation asyncLoad)
    {
        if (asyncLoad.progress < 0.9f || asyncLoad.allowSceneActivation) return;

        CreateNewSaveFile();
        asyncLoad.allowSceneActivation = true;
    }

    // Loads the next scene asynchronously until the intro credits have finished
    private IEnumerator LoadNextLevelAsync()
    {
        yield return new WaitForSeconds(0.1f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (hasFinishIntroCredits)
            {
                //Debug.Log("The intro credits have finished!");
                ActivateNextScene(asyncLoad);
                yield break;
            }
            yield return null;
        }
    }

    // Lerps the alpha of the overlay to another over a duration (duration = seconds)
    private IEnumerator LerpOverlay(float startAlpha, float endAlpha)
    {
        Color startColor = blackOverlay.ReturnImageColor(startAlpha);
        Color endColor = blackOverlay.ReturnImageColor(endAlpha);
        blackOverlay.color = startColor;

        float duration = lerpDuration;
        float time = 0f;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = endColor;
        yield return new WaitForSeconds(0.5f);
        EndAlphaCheck();
    }

    // Plays the intro credits sequence
    private IEnumerator PlayIntroCredits()
    {
        foreach (Transform child in introCredits)
        {
            child.gameObject.SetActive(true);
            yield return new WaitForSeconds(displayNameDuration);
            child.gameObject.SetActive(false);
        }

        FadeOutOverlay();
    }

    // Deletes all of the appropriate player pref keys - creates a new save file
    private void CreateNewSaveFile()
    {
        PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
        PlayerPrefs.DeleteKey("listOfArtifacts");

        PlayerPrefs.DeleteKey("cameraIndex");
        PlayerPrefs.DeleteKey("savedScene");
        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();

        //Debug.Log("Created new save file!");
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "IC_Credits":
                    introCredits = child;
                    break;
                case "IC_BlackOverlay":
                    blackOverlay = child.GetComponent<Image>();
                    break;
                default:
                    break;
            }

            Debug.Log(child.name);
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
    }

}
