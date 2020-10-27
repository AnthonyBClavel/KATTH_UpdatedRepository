using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string levelToLoad;

    public GameObject optionsScreen;

    public GameObject loadingScreen, loadingIcon, titleScreenCard;
    public Text loadingText;
    public Slider loadingBar;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            titleScreenCard.SetActive(false);
        }
    }

    public void ContinueGame()
    {
        Debug.Log("Continue Successful");
    }

    public void NewGame()
    {
        //SceneManager.LoadScene(firstlevel);
        StartCoroutine(LoadLevelAsync());
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false); ;
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Quit Successful");
    }

    public IEnumerator LoadLevelAsync()
    {
        loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadingBar.value = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingText.text = "Press Any Key To Continue";
                loadingIcon.SetActive(false);

                if (Input.anyKeyDown)
                {
                    loadingText.gameObject.SetActive(false);
                    loadingBar.gameObject.SetActive(false);
                    loadingIcon.gameObject.SetActive(false);
                    
                    asyncLoad.allowSceneActivation = true;
                    //Time.timeScale = 1f;
                }
            }

            yield return null;
        }
    }
}
