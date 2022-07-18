using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    private float finalAlpha = 0.78f; // Orignal Value = 0.78f
    private float zeroAlpha = 0f;
    private float popInDurationPM;

    private bool isChangingMenus = false;
    private bool isPaused = false;
    private bool canPause = true;

    private GameObject previousMenuButton;
    private GameObject lastSelectedObject;
    private GameObject pauseMenuBG;
    private GameObject pauseMenu;
    private GameObject optionsMenu;
    private GameObject safetyMenu;

    private GameObject resumeButton;
    private GameObject optionsButton;
    private GameObject quitButton;

    private Animator pauseMenuAnim;
    private Image background;

    private GraphicRaycaster graphicsRaycaster;
    private IEnumerator lerpBackgroundCoroutine;

    private CharacterDialogue characterDialogueScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private OptionsMenu optionsMenuScript;
    private EventSystem eventSystemScript;
    private SafetyMenu safetyMenuScript;

    public bool CanPause
    {
        get { return canPause; }
        set { canPause = value; }
    }

    public float PopInDurationPM
    {
        get { return popInDurationPM; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Update is called once per frame
    void Update()
    {
        PauseMenuInputCheck();
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

    // Checks to set the resume button as the current selected game object
    public void SelectResumeButton() => SetCurrentSelected(resumeButton);

    // Checks to set the options button as the current selected game object
    public void SelectOptionsButton() => SetCurrentSelected(optionsButton);

    // Checks to set the quit button as the current selected game object
    public void SelectQuitButton() => SetCurrentSelected(quitButton);

    // Opens the options menu
    public void OpenOptionsMenu() => optionsMenuScript.OpenOptionsMenu();

    // Opens the safety menu
    public void OpenSafetyMenu() => safetyMenuScript.OpenSafetyMenu();
    /***************************** Event functions END here *****************************/

    // Fades in the pause menu background
    private void FadeInBackground() => StartLerpBackgroundCoroutine(finalAlpha);

    // Fades out the pause menu background
    private void FadeOutBackground() => StartLerpBackgroundCoroutine(zeroAlpha);

    // Pops out the pause menu
    public void PopOutPauseMenu() => StartCoroutine(PopOutPauseMenuSequence());

    // Pops in the pause menu
    public void PopInPauseMenu() => StartCoroutine(PopInPauseMenuSequence());

    // Plays the sequence for unpausing the game
    private void Resume() => StartCoroutine(ResumeDelay());

    // Plays the sequence for pausing the game
    private void Pause() => StartCoroutine(PauseDelay());

    // Checks to set the main variables for the pause menu
    private void SetIsPaused(bool value)
    {
        isPaused = value;
        pauseMenu.SetActive(value);
        Time.timeScale = value ? 0f : 1f;
        ReselectLastSelected.instance.enabled = value;
        SetCurrentSelected(value ? resumeButton : null);
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

    // Checks for the pause menu input
    private void PauseMenuInputCheck()
    {
        if (blackOverlayScript.IsChangingScenes || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (optionsMenuScript.IsChangingMenus || safetyMenuScript.IsChangingMenus) return;

        CloseOptionMenuCheck();
        CloseSafetyMenuCheck();   
        PauseCheck();
    }

    // Checks to close the options menu
    private void CloseOptionMenuCheck()
    {
        if (!optionsMenuScript.IsOptionsMenu) return;

        optionsMenuScript.CloseOptionsMenu();
    }

    // Checks to close the safety menu
    private void CloseSafetyMenuCheck()
    {
        if (!safetyMenuScript.IsSafetyMenu) return;

        safetyMenuScript.CloseSafetyMenuNB();
    }

    // Checks to pause/unpause the game
    private void PauseCheck()
    {
        if (isChangingMenus || !canPause || playerScript.IsCrossingBridge) return;

        if (!isPaused) Pause();

        else Resume();
    }

    // Checks to set the player bools true - to enable the player input
    private void SetPlayerBoolTrueCheck()
    {
        if (playerScript.OnBridge() || playerScript.transform.position != playerScript.Destination) return;
        if (characterDialogueScript.InDialogue) return;

        playerScript.SetPlayerBoolsTrue();
    }

    // Starts the coroutine that lerps the alpha of the background
    private void StartLerpBackgroundCoroutine(float endAlpha)
    {
        if (lerpBackgroundCoroutine != null) StopCoroutine(lerpBackgroundCoroutine);

        lerpBackgroundCoroutine = LerpBackground(endAlpha);
        StartCoroutine(lerpBackgroundCoroutine);
    }

    // Plays the sequence for pausing the game
    private IEnumerator PauseDelay()
    {
        pauseMenuBG.SetActive(true);
        isChangingMenus = true;
        SetIsPaused(true);

        audioManagerScript.PauseAllAudio();
        playerScript.SetPlayerBoolsFalse();
        FadeInBackground();

        // Note: must call this method after pausing all audio!
        audioManagerScript.PlayMenuButtonSelectSFX();

        // Note: waits for the pause menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(popInDurationPM);
        EnableInput_PM();
    }

    // Plays the sequence for unpausing the game
    private IEnumerator ResumeDelay()
    {
        DisableInput_PM();
        safetyMenu.SetActive(false);
        optionsMenu.SetActive(false);

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(popInDurationPM);
        audioManagerScript.UnPauseAllAudio();
        SetPlayerBoolTrueCheck();
        FadeOutBackground();
        PopOutPauseMenu();

        // Note: waits for the pause menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(popInDurationPM);
        pauseMenuBG.SetActive(false);
        isChangingMenus = false;
        SetIsPaused(false);
    }

    // Plays the sequence for popping in the pause menu
    private IEnumerator PopInPauseMenuSequence()
    {
        pauseMenu.SetActive(true);
        SetCurrentSelected(previousMenuButton);

        yield return new WaitForSecondsRealtime(popInDurationPM);
        EnableInput_PM();
    }

    // Plays the sequence for popping out the pause menu
    private IEnumerator PopOutPauseMenuSequence()
    {
        DisableInput_PM();
        pauseMenuAnim.SetTrigger("PM_PopOut");

        yield return new WaitForSecondsRealtime(popInDurationPM);
        pauseMenu.SetActive(false);
        SetCurrentSelected(null);
    }

    // Lerps the alpha of the background to another over a duration (duration = seconds)
    private IEnumerator LerpBackground(float endAlpha)
    {
        Color endColor = background.ReturnImageColor(endAlpha);
        Color startColor = background.color;

        float duration = popInDurationPM;
        float time = 0;

        while (time < duration)
        {
            background.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        background.color = endColor;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        eventSystemScript = FindObjectOfType<EventSystem>();
        optionsMenuScript = FindObjectOfType<OptionsMenu>();
        safetyMenuScript = FindObjectOfType<SafetyMenu>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {            
                case "PauseMenu":
                    pauseMenuAnim = child.GetComponent<Animator>();
                    pauseMenu = child.gameObject;
                    pauseMenuAnim.keepAnimatorControllerStateOnDisable = false;
                    break;
                case "PM_Background":
                    background = child.GetComponent<Image>();
                    pauseMenuBG = child.gameObject;
                    break;
                case "PM_ResumeButton":
                    resumeButton = child.gameObject;
                    break;
                case "PM_OptionsButton":
                    optionsButton = child.gameObject;
                    break;
                case "PM_QuitButton":
                    quitButton = child.gameObject;
                    break;
                case "SafetyMenu":
                    safetyMenu = child.gameObject;
                    break;
                case "OptionsMenu":
                    optionsMenu = child.gameObject;
                    break;
                case "DS_Holder":
                    optionsMenu = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.parent.name == "OptionsMenu" || child.parent.name == "SafetyMenu") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        SetVariables(transform.parent);

        popInDurationPM = pauseMenuAnim.ReturnClipLength("PauseMenuPopIn");
        graphicsRaycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
    }

    // Enables the input to interact with pause menu
    private void EnableInput_PM()
    {
        eventSystemScript.sendNavigationEvents = true;
        graphicsRaycaster.enabled = true;
        isChangingMenus = false;
    }

    // Disables the input to interact with pause menu
    private void DisableInput_PM()
    {
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
        isChangingMenus = true;
    }

}
