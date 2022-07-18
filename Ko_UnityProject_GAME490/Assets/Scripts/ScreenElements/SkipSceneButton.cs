using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    static readonly string tutorialZone = "TutorialMap";
    private string sceneName;

    private bool canRecieveInput = false;
    private bool hasSkippedScene = false;

    private Animator skipButtonAnimator;
    private TextMeshProUGUI skipSceneText;

    private Image fillBackground;
    private Image fill;
    private Image bar;

    private KeyCode skipSceneKeyCode = KeyCode.X;
    private IEnumerator lerpTextCoroutine;
    private IEnumerator lerpBarCoroutine;
    private IEnumerator inputCoroutine;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private TorchMeter torchMeterScript;
    private PauseMenu pauseMenuScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSkipSceneButtonInactive();
    }

    // Sets the skip scene button active
    [ContextMenu("Set Skip Scene Button Active")]
    public void SetSkipSceneButtonActive()
    {
        enabled = true;
        canRecieveInput = true;
        gameObject.SetActive(true);
        bar.gameObject.SetActive(true);

        StartLerpTextCoroutine(zeroAlpha, fullAlpha);          
        StartInputCoroutine();
    }

    // Sets the skip scene button inactive
    public void SetSkipSceneButtonInactive()
    {      
        if (lerpTextCoroutine != null) 
            StopCoroutine(lerpTextCoroutine);

        if (inputCoroutine != null) 
            StopCoroutine(inputCoroutine);

        skipSceneText.SetTextAlpha(zeroAlpha);
        bar.gameObject.SetActive(false);    
        canRecieveInput = false;

        if (sceneName == tutorialZone) return;
        gameObject.SetActive(false);
        enabled = false;
    }

    // Checks for the input that lerps the bar's fill amount
    private void LerpBarCheck()
    {
        if (!Input.GetKey(skipSceneKeyCode) || !canRecieveInput) return;
        if (!playerScript.CanMove || torchMeterScript.CurrentVal <= 0) return;

        if (lerpTextCoroutine != null)
            StopCoroutine(lerpTextCoroutine);

        playerScript.SetPlayerBoolsFalse();
        audioManagerScript.PlayPopUpSFX();

        skipSceneText.SetTextAlpha(fullAlpha);
        pauseMenuScript.CanPause = false;
        canRecieveInput = false;

        PopInSkipSceneButton();
        StartLerpBarCoroutine();
    }

    // Play the pop out animation for the skip scene button
    private void PopOutSkipSceneButton() => skipButtonAnimator.Play("PopOut");

    // Play the pop in animation for the skip scene button
    private void PopInSkipSceneButton() => skipButtonAnimator.Play("PopIn");

    // Starts the coroutine that lerps the bar's fill amount
    private void StartLerpBarCoroutine()
    {
        if (lerpBarCoroutine != null) StopCoroutine(lerpBarCoroutine);

        lerpBarCoroutine = LerpFillAmount(lerpDuration);
        StartCoroutine(lerpBarCoroutine);
    }

    // Starts the coroutine that lerps the text's alpha - fades the text
    private void StartLerpTextCoroutine(float startAlpha, float endAlpha)
    {
        if (lerpTextCoroutine != null) StopCoroutine(lerpTextCoroutine);

        lerpTextCoroutine = LerpTextAlpha(startAlpha, endAlpha);
        StartCoroutine(lerpTextCoroutine);
    }

    // Starts the coroutine that checks for the skip scene button input
    private void StartInputCoroutine()
    {
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        inputCoroutine = SkipSceneInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // Checks if the bar has lerped to its max fill amount - returns true if so, false otherwise
    private bool HasLerpedToMax(bool hasLepredToMax)
    {
        StartLerpTextCoroutine(fullAlpha, zeroAlpha);
        PopOutSkipSceneButton();

        if (!hasLepredToMax)
        {
            playerScript.SetPlayerBoolsTrue();
            pauseMenuScript.CanPause = true;
            return false;
        }

        audioManagerScript.PlaySkippedSceneSFX();
        playerScript.SetPlayerBoolsFalse();
        blackOverlayScript.GameFadeOut();
        playerScript.HasFinishedZone = true;
        hasSkippedScene = true;

        fill.fillAmount = maxFillAmount;
        return true;
    }

    // Checks if the text can lerp its alpha again - return true if so, false otherwise
    private bool CanLoopTextAlpha()
    {
        if (hasSkippedScene) return false;

        if (skipSceneText.color.a == zeroAlpha)
            canRecieveInput = true;

        return true;
    }

    // Lerps the bar's fill amount to its max/min over a duration (duration = seconds)
    private IEnumerator LerpFillAmount(float duration)
    {
        float startFillAmount = fill.fillAmount;
        float time = 0f;

        // Checks to lerp the bar to its max fill amount
        while (time < duration && Input.GetKey(skipSceneKeyCode))
        {
            fill.fillAmount = Mathf.Lerp(startFillAmount, maxFillAmount, time / duration);
            time += Time.deltaTime;
            yield return null;
        }     
        if (HasLerpedToMax(time >= duration)) yield break;

        // Lerps to bar to its min fill amount
        while (fill.fillAmount > minFillAmount + 0.01f)
        {
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, minFillAmount, lerpSpeed * Time.deltaTime);
            yield return null;
        }

        fill.fillAmount = minFillAmount;
    }

    // Lerps the alpha of the text to another over a duration (duration = seconds)
    private IEnumerator LerpTextAlpha(float startAlpha, float endAlpha)
    {
        Color startColor = skipSceneText.ReturnTextColor(startAlpha);
        Color endColor = skipSceneText.ReturnTextColor(endAlpha);

        float duration = textFadeDuration;
        float time = 0f;

        while (time < duration)
        {
            skipSceneText.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        skipSceneText.color = endColor;

        if (!CanLoopTextAlpha()) yield break;
        StartLerpTextCoroutine(endAlpha, startAlpha);
    }

    // Checks for the input that activates the skip scene button
    private IEnumerator SkipSceneInputCheck()
    {
        while (gameObject.activeInHierarchy && !blackOverlayScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0) LerpBarCheck();
            yield return null;
        }
        //Debug.Log("Stopped looking for the skip scene input check");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
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
                    bar = child.GetComponent<Image>();
                    skipButtonAnimator = child.GetComponent<Animator>();
                    skipButtonAnimator.keepAnimatorControllerStateOnDisable = false;
                    break;
                case "SSB_FillBG":
                    fillBackground = child.GetComponent<Image>();
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

        fillBackground.SetImageAlpha(0f);
        skipSceneText.SetTextAlpha(0f);
    }
}
