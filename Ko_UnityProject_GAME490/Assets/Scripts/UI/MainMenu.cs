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
    public GameObject safetyMenu;
    public GameObject continueFirstButton, optionsFirstButton, optionsClosedButton, newGameButton, creditsButton, quitGameButton, safetyFirstButton, safetySecondButton;
    public GameObject loadingScreen, loadingIcon, pressEnterText, gameLogo;
    public GameObject PressEnterSFX;

    private GameObject lastSelectedObject;
    private EventSystem eventSystem;

    [Header("Loading Screen Elements")]
    public TextMeshProUGUI loadingText;
    public Slider loadingBar;
    public Sprite[] loadingScreenSprites;

    [Header("Animators")]
    public Animator titleScreenLogoAnim;
    public Animator pressEnterTextAnim;
    public Animator mainMenuButtonsAnim;
    public Animator optionsScreenAnim;
    public Animator safetyMenuAnim;
    public Animator lowPolySceneAnim;

    [Header("Bools")]
    public bool isOptionsMenu;
    public bool canPlayButtonSFX;
    public bool canFadeLogo;
    public bool isQuitingGame;
    public bool isSafetyMenu;
    private bool hasPressedEnter;
    private bool isChangingMenus;

    [Header("Audio")]
    public AudioClip buttonClickSFX;
    public AudioClip buttonSelectSFX;

    private EndCredits endCreditsScript;

    void Awake()
    {
        endCreditsScript = FindObjectOfType<EndCredits>();
        DetermineLevelToLoad();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("SetActiveDelay");
        canPlayButtonSFX = true;
        canFadeLogo = false;
        isChangingMenus = false;
        hasPressedEnter = false;
        eventSystem = FindObjectOfType<EventSystem>();

        /*if (!SaveManager.hasSaveFile())
        {
            continueFirstButton.SetActive(false);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        lastSelectedObject = eventSystem.currentSelectedGameObject;

        // If enter is already pressed once, you cannot call this function again
        if (!hasPressedEnter && pressEnterText.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OpenMainMenu();
                Instantiate(PressEnterSFX, transform.position, transform.rotation);
                hasPressedEnter = true;
            }
        }

        // Close the options menu by pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape) && isOptionsMenu && !isChangingMenus)
        {
            StartCoroutine("CloseOptionsDelay");
        }
        // Close safety menu by pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape) && isSafetyMenu && !isChangingMenus)
        {
            StartCoroutine("CloseSafetyMenuDelay");
        }
        
        /*** Debugging: To load the FifthMap via main menu ***/
        /*if(Input.GetKeyDown(KeyCode.P))
        {
            string fifthWorld = "FifthMap";
            PlayerPrefs.SetString("savedScene", fifthWorld);
        }*/
        /*** Debugging ends here ***/

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.Delete))
        {
            SaveManager.DeleteGame();
        }*/
        /*** End Debugging ***/
    }

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
    }

    public void NewGame()
    {
        CreateNewSaveFile();
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
        DisableMenuInputMM();
        mainMenuButtonsAnim.SetTrigger("MMB_PopOut");
        canFadeLogo = false;
        endCreditsScript.StartEndCreditsManually();
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

    public void PopOut_MMB()
    {
        canFadeLogo = false;
        mainMenuButtonsAnim.SetTrigger("MMB_PopOut");
    }


    /*** On Pointer Enter functions start here ***/
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

    // Loads the next level asynchronously while the loading screen is active
    public IEnumerator LoadLevelAsync()
    {
        DetermineLevelToLoad();
        loadingScreen.SetActive(true);
        ChangeLoadingScreenImg();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadingBar.value = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingText.text = "Press SPACE to Continue";
                loadingIcon.SetActive(false);

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    loadingScreen.GetComponent<Image>().color = Color.black;
                    loadingScreen.GetComponentInChildren<TipsManager>().gameObject.SetActive(false);
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

    public void CreateNewSaveFile()
    {
        Debug.Log("Updated Save File");

        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");

        PlayerPrefs.DeleteKey("b_x");
        PlayerPrefs.DeleteKey("b_y");
        PlayerPrefs.DeleteKey("b_z");

        PlayerPrefs.DeleteKey("pc_x");
        PlayerPrefs.DeleteKey("pc_y");
        PlayerPrefs.DeleteKey("pc_z");
        PlayerPrefs.DeleteKey("cameraIndex");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");
        //PlayerPrefs.DeleteKey("savedScene");

        string tutorialScene = "TutorialMap";
        PlayerPrefs.SetString("savedScene", tutorialScene);
    }

    // Selects the credits button - ONLY used after credits have ended
    public void SelectCreditsButtonAfterCredits()
    {
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(creditsButton);
    }

    // Checks which scene to load when your save file is deleted/null
    private void DetermineLevelToLoad()
    {
        if (string.IsNullOrWhiteSpace(levelToLoad) == true)
        {
            levelToLoad = "TutorialMap";
        }
        else if (string.IsNullOrWhiteSpace(levelToLoad) == false)
        {
            levelToLoad = PlayerPrefs.GetString("savedScene");
        }
    }

    // Delays the button input so you can actually see the button press animations
    private IEnumerator SetActiveDelay()
    {
        gameLogo.SetActive(true);
        yield return new WaitForSecondsRealtime(4f);
        pressEnterText.SetActive(true);
    }

    private IEnumerator OpenMainMenuDelay()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        pressEnterTextAnim.SetTrigger("TextExit");
        titleScreenLogoAnim.SetTrigger("MoveUp");

        yield return new WaitForSecondsRealtime(0.4f);
        FindObjectOfType<TitleScreenEffects>().SharpenBG();
        pressEnterText.SetActive(false);
        lowPolySceneAnim.SetTrigger("MoveDown");

        yield return new WaitForSecondsRealtime(2.4f);
        mainMenuButtons.SetActive(true);
        // Clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        // Set new selected object
        EventSystem.current.SetSelectedGameObject(continueFirstButton);

        yield return new WaitForSecondsRealtime(1.5f);
        EnableMenuInputMM();
        mainMenuButtonsAnim.speed = 2;
    }

    private IEnumerator StartGameDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        StartCoroutine("LoadLevelAsync");
    }

    private IEnumerator OpenOptionsDelay()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        canFadeLogo = false;
        mainMenuButtonsAnim.SetTrigger("MMB_PopOut"); // The main menu buttons are set inactive and the options menu is set active at the end of the animation via anim event

        yield return new WaitForSecondsRealtime(0.85f);
        optionsScreen.SetActive(true);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsFirstButton);
        EnableMenuInputMM();
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false; //
    }

    private IEnumerator CloseOptionsDelay()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = false;
        canFadeLogo = true;
        optionsScreenAnim.SetTrigger("OS_PopOut"); // The options screen is set inactive and the main menu is set active at the end of the animation via anim event

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsClosedButton);
        //EnableMenuInputMM();   
        EnableInputDelay();
    }

    private IEnumerator OpenSafetyMenuDelay()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        mainMenuButtonsAnim.SetTrigger("MMB_PopOut");  // The main menu is set inactive and the safety menu is set active at the end of the animation via anim event
        isSafetyMenu = true;
        canFadeLogo = false;

        yield return new WaitForSecondsRealtime(0.85f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(safetyFirstButton);
        EnableMenuInputMM();
    }

    // For closing the safety menu when you press "no"
    private IEnumerator CloseSafetyMenuDelay()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        safetyMenuAnim.SetTrigger("SM_PopOut"); // The safety menu is set inactive and the main menu is set active at the end of the animation via anim event     
        isSafetyMenu = false;
        canFadeLogo = true;
        isQuitingGame = false;

        yield return new WaitForSecondsRealtime(0.2f);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(quitGameButton);
        //EnableMenuInputMM();
        EnableInputDelay();
    }

    // For closing the safety menu when you press "yes"
    private IEnumerator CloseSafetyMenuDelay02()
    {
        DisableMenuInputMM();

        yield return new WaitForSecondsRealtime(0.15f);
        safetyMenuAnim.SetTrigger("SM_PopOut");
        isSafetyMenu = false;
        canFadeLogo = true;
        isQuitingGame = true;
    }

    private IEnumerator QuitGameDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Application.Quit();
        Debug.Log("Quit Successful");
    }

    private void EnableMenuInputMM()
    {
        canPlayButtonSFX = true;
        isChangingMenus = false;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        gameObject.GetComponent<GraphicRaycaster>().enabled = true;
        //mainMenuButtons.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //optionsScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //safetyMenu.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }

    public void DisableMenuInputMM()
    {
        canPlayButtonSFX = false;
        isChangingMenus = true;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        gameObject.GetComponent<GraphicRaycaster>().enabled = false;
        //mainMenuButtons.GetComponent<CanvasGroup>().blocksRaycasts = false;
        //optionsScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;
        //safetyMenu.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
    }

    // Enables the input after a delay - ONLY used after credits have ended
    public void EnableInputDelay()
    {
        StartCoroutine("DelayEnableInput");
    }

    // Enables the input after the specified time
    private IEnumerator DelayEnableInput()
    {
        yield return new WaitForSeconds(0.25f);
        EnableMenuInputMM();
    }

    // Sets a random image/sprite for the loading screen
    private void ChangeLoadingScreenImg()
    {
        if (loadingScreenSprites != null)
            SetRandomSprite(loadingScreenSprites[UnityEngine.Random.Range(0, loadingScreenSprites.Length)]);
    }

    // Gets a random sprite from its respective array
    private void SetRandomSprite(Sprite newLoadingScreenImg)
    {
        if (loadingScreen.GetComponent<Image>().sprite.name == newLoadingScreenImg.name)    
            return;
        else
            loadingScreen.GetComponent<Image>().sprite = newLoadingScreenImg;
    }


}
