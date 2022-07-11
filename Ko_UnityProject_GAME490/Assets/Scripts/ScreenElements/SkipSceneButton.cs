using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkipSceneButton : MonoBehaviour
{
    [SerializeField] [Range(1f, 5f)]
    private float lerpDuration = 3f; // Original Value = 3f
    [SerializeField] [Range(1f, 20f)]
    private float lerpSpeed = 10f; // Original Value = 10f
    [SerializeField] [Range(0.1f, 2f)]
    private float textFadeDuration = 0.75f; // Original Value = 0.75f

    private float fullAlpha = 1f;
    private float zeroAlpha = 0f;
    private float maxFillAmount = 1f;
    private float minFillAmount = 0f;

    private bool canRecieveInput = false;
    private bool hasSkippedScene = false;

    private Image fill;
    private Image skipSceneFillBG;
    private Animator skipButtonAnimator;
    private TextMeshProUGUI skipSceneText;

    private KeyCode skipSceneKeyCode = KeyCode.X;
    private IEnumerator lerpTextCoroutine;
    private IEnumerator resetBarCoroutine;
    private IEnumerator lerpBarCoroutine;
    private IEnumerator inputCoroutine;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private GameManager gameManagerScript;
    private PauseMenu pauseMenuScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSkipSceneButtonInactive();
    }

    [ContextMenu("Set SkipSceenButton Active")]
    // Sets the skip scene button ACTIVE
    public void SetSkipSceneButtonActive()
    {
        enabled = true;
        canRecieveInput = true;
        gameObject.SetActive(true);
        
        StartLerpTextCoroutine(fullAlpha);
        StartInputCoroutine();
    }

    // Sets the skip scene button INACTIVE
    public void SetSkipSceneButtonInactive()
    {
        if (lerpTextCoroutine != null) StopCoroutine(lerpTextCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        skipSceneText.SetTextAlpha(zeroAlpha);
        gameObject.SetActive(false);
        canRecieveInput = false;
        enabled = false;
    }

    // Checks to loop the text alpha coroutine
    private void LoopTextAlphaCheck()
    {
        if (hasSkippedScene) return;

        bool atZeroAlpha = skipSceneText.color.a == zeroAlpha;
        float endAlpha = atZeroAlpha ? fullAlpha : zeroAlpha;

        if (atZeroAlpha) canRecieveInput = true;
        StartLerpTextCoroutine(endAlpha);
    }

    // Plays a new animation state for the skip scene button
    private void ChangeAnimationState(string newState)
    {
        switch (newState)
        {
            case ("PopOut"):
                skipButtonAnimator.Play(newState);
                break;
            case ("PopIn"):
                skipButtonAnimator.Play(newState);
                break;
            default:
                //Debug.Log("Animation state does not exist");
                break;
        }
    }

    // Starts the coroutine that lerps the bar's fill amount to its max
    private void StartLerpBarCoroutine()
    {
        if (lerpBarCoroutine != null) StopCoroutine(lerpBarCoroutine);

        lerpBarCoroutine = LerpToMaxFillAmount(lerpDuration);
        StartCoroutine(lerpBarCoroutine);
    }

    // Starts the coroutine that lerps the bar's fill amount to its min
    private void StartResetBarCoroutine()
    {
        if (resetBarCoroutine != null) StopCoroutine(resetBarCoroutine);

        resetBarCoroutine = LerpToMinFillAmount();
        StartCoroutine(resetBarCoroutine);
    }

    // Starts the coroutine that lerps the text alpha
    private void StartLerpTextCoroutine(float endAlpha)
    {
        if (lerpTextCoroutine != null) StopCoroutine(lerpTextCoroutine);

        lerpTextCoroutine = LerpTextAlpha(endAlpha, textFadeDuration);
        StartCoroutine(lerpTextCoroutine);
    }

    // Starts the coroutine that checks for the skip scene button input
    private void StartInputCoroutine()
    {
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        inputCoroutine = SkipSceneInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // Checks when the the bar can lerp to its max fill amount
    private void LerpToMaxCheck()
    {
        if (!canRecieveInput || !playerScript.CanMove || !Input.GetKey(skipSceneKeyCode)) return;

        if (lerpTextCoroutine != null) StopCoroutine(lerpTextCoroutine);
        pauseMenuScript.CanPause = false;
        canRecieveInput = false;

        skipSceneText.SetTextAlpha(fullAlpha);
        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PlayPopUpSFX();      

        ChangeAnimationState("PopIn");
        StartLerpBarCoroutine();
    }

    // Checks if the the bar has lerped to its max fill amount
    private void MaxFillAmountCheck()
    {
        ChangeAnimationState("PopOut");
        StartLerpTextCoroutine(zeroAlpha);

        // Skips the scene if the bar was filled
        if (fill.fillAmount == maxFillAmount)
        {
            gameManagerScript.ResetCollectedArtifactsCheck();
            audioManagerScript.PlaySkippedSceneSFX();
            blackOverlayScript.GameFadeOut();
            playerScript.SetPlayerBoolsFalse();
            playerScript.HasFinishedZone = true;
            hasSkippedScene = true;
        }
        // Lerps the bar to its min fill amount otherwise
        else
        {
            playerScript.SetPlayerBoolsTrue();
            pauseMenuScript.CanPause = true;
            StartResetBarCoroutine();
        }
    }

    // Lerps the alpha of the text over a specific duration (alpha = alpha to lerp to, duration = seconds)
    private IEnumerator LerpTextAlpha(float endAlpha, float duration)
    {
        Color startColor = skipSceneText.color;
        Color endColor = skipSceneText.ReturnTextColor(endAlpha);
        float time = 0f;

        while (time < duration)
        {
            skipSceneText.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        skipSceneText.color = endColor;
        LoopTextAlphaCheck();
    }

    // Lerps the bar's fill amount to its max over a duration (duration = seconds)
    private IEnumerator LerpToMaxFillAmount(float duration)
    {
        float startFillAmount = fill.fillAmount;
        float time = 0f;

        while (time < duration && Input.GetKey(skipSceneKeyCode))
        {
            fill.fillAmount = Mathf.Lerp(startFillAmount, maxFillAmount, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (time >= duration) fill.fillAmount = maxFillAmount;
        MaxFillAmountCheck();
    }

    // Lerps the bar's fillAmount to its min
    // Note: content.fillAmount will always get closer to zero, but never equal it
    private IEnumerator LerpToMinFillAmount()
    {
        while (fill.fillAmount > minFillAmount + 0.01f)
        {
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, minFillAmount, lerpSpeed * Time.deltaTime);
            yield return null;
        }

        fill.fillAmount = minFillAmount;
    }

    // Checks for the input that lerps the skip scene button
    private IEnumerator SkipSceneInputCheck()
    {
        while (gameObject.activeInHierarchy && !blackOverlayScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0) LerpToMaxCheck();
            yield return null;
        }
        //Debug.Log("Stopped looking for skip scene input check");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        gameManagerScript = FindObjectOfType<GameManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "SSB_Bar":
                    skipButtonAnimator = child.GetComponent<Animator>();
                    skipButtonAnimator.keepAnimatorControllerStateOnDisable = false;
                    break;
                case "SSB_FillBG":
                    skipSceneFillBG = child.GetComponent<Image>();
                    break;
                case "SSB_Fill":
                    fill = child.GetComponent<Image>();
                    break;
                case "SSB_Text":
                    skipSceneText = child.GetComponent<TextMeshProUGUI>();
                    break;
                default:
                    break;
            }
            
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);

        skipSceneFillBG.SetImageAlpha(0f);
        skipSceneText.SetTextAlpha(0f);
    }
}
