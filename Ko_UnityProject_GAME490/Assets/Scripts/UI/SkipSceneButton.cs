using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private float maxTextAlpha = 1f;
    private float minTextAlpha = 0f;
    private float maxFillAmount = 1f;
    private float minFillAmount = 0f;

    private bool canRecieveInput = false;
    private bool hasSkippedScene = false;

    private GameObject skipSceneButton;
    private Image content;
    private Image skipSceneBar;
    private Animator skipButtonAnimator;
    private TextMeshProUGUI skipSceneText;

    private KeyCode skipSceneKeyCode = KeyCode.X;
    private IEnumerator lerpTextCoroutine;
    private IEnumerator resetBarCoroutine;
    private IEnumerator lerpBarCoroutine;
    private IEnumerator inputCoroutine;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private GameHUD gameHUDScript;
    private TransitionFade transitionFadeScript;
    private GameManager gameManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    [ContextMenu("Set SkipSceenButton Active")]
    // Sets the skip scene button active
    public void SetSkipSceneButtonActive()
    {
        skipSceneButton.SetActive(true);
        canRecieveInput = true;

        StartLerpTextCoroutine(maxTextAlpha);
        StartInputCoroutine();
    }

    // Sets the skip scene button inactive
    public void SetSkipSceneButtonInactive()
    {
        if (lerpTextCoroutine != null) StopCoroutine(lerpTextCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        skipButtonAnimator.Rebind(); // Resets the animator
        skipSceneText.SetTextAlpha(minTextAlpha);

        skipSceneButton.SetActive(false);
        canRecieveInput = false;
    }

    // Checks to loop the text alpha coroutine
    private void LoopTextAlphaCheck()
    {
        if (hasSkippedScene) return;

        bool atZeroAlpha = skipSceneText.color.a == minTextAlpha;
        float endAlpha = (atZeroAlpha) ? maxTextAlpha : minTextAlpha;

        if (atZeroAlpha) canRecieveInput = true;
        StartLerpTextCoroutine(endAlpha);
    }

    // Plays a new animation state for the skip scene button
    private void ChangeAnimationState(string newState)
    {
        switch (newState)
        {
            case ("NotHoldingButton"):
                skipButtonAnimator.Play(newState);
                break;
            case ("HoldingButton"):
                skipButtonAnimator.Play(newState);
                break;
            default:
                //Debug.Log("Animation state was not found");
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
        if (Time.deltaTime == 0 || !Input.GetKey(skipSceneKeyCode) || !canRecieveInput || !playerScript.CanMove) return;

        if (lerpTextCoroutine != null)
            StopCoroutine(lerpTextCoroutine);

        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PlayPopUpSFX();
        pauseMenuScript.CanPause = false;

        ChangeAnimationState("HoldingButton");
        skipSceneText.SetTextAlpha(maxTextAlpha);
        canRecieveInput = false;

        StartLerpBarCoroutine();
    }

    // Checks if the the bar has lerped to its max fill amount
    private void MaxFillAmountCheck()
    {
        ChangeAnimationState("NotHoldingButton");
        StartLerpTextCoroutine(minTextAlpha);

        // Skips the scene if the bar was filled
        if (content.fillAmount == maxFillAmount)
        {
            gameManagerScript.ResetCollectedArtifactsCheck();
            transitionFadeScript.GameFadeOut();

            audioManagerScript.PlayChime02SFX();
            playerScript.SetPlayerBoolsFalse();
            playerScript.HasFinishedZone = true;
            hasSkippedScene = true;
        }
        // Lerps the bar to its min otherwise
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
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
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

    // Lerps the bar's fillAmount to its max over a duration (duration = seconds)
    private IEnumerator LerpToMaxFillAmount(float duration)
    {
        float startFillAmount = content.fillAmount;
        float time = 0f;

        while (time < duration && Input.GetKey(skipSceneKeyCode))
        {
            content.fillAmount = Mathf.Lerp(startFillAmount, maxFillAmount, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (time >= duration) content.fillAmount = maxFillAmount;
        MaxFillAmountCheck();
    }

    // Lerps the bar's fillAmount to its min
    private IEnumerator LerpToMinFillAmount()
    {
        while (content.fillAmount > minFillAmount + 0.01f)
        {
            // Note: content.fillAmount will always get closer to zero, but never equal it
            content.fillAmount = Mathf.Lerp(content.fillAmount, minFillAmount, lerpSpeed * Time.deltaTime);
            yield return null;
        }

        content.fillAmount = minFillAmount;
    }

    // Checks for the input that lerps the skip scene button
    private IEnumerator SkipSceneInputCheck()
    {
        while (skipSceneButton.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            LerpToMaxCheck();
            yield return null;
        }
        //Debug.Log("Stopped looking for skip scene input check");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        gameManagerScript = FindObjectOfType<GameManager>();
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

                    if (child02.name == "Bar")
                    {
                        skipSceneBar = child02.GetComponent<Image>();

                        for (int k = 0; k < skipSceneBar.transform.childCount; k++)
                        {
                            GameObject child03 = skipSceneBar.transform.GetChild(k).gameObject;

                            if (child03.name == "FillContent")
                                content = child03.GetComponent<Image>();
                        }
                    }
                    if (child02.name == "Text")
                        skipSceneText = child02.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        skipSceneText.SetTextAlpha(0f);
        skipSceneBar.color = Color.clear;
        SetSkipSceneButtonInactive();
    }
}
