using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SafetyMenu : MonoBehaviour
{
    private bool isChangingMenus = false;
    private bool isSafetyMenu = false;

    private string mainMenu = "MainMenu"; // Name of main menu scene
    private string sceneName;

    private GameObject safetyMenu;
    private GameObject yesButton;
    private GameObject noButton;

    private GameObject previousMenuButton;
    private GameObject lastSelectedObject;

    private GameObject deathScreenHolder;
    private GameObject mainCanvas;

    private GameObject safetyMenuTextDS;
    private GameObject safetyMenuText;

    private GraphicRaycaster graphicsRaycaster;
    private Animator safetyMenuAnim;

    private BlackOverlay blackOverlayScript;
    private AudioManager audioManagerScript;
    private EventSystem eventSystemScript;
    private DeathScreen deathScreenScript;
    private PauseMenu pauseMenuScript;
    private HUD headsUpDisplayScript;
    private MainMenu mainMenuScript;

    public bool IsChangingMenus
    {
        get { return isChangingMenus; }
    }

    public bool IsSafetyMenu
    {
        get { return isSafetyMenu; }
    }

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    /***************************** Event functions START here *****************************/
    // Opens the safety menu
    public void OpenSafetyMenu() => StartCoroutine(OpenSafetyMenuDelay());

    // Closes the saftey menu after pressing the no button (NB)
    public void CloseSafetyMenuNB() => StartCoroutine(CloseSafetyMenuDelay());

    // Closes the saftey menu after pressing the yes button (YB)
    public void CloseSafetyMenuYB() => StartCoroutine(CloseSafetyMenuDelay(true));

    // Checks to set the yes button as the current selected game object
    public void SelectYesButton() => SetCurrentSelected(yesButton);

    // Checks to set the no button as the current selected game object
    public void SelectNoButton() => SetCurrentSelected(noButton);

    // Play the sfx for clicking a button
    public void PlayButtonClickSFX() => audioManagerScript.PlayMenuButtonClickSFX();

    // Checks to play the sfx for selecting a button
    public void PlayButtonSelectedSFX()
    {
        if (isChangingMenus || lastSelectedObject == eventSystemScript.currentSelectedGameObject) return;

        lastSelectedObject = eventSystemScript.currentSelectedGameObject;
        audioManagerScript.PlayMenuButtonSelectSFX();   
    }
    /***************************** Event functions END here *****************************/

    // Plays the pop out animation for the safety menu
    private void PopOutSafetyMenu() => safetyMenuAnim.SetTrigger("SM_PopOut");

    // Checks to set the current selected game object
    private void SetCurrentSelected(GameObject objectToSelect)
    {
        if (lastSelectedObject == objectToSelect) return;

        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(objectToSelect);
        lastSelectedObject = objectToSelect; // Call this last!
    }

    // Checks to pop out of the current menu
    private void PopOutOfCurrentMenu()
    {
        if (sceneName == mainMenu)
        {
            // Pop out of main menu here...
        }
        else if (deathScreenHolder.activeInHierarchy)
        {
            deathScreenScript.PopOutDeathScreen();
            audioManagerScript.PlayPopUpSFX();
            safetyMenuTextDS.SetActive(true);
            safetyMenuText.SetActive(false);
        }
        else
            pauseMenuScript.PopOutPauseMenu();
    }

    // Checks which menu to set active after closing the safety menu
    private void SetPreviousMenuActive()
    {
        if (blackOverlayScript.IsChangingScenes) return;

        if (sceneName != mainMenu && deathScreenHolder.activeInHierarchy)
        {
            safetyMenuText.SetActive(true);
            safetyMenuTextDS.SetActive(false);
            deathScreenScript.PopInDeathScreen();
        }
        else mainCanvas.SetActive(true);
    }

    // Plays the sequence for opening the safety menu
    private IEnumerator OpenSafetyMenuDelay()
    {
        previousMenuButton = eventSystemScript.currentSelectedGameObject;
        DisableInput_SM();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(0.15f);
        PopOutOfCurrentMenu();                          
        isSafetyMenu = true;

        // Note: waits for the current menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(0.15f);
        mainCanvas.SetActive(false);
        safetyMenu.SetActive(true);
        SetCurrentSelected(yesButton);

        // Note: waits for the safety menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(0.1f);
        EnableInput_SM();
    }

    // Plays the sequence for closing the safety menu
    // Note: the bool will always be false if the parameter is not set
    private IEnumerator CloseSafetyMenuDelay(bool isQuitting = false)
    {
        DisableInput_SM();
        if (isQuitting) blackOverlayScript.GameFadeOut();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(0.15f);
        PopOutSafetyMenu();
        isSafetyMenu = false;

        if (isQuitting) yield break;
        // Note: waits for the safety menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(0.15f);
        safetyMenu.SetActive(false);
        SetPreviousMenuActive();
        SetCurrentSelected(previousMenuButton);

        // Note: waits for the previous menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(0.1f);
        lastSelectedObject = null;
        EnableInput_SM();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        deathScreenScript = (sceneName != mainMenu) ? FindObjectOfType<DeathScreen>() : null;
        pauseMenuScript = (sceneName != mainMenu) ? FindObjectOfType<PauseMenu>() : null;
        headsUpDisplayScript = (sceneName != mainMenu) ? FindObjectOfType<HUD>() : null;
        mainMenuScript = (sceneName == mainMenu) ? FindObjectOfType<MainMenu>() : null;

        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        eventSystemScript = FindObjectOfType<EventSystem>();
    }

    // Sets the main UI canvas to use
    private void SetMainCanvas()
    {
        Transform menu = (sceneName != mainMenu) ? pauseMenuScript.transform : mainMenuScript.transform;

        foreach (Transform child in menu)
        {
            switch (child.name)
            {
                case "PauseMenu":
                    mainCanvas = child.gameObject;
                    break;
                case "MainMenu":
                    mainCanvas = child.gameObject;
                    break;
                default:
                    break;
            }
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
                case "SafetyMenu":
                    safetyMenuAnim = child.GetComponent<Animator>();
                    safetyMenu = safetyMenuAnim.gameObject;
                    break;
                case "SM_Text":
                    safetyMenuText = child.gameObject;
                    break;
                case "SM_DeathScreenText":
                    safetyMenuTextDS = child.gameObject;
                    break;
                case "SM_YesButton":
                    yesButton = child.gameObject;
                    break;
                case "SM_NoButton":
                    noButton = child.gameObject;
                    break;
                case "DS_Holder":
                    deathScreenHolder = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.name == "CharacterDialogue" || child.name == "NotificationBubbles") continue;
            if (child.name == "KeybindButtons" || child.name == "TorchMeter") continue;

            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetMainCanvas();
        SetVariables(transform);
        SetVariables(headsUpDisplayScript.transform);

        graphicsRaycaster = transform.parent.GetComponent<GraphicRaycaster>();
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
    }

    // Enables the input to interact with safety menu
    private void EnableInput_SM()
    {
        eventSystemScript.sendNavigationEvents = true;
        graphicsRaycaster.enabled = true;
        isChangingMenus = false;
    }

    // Disbales the input to interact with safety menu
    private void DisableInput_SM()
    {
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
        isChangingMenus = true;
    }

}
