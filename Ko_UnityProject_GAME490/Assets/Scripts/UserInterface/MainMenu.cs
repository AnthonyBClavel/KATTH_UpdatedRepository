using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public Volume postProcessingVolume;
    private DepthOfField depthOfField;

    private float scaleFadeInDurationGL; // Original Value = 4f
    private float moveUpDurationGL; // Original Value = 4f
    private float popOutDurationPET; // Original Value = 0.6f (0.4f also)
    private float popInDurationMM; // Original Value = 1.66f
    private float buttonSpacing = 42.5f; // Original Value = 85f

    private bool hasPressedEnter = false;
    private bool isChangingMenus = false;

    private bool isContinueGame = false;
    private bool isNewGame = false;

    private GameObject previousMenuButton;
    private GameObject lastSelectedObject;
    private GameObject mainMenu;

    private GameObject firstButton;
    private GameObject continueButton;
    private GameObject newGameButton;
    private GameObject optionsButton;
    private GameObject creditsButton;
    private GameObject quitButton;

    private Animator pressEnterTextAnim;
    public Animator enviornmentAnim;
    private Animator mainMenuAnim;
    private Animator gameLogoAnim;

    private RectTransform mainMenuRT;
    private RectTransform gameLogoRT;

    private GraphicRaycaster graphicsRaycaster;
    private TextMeshProUGUI pressEnterText;
    private Image mainMenuBackground;
    private Image gameLogo;

    private BlackOverlay blackOverlayScript;
    private LevelManager levelManagerScript;
    private AudioManager audioManagerScript;
    private OptionsMenu optionsMenuScript;
    private SaveManager saveManagerScript;
    private EventSystem eventSystemScript;
    private EndCredits endCreditsScript;
    private SafetyMenu safetyMenuScript;

    public float PopInDurationMM
    {
        get { return popInDurationMM; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayMainMenuIntroCheck();
    }

    // Update is called once per frame
    void Update()
    {
        MainMenuInputCheck();
    }

    /***************************** Event functions START here *****************************/
    // Checks to play the sfx for selecting a button
    public void PlayButtonSelectedSFX()
    {
        if (isChangingMenus || lastSelectedObject == eventSystemScript.currentSelectedGameObject) return;

        lastSelectedObject = eventSystemScript.currentSelectedGameObject;
        audioManagerScript.PlayMenuButtonSelectSFX();
    }

    // Plays the sfx for clicking a button
    public void PlayButtonClickSFX() => audioManagerScript.PlayMenuButtonClickSFX();

    // Checks to set the continue button as the current selected game object
    public void SelectContinueButton() => SetCurrentSelected(continueButton);

    // Checks to set the new game button as the current selected game object
    public void SelectNewGameButton() => SetCurrentSelected(newGameButton);

    // Checks to set the options button as the current selected game object
    public void SelectOptionsButton() => SetCurrentSelected(optionsButton);

    // Checks to set the credits button as the current selected game object
    public void SelectCreditsButton() => SetCurrentSelected(creditsButton);

    // Checks to set the quit button as the current selected game object
    public void SelectQuitButton() => SetCurrentSelected(quitButton);

    // Opens the options menu
    public void OpenOptionsMenu() => optionsMenuScript.OpenOptionsMenu();

    // Opens the safety menu
    public void OpenSafetyMenu() => safetyMenuScript.OpenSafetyMenu();

    // Starts the sequence for loading a saved game
    public void ContinueGame()
    {
        isContinueGame = true;
        isNewGame = false;
        blackOverlayScript.GameFadeOut();
        PopOutMainMenu();
    }

    // Starts the sequence for loading a new game
    public void NewGame()
    {
        isNewGame = true;
        isContinueGame = false;
        blackOverlayScript.GameFadeOut();
        PopOutMainMenu();
    }

    // Plays the end credits
    public void PlayCredits()
    {
        endCreditsScript.StartEndCredits();
        blackOverlayScript.GameFadeOut();
        PopOutMainMenu();
    }
    /***************************** Event functions END here *****************************/

    // Plays the pop out animation for the main menu buttons
    private void PopOutMainMenuButtons() => mainMenuAnim.SetTrigger("MM_PopOut");

    // Fades out the game logo
    private void FadeOutGameLogo() => gameLogoAnim.SetTrigger("GL_FadeOut");

    // Fades in the game logo
    private void FadeInGameLogo() => gameLogoAnim.SetTrigger("GL_FadeIn");

    // Pops the main menu out of the scene
    public void PopOutMainMenu() => StartCoroutine(PopOutMainMenuSequence());

    // Pops the main menu into the scene
    public void PopInMainMenu() => StartCoroutine(PopInMainMenuSequence());

    // Removes the blur effect in the scene - sharpens the background
    private void SharpenBackground(float duration)
    {
        StartCoroutine(LerpBackgroundAlpha(0f, duration));
        StartCoroutine(LerpFocalLength(1f, duration));
    }

    // Checks to set the current selected game object
    private void SetCurrentSelected(GameObject objectToSelect)
    {
        previousMenuButton = eventSystemScript.currentSelectedGameObject;
        if (lastSelectedObject == objectToSelect) return;

        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(objectToSelect);
        lastSelectedObject = objectToSelect; // Call this last!
    }

    // Checks to load a saved game, load a new game, or quit the game
    public void LoadNextScene()
    {
        if (isContinueGame)
        {
            string savedScene = PlayerPrefs.GetString("savedScene");
            levelManagerScript.LoadNextScene(savedScene);
        }
        else if (isNewGame)
        {
            saveManagerScript.DeleteAllPlayerPrefs();
            levelManagerScript.LoadNextScene();
        }
        else if (!isContinueGame && !isNewGame)
        {
            //PlayerPrefs.DeleteKey("hasOpenedGame");
            StartCoroutine(QuitGameDelay());
        }
    }

    // Checks which main menu intro sequence to play
    // Note: the inital intro sequence will only play after you open or finish the game
    private void PlayMainMenuIntroCheck()
    {
        if (PlayerPrefs.GetInt("hasOpenedGame") == 1)
            StartCoroutine(AlternateIntroSequence());
        else
            StartCoroutine(InitialIntroSequence());
    }

    // Checks for the main menu input
    private void MainMenuInputCheck()
    {
        if (blackOverlayScript.IsChangingScenes || !Input.GetKeyDown(KeyCode.Escape)) return;

        CloseOptionMenuCheck();
        CloseSafetyMenuCheck();
    }

    // Checks to close the options menu
    private void CloseOptionMenuCheck()
    {
        if (optionsMenuScript.IsChangingMenus || !optionsMenuScript.IsOptionsMenu) return;

        optionsMenuScript.CloseOptionsMenu();
    }

    // Checks to close the safety menu
    private void CloseSafetyMenuCheck()
    {
        if (safetyMenuScript.IsChangingMenus || !safetyMenuScript.IsSafetyMenu) return;

        safetyMenuScript.CloseSafetyMenuNB();
    }

    // Checks for the input that pops out the press enter text
    private void PopOutTextCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        audioManagerScript.PlayLongWindGushSFX();
        audioManagerScript.PlayPressEnterSFX();
        audioManagerScript.PlayChimeSFX();
        hasPressedEnter = true;
    }

    // Plays the sequence for popping into the main menu
    private IEnumerator PopInMainMenuSequence()
    {
        FadeInGameLogo();
        mainMenu.SetActive(true);
        SetCurrentSelected(previousMenuButton);
        yield return new WaitForSecondsRealtime(popInDurationMM);
        EnableInput_MM();
    }

    // Plays the sequence for popping out of the main menu
    private IEnumerator PopOutMainMenuSequence()
    {
        DisableMenu_MM();
        FadeOutGameLogo();
        PopOutMainMenuButtons();
        yield return new WaitForSecondsRealtime(popInDurationMM);
        mainMenu.SetActive(false);
        SetCurrentSelected(null);
    }

    // Plays the main menu intro sequence - long version
    private IEnumerator InitialIntroSequence()
    {
        DisableMenu_MM();
        blackOverlayScript.GameFadeIn();
        gameLogo.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(scaleFadeInDurationGL);
        pressEnterText.gameObject.SetActive(true);
        while (!hasPressedEnter)
        {
            PopOutTextCheck();
            yield return null;
        }
        saveManagerScript.SetHasOpenedGame();

        yield return new WaitForSecondsRealtime(0.15f);
        pressEnterTextAnim.SetTrigger("PET_PopOut");
        gameLogoAnim.SetTrigger("GL_MoveUp");

        yield return new WaitForSecondsRealtime(popOutDurationPET);
        enviornmentAnim.SetTrigger("LPS_MoveDown");
        pressEnterText.gameObject.SetActive(false);
        SharpenBackground(moveUpDurationGL * 0.5f);

        yield return new WaitForSecondsRealtime(moveUpDurationGL - popInDurationMM);
        mainMenu.SetActive(true);
        SetCurrentSelected(firstButton);
        audioManagerScript.PlayMenuButtonSelectSFX();

        yield return new WaitForSecondsRealtime(popInDurationMM);
        mainMenuAnim.speed = 2;
        popInDurationMM /= 2f;
        EnableInput_MM();
    }

    // Plays the alternate main menu intro sequence - short version
    private IEnumerator AlternateIntroSequence()
    {
        DisableMenu_MM();
        blackOverlayScript.GameFadeIn();
        SharpenBackground(blackOverlayScript.GameFadeDuration * 0.5f);

        enviornmentAnim.transform.position -= new Vector3(0, 3, 0);
        gameLogoRT.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        gameLogoRT.anchoredPosition = new Vector2(0, 240);

        yield return new WaitForSecondsRealtime(blackOverlayScript.GameFadeDuration * 0.5f);
        gameLogo.gameObject.SetActive(true);
        mainMenu.SetActive(true);

        audioManagerScript.PlayMenuButtonSelectSFX();
        SetCurrentSelected(firstButton);
        FadeInGameLogo();

        yield return new WaitForSecondsRealtime(popInDurationMM);
        mainMenuAnim.speed = 2;
        popInDurationMM /= 2f;
        EnableInput_MM();
    }

    // Quits the game after a duration
    private IEnumerator QuitGameDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Application.Quit();
        //Debug.Log("You have quit the game!");
    }

    // Lerps the alpha of the background to another over a duration (duration = seconds)
    private IEnumerator LerpBackgroundAlpha(float endAlpha, float duration)
    {
        Color startColor = mainMenuBackground.color;
        Color endColor = mainMenuBackground.ReturnImageColor(endAlpha);
        float time = 0f;

        while (time < duration)
        {
            mainMenuBackground.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        mainMenuBackground.color = endColor;
    }

    // Lerps the value of the focal length to another over duration (duration = seconds)
    private IEnumerator LerpFocalLength(float endValue, float duration)
    {
        float startValue = depthOfField.focalLength.value;
        float time = 0f;

        while (time < duration)
        {
            depthOfField.focalLength.value = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        depthOfField.focalLength.value = endValue;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        levelManagerScript = FindObjectOfType<LevelManager>();
        optionsMenuScript = FindObjectOfType<OptionsMenu>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        eventSystemScript = FindObjectOfType<EventSystem>();
        endCreditsScript = FindObjectOfType<EndCredits>();
        safetyMenuScript = FindObjectOfType<SafetyMenu>();
    }

    // Sets the first button to select within the main menu
    private void SetFirstButton()
    {
        string savedScene = PlayerPrefs.GetString("savedScene");

        if (savedScene == string.Empty)
        {
            mainMenuRT.anchoredPosition += new Vector2(0, buttonSpacing);
            continueButton.SetActive(false);
            firstButton = newGameButton;
            popInDurationMM = 85f/60f;
        }
        else if (savedScene != string.Empty)
        {
            //popInDurationMM = mainMenuButtonsAnim.ReturnClipLength("MainMenuPopIn");
            firstButton = continueButton;
            popInDurationMM = 100f/60f;
        }
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "MainMenu":
                    mainMenuRT = child.GetComponent<RectTransform>();
                    mainMenuAnim = child.GetComponent<Animator>();
                    mainMenu = child.gameObject;
                    break;
                case "MM_Background":
                    mainMenuBackground = child.GetComponent<Image>();
                    break;
                case "MM_GameLogo":
                    gameLogoRT = child.GetComponent<RectTransform>();
                    gameLogoAnim = child.GetComponent<Animator>();
                    gameLogo = child.GetComponent<Image>();
                    break;
                case "MM_PressEnterText":
                    pressEnterText = child.GetComponent<TextMeshProUGUI>();
                    pressEnterTextAnim = child.GetComponent<Animator>();
                    break;
                case "ContinueButton":
                    continueButton = child.gameObject;
                    break;
                case "NewGameButton":
                    newGameButton = child.gameObject;
                    break;
                case "OptionsButton":
                    optionsButton = child.gameObject;
                    break;
                case "CreditsButton":
                    creditsButton = child.gameObject;
                    break;
                case "QuitButton":
                    quitButton = child.gameObject;
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        SetFirstButton();

        popOutDurationPET = pressEnterTextAnim.ReturnClipLength("PressEnterTextPopOut");
        scaleFadeInDurationGL = gameLogoAnim.ReturnClipLength("GameLogoScaleFadeIn");
        moveUpDurationGL = gameLogoAnim.ReturnClipLength("GameLogoMoveUp");

        mainMenuAnim.keepAnimatorControllerStateOnDisable = false;
        postProcessingVolume.profile.TryGet(out depthOfField);

        graphicsRaycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
    }

    // Enables the input to interact with main menu
    private void EnableInput_MM()
    {
        eventSystemScript.sendNavigationEvents = true;
        graphicsRaycaster.enabled = true;
        isChangingMenus = false;
    }

    // Disables the input to interact with main menu
    public void DisableMenu_MM()
    {
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
        isChangingMenus = true;
    }

}
