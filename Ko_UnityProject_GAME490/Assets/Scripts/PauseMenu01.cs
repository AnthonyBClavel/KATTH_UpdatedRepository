using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PauseMenu01 : MonoBehaviour
{
    public GameObject optionsScreen;
    public GameObject pauseMenu;
    public GameObject player;

    public string mainMenuScene;

    private bool isPaused;

    public GameObject loadingScreen, loadingIcon;
    public Text loadingText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
        player.GetComponent<TileMovementV2>().enabled = true;
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        isPaused = true;
        player.GetComponent<TileMovementV2>().enabled = false;
        Time.timeScale = 0f;
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void QuitToMain()
    {
        //SceneManager.LoadScene(mainMenuScene);
        //Time.timeScale = 1f;

        StartCoroutine(LoadMainAsync());
        Time.timeScale = 1f;
    }

    private IEnumerator LoadMainAsync()
    {
        loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainMenuScene);

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingText.text = "Press Any Key To Continue";
                loadingIcon.SetActive(false);

                if (Input.anyKeyDown)
                {
                    loadingText.gameObject.SetActive(false);
                    loadingIcon.gameObject.SetActive(false);

                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
