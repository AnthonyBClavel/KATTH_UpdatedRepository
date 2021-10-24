using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchMeter : MonoBehaviour  
{
    private bool isTorchMeter = true;

    [Range(1f, 10f)]
    public float lerpSpeed = 2f;
    [Range(0.01f, 1f)]
    public float torchIconSpeed = 0.08f;
    private int torchIconIndex;
    private float fillAmount = 1f; // Set this to zero if you want to mute the fire loop sfx during intro
    private float maxValue = 1f; // Do not set to zero
    private float currentValue = 1f; // Do not set to zero

    public Sprite[] torchIconSprites;
    private TextMeshProUGUI valueText;
    private Image mask;
    private Image content;
    private Image torchIcon;
    private Image otherTorchIcon;
    private Image bar;

    private GameObject torchMeter;
    private Animator torchAnim;
    private AudioSource loopingFireSFX;

    private Color32 fullBarColor = new Color32(255, 130, 188, 255);
    private Color32 lowBarColor = new Color32(254, 104, 174, 255);
    private Color32 fullAlpha = new Color32(255, 255, 255, 255);
    private Color32 zeroAlpha = new Color32(255, 255, 255, 0);

    private ParticleSystem torchFireParticle;
    private ParticleSystem.MainModule torchFireMain;
    private IEnumerator flameIconCoroutine;
    private IEnumerator lerpFillAmountCoroutine;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        torchIconIndex = 0;
        torchIcon.sprite = torchIconSprites[torchIconIndex];
        PlayTorchFlameIconAnim();
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
            StartTorchMeterCoroutine();
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
        otherTorchIcon.enabled = true;
        torchIcon.enabled = true;
        valueText.color = fullAlpha;
        PlayTorchFlameIconAnim();
    }

    // Disables all of the torch meter's sprites
    public void TurnOffTorchMeter()
    {
        bar.enabled = false;
        mask.enabled = false;
        content.enabled = false;
        otherTorchIcon.enabled = false;
        torchIcon.enabled = false;
        valueText.color = zeroAlpha;
    }

    // Enables/disables the torch meter's sprites
    public void IsTorchMeterCheck()
    {
        isTorchMeter = !isTorchMeter;

        if (isTorchMeter)
            TurnOnTorchMeter();
        else if (!isTorchMeter)
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

    // Resets the torch meter's values and variables
    public void ResetTorchMeterElements()
    {
        content.fillAmount = 1f;
        content.color = fullBarColor;
        torchIcon.color = fullAlpha;
        loopingFireSFX.volume = 0.9f;
        torchFireMain.startLifetime = 0.72f;
    }

    // Starts the coroutine for the torch meter
    private void StartTorchMeterCoroutine()
    {
        if (lerpFillAmountCoroutine != null)
            StopCoroutine(lerpFillAmountCoroutine);

        lerpFillAmountCoroutine = LerpFillAmount();
        StartCoroutine(lerpFillAmountCoroutine);
    }

    // Lerps the torch meter's fill amount - adjusts its color, value, and volume
    private IEnumerator LerpFillAmount()
    {
        // While the absolute value of the floats is greater than 0.0001f - until they are approximately equal to one another
        // Note: The content.fillAmount in lerp will always get closer to fillAmount, but never equal it, so the coroutine would endlessly play
        while (Mathf.Abs(fillAmount - content.fillAmount) > 0.001f)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
            content.color = Color.Lerp(lowBarColor, fullBarColor, fillAmount);
            torchIcon.color = Color.Lerp(zeroAlpha, fullAlpha, fillAmount);
            loopingFireSFX.volume = Mathf.Lerp(0f, 0.9f, fillAmount);
            torchFireMain.startLifetime = Mathf.Lerp(0f, 0.72f, fillAmount);
            yield return null;
        }
    }

    // Starts the corouitne to play the torch icon animation
    public void PlayTorchFlameIconAnim()
    {
        if (flameIconCoroutine != null)
            StopCoroutine(flameIconCoroutine);

        flameIconCoroutine = TorchIconAnimation();
        StartCoroutine(flameIconCoroutine);
    }

    // Sets the next/new sprite for the flame icon at the end of each time interval
    private IEnumerator TorchIconAnimation()
    {
        //spriteIndex = 0;
        //torchFlameIcon.sprite = flameIconSprites[spriteIndex];

        while (torchIcon.isActiveAndEnabled) 
        {
            yield return new WaitForSeconds(torchIconSpeed);
            torchIconIndex++;
            if (torchIconIndex > torchIconSprites.Length - 1 || torchIconIndex < 0)
                torchIconIndex = 0;
            torchIcon.sprite = torchIconSprites[torchIconIndex];
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the images by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "TorchMeter")
            {
                torchMeter = child;

                for (int j = 0; j < torchMeter.transform.childCount; j++)
                {
                    GameObject child02 = torchMeter.transform.GetChild(j).gameObject;

                    if (child02.name == "Mask")
                    {
                        mask = child02.GetComponent<Image>();

                        for (int k = 0; k < mask.transform.childCount; k++)
                        {
                            GameObject child03 = mask.transform.GetChild(k).gameObject;

                            if (child03.name == "Content")
                                content = child03.GetComponent<Image>();
                        }
                    }
                    if (child02.name == "Icons")
                    {
                        GameObject iconsHolder = child02;

                        for (int l = 0; l < iconsHolder.transform.childCount; l++)
                        {
                            GameObject child04 = iconsHolder.transform.GetChild(l).gameObject;

                            if (child04.name == "TorchFlameIcon")
                                torchIcon = child04.GetComponent<Image>();

                            if (child04.name == "TorchIcon")
                                otherTorchIcon = child04.GetComponent<Image>();
                        }
                    }
                    if (child02.name == "ValueText")
                        valueText = child02.GetComponent<TextMeshProUGUI>();
                }
            }
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

        bar = torchMeter.GetComponent<Image>();
        torchAnim = torchMeter.GetComponent<Animator>();
        torchFireMain = torchFireParticle.main;
        loopingFireSFX = audioManagerScript.loopingFireAS;
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
