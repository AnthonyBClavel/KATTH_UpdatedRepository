using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Bools")]
    public bool canPause;
    public bool isPaused;
    public bool isOptionsMenu;
    public bool isSafetyMenu;
    public bool isChangingMenus;
    public bool isChangingScenes;
    private bool canPlayButtonSFX;
    private bool hasPressedESC;
    private bool hasPlayedSelectedSFX;

    [Header("Pause Menu Elements")]
    private GameObject pauseMenuButtons;
    private GameObject safetyMenuButtons;
    private GameObject optionsMenu;
    private GameObject safetyMenu;
    private GameObject pauseMenu;
    private GameObject pauseMenuBG;
    private GameObject pauseFirstButton;
    private GameObject optionsClosedButton;
    private GameObject mainMenuButton;
    private GameObject safetyFirstButton;
    private GameObject safetySecondButton;
    private GameObject safetyMenuText;
    private GameObject safetyMenuDeathScreenText;
    //private GameObject optionsFirstButton;

    private Animator pauseMenuAnim;
    private Animator pauseMenuBGAnim;
    private Animator optionsMenuAnim;
    private Animator safetyMenuAnim;
    private Animator deathScreenAnim;

    private GameObject lastSelectedObject;
    private EventSystem eventSystem;
    private GameHUD gameHUDScript;
    private CharacterDialogue characterDialogueScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        isChangingScenes = false;
        isChangingMenus = false;
    }

    // Update is called once per frame
    void Update()
    {
        LastSelectedButtonCheck();
        PressingESC();
    }

    // Resumes the game
    public void Resume()
    {
        StartCoroutine("ResumeDelay");
    }

    // Pauses the game
    public void Pause()
    {   
        Time.timeScale = 0f;
        isPaused = true;
        hasPlayedSelectedSFX = false;
        pauseMenu.SetActive(true);
        pauseMenuBG.SetActive(true);
        playerScript.SetPlayerBoolsFalse();

        eventSystem.SetSelectedGameObject(null); // Clear last selected object then...
        eventSystem.SetSelectedGameObject(pauseFirstButton); // Set new selected object
        EnableMenuInputPS();
    }

    // Opens the options menu
    public void OpenOptions()
    {
        StartCoroutine("OpenOptionsDelay");
    }

    // Closes the options menu
    public void CloseOptions()
    {
        StartCoroutine("CloseOptionsDelay");
    }

    // Load the main menu scene
    /*public void QuitToMain()
    {
        gameManagerScript.LoadNextSceneCheck();
    }*/

    // Opens the safety menu
    public void OpenSafetyMenu()
    {
        if (gameHUDScript.isDeathScreen)
            StartCoroutine("OpenSafetyMenuDelay_DS");
        else
            StartCoroutine("OpenSafetyMenuDelay");
    }

    // Closes the saftey menu (after pressing "no" button)
    public void CloseSafetyMenu()
    {
        if (gameHUDScript.isDeathScreen)         
            StartCoroutine("CloseSafetyMenuDelay_DS");
        else
            StartCoroutine("CloseSafetyMenuDelay");
    }

    // Closes the saftey menu (after pressing "yes" button)
    public void CloseSafetyMenu02()
    {
        StartCoroutine("CloseSafetyMenuDelay02");
    }

    /*** "On Pointer Enter" functions start here ***/
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
    /*** "On Pointer Enter" functions end here ***/

    // Checks for the last selected button AND when to play the button selected sfx
    private void LastSelectedButtonCheck()
    {
        if (lastSelectedObject != eventSystem.currentSelectedGameObject)
        {
            if (canPlayButtonSFX && !isOptionsMenu)
            {
                audioManagerScript.PlayButtonSelectSFX();
                hasPlayedSelectedSFX = true;
            }
            lastSelectedObject = eventSystem.currentSelectedGameObject;
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
        if (Input.GetKeyDown(KeyCode.Escape) && !isChangingMenus)
        {
            // Opens/closes the pause menu (pause/resume game)
            if (!isOptionsMenu && !isSafetyMenu && !isChangingScenes)
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
                StartCoroutine("CloseOptionsDelay");
            }
            // Closes the safety menu
            if (isSafetyMenu)
            {
                hasPressedESC = true;
                CloseSafetyMenu();
            }
        }
    }

    // Delays the button input so you can actually see the button press animations
    private IEnumerator ResumeDelay()
    {
        DisableMenuInputsPS();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        pauseMenuAnim.SetTrigger("PS_PopOut");
        pauseMenuBGAnim.SetTrigger("PS_FadeOut");
        optionsMenu.SetActive(false);
        safetyMenu.SetActive(false);
        Time.timeScale = 1f;

        if (characterDialogueScript.canStartDialogue)  // When the player isn't interacting with an npc/artifact
            playerScript.SetPlayerBoolsTrue();

        yield return new WaitForSecondsRealtime(0.15f);
        isChangingMenus = false;
        isPaused = false;
        pauseMenu.SetActive(false);
        pauseMenuBG.SetActive(false);        
    }

    // For opening the options menu
    private IEnumerator OpenOptionsDelay()
    {
        DisableMenuInputsPS();
        audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isOptionsMenu = true;
        pauseMenuAnim.SetTrigger("PS_PopOut"); 
        
        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "PS_PopOut" animation
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);

        //eventSystem.SetSelectedGameObject(null);
        //eventSystem.SetSelectedGameObject(optionsFirstButton);
        yield return new WaitForSecondsRealtime(0.1f); // At the end of the option menu's pop in animation
        EnableMenuInputPS();
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
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

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(optionsClosedButton);
        yield return new WaitForSecondsRealtime(0.1f); // At the end of the pause menu's pop in animation   
        EnableMenuInputPS();
    }

    // For opening the saftey menu 
    private IEnumerator OpenSafetyMenuDelay()
    {
        DisableMenuInputsPS();
        audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = true;
        pauseMenuAnim.SetTrigger("PS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "PS_PopOut" animation
        pauseMenu.SetActive(false);
        safetyMenu.SetActive(true);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(safetyFirstButton);
        yield return new WaitForSecondsRealtime(0.1f); // At the end of the safety menu's pop in animation
        EnableMenuInputPS();
    }

    // For opening the saftey menu (while death screen is active)
    private IEnumerator OpenSafetyMenuDelay_DS()
    {
        DisableMenuInputsPS();
        playerScript.canRestartPuzzle = false;

        yield return new WaitForSecondsRealtime(0.15f);
        audioManagerScript.PlayPopUpSFX();
        isSafetyMenu = true;
        deathScreenAnim.SetTrigger("DS_PopOut");

        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "DS_PopOut" animation
        safetyMenu.SetActive(true);
        safetyMenuText.SetActive(false);
        safetyMenuDeathScreenText.SetActive(true);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(safetyFirstButton);
        yield return new WaitForSecondsRealtime(0.1f); // At the end of the safety menu's pop in animation
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "no"
    private IEnumerator CloseSafetyMenuDelay()
    {
        DisableMenuInputsPS();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");  
        
        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "SM_PopOut" animation
        safetyMenu.SetActive(false);
        if (!isChangingScenes)
            pauseMenu.SetActive(true);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(mainMenuButton);
        yield return new WaitForSecondsRealtime(0.1f); // At the end of the pause menu's pop in animation
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "no" (while death screen is active)
    private IEnumerator CloseSafetyMenuDelay_DS()
    {
        DisableMenuInputsPS();

        if (!hasPressedESC)
            audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");
                                                
        yield return new WaitForSecondsRealtime(0.15f); // At the end of the "SM_PopOut" animation
        safetyMenu.SetActive(false);
        if (!isChangingScenes)
        {
            safetyMenuDeathScreenText.SetActive(false);
            safetyMenuText.SetActive(true);
            deathScreenAnim.SetTrigger("DS_PopIn");
        }

        yield return new WaitForSecondsRealtime(0.1f); // At the end of the death screen's pop in animation
        playerScript.canRestartPuzzle = true;
        EnableMenuInputPS();
    }

    // For closing the safety menu when you press "yes"
    private IEnumerator CloseSafetyMenuDelay02()
    {
        DisableMenuInputsPS();
        transitionFadeScript.GameFadeOut();
        audioManagerScript.PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);
        isSafetyMenu = false;
        safetyMenuAnim.SetTrigger("SM_PopOut");        
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "PauseMenuHolder")
            {
                GameObject pauseMenuHolder = child;

                for (int j = 0; j < pauseMenuHolder.transform.childCount; j++)
                {
                    GameObject child02 = pauseMenuHolder.transform.GetChild(j).gameObject;

                    if (child02.name == "PauseMenuBG")
                        pauseMenuBG = child02;

                    if (child02.name == "PauseMenu")
                    {
                        pauseMenu = child02;

                        for (int k = 0; k < pauseMenu.transform.childCount; k++)
                        {
                            GameObject child03 = pauseMenu.transform.GetChild(k).gameObject;

                            if (child03.name == "ButtonHolder")
                                pauseMenuButtons = child03;
                        }
                    }                     
                }
            }

            if (child.name == "SafetyMenu")
            {
                safetyMenu = child;

                for (int h = 0; h < safetyMenu.transform.childCount; h++)
                {
                    GameObject child04 = safetyMenu.transform.GetChild(h).gameObject;

                    if (child04.name == "SafetyMenuAnim")
                    {
                        GameObject safteyMenuAnim = child04;

                        for (int g = 0; g < safteyMenuAnim.transform.childCount; g++)
                        {
                            GameObject child05 = safteyMenuAnim.transform.GetChild(g).gameObject;

                            if (child05.name == "SafetyMenuRegularText")
                                safetyMenuText = child05;
                            if (child05.name == "SafetyMenuDeathScreenText")
                                safetyMenuDeathScreenText = child05;
                            if (child05.name == "ButtonHolder")
                                safetyMenuButtons = child05;
                        }
                    }
                }
            }

            if (child.name == "OptionsMenuHolder")
            {
                GameObject optionsMenuHolder = child;

                for (int f = 0; f < optionsMenuHolder.transform.childCount; f++)
                {
                    GameObject child06 = optionsMenuHolder.transform.GetChild(f).gameObject;

                    if (child06.name == "OptionsMenu")
                        optionsMenu = child06;
                }
            }
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreenAnim = child.GetComponent<Animator>();
        }

        for (int i = 0; i < pauseMenuButtons.transform.childCount; i++)
        {
            GameObject child = pauseMenuButtons.transform.GetChild(i).gameObject;

            if (child.name == "ResumeButton")
                pauseFirstButton = child;
            if (child.name == "OptionsButton")
                optionsClosedButton = child;
            if (child.name == "MainMenuButton")
                mainMenuButton = child;
        }

        for (int i = 0; i < safetyMenuButtons.transform.childCount; i++)
        {
            GameObject child = safetyMenuButtons.transform.GetChild(i).gameObject;

            if (child.name == "YesButton")
                safetyFirstButton = child;
            if (child.name == "NoButton")
                safetySecondButton = child;
        }

        pauseMenuAnim = pauseMenu.GetComponent<Animator>();
        pauseMenuBGAnim = pauseMenuBG.GetComponent<Animator>();
        optionsMenuAnim = optionsMenu.GetComponent<Animator>();
        safetyMenuAnim = safetyMenu.GetComponent<Animator>();
    }

    private void EnableMenuInputPS()
    {
        canPlayButtonSFX = true;
        isChangingMenus = false;
        hasPressedESC = false;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        GetComponent<GraphicRaycaster>().enabled = true;
    }

    private void DisableMenuInputsPS()
    {
        canPlayButtonSFX = false;
        isChangingMenus = true;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        GetComponent<GraphicRaycaster>().enabled = false;
    }

}
