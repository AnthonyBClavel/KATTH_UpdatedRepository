using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu01 : MonoBehaviour
{
    public string mainMenuScene;

    [Header("Pause Menu Elements")]
    public GameObject optionsScreen;
    public GameObject pauseMenu;
    public GameObject player;
    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton, mainMenuButton;
    public Animator pauseScreenAnim;

    private GameObject lastSelectedObject;
    private LevelFade levelFade;
    private EventSystem eventSystem;

    [Header("Bools")]
    public bool isOptionsMenu;
    public bool isChangingScenes;
    private bool isPaused;
    public bool canPause;

    [Header("Loading Screen Elements")]
    public GameObject loadingScreen, loadingIcon;
    public TextMeshProUGUI loadingText;

    // Start is called before the first frame update
    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        levelFade = FindObjectOfType<LevelFade>();
        isChangingScenes = false;
        canPause = true;
    }

    // Update is called once per frame
    void Update()
    {     
        lastSelectedObject = eventSystem.currentSelectedGameObject;

        // Open or close the pause menu with ESC
        if (Input.GetKeyDown(KeyCode.Escape) && !isOptionsMenu && !isChangingScenes)
        {
            if (isPaused)
            {
                Resume();
            }
            else if (!isPaused && canPause)
            {
                Pause();
            }
        }
        // You can close the options menu by pressing ESC
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
        
        levelFade.enableMenuInputs();
        // Clear selected object
        eventSystem.SetSelectedGameObject(null);
        // Set new selected object
        eventSystem.SetSelectedGameObject(pauseFirstButton);
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

    /*private SaveSlot makeSaveSlot()
    {
        string sceneName = player.GetComponent<TileMovementV2>().sceneName;

        Vector3 position = player.GetComponent<TileMovementV2>().checkpoint.transform.position;
        float x = position.x;
        float y = position.y;
        float z = position.z;
        float[] playerPosition = { x, y, z };

        string puzzleName = player.GetComponent<TileMovementV2>().puzzle.name;

        int currCameraIndex = player.GetComponent<TileMovementV2>().main_camera.GetComponent<CameraController>().currentIndex;

        return new SaveSlot(sceneName, playerPosition, puzzleName, currCameraIndex);
    }*/


    // On Pointer Enter functions start here
    public void SelectResumeButton()
    {
        if (lastSelectedObject != pauseFirstButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(pauseFirstButton);
        }

    }

    public void SelectOptionsButton()
    {
        if (lastSelectedObject != optionsClosedButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(optionsClosedButton);
        }

    }

    public void SelectMainMenuButton()
    {
        if (lastSelectedObject != mainMenuButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(mainMenuButton);
        }

    }
    // On Pointer Enter functions end here


    // Loads the next scene in the background while the loading screen plays
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

    // The Functions below delay the button input so that you can actually see the button press animations
    private IEnumerator ResumeDelay()
    {
        canPause = false;
        levelFade.disableMenuInputs();
        yield return new WaitForSecondsRealtime(0.15f);
        pauseScreenAnim.SetTrigger("PS_PopOut");
        Time.timeScale = 1f;
        optionsScreen.SetActive(false);
        isPaused = false;
        player.GetComponent<TileMovementV2>().enabled = true;
        yield return new WaitForSecondsRealtime(0.15f);
        pauseMenu.SetActive(false);
        canPause = true;
    }

    private IEnumerator OpenOptionsDelay()
    {
        levelFade.disableMenuInputs();
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        optionsScreen.SetActive(true);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsFirstButton);
    }

    private IEnumerator CloseOptionsDelay()
    {
        levelFade.enableMenuInputs();
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = false;
        optionsScreen.SetActive(false);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsClosedButton);
    }

    /*private IEnumerator QuitToMainDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;
        StartCoroutine("LoadMainAsync");
    }*/


}
