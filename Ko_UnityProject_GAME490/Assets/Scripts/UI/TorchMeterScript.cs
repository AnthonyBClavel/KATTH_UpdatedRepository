using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchMeterScript : MonoBehaviour  
{
    private bool isTorchMeter = true;
    private float lerpSpeed = 2f;
    private float fillAmount;

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

    public float MaxValue { get; set; }

    public float Value
    {
        set
        {
            // Take everything behind the colon and place it as the tmp value...
            //string[] tmp = valueText.text.Split(':');   
            //valueText.text = tmp[0] + ": " + value;

            valueText.text = value.ToString();
            fillAmount = Map(value, MaxValue);
        }
    }

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

    // Sets the torch meter's max value
    public void setMaxValue(float value)
    {
        MaxValue = value;
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
                for (int h = 0; h < child.transform.childCount; h++)
                {
                    GameObject child03 = child.transform.GetChild(h).gameObject;
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

        bar = GetComponent<Image>();
        torchAnim = GetComponent<Animator>();
        torchFireParticle = playerScript.torchFireParticle;
        torchFireMain = torchFireParticle.main;
        loopingFireSFX = audioManagerScript.loopingFireSFX;
        playerScript.torchMeterMoves.Initialize();
    }

    private float Map(float value, float max)
    {
        return value / max;
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

}
