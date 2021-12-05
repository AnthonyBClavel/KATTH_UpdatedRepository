using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    private float fadeLength = 0.15f; // 9/60 fps

    private bool canPause = true;
    private bool isPaused = false;
    private bool isOptionsMenu = false;
    private bool isSafetyMenu = false;
    private bool isChangingMenus = false;
    private bool canPlayButtonSFX = false;
    private bool hasPressedESC = true;
    private bool hasPlayedSelectedSFX = true;

    private GameObject deathScreen;
    private GameObject optionsMenu;
    private GameObject safetyMenu;
    private GameObject pauseMenu;
    private GameObject background;
    private GameObject pauseFirstButton;
    private GameObject optionsClosedButton;
    private GameObject quitToMainButton;
    private GameObject safetyFirstButton;
    private GameObject safetySecondButton;
    private GameObject safetyMenuText;
    private GameObject safetyMenuDeathScreenText;
    private GameObject lastSelectedObject;
    //private GameObject optionsFirstButton;

    private Animator pauseMenuAnim;
    private Animator optionsMenuAnim;
    private Animator safetyMenuAnim;
    private Animator deathScreenAnim;

    private Image pauseMenuBG;
    private Color finalAlpha = new Color(0, 0, 0, 0.78f);
    private Color zeroAlpha = new Color(0, 0, 0, 0);
    private GraphicRaycaster graphicsRaycaster;
    private IEnumerator fadeBackgroundCoroutine;

    private EventSystem eventSystemScript;
    private GameHUD gameHUDScript;
    private CharacterDialogue characterDialogueScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private OptionsMenu optionsMenuScript;
    private SafetyMenu safetyMenuScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        graphicsRaycaster.enabled = false;
        eventSystemScript.sendNavigationEvents = false;
    }

    // Update is called once per frame
    void Update()
    {
        LastSelectedButtonCheck();
        PressingESC();
    }

    // Returns the value of isChangingMenus
    public bool IsChangingMenus
    {
        get
        {
            return isChangingMenus;
        }
    }

    // Returns the value of isSafetyMenu
    public bool IsSafetyMenu
    {
        get
        {
            return isSafetyMenu;
        }
    }

    // Returns the value of isPaused
    public bool IsPaused
    {
        get
        {
            return isPaused;
        }
    }

    // Returns or sets the value of canPause
    public bool CanPause
    {
        get
        {
            return canPause;
        }
        set
        {
            canPause = value;
        }
    }

    // Resumes the game
    public void Resume()
    {
        StartCoroutine(ResumeDelay());
    }

    // Pauses the game
    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
        hasPlayedSelectedSFX = false;
        pauseMenu.SetActive(true);
        background.SetActive(true);
        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PauseAllAudio();
        LerpPauseMenuBG(finalAlpha, fadeLength);

        eventSystemScript.SetSelectedGameObject(null); // Clear last selected object then...
        eventSystemScript.SetSelectedGameObject(pauseFirstButton); // Set new selected object
        //EnableMenuInputPS(); // Now called at the end of LerpAlpha()
    }

    // Opens the options menu
    public void OpenOptionsMenu()
    {
        StartCoroutine(OpenOptionsDelay());
    }

    // Closes the options menu
    public void CloseOptionsMenu()
    {
        StartCoroutine(CloseOptionsDelay());
    }

    // Opens the safety menu
    public void OpenSafetyMenu()
    {
        if (deathScreen.activeSelf)
            StartCoroutine(OpenSafetyMenuDelay_DS());
        else
            StartCoroutine(OpenSafetyMenuDelay());
    }

    // Closes the saftey menu (after pressing "no" button)
    public void CloseSafetyMenu()
    {
        if (deathScreen.activeSelf)
            StartCoroutine(CloseSafetyMenuDelay_DS());
        else
            StartCoroutine(CloseSafetyMenuDelay());
    }

    // Closes the saftey menu (after pressing "yes" button)
    public void CloseSafetyMenu02()
    {
        StartCoroutine(CloseSafetyMenuDelay02());
    }

    /*** "On Pointer Enter" functions start here ***/
    public void SelectResumeButton()
    {
        if (lastSelectedObject != pauseFirstButton)
        {
            eventSystemScript.SetSelectedGameObject(null);
            eventSystemScript.SetSelectedGameObject(pauseFirstButton);
        }
    }
    public void SelectOptionsButton()
    {
        if (lastSelectedObject != optionsClosedButton)
        {
            eventSystemScript.SetSelectedGameObject(null);
            eventSystemScript.SetSelectedGameObject(optionsClosedButton);
        }
    }
    public void SelectQuitToMainButton()
    {
        if (lastSelectedObject != quitToMainButton)
        {
            eventSystemScript.SetSelectedGameObject(null);
            eventSystemScript.SetSelectedGameObject(quitToMainButton);
        }
    }
    public void SelectYesButton()
    {
        if (lastSelectedObject != safetyFirstButton)
        {
            eventSystemScript.SetSelectedGameObject(null);
            eventSystemScript.SetSelectedGameObject(safetyFirstButton);
        }
    }
    public void SelectNoButton()
    {
        if (lastSelectedObject != safetySecondButton)
        {
            eventSystemScript.SetSelectedGameObject(null);
            eventSystemScript.SetSelectedGameObject(safetySecondButton);
        }
    }
    /*** "On Pointer Enter" functions end here ***/

    // Checks for the last selected button AND when to play the button selected sfx
    private void LastSelectedButtonCheck()
    {
        if (eventSystemScript.currentSelectedGameObject != null && lastSelectedObject != eventSystemScript.currentSelectedGameObject)
        {
            if (canPlayButtonSFX && !isOptionsMenu)
            {
                audioManagerScript.PlayButtonSelectSFX();
                hasPlayedSelectedSFX = true;
            }
            lastSelectedObject = eventSystemScript.currentSelectedGameObject;
        }
        else if (lastSelectedObject == pauseFirstButton && !hasPlayedSelectedSFX)
        {
            audioManagerScript.PlayButtonSelectSFX();
            hasPlayedSelectedSFX = true;
        }
    }

    // Checks when to pause/resume game - also sets alternate method for closing options and saftey menu
    private void PressingESC()
    {
        // Pressing ESC...
        if (Input.GetKeyDown(KeyCode.Escape) && !transitionFadeScript.IsChangingScenes && !isChangingMenus)
        {
            // Opens/closes the pause menu (pause/resume game)
            if (!isOptionsMenu && !isSafetyMenu)
            {
                if (isPaused)
                {
                    hasPressedESC = true;
                    Resume();
                }
                else if (!isPaused && canPause)
                    Pause();
            }
            // Closes the options menu
            if (isOptionsMenu)
            {
                hasPressedESC = true;
                StartCoroutine(CloseOptionsDelay());
            }
            // Closes the safety menu
            if (isSafetyMenu)
            {
                hasPressedESC = true;
                CloseSafetyMenu();
            }
        }
    }

    // Starts the coroutine that fades the pause menu background
    private void LerpPauseMenuBG(Color endAlpha, float duration)
    {
        if (fadeBackgroundCoroutine != null)
            StopCoroutine(fadeBackgroundCoroutine);

        fadeBackgroundCoroutine = LerpAlpha(endAlpha, duration);
        StartCoroutine(fadeBackgroundCoroutine);
    }

    // Lerps the alpha of an image to another over a duration (endAlpha = alpha to lerp to, duration = seconds)
    private IEnumerator LerpAlpha(Color endAlpha, float duration)
    {
        DisableMenuInputsPS();
        float time = 0;
        Color startAlpha = pauseMenuBG.color;

        while (time < duration)
        {
            pauseMenuBG.color = Color.Lerp(startAlpha, endAlpha, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        pauseMenuBG.color = endAlpha;

        if (endAlpha == zeroAlpha)
        {
            isPaused = false;
            isChangingMenus = false;
            background.SetActive(false);
            pauseMenu.SetActive(false);
        }
        else if (endAlpha == finalAlpha)
            EnableMenuInputPS();
    }

    // Delays the button input so you can actually see the button press animations
    private IEnumerator ResumeDelay()
    {
        DisableMenuInputsPS();
        audioManagerScript.UnPauseAllAudio();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClick01SFX();

        yield return new WaitForSecondsRealtime(0.15f);
        LerpPauseMenuBG(zeroAlpha, fadeLength);
        pauseMenuAnim.SetTrigger("PS_PopOut");
        optionsMenu.SetActive(false);
        safetyMenu.SetActive(false);
        Time.timeScale = 1f;

        if (characterDialogueScript.canStartDialogue)  // When the player isn't interacting with an npc/artifact
            playerScript.SetPlayerBoolsTrue();

        // Lines below are now called at the end of LerpAlpha()
        /*yield return new WaitForSecondsRealtime(fadeLength);
        isChangingMenus = false;
        isPaused = false;
        pauseMenu.SetActive(false);*/
    }

    // For opening the options menu
    private IEnumerator OpenOptionsDelay()
    {
        DisableMenuInputsPS();
        audioManagerScript.PlayButtonClick01SFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        pauseMenuAnim.SetTrigger("PS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "PS_PopOut" animation
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
        //eventSystemScript.SetSelectedGameObject(null);
        //eventSystemScript.SetSelectedGameObject(optionsFirstButton);

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the option menu's pop in animation
        EnableMenuInputPS();
        eventSystemScript.sendNavigationEvents = false;
    }

    // For closing the options menu
    private IEnumerator CloseOptionsDelay()
    {
        DisableMenuInputsPS();

        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = false;
        optionsMenuAnim.SetTrigger("OS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "OS_PopOut" animation
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(optionsClosedButton);

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the pause menu's pop in animation   
        EnableMenuInputPS();
    }

    // For opening the saftey menu 
    private IEnumerator OpenSafetyMenuDelay()
    {
        DisableMenuInputsPS();
        audioManagerScript.PlayButtonClick01SFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = true;
        pauseMenuAnim.SetTrigger("PS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "PS_PopOut" animation
        pauseMenu.SetActive(false);
        safetyMenu.SetActive(true);
        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(safetyFirstButton);

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the safety menu's pop in animation
        EnableMenuInputPS();
    }

    // For opening the saftey menu (while death screen is active)
    private IEnumerator OpenSafetyMenuDelay_DS()
    {
        DisableMenuInputsPS();

        yield return new WaitForSecondsRealtime(0.15f);
        audioManagerScript.PlayPopUpSFX();
        isSafetyMenu = true;
        deathScreenAnim.SetTrigger("DS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "DS_PopOut" animation
        safetyMenu.SetActive(true);
        safetyMenuText.SetActive(false);
        safetyMenuDeathScreenText.SetActive(true);
        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(safetyFirstButton);

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the safety menu's pop in animation
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "no"
    private IEnumerator CloseSafetyMenuDelay()
    {
        DisableMenuInputsPS();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClick01SFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "SM_PopOut" animation
        safetyMenu.SetActive(false);
        if (!transitionFadeScript.IsChangingScenes)
            pauseMenu.SetActive(true);
        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(quitToMainButton);

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the pause menu's pop in animation
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "no" (while death screen is active)
    private IEnumerator CloseSafetyMenuDelay_DS()
    {
        DisableMenuInputsPS();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClick01SFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "SM_PopOut" animation
        safetyMenu.SetActive(false);
        if (!transitionFadeScript.IsChangingScenes)
        {
            safetyMenuDeathScreenText.SetActive(false);
            safetyMenuText.SetActive(true);
            deathScreenAnim.SetTrigger("DS_PopIn");
        }

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the death screen's pop in animation
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "yes"
    private IEnumerator CloseSafetyMenuDelay02()
    {
        DisableMenuInputsPS();
        audioManagerScript.PlayButtonClick01SFX();
        playerScript.SetExitZoneElements();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        eventSystemScript = FindObjectOfType<EventSystem>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        optionsMenuScript = FindObjectOfType<OptionsMenu>();
        safetyMenuScript = FindObjectOfType<SafetyMenu>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "PauseMenuBG")
            {
                background = child;
                pauseMenuBG = background.GetComponent<Image>();
            }

            if (child.name == "PauseMenu")
            {
                pauseMenu = child;

                for (int j = 0; j < pauseMenu.transform.childCount; j++)
                {
                    GameObject child02 = pauseMenu.transform.GetChild(j).gameObject;

                    if (child02.name == "ButtonLayout")
                    {
                        GameObject pauseMenuButtonLayout = child02;

                        for (int k = 0; k < pauseMenuButtonLayout.transform.childCount; k++)
                        {
                            GameObject child03 = pauseMenuButtonLayout.transform.GetChild(k).gameObject;

                            if (child03.name == "ResumeButton")
                                pauseFirstButton = child03;
                            if (child03.name == "OptionsButton")
                                optionsClosedButton = child03;
                            if (child03.name == "QuitToMainButton")
                                quitToMainButton = child03;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < safetyMenuScript.transform.childCount; i++)
        {
            GameObject child = safetyMenuScript.transform.GetChild(i).gameObject;

            if (child.name == "SafetyMenu")
            {
                safetyMenu = child;

                for (int j = 0; j < safetyMenu.transform.childCount; j++)
                {
                    GameObject child02 = safetyMenu.transform.GetChild(j).gameObject;

                    if (child02.name == "SafetyMenuRegularText")
                        safetyMenuText = child02;
                    if (child02.name == "SafetyMenuDeathScreenText")
                        safetyMenuDeathScreenText = child02;
                    if (child02.name == "ButtonLayout")
                    {
                        GameObject safetyMenuButtonLayout = child02;

                        for (int k = 0; k < safetyMenuButtonLayout.transform.childCount; k++)
                        {
                            GameObject child03 = safetyMenuButtonLayout.transform.GetChild(k).gameObject;

                            if (child03.name == "YesButton")
                                safetyFirstButton = child03;
                            if (child03.name == "NoButton")
                                safetySecondButton = child03;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < optionsMenuScript.transform.childCount; i++)
        {
            GameObject child = optionsMenuScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionsMenu")
                optionsMenu = child;
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreen = child;            
        }

        pauseMenuAnim = pauseMenu.GetComponent<Animator>();
        optionsMenuAnim = optionsMenu.GetComponent<Animator>();
        safetyMenuAnim = safetyMenu.GetComponent<Animator>();
        deathScreenAnim = deathScreen.GetComponent<Animator>();
        graphicsRaycaster = transform.parent.GetComponent<GraphicRaycaster>();
    }

    private void EnableMenuInputPS()
    {
        canPlayButtonSFX = true;
        isChangingMenus = false;
        hasPressedESC = false;
        graphicsRaycaster.enabled = true;
        eventSystemScript.sendNavigationEvents = true;
    }

    private void DisableMenuInputsPS()
    {
        canPlayButtonSFX = false;
        isChangingMenus = true;
        graphicsRaycaster.enabled = false;
        eventSystemScript.sendNavigationEvents = false;
    }

}
