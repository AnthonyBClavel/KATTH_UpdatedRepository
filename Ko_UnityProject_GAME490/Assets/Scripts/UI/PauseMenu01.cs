using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu01 : MonoBehaviour
{
    public GameObject optionsScreen;
    public GameObject pauseMenu;
    public GameObject player;

    public Animator pauseScreenAnim;

    public bool isOptionsMenu;

    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton;

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
        //open or close the pause menu with ESC
        if(Input.GetKeyDown(KeyCode.Escape) && !isOptionsMenu)
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
        //pressing escape in options menu just closes the option tab
        if (Input.GetKeyDown(KeyCode.Escape) && isOptionsMenu)
        {
            StartCoroutine("CloseOptionsDelay");
        }
    }

    public void Resume()
    {
        StartCoroutine("ResumeDelay");
    }
    public void ResumeImmediately()
    {
        Time.timeScale = 1f;
        optionsScreen.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
        player.GetComponent<TileMovementV2>().enabled = true;
    }

    public void Pause()
    {
        StopCoroutine("ResumeDelay");
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        isPaused = true;
        player.GetComponent<TileMovementV2>().enabled = false; 

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set new selected object
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }

    public void OpenOptions()
    {
        StartCoroutine("OpenOptionsDelay");
    }

    public void CloseOptions()
    {
        StartCoroutine("CloseOptionsDelay");
    }

    public void QuitToMain()
    {
        StartCoroutine("LoadMainAsync");
    }

    //loads the next scene in the background while the loading screen plays
    private IEnumerator LoadMainAsync()
    {
        //yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;

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

    //functions below delay the button input so that you can actually see the button press animation
    private IEnumerator ResumeDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        pauseScreenAnim.SetTrigger("PS_PopOut");
        Time.timeScale = 1f;
        optionsScreen.SetActive(false);
        isPaused = false;
        player.GetComponent<TileMovementV2>().enabled = true;
        yield return new WaitForSecondsRealtime(0.15f);
        pauseMenu.SetActive(false);
    }

    private IEnumerator OpenOptionsDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        optionsScreen.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    private IEnumerator CloseOptionsDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = false;
        optionsScreen.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsClosedButton);
    }

    /*private IEnumerator QuitToMainDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;
        StartCoroutine("LoadMainAsync");
    }*/
}
