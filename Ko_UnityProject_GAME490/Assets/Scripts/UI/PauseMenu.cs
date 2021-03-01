﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public string mainMenuScene;

    [Header("Pause Menu Elements")]
    public GameObject optionsScreen;
    public GameObject safetyMenu;
    public GameObject pauseScreen;
    public GameObject pauseScreenBG;
    public GameObject player;
    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton, mainMenuButton, safetyFirstButton, safetySecondButton;
    public Animator pauseScreenAnim;
    public Animator pauseScreenBgAnim;
    public Animator optionsScreenAnim;
    public Animator safetyMenuAnim;

    private GameObject lastSelectedObject;  
    private EventSystem eventSystem;

    [Header("Bools")]
    public bool isOptionsMenu;
    public bool isChangingScenes;
    public bool canPause;
    public bool isSafetyMenu;
    public bool canPlayButtonSFX;
    public bool isPaused;

    [Header("Loading Screen Elements")]
    public GameObject loadingScreen, loadingIcon;
    public TextMeshProUGUI loadingText;

    // Start is called before the first frame update
    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        isChangingScenes = false;
        canPlayButtonSFX = true;
        //canPause = true;
        StartCoroutine("DelayPauseMenuInput");
    }

    // Update is called once per frame
    void Update()
    {     
        lastSelectedObject = eventSystem.currentSelectedGameObject;

        // Open or close the pause menu with ESC
        if (Input.GetKeyDown(KeyCode.Escape) && !isOptionsMenu && !isChangingScenes && !isSafetyMenu)
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
        // Close the options menu by pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape) && isOptionsMenu)
        {
            StartCoroutine("CloseOptionsDelay");
        }
        // Close safety menu by pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape) && isSafetyMenu)
        {
            StartCoroutine("CloseSafetyMenuDelay");
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
        pauseScreen.SetActive(false);
        isPaused = false;
        player.GetComponent<TileMovementController>().enabled = true;
    }

    public void Pause()
    {
        StopCoroutine("ResumeDelay");      
        Time.timeScale = 0f;
        isPaused = true;
        canPause = false;
        pauseScreen.SetActive(true);
        pauseScreenBG.SetActive(true);      
        player.GetComponent<TileMovementController>().enabled = false;

        EnableMenuInputPS();
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

    public void OpenSafetyMenu()
    {
        StartCoroutine("OpenSafetyMenuDelay");
    }

    // For closing the saftey menu when you press "No"
    public void CloseSafetyMenu()
    {
        StartCoroutine("CloseSafetyMenuDelay");
    }

    // For closing the saftey menu when you press "Yes"
    public void CloseSafetyMenu02()
    {
        StartCoroutine("CloseSafetyMenuDelay02");
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


    /*** On Pointer Enter functions start here ***/
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

    public void SelectYesButton()
    {
        if (lastSelectedObject != safetyFirstButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(safetyFirstButton);
        }

    }
    public void SelectNoButton()
    {
        if (lastSelectedObject != safetySecondButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(safetySecondButton);
        }

    }
    /*** On Pointer Enter functions end here ***/


    // Loads the next scene asynchronously while the loading screen is active
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

    // Delays the button input so you can actually see the button press animations
    private IEnumerator ResumeDelay()
    {
        DisableMenuInputsPS();

        yield return new WaitForSecondsRealtime(0.15f);      
        pauseScreenAnim.SetTrigger("PS_PopOut");
        pauseScreenBgAnim.SetTrigger("PS_FadeOut");     
        optionsScreen.SetActive(false);
        safetyMenu.SetActive(false);
        player.GetComponent<TileMovementController>().enabled = true;
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(0.15f);
        isPaused = false;
        canPause = true;
        pauseScreen.SetActive(false);
        pauseScreenBG.SetActive(false);        
    }

    private IEnumerator OpenOptionsDelay()
    {
        DisableMenuInputsPS();
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        pauseScreenAnim.SetTrigger("PS_PopOut"); // The pause menu is set inactive and the safety menu is set active at the end of the animation via anim event

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsFirstButton);
        EnableMenuInputPS();
    }

    private IEnumerator CloseOptionsDelay()
    {
        DisableMenuInputsPS();
        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = false;
        optionsScreenAnim.SetTrigger("OS_PopOut");

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsClosedButton);
        EnableMenuInputPS();
    }

    private IEnumerator OpenSafetyMenuDelay()
    {
        DisableMenuInputsPS();
        yield return new WaitForSecondsRealtime(0.15f);
        pauseScreenAnim.SetTrigger("PS_PopOut");  // The pause menu is set inactive and the safety menu is set active at the end of the animation via anim event
        isSafetyMenu = true;

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(safetyFirstButton);
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "no"
    private IEnumerator CloseSafetyMenuDelay()
    {
        DisableMenuInputsPS();
        yield return new WaitForSecondsRealtime(0.15f);
        safetyMenuAnim.SetTrigger("SM_PopOut"); // The safety menu is set inactive and the pause menu is set active at the end of the animation via anim event     
        isSafetyMenu = false;

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(mainMenuButton);
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "yes"
    private IEnumerator CloseSafetyMenuDelay02()
    {
        DisableMenuInputsPS();
        yield return new WaitForSecondsRealtime(0.15f);
        safetyMenuAnim.SetTrigger("SM_PopOut");
        isSafetyMenu = false;
    }

    private void EnableMenuInputPS()
    {
        canPlayButtonSFX = true;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        safetyMenu.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
        pauseScreen.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
        optionsScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    private void DisableMenuInputsPS()
    {
        canPlayButtonSFX = false;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        safetyMenu.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        pauseScreen.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        optionsScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    /*private IEnumerator QuitToMainDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;
        StartCoroutine("LoadMainAsync");
    }*/

    // Cant pause until the scene fully fades in (to avoid layering issues in UI)

    private IEnumerator DelayPauseMenuInput()
    {
        canPause = false;
        yield return new WaitForSeconds(2.0f);
        canPause = true;
    }

}