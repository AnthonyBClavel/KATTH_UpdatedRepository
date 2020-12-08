using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public string levelToLoad;

    [Header("Game Objects")]
    public GameObject optionsScreen;
    public GameObject mainMenuButtons;
    public GameObject continueFirstButton, optionsFirstButton, optionsClosedButton, newGameButton, creditsButton, quitGameButton;
    public GameObject loadingScreen, loadingIcon, pressEnterText, gameLogo;

    private GameObject lastSelectedObject;
    private EventSystem eventSystem;

    [Header("Loading Screen Elements")]
    public TextMeshProUGUI loadingText;
    public Slider loadingBar;

    [Header("Animators")]
    public Animator titleScreenLogoAnim;
    public Animator pressEnterTextAnim;
    public Animator mainMenuButtonsAnim;
    public Animator lowPolySceneAnim;

    [Header("Bools")]
    public bool isOptionsMenu;
    private bool hasPressedEnter;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("setActiveDelay");

        eventSystem = FindObjectOfType<EventSystem>();

        /*if (!SaveManager.hasSaveFile())
        {
            continueFirstButton.SetActive(false);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //if enter is already pressed once, you cannot call this function again
        if (Input.GetKeyDown(KeyCode.Return) && !hasPressedEnter)
        {
            OpenMainMenu();
            hasPressedEnter = true;
        }

        // Debugging..
        /*if (Input.GetKeyDown(KeyCode.Delete))
        {
            SaveManager.DeleteGame();
        }*/
    }

    //opens the main menu canvas
    public void OpenMainMenu()
    {
        StartCoroutine("OpenMainMenuDelay");
    }

    public void ContinueGame()
    {
        /*SaveSlot save = SaveManager.LoadGame();
        if(save == null)
        {
            Debug.Log("No save to load");
            return;
        }
        if (save.getSceneName() != null && !string.IsNullOrEmpty(save.getSceneName()))
            levelToLoad = save.getSceneName();
        else
            SaveManager.DeleteGame();*/

        StartCoroutine("LoadLevelAsync");
        Debug.Log("Continue Successful");
    }

    public void NewGame()
    {
        //SaveManager.DeleteGame();

        StartCoroutine("LoadLevelAsync");
    }

    public void OpenOptions()
    {
        StartCoroutine("OpenOptionsDelay");
    }

    public void CloseOptions()
    {
        StartCoroutine("CloseOptionsDelay");
    }

    public void QuitGame()
    {
        StartCoroutine("QuitGameDelay");
    }

    public void PlayCredits()
    {
        Debug.Log("Credits have been played");
    }


    //On Pointer Enter functions start here
    public void SelectContinueButton()
    {
        if (lastSelectedObject != continueFirstButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(continueFirstButton);
        }

    }

    public void SelectNewGameButton()
    {
        if (lastSelectedObject != newGameButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(newGameButton);
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

    public void SelectCreditsButton()
    {
        if (lastSelectedObject != creditsButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(creditsButton);
        }

    }
    public void SelectQuitGameButton()
    {
        if (lastSelectedObject != quitGameButton)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(quitGameButton);
        }

    }
    //On Pointer Enter functions start here


    //loads the next level in the background while the loading screen plays
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

    //functions below delay the button input so that you can actually see the button press animation

    private IEnumerator setActiveDelay()
    {
        gameLogo.SetActive(true);
        hasPressedEnter = true;
        yield return new WaitForSeconds(4f);
        pressEnterText.SetActive(true);
        hasPressedEnter = false;
    }

    private IEnumerator OpenMainMenuDelay()
    {
        yield return new WaitForSeconds(0.15f);
        pressEnterTextAnim.SetTrigger("TextExit");
        titleScreenLogoAnim.SetTrigger("MoveUp");
        yield return new WaitForSeconds(0.4f);
        FindObjectOfType<TitleScreenEffects>().SharpenBG();
        pressEnterText.SetActive(false);
        lowPolySceneAnim.SetTrigger("MoveDown");
        yield return new WaitForSeconds(2.4f);
        mainMenuButtons.SetActive(true);

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set new selected object
        EventSystem.current.SetSelectedGameObject(continueFirstButton);
    }

    private IEnumerator NewGameDelay()
    {
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator OpenOptionsDelay()
    {
        yield return new WaitForSeconds(0.15f);
        optionsScreen.SetActive(true);
        isOptionsMenu = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    private IEnumerator CloseOptionsDelay()
    {
        yield return new WaitForSeconds(0.15f);
        optionsScreen.SetActive(false);
        isOptionsMenu = false;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsClosedButton);
    }
    private IEnumerator QuitGameDelay()
    {
        yield return new WaitForSeconds(0.15f);
        Application.Quit();
        Debug.Log("Quit Successful");
    }


}
