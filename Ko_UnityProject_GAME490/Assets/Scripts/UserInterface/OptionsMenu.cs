using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer theMixer;

    private string mainMenu = "MainMenu";
    private string sceneName;

    private int selectedResolution;
    private float popInDurationOM;

    private bool isChangingMenus = false;
    private bool isOptionsMenu = false;

    private GraphicRaycaster graphicsRaycaster;
    private Animator optionMenuAnim;
    private GameObject optionsMenu;
    private ResItem[] resolutions;

    private Toggle fullScreenToggle;
    private Toggle vSyncToggle;

    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sFXSlider;

    private TextMeshProUGUI masterVolumeText;
    private TextMeshProUGUI musicVolumeText;
    private TextMeshProUGUI sFXVolumeText;
    private TextMeshProUGUI resolutionText;

    private EventSystem eventSystemScript;
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;

    public bool IsChangingMenus
    {
        get { return isChangingMenus; }
    }

    public bool IsOptionsMenu
    {
        get { return isOptionsMenu; }
    }

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetVolumeSliders(); // Must be called in Start()!
    }

    /***************************** Event functions START here *****************************/
    // Closes the options menu
    public void CloseOptionsMenu() => StartCoroutine(CloseOptionsMenuDelay());

    // Opens the options menu
    public void OpenOptionsMenu() => StartCoroutine(OpenOptionsMenuDelay());

    // Apllies the changes made in the graphics settings
    public void ApplyGraphics()
    {
        ResolutionCheck();
        VsyncCheck();

        //Debug.Log("Applied Graphic Settings");
    }

    // Sets the resolution text to the previous resolution 
    public void PreviousResolution()
    {
        if (selectedResolution <= 0) return;

        SetResolutionText(--selectedResolution);
    }

    // Sets the resolution text to the next resolution 
    public void NextResolution()
    {
        if (selectedResolution >= resolutions.Length - 1) return;

        SetResolutionText(++selectedResolution);
    }

    // Sets the value and text for the master volume slider
    public void SetMasterVolume()
    {
        float masterSliderValue = masterSlider.value;

        theMixer.SetFloat("MasterVol", Mathf.Log10(masterSliderValue) * 22);
        masterVolumeText.text = $"{Mathf.RoundToInt(masterSliderValue * 100)}";

        PlayerPrefs.SetFloat("MasterVol", masterSliderValue);       
    }

    // Sets the value and text for the music volume slider
    public void SetMusicVolume()
    {
        float musicSliderValue = musicSlider.value;

        theMixer.SetFloat("MusicVol", Mathf.Log10(musicSliderValue) * 22);
        musicVolumeText.text = $"{Mathf.RoundToInt(musicSliderValue * 100)}";

        PlayerPrefs.SetFloat("MusicVol", musicSliderValue);
    }

    // Sets the value and text for the sfx volume slider
    public void SetSFXVolume()
    {
        float sFXSliderValue = sFXSlider.value;

        theMixer.SetFloat("SFXVol", Mathf.Log10(sFXSliderValue) * 22);
        sFXVolumeText.text = $"{Mathf.RoundToInt(sFXSliderValue * 100)}";

        PlayerPrefs.SetFloat("SFXVol", sFXSliderValue);
    }
    /***************************** Event functions END here *****************************/

    // Plays the pop out animation for the options menu
    private void PopOutOptionsMenu() => optionMenuAnim.SetTrigger("OM_PopOut");

    // Checks which menu to pop out before opening the options menu
    private void PopOutOfPreviousMenu()
    {
        if (sceneName == mainMenu)
            mainMenuScript.PopOutMainMenu();
        else
            pauseMenuScript.PopOutPauseMenu();
    }

    // Checks which menu to pop in after closing the options menu
    private void PopIntoPreviousMenu()
    {
        if (sceneName == mainMenu)
            mainMenuScript.PopInMainMenu();
        else 
            pauseMenuScript.PopInPauseMenu();
    }

    // Returns the pop in animation duration for the previous menu
    // Note: a menu's pop in animation is the same as its pop out - its pop in animation is played in reverse
    private float PreviousMenuPopInDuration()
    {
        return (sceneName == mainMenu) ? mainMenuScript.PopInDurationMM : pauseMenuScript.PopInDurationPM;
    }

    // Checks to set/update the resolution text
    private void SetResolutionText(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex > resolutions.Length - 1) return;

        string screenWidth = resolutions[resolutionIndex].horizontal.ToString();
        string screenHeight = resolutions[resolutionIndex].vertical.ToString();
        resolutionText.text = $"{screenWidth} x {screenHeight}";
    }

    // Checks to set/update the current resolution
    private void ResolutionCheck()
    {
        if (selectedResolution < 0 || selectedResolution > resolutions.Length - 1) return;

        int screenWidth = resolutions[selectedResolution].horizontal;
        int screenHeight = resolutions[selectedResolution].vertical;
        Screen.SetResolution(screenWidth, screenHeight, fullScreenToggle.isOn);
    }

    // Checks to set vsync on/off
    // Note: vsync on = 1, vsync off = 0
    private void VsyncCheck()
    {
        int vSyncCount = vSyncToggle.isOn ? 1 : 0;
        QualitySettings.vSyncCount = vSyncCount;
    }

    // Plays the sequence for opening the options menu
    private IEnumerator OpenOptionsMenuDelay()
    {
        DisableInput_OM();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(popInDurationOM);
        PopOutOfPreviousMenu();
        isOptionsMenu = true;

        // Note: waits for the previous menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(PreviousMenuPopInDuration());
        optionsMenu.SetActive(true);

        // Note: waits for the options menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(popInDurationOM);
        EnableInput_OM();
        eventSystemScript.sendNavigationEvents = false;
    }

    // Plays the sequence for closing the options menu
    private IEnumerator CloseOptionsMenuDelay()
    {
        DisableInput_OM();

        // Note: waits for the button to play its "clicked" animation
        yield return new WaitForSecondsRealtime(popInDurationOM);
        PopOutOptionsMenu();
        isOptionsMenu = false;

        // Note: waits for the options menu to play its "pop out" animation
        yield return new WaitForSecondsRealtime(popInDurationOM);
        optionsMenu.SetActive(false);
        PopIntoPreviousMenu();

        // Note: waits for the previous menu to play its "pop in" animation
        yield return new WaitForSecondsRealtime(PreviousMenuPopInDuration());
        EnableInput_OM();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        pauseMenuScript = (sceneName != mainMenu) ? FindObjectOfType<PauseMenu>() : null;
        mainMenuScript = (sceneName == mainMenu) ? FindObjectOfType<MainMenu>() : null;
        eventSystemScript = FindObjectOfType<EventSystem>();
    }

    // Sets the resolutions to use - (540 X 960), (720 x 1280), (1080 x 1920)
    private void SetResolutions()
    {
        resolutions = new ResItem[3];

        resolutions[0] = new ResItem { vertical = 540, horizontal = 960 };
        resolutions[1] = new ResItem { vertical = 720, horizontal = 1280 };
        resolutions[2] = new ResItem { vertical = 1080, horizontal = 1920 };

        SetInitialResolutionText();
    }

    // Sets the resolution text to the current/initial screen size
    private void SetInitialResolutionText()
    {
        bool foundResolution = false;

        // Checks if the current resolution is within the array of resolutions
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (Screen.width != resolutions[i].horizontal || Screen.height != resolutions[i].vertical) continue;

            SetResolutionText(i);
            selectedResolution = i;
            foundResolution = true;
            break;
        }

        // Sets the resolution text as the current resolution otherwise
        if (foundResolution) return;
        resolutionText.text = $"{Screen.width} x {Screen.height}";
    }

    // Sets the value for all volume sliders
    private void SetVolumeSliders()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol");

        if (PlayerPrefs.HasKey("MusicVol"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol");

        if (PlayerPrefs.HasKey("SFXVol"))
            sFXSlider.value = PlayerPrefs.GetFloat("SFXVol");
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "OptionsMenu":
                    optionMenuAnim = child.GetComponent<Animator>();
                    optionsMenu = optionMenuAnim.gameObject;
                    break;
                case "FS_Toggle":
                    fullScreenToggle = child.GetComponent<Toggle>();
                    break;
                case "VS_Toggle":
                    vSyncToggle = child.GetComponent<Toggle>();
                    break;
                case "RES_Text":
                    resolutionText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "MasterVolumeSlider":
                    masterSlider = child.GetComponent<Slider>();
                    break;
                case "MasterVolumeText":
                    masterVolumeText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "MusicVolumeSlider":
                    musicSlider = child.GetComponent<Slider>();
                    break;
                case "MusicVolumeText":
                    musicVolumeText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "SFXVolumeSlider":
                    sFXSlider = child.GetComponent<Slider>();
                    break;
                case "SFXVolumeText":
                    sFXVolumeText = child.GetComponent<TextMeshProUGUI>();
                    break;
                default:
                    break;
            }

            if (child.name.Contains("Button") || child.name.Contains("Slider") || child.name.Contains("Toggle")) continue;
            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        SetResolutions();

        // Checks to toggle the buttons on/off...
        vSyncToggle.isOn = QualitySettings.vSyncCount == 1;
        fullScreenToggle.isOn = Screen.fullScreen;

        popInDurationOM = optionMenuAnim.ReturnClipLength("OptionsMenuPopIn");
        graphicsRaycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
    }

    // Enables the input to interact with options menu
    private void EnableInput_OM()
    {
        eventSystemScript.sendNavigationEvents = true;
        graphicsRaycaster.enabled = true;
        isChangingMenus = false;
    }

    // Disbales the input to interact with options menu
    private void DisableInput_OM()
    {
        eventSystemScript.sendNavigationEvents = false;
        graphicsRaycaster.enabled = false;
        isChangingMenus = true;
    }

}
