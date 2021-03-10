using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchMeterScript : MonoBehaviour  
{
    public Image content;
    public Image flameIcon;
    public TextMeshProUGUI valueText;

    public Color fullColor;
    public Color lowColor;
    public Color fullFlameColor;
    public Color lowFlameColor;
    public float lerpSpeed;

    public ParticleSystem fireParticle;

    private float fillAmount;
    private ParticleSystem.MainModule main;
    private AudioSource audioSource;
    private Animator torchAnim;

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

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        main = fireParticle.main;

        torchAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        RadialBar();

        /*** For Debugging purposes ***/
        /*if(Input.GetKeyDown(KeyCode.P))
        {
            TorchMeterPopIn();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            TorchMeterPopOut();
        }*/
        /*** End Debugging ***/
    }

    // Adjusts the torch meter bar based on color, value, and volume
    private void RadialBar()
    {
        if(fillAmount != content.fillAmount)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);

            content.color = Color.Lerp(lowColor, fullColor, fillAmount);

            flameIcon.color = Color.Lerp(lowFlameColor, fullFlameColor, fillAmount);

            audioSource.volume = Mathf.Lerp(0f, 0.9f, fillAmount);

            main.startLifetime = Mathf.Lerp(0f, 0.72f, fillAmount);
        }
    }

    private float Map(float value, float max)
    {
        return value / max;
    }

    public void setMaxValue(float value)
    {
        MaxValue = value;
    }

    public void TorchMeterPopIn()
    {
        torchAnim.SetTrigger("PopIn");
    }

    public void TorchMeterPopOut()
    {
        torchAnim.SetTrigger("PopOut");
    }

    // Reset the torch meter's fill amounts, volume, and particle effect
    public void ResetTorchMeterElements()
    {
        content.fillAmount = 1f;
        content.color = fullColor;
        flameIcon.color = fullFlameColor;
        audioSource.volume = 0.9f;
        main.startLifetime = 0.72f;
    }

}
