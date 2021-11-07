using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer theMixer;

    public ResItem[] resolutions; // Resolutions: (540 X 960), (720 x 1280), (1080 x 1920)
    public ResItem[] resolutions2; // Resolutions: (540 X 960), (720 x 1280), (1080 x 1920)
    private int selectedResolution;

    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sFXSlider;

    private TextMeshProUGUI masterSliderValText;
    private TextMeshProUGUI musicSliderValText;
    private TextMeshProUGUI sFXSliderValText;
    private TextMeshProUGUI currentResolutionText;

    private GameObject masterVolumeHolder;
    private GameObject musicVolumeHolder;
    private GameObject sFXVolumeHolder;
    private GameObject fullScreenHolder;
    private GameObject vSyncHolder;
    private GameObject resolutionHolder;

    private Toggle fullScreenToggle;
    private Toggle vSyncToggle;

    private PauseMenu pauseMenuScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        fullScreenToggle.isOn = Screen.fullScreen;

        SetVSyncCheck();
        SetResolutionCheck();
        SetVolumeSliders();
        SetResolutionsArray();
    }

    // Closes the options menu
    public void CloseOptionsMenu()
    {
        pauseMenuScript.CloseOptions();
    }

    // Selects the next smallest resolution - for pressing the left arrow on the resolution label
    public void ResLeft()
    {
        selectedResolution--;

        if (selectedResolution < 0)
            selectedResolution = 0;

        UpdateCurrentResolutionLabel();
    }

    // Selects the next biggest resolution - for pressing the right arrow on the resolution label
    public void ResRight()
    {
        selectedResolution++;

        if (selectedResolution > resolutions.Length - 1) 
            selectedResolution = resolutions.Length - 1;

        UpdateCurrentResolutionLabel();
    }

    // Apllies the changes made in the graphics settings
    public void ApplyGraphics()
    {
        if (vSyncToggle.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;

        // Sets the resolution
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullScreenToggle.isOn);
        Debug.Log("Applied Graphic Settings");
    }

    // Sets the volume for the master slider - for the OnValueChanged event in the slider's component
    public void SetMasterVol()
    {
        float masterSliderValue = masterSlider.value;

        theMixer.SetFloat("MasterVol", Mathf.Log10(masterSliderValue) * 22);
        PlayerPrefs.SetFloat("MasterVol", masterSliderValue);

        // Updates the slider's value text
        masterSliderValText.text = Mathf.RoundToInt(masterSliderValue * 100) + "";
    }

    // Sets the volume for the music slider - for the OnValueChanged event in the slider's component
    public void SetMusicVol()
    {
        float musicSliderValue = musicSlider.value;

        theMixer.SetFloat("MusicVol", Mathf.Log10(musicSliderValue) * 22);
        PlayerPrefs.SetFloat("MusicVol", musicSliderValue);

        // Updates the slider's value text
        musicSliderValText.text = Mathf.RoundToInt(musicSliderValue * 100) + "";
    }

    // Sets the volume for the sfx slider - for the OnValueChanged event in the slider's component
    public void SetSFXVol()
    {
        float sFXSliderValue = sFXSlider.value;

        theMixer.SetFloat("SFXVol", Mathf.Log10(sFXSliderValue) * 22);
        PlayerPrefs.SetFloat("SFXVol", sFXSliderValue);

        // Updates the slider's value text
        sFXSliderValText.text = Mathf.RoundToInt(sFXSliderValue * 100) + "";
    }

    // Sets the last saved volume for all sliders
    private void SetVolumeSliders()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol");

        if (PlayerPrefs.HasKey("MusicVol"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol");

        if (PlayerPrefs.HasKey("SFXVol"))
            sFXSlider.value = PlayerPrefs.GetFloat("SFXVol");
    }

    // Updates the current resolution text
    private void UpdateCurrentResolutionLabel()
    {
        currentResolutionText.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
    }

    // Checks if vsync is on/off
    private void SetVSyncCheck()
    {
        if (QualitySettings.vSyncCount == 0)
            vSyncToggle.isOn = false;
        else
            vSyncToggle.isOn = true;
    }

    // Searches for the current resolution in the list and sets it - sets the current unlisted resolution otherwise
    private void SetResolutionCheck()
    {
        bool foundRes = false;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                foundRes = true;
                selectedResolution = i;
                UpdateCurrentResolutionLabel();
            }
        }

        if (!foundRes)
            currentResolutionText.text = Screen.width.ToString() + " x " + Screen.height.ToString();
    }


    private void SetResolutionsArray()
    {
        //resolutions2 = new ResItem[3];
        resolutions2[0].vertical = 100;
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "OptionsMenu")
            {
                GameObject optionsMenu = child;

                for (int j = 0; j < optionsMenu.transform.childCount; j++)
                {
                    GameObject child02 = optionsMenu.transform.GetChild(j).gameObject;

                    if (child02.name == "AudioLayout")
                    {
                        GameObject audioLayout = child02;

                        for (int k = 0; k < audioLayout.transform.childCount; k++)
                        {
                            GameObject child03 = audioLayout.transform.GetChild(k).gameObject;

                            if (child03.name == "MasterVolumeHolder")
                                masterVolumeHolder = child03;
                            if (child03.name == "MusicVolumeHolder")
                                musicVolumeHolder = child03;
                            if (child03.name == "SFXVolumeHolder")
                                sFXVolumeHolder = child03;
                        }
                    }

                    if (child02.name == "GraphicsLayout")
                    {
                        GameObject graphicsLayout = child02;

                        for (int l = 0; l < graphicsLayout.transform.childCount; l++)
                        {
                            GameObject child04 = graphicsLayout.transform.GetChild(l).gameObject;

                            if (child04.name == "FullScreenHolder")
                                fullScreenHolder = child04;
                            if (child04.name == "VSyncHolder")
                                vSyncHolder = child04;
                            if (child04.name == "ResolutionHolder")
                                resolutionHolder = child04;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < masterVolumeHolder.transform.childCount; i++)
        {
            GameObject child = masterVolumeHolder.transform.GetChild(i).gameObject;

            if (child.name == "SliderValueText")
                masterSliderValText = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "MasterSlider")
                masterSlider = child.GetComponent<Slider>();
        }

        for (int i = 0; i < musicVolumeHolder.transform.childCount; i++)
        {
            GameObject child = musicVolumeHolder.transform.GetChild(i).gameObject;

            if (child.name == "SliderValueText")
                musicSliderValText = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "MusicSlider")
                musicSlider = child.GetComponent<Slider>();
        }

        for (int i = 0; i < sFXVolumeHolder.transform.childCount; i++)
        {
            GameObject child = sFXVolumeHolder.transform.GetChild(i).gameObject;

            if (child.name == "SliderValueText")
                sFXSliderValText = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "SFXSlider")
                sFXSlider = child.GetComponent<Slider>();
        }

        for (int i = 0; i < fullScreenHolder.transform.childCount; i++)
        {
            GameObject child = fullScreenHolder.transform.GetChild(i).gameObject;

            if (child.name == "FullScreenToggle")
                fullScreenToggle = child.GetComponent<Toggle>();
        }

        for (int i = 0; i < vSyncHolder.transform.childCount; i++)
        {
            GameObject child = vSyncHolder.transform.GetChild(i).gameObject;

            if (child.name == "VSyncToggle")
                vSyncToggle = child.GetComponent<Toggle>();
        }

        for (int i = 0; i < resolutionHolder.transform.childCount; i++)
        {
            GameObject child = resolutionHolder.transform.GetChild(i).gameObject;

            if (child.name == "CurrentResolution")
            {
                GameObject currentResolution = child;

                for (int j = 0; j < currentResolution.transform.childCount; j++)
                {
                    GameObject child02 = currentResolution.transform.GetChild(j).gameObject;

                    if (child02.name == "CurrentResText")
                        currentResolutionText = child02.GetComponent<TextMeshProUGUI>();
                }
            }
        }
    }

}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}