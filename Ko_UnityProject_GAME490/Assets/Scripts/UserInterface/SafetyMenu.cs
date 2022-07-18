using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SafetyMenu : MonoBehaviour
{
    private string mainMenu = "MainMenu";
    private string sceneName;
    private float popInDurationSM;

    private bool isChangingMenus = false;
    private bool isSafetyMenu = false;

    private GameObject safetyMenu;
    private GameObject yesButton;
    private GameObject noButton;

    private GameObject lastSelectedObject;
    private GameObject deathScreenHolder;

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
    // Checks to play the sfx for selecting a button
    public void PlayButtonSelectedSFX()
    {
        if (isChangingMenus || lastSelectedObject == eventSystemScript.currentSelectedGameObject) return;

        lastSelectedObject = eventSystemScript.currentSelectedGameObject;
        audioManagerScript.PlayMenuButtonSelectSFX();
    }

    // Plays the sfx for clicking a button
    public void PlayButtonClickSFX() => audioManagerScript.PlayMenuButtonClickSFX();

    // Opens the safety menu
    public void OpenSafetyMenu() => StartCoroutine(OpenSafetyMenuDelay());

    // Closes the saftey menu after pressing the yes button (YB)
    public void CloseSafetyMenuYB() => StartCoroutine(CloseSafetyMenuDelay(true));

    // Closes the saftey menu after pressing the no button (NB)
    public void CloseSafetyMenuNB() => StartCoroutine(CloseSafetyMenuDelay());

    // Checks to set the yes button as the current selected game object
    public void SelectYesButton() => SetCurrentSelected(yesButton);

    // Checks to set the no button as the current selected game object
    public void SelectNoButton() => SetCurrentSelected(noButton);
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

    // Checks which menu to pop out before opening the safety menu
    private void PopOutOfPreviousMenu()
    {
        if (sceneName == mainMenu) 
            mainMenuScript.PopOutMainMenu();

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
    private void PopIntoPreviousMenu()
    {
        if (blackOverlayScript.IsChangingScenes) return;

        if (sceneName == mainMenu)
            mainMenuScript.PopInMainMenu();

        else if (deathScreenHolder.activeInHierarchy)
        {
            safetyMenuText.SetActive(true);
            safetyMenuTextDS.SetActive(false);
            deathScreenScript.PopInDeathScreen();
        }
        else
            pauseMenuScript.PopInPauseMenu();
    }

    // Returns the pop in animation duration for the previous menu
    // Note: a menu's pop in animation is the same as its pop out - its pop in animation is played in reverse
    private float PreviousMenuPopInDuration()
    {
        return (sceneName == mainMenu) ? mainMenuScript.PopInDurationMM : pauseMenuScript.PopInDurationPM;
    }

    // Plays the sequence for opening the safety menu
    private IEnumerator OpenSafetyMenuDelay()
    {
        DisableInput_SM();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(popInDurationSM);
        PopOutOfPreviousMenu();
        isSafetyMenu = true;

        // Note: waits for the previous menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(PreviousMenuPopInDuration());
        safetyMenu.SetActive(true);
        SetCurrentSelected(yesButton);

        // Note: waits for the safety menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(popInDurationSM);
        EnableInput_SM();
    }

    // Plays the sequence for closing the safety menu
    // Note: the bool will always be false if the parameter is not set
    private IEnumerator CloseSafetyMenuDelay(bool isQuitting = false)
    {
        DisableInput_SM();
        if (isQuitting) blackOverlayScript.GameFadeOut();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(popInDurationSM);
        PopOutSafetyMenu();
        isSafetyMenu = false;

        if (isQuitting) yield break;
        // Note: waits for the safety menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(popInDurationSM);
        safetyMenu.SetActive(false);
        PopIntoPreviousMenu();

        // Note: waits for the previous menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(PreviousMenuPopInDuration());
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
        SetVariables(transform);
        if (sceneName != mainMenu) SetVariables(headsUpDisplayScript.transform);

        popInDurationSM = safetyMenuAnim.ReturnClipLength("SafetyMenuPopIn");
        graphicsRaycaster = GetComponentInParent<GraphicRaycaster>();
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
