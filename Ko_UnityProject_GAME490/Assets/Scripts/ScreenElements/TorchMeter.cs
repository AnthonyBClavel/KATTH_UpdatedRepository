using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchMeter : MonoBehaviour  
{
    [SerializeField] [Range(1f, 10f)]
    private float lerpSpeed = 2f; // Original Value = 2f
    [SerializeField] [Range(0.01f, 1f)]
    private float flameIconSpeed = 0.08f; // Original Value = 0.8f

    private float currentValue = 1f;
    private float maxValue = 1f;
    private float fillAmount = 1f;

    private Image fillBackground;
    private Image fill;
    private Image bar;
    private Image flameIcon;
    private Image torchIcon;

    public Sprite[] flameIconSprites;
    private TextMeshProUGUI valueText;
    private Animator animator;

    private Color32 fullBarColor = new Color32(255, 130, 188, 255);
    private Color32 lowBarColor = new Color32(254, 104, 174, 255);
    private Color32 flameIconZA; // ZA = zero alpha
    private Color32 flameIconFA; // FA = full alpha

    private ParticleSystem.MainModule torchFireMain;
    private ParticleSystem torchFireParticle;

    private IEnumerator lerpFillAmountCoroutine;
    private IEnumerator flameIconCoroutine;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;

    public float MaxVal
    {
        get { return maxValue; }
        set { maxValue = value; }
    }

    public float CurrentVal
    {
        get { return currentValue; }
        set
        {
            currentValue = Mathf.Clamp(value, 0, maxValue);
            valueText.text = currentValue.ToString();
            fillAmount = currentValue / maxValue;
            StartLerpFillAmountCoroutine();
        }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Called every time the object is enabled
    void OnEnable()
    {
        StartFlameIconCoroutine();
    }

    // Enables all of the torch meter's sprites
    public void TurnOnTorchMeter()
    {
        fillBackground.enabled = true;
        fill.enabled = true;
        bar.enabled = true;

        flameIcon.enabled = true;
        torchIcon.enabled = true;
   
        valueText.SetTextAlpha(1f);
        StartFlameIconCoroutine();
    }

    // Disables all of the torch meter's sprites
    public void TurnOffTorchMeter()
    {
        fillBackground.enabled = false;
        fill.enabled = false;
        bar.enabled = false;

        flameIcon.enabled = false;
        torchIcon.enabled = false;

        valueText.SetTextAlpha(0f);
    }

    // Resets the torch meter's variables/values
    public void ResetTorchMeter()
    {
        audioManagerScript.ResetTorchFireVolume();
        torchFireMain.startLifetime = 0.72f;

        fill.color = fullBarColor;
        fill.fillAmount = 1f;
        CurrentVal = MaxVal;
       
        flameIcon.SetImageAlpha(1f);      
    }

    // Checks to enables/disable the torch meter's sprites
    public void ToggleTorchMeter()
    {
        if (bar.enabled) TurnOffTorchMeter();

        else TurnOnTorchMeter();
    }

    // Plays the pop out animation for the torch meter
    public void PopOutTorchMeter() => animator.Play("PopOut");

    // Plays the pop in animation for the torch meter
    public void PopInTorchMeter() => animator.Play("PopIn");

    // Starts the coroutine for the torch meter
    private void StartLerpFillAmountCoroutine()
    {
        if (lerpFillAmountCoroutine != null) StopCoroutine(lerpFillAmountCoroutine);

        lerpFillAmountCoroutine = LerpFillAmount();
        StartCoroutine(lerpFillAmountCoroutine);
    }

    // Starts the corouitne that plays the flame icon animation
    public void StartFlameIconCoroutine()
    {
        if (flameIconCoroutine != null) StopCoroutine(flameIconCoroutine);

        flameIconCoroutine = PlayFlameIconAnimation();
        StartCoroutine(flameIconCoroutine);
    }

    // Lerps the torch meter's fill amount to another = also lerps the related color, text alpha, and volume
    // Note: content.fillAmount will always lerp closer to fillAmount, but never equal it
    private IEnumerator LerpFillAmount()
    {
        float maxVol = audioManagerScript.TorchFireSFX.volume;

        while (Mathf.Abs(fillAmount - fill.fillAmount) > 0.001f)
        {          
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
            fill.color = Color.Lerp(lowBarColor, fullBarColor, fillAmount);
            flameIcon.color = Color.Lerp(flameIconZA, flameIconFA, fillAmount);

            audioManagerScript.SetTorchFireVolume(Mathf.Lerp(0f, maxVol, fillAmount));
            torchFireMain.startLifetime = Mathf.Lerp(0f, 0.72f, fillAmount);
            yield return null;
        }
    }

    // Plays the animation for the flame icon - updates the image with a new sprite after every time interval
    private IEnumerator PlayFlameIconAnimation()
    {
        while (flameIcon.enabled && flameIcon.gameObject.activeInHierarchy)
        {
            foreach(Sprite sprite in flameIconSprites)
            {
                yield return new WaitForSeconds(flameIconSpeed);
                flameIcon.sprite = sprite;
            }

            yield return null;
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "TM_Bar":
                    animator = child.GetComponent<Animator>();
                    bar = child.GetComponent<Image>();
                    break;
                case "TM_FillBG":
                    fillBackground = child.GetComponent<Image>();
                    break;
                case "TM_Fill":
                    fill = child.GetComponent<Image>();
                    break;
                case "TM_Flame":
                    flameIcon = child.GetComponent<Image>();
                    break;
                case "TM_Torch":
                    torchIcon = child.GetComponent<Image>();
                    break;
                case "TM_Text":
                    valueText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "TorchFire":
                    torchFireParticle = child.GetComponent<ParticleSystem>();
                    torchFireMain = torchFireParticle.main;
                    break;
                default:
                    break;
            }

            if (child.name == "Bip001") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        SetVariables(playerScript.transform);

        flameIconZA = flameIcon.ReturnImageColor(0f);
        flameIconFA = flameIcon.ReturnImageColor();
    }

    // Checks to increase/decrease the torch meter's current value - For Debugging Purposes ONLY
    public void DebuggingCheck()
    {
        if (CurrentVal < MaxVal && Input.GetKeyDown(KeyCode.RightBracket)) // Debug key is "]" (right bracket)
        {
            CurrentVal++;
            Debug.Log($"Debugging: increased torch meter to {CurrentVal}");
        }
        else if (CurrentVal > 0 && Input.GetKeyDown(KeyCode.LeftBracket)) // Debug key is "[" (left bracket)
        {
            CurrentVal--;
            Debug.Log($"Debugging: decreased torch meter to {CurrentVal}");
        }
    }

}
