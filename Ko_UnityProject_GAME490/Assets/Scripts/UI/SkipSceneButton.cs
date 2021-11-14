using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SkipSceneButton : MonoBehaviour
{
    private bool canHoldButton = true;
    private bool hasSkippedScene = false;

    [Range(1f, 5f)]
    public float lerpLength = 3f;
    [Range(1f, 20f)]
    public float lerpSpeed = 10f;
    [Range(0.1f, 2f)]
    public float textFadeLength = 0.75f;

    private GameObject skipSceneButton;
    private Image content;
    private Image skipSceneBar;
    private Animator skipButtonAnimator;

    private TextMeshProUGUI skipSceneText;
    private Color32 zeroAlpha;
    private Color32 fullAlpha;

    private KeyCode skipSceneKeyCode = KeyCode.X;
    private IEnumerator fadeTextCoroutine;
    private IEnumerator resetFillAmountCoroutine;
    private IEnumerator lerpFillAmountCoroutine;

    private GameManager gameManagerScript;
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private TorchMeter torchMeterScript;
    private AudioManager audioManagerScript;
    private GameHUD gameHUDScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetActiveCheck();
    }

    // Update is called once per frame
    void Update()
    {
        SkipSceneCheck();
    }

    // Checks if the object with this script should be active/inactive (only active in the tutorial zone)
    private void SetActiveCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "TutorialMap")
        {
            skipSceneButton.SetActive(true);
            FadeSkipSceneText(fullAlpha, textFadeLength);
        }         
        else
        {
            skipSceneButton.SetActive(false);
            this.enabled = false; // Disables the script - Don't need the code running in update loop if the skipSceneButton is inactive
        }
    }

    // Checks when the skip scene button can be held/activated
    private void SkipSceneCheck()
    {
        if (canHoldButton && skipSceneButton.activeSelf)
        {
            if (!transitionFadeScript.IsChangingScenes && !pauseMenuScript.IsPaused && pauseMenuScript.CanPause && playerScript.CanMove && torchMeterScript.CurrentVal > 0)
            {
                if (Input.GetKeyDown(skipSceneKeyCode) || Input.GetKey(skipSceneKeyCode))
                {
                    LerpSkipSceneBar();
                    canHoldButton = false;
                }
            }
        }
    }

    // Starts the coroutine for lerping the bar's fill amount
    private void LerpSkipSceneBar()
    {
        if (lerpFillAmountCoroutine != null)
            StopCoroutine(lerpFillAmountCoroutine);

        lerpFillAmountCoroutine = LerpFillAmount(lerpLength);
        StartCoroutine(lerpFillAmountCoroutine);
    }

    // Starts the coroutine for resetting the skip scene bar
    private void ResetSkipSceneBar()
    {
        if (resetFillAmountCoroutine != null)
            StopCoroutine(resetFillAmountCoroutine);

        resetFillAmountCoroutine = ResetFillAmount();
        StartCoroutine(resetFillAmountCoroutine);
    }

    // Starts the coroutine for fading the skip scene text (loops)
    private void FadeSkipSceneText(Color endValue, float duration)
    {
        if (fadeTextCoroutine != null)
            StopCoroutine(fadeTextCoroutine);

        fadeTextCoroutine = LerpTextAlpha(endValue, duration);
        StartCoroutine(fadeTextCoroutine);
    }

    // Lerps the fillAmount over a specific duration (duration = seconds)
    private IEnumerator LerpFillAmount(float duration)
    {
        if (!skipButtonAnimator.enabled)
            skipButtonAnimator.enabled = true;
        if (fadeTextCoroutine != null)
            StopCoroutine(fadeTextCoroutine);

        skipSceneText.color = fullAlpha;
        skipButtonAnimator.SetTrigger("Holding");
        pauseMenuScript.CanPause = false;
        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PlayPopUpSFX();

        float time = 0f;
        float startFillAmount = content.fillAmount;
        float endFillAmount = 1f;

        while (time < duration && Input.GetKey(skipSceneKeyCode))
        {
            if (Input.GetKeyUp(skipSceneKeyCode))
                yield break;

            content.fillAmount = Mathf.Lerp(startFillAmount, endFillAmount, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (time >= duration)
        {
            content.fillAmount = endFillAmount;
            audioManagerScript.PlayChime02SFX();
            playerScript.SetExitZoneElements();
            playerScript.HasFinishedZone = true;
            hasSkippedScene = true;
        }
        else if (time < duration)
        {
            ResetSkipSceneBar();
            playerScript.SetPlayerBoolsTrue();
            pauseMenuScript.CanPause = true;
        }

        skipButtonAnimator.SetTrigger("NotHolding");
        FadeSkipSceneText(zeroAlpha, textFadeLength);
    }

    // Lerps the fill amount 
    private IEnumerator ResetFillAmount()
    {
        // When the fill amount is approximately equal to zero
        // Note: The content.fillAmount in lerp will always get closer to zero, but never equal it, so the coroutine would endlessly play
        while (content.fillAmount > 0.01f)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, 0f, Time.deltaTime * lerpSpeed);
            yield return null;
        }

        content.fillAmount = 0f;
    }

    // Lerps the alpha of the text over a specific duration (duration = seconds)
    private IEnumerator LerpTextAlpha(Color endValue, float duration)
    {
        float time = 0;
        Color startValue = skipSceneText.color;

        while (time < duration)
        {
            skipSceneText.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        skipSceneText.color = endValue;

        // Checks to fade the skipSceneText (to loop it)
        if (!hasSkippedScene)
        {
            if (skipSceneText.color == zeroAlpha)
            {
                // Resets the skip scene button
                if (!canHoldButton)
                    canHoldButton = true;

                FadeSkipSceneText(fullAlpha, textFadeLength);
            }
            else if (skipSceneText.color == fullAlpha)
                FadeSkipSceneText(zeroAlpha, textFadeLength);
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
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
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "SkipSceneButton")
            {
                skipSceneButton = child;

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

        skipButtonAnimator = skipSceneButton.GetComponent<Animator>();
        skipButtonAnimator.enabled = false;

        zeroAlpha = new Color(skipSceneText.color.r, skipSceneText.color.g, skipSceneText.color.b, 0);
        fullAlpha = new Color(skipSceneText.color.r, skipSceneText.color.g, skipSceneText.color.b, 1);
        skipSceneText.color = zeroAlpha;
        skipSceneBar.color = Color.clear; // Note: Must disable the animator beforehand for this to work!
    }

}
