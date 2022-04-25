using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SkipSceneButton : MonoBehaviour
{
    private bool hasSkippedScene = false;
    private bool canRecieveInput = true;

    [Range(1f, 5f)]
    public float lerpLength = 3f; // 3f = Original Value
    [Range(1f, 20f)]
    public float lerpSpeed = 10f; // 10f = Original Value
    [Range(0.1f, 2f)]
    public float textFadeLength = 0.75f; // 0.75f = Original Value

    private GameObject skipSceneButton;
    private Image content;
    private Image skipSceneBar;
    private Animator skipButtonAnimator;
    private TextMeshProUGUI skipSceneText;

    private KeyCode skipSceneKeyCode = KeyCode.X;
    private IEnumerator textAlphaCoroutine;
    private IEnumerator resetFillAmountCoroutine;
    private IEnumerator lerpFillAmountCoroutine;
    private IEnumerator inputCorouitne;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private TorchMeter torchMeterScript;
    private AudioManager audioManagerScript;
    private GameHUD gameHUDScript;
    private TransitionFade transitionFadeScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // The skip scene button is only active in the tutorial zone
        if (SceneManager.GetActiveScene().name == "TutorialMap")
            SetSkipSceneButtonActive();
        else
            SetSkipSceneButtonInactive();
    }

    // Checks if the skip scene button should be active/inactive
    public void SetSkipSceneButtonActive()
    {
        skipSceneButton.SetActive(true);
        StartTextAlphaCoroutine(1f, textFadeLength);
        StartInputCoroutine();
    }

    // Checks if the skip scene button should be active/inactive
    public void SetSkipSceneButtonInactive()
    {
        if (textAlphaCoroutine != null)
            StopCoroutine(textAlphaCoroutine);

        if (inputCorouitne != null)
            StopCoroutine(inputCorouitne);

        skipButtonAnimator.Rebind(); // Resets the animator
        skipSceneText.SetTextAlpha(0f);
        skipSceneButton.SetActive(false);
    }

    // Checks to loop the text alpha coroutine
    private void LoopTextAlphaCheck()
    {
        if (!hasSkippedScene)
        {
            Color skipSceneTextColor = skipSceneText.color;

            if (skipSceneTextColor.a == 0f) // If zero alpha
            {
                canRecieveInput = true;
                StartTextAlphaCoroutine(1f, textFadeLength);
            }
            else if (skipSceneTextColor.a == 1f) // If full alpha
                StartTextAlphaCoroutine(0f, textFadeLength);
        }
    }

    // Plays a new animation state for the skip scene button
    private void ChangeAnimationStateSSB(string newState)
    {
        switch (newState)
        {
            case ("NotHoldingButton"):
                skipButtonAnimator.Play("NotHoldingButton");
                break;
            case ("HoldingButton"):
                skipButtonAnimator.Play("HoldingButton");
                break;
            default:
                //Debug.Log("Animation state was found");
                break;
        }
    }

    // Starts the coroutine thats lerps the bar's fill amount
    private void LerpSkipSceneBar()
    {
        if (lerpFillAmountCoroutine != null)
            StopCoroutine(lerpFillAmountCoroutine);

        lerpFillAmountCoroutine = LerpFillAmount(lerpLength);
        StartCoroutine(lerpFillAmountCoroutine);
    }

    // Starts the coroutine thats resets the bar's fill amount
    private void ResetSkipSceneBar()
    {
        if (resetFillAmountCoroutine != null)
            StopCoroutine(resetFillAmountCoroutine);

        resetFillAmountCoroutine = ResetFillAmount();
        StartCoroutine(resetFillAmountCoroutine);
    }

    // Starts the coroutine that fades the text alpha int/out
    private void StartTextAlphaCoroutine(float endValue, float duration)
    {
        if (textAlphaCoroutine != null)
            StopCoroutine(textAlphaCoroutine);

        textAlphaCoroutine = LerpTextAlpha(endValue, duration);
        StartCoroutine(textAlphaCoroutine);
    }

    // Starts the coroutine that checks for the skip scene button input
    private void StartInputCoroutine()
    {
        if (inputCorouitne != null)
            StopCoroutine(inputCorouitne);

        inputCorouitne = SkipSceneInputCheck();
        StartCoroutine(inputCorouitne);
    }

    // Lerps the bar's fillAmount to its max over a duration (duration = seconds)
    private IEnumerator LerpFillAmount(float duration)
    {
        if (textAlphaCoroutine != null)
            StopCoroutine(textAlphaCoroutine);

        ChangeAnimationStateSSB("HoldingButton");
        skipSceneText.SetTextAlpha(1f);
        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PlayPopUpSFX();
        pauseMenuScript.CanPause = false;

        float startFillAmount = content.fillAmount;
        float endFillAmount = 1f;
        float time = 0f;

        while (time < duration && Input.GetKey(skipSceneKeyCode))
        {
            content.fillAmount = Mathf.Lerp(startFillAmount, endFillAmount, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Checks if the bar was filled (if the scene can be skipped)
        if (time >= duration)
        {
            content.fillAmount = endFillAmount;
            audioManagerScript.PlayChime02SFX();
            playerScript.SetFinishedZoneElements();
            playerScript.HasFinishedZone = true;
            hasSkippedScene = true;
        }
        // If the bar was not filled
        else
        {
            ResetSkipSceneBar();
            playerScript.SetPlayerBoolsTrue();
            pauseMenuScript.CanPause = true;
        }

        ChangeAnimationStateSSB("NotHoldingButton");
        StartTextAlphaCoroutine(0f, textFadeLength);
    }

    // Lerps the bar's fillAmount to its min (zero)
    private IEnumerator ResetFillAmount()
    {
        while (content.fillAmount > 0.01f)
        {
            // Note: content.fillAmount will always get closer to zero, but never equal it
            content.fillAmount = Mathf.Lerp(content.fillAmount, 0f, Time.deltaTime * lerpSpeed);
            yield return null;
        }

        content.fillAmount = 0f;     
    }

    // Lerps the alpha of the text over a specific duration (alpha = alpha to lerp to, duration = seconds)
    // Note: 0f = zero alpha, 1f = full alpha
    private IEnumerator LerpTextAlpha(float alpha, float duration)
    {
        Color startColor = skipSceneText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
        float time = 0;

        while (time < duration)
        {
            skipSceneText.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        skipSceneText.color = endColor;
        LoopTextAlphaCheck();
        StartInputCoroutine();
    }

    // Checks for the input that lerps the skip scene button
    private IEnumerator SkipSceneInputCheck()
    {
        while (canRecieveInput && skipSceneButton.activeSelf)
        {
            if (!transitionFadeScript.IsChangingScenes && !pauseMenuScript.IsPaused && pauseMenuScript.CanPause && playerScript.CanMove && torchMeterScript.CurrentVal > 0)
            {
                if (Input.GetKeyDown(skipSceneKeyCode) || Input.GetKey(skipSceneKeyCode))
                {
                    LerpSkipSceneBar();
                    canRecieveInput = false;
                    yield break;
                }
            }

            yield return null;
        }
        //Debug.Log("Stopped looking for tutorial dialogue inputCheck");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "SkipSceneButton")
            {
                skipSceneButton = child;
                skipButtonAnimator = skipSceneButton.GetComponent<Animator>();

                for (int j = 0; j < skipSceneButton.transform.childCount; j++)
                {
                    GameObject child02 = skipSceneButton.transform.GetChild(j).gameObject;
                    string childName02 = child02.name;

                    if (childName02 == "Bar")
                    {
                        skipSceneBar = child02.GetComponent<Image>();

                        for (int k = 0; k < skipSceneBar.transform.childCount; k++)
                        {
                            GameObject child03 = skipSceneBar.transform.GetChild(k).gameObject;

                            if (child03.name == "FillContent")
                                content = child03.GetComponent<Image>();
                        }
                    }
                    if (childName02 == "Text")
                        skipSceneText = child02.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        skipSceneText.SetTextAlpha(0f);
        skipSceneBar.color = Color.clear;
    }

}
