using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchMeter : MonoBehaviour  
{
    private bool isTorchMeter = true;
    private float lerpSpeed = 2f;
    private float fillAmount = 1f; // Set this to zero if you want to mute the fire loop sfx during intro
    private float maxValue = 1f; // Do not set to zero
    private float currentValue = 1f; // Do not set to zero

    private Animator torchAnim;
    private AudioSource loopingFireSFX;
    private ParticleSystem torchFireParticle;
    private ParticleSystem.MainModule torchFireMain;

    private TextMeshProUGUI valueText;
    private Image mask;
    private Image content;
    private Image torchFlameIcon;
    private Image torchIcon;
    private Image bar;

    private Color32 fullBarColor = new Color32(255, 130, 188, 255);
    private Color32 lowBarColor = new Color32(254, 104, 174, 255);
    private Color32 fullAlpha = new Color32(255, 255, 255, 255);
    private Color32 zeroAlpha = new Color32(255, 255, 255, 0);

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;

    void Awake()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        playerScript = FindObjectOfType<TileMovementController>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RadialBar();
    }

    // Returns or sets the valeu of currentValue
    public float CurrentVal
    {
        // Returns the currentValue...
        get
        {
            return currentValue;
        }

        // Sets sets the fillAmount and valueText accordingly as well
        set
        {
            currentValue = Mathf.Clamp(value, 0, maxValue);
            valueText.text = currentValue.ToString();
            fillAmount = currentValue / maxValue;
        }
    }

    // Returns or sets the value of maxValue
    public float MaxVal
    {
        get
        {
            return maxValue;
        }
        set
        {
            maxValue = value;
        }
    }

    // Enables all of the torch meter's sprites
    public void TurnOnTorchMeter()
    {
        bar.enabled = true;
        mask.enabled = true;
        content.enabled = true;
        torchIcon.enabled = true;
        torchFlameIcon.enabled = true;
        valueText.color = fullAlpha;
    }

    // Disables all of the torch meter's sprites
    public void TurnOffTorchMeter()
    {
        bar.enabled = false;
        mask.enabled = false;
        content.enabled = false;
        torchIcon.enabled = false;
        torchFlameIcon.enabled = false;
        valueText.color = zeroAlpha;
    }

    // Enables/disables the torch meter's sprites
    public void isTorchMeterCheck()
    {
        isTorchMeter = !isTorchMeter;

        if (isTorchMeter)
            TurnOnTorchMeter();
        if (!isTorchMeter)
            TurnOffTorchMeter();
    }

    // Triggers the torch meter's "PopIn" animation
    public void TorchMeterPopIn()
    {
        torchAnim.SetTrigger("PopIn");
    }

    // Triggers the torch meter's "PopOut" animation
    public void TorchMeterPopOut()
    {
        torchAnim.SetTrigger("PopOut");
    }

    // Reset the torch meter's values and variables
    public void ResetTorchMeterElements()
    {
        content.fillAmount = 1f;
        content.color = fullBarColor;
        torchFlameIcon.color = fullAlpha;
        loopingFireSFX.volume = 0.9f;
        torchFireMain.startLifetime = 0.72f;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the images by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            Image image = child.GetComponent<Image>();

            if (child.name == "Mask")
            {
                mask = image;

                for (int j = 0; j < child.transform.childCount; j++)
                {
                    GameObject child02 = child.transform.GetChild(j).gameObject;
                    Image image02 = child02.GetComponent<Image>();

                    if (child02.name == "Content")
                        content = image02;

                }
            }
            if (child.name == "Icons")
            {
                for (int k = 0; k < child.transform.childCount; k++)
                {
                    GameObject child03 = child.transform.GetChild(k).gameObject;
                    Image image03 = child03.GetComponent<Image>();

                    if (child03.name == "TorchFlameIcon")
                        torchFlameIcon = image03;

                    if (child03.name == "TorchIcon")
                        torchIcon = image03;
                }
            }
            if (child.name == "ValueText")
                valueText = child.GetComponent<TextMeshProUGUI>();
        }

        for (int i = 0; i < playerScript.transform.childCount; i++)
        {
            GameObject child = playerScript.transform.GetChild(i).gameObject;

            if (child.name == "PlayerModel")
            {
                GameObject playerModel = child;

                for (int j = 0; j < playerModel.transform.childCount; j++)
                {
                    GameObject child02 = playerModel.transform.GetChild(j).gameObject;

                    if (child02.name == "Ko")
                        torchFireParticle = child02.GetComponentInChildren<ParticleSystem>();
                }
            }
        }

        bar = GetComponent<Image>();
        torchAnim = GetComponent<Animator>();
        torchFireMain = torchFireParticle.main;
        loopingFireSFX = audioManagerScript.loopingFireAS;
    }

    // Adjusts the torch meter's color, value, and volume
    private void RadialBar()
    {
        if (fillAmount != content.fillAmount)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);

            content.color = Color.Lerp(lowBarColor, fullBarColor, fillAmount);

            torchFlameIcon.color = Color.Lerp(zeroAlpha, fullAlpha, fillAmount);

            loopingFireSFX.volume = Mathf.Lerp(0f, 0.9f, fillAmount);

            torchFireMain.startLifetime = Mathf.Lerp(0f, 0.72f, fillAmount);
        }
    }

    // Checks to increase or decrease the torch meter's current value - For Debugging Purposes Only!
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket) && CurrentVal > 0) // [
            {
                CurrentVal--;
                Debug.Log("Debugging: Decreased Torch Meter To " + CurrentVal);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket) && CurrentVal < MaxVal) // ]
            {
                CurrentVal++;
                Debug.Log("Debugging: Increased Torch Meter To " + CurrentVal);
            }
        }
    }

}
