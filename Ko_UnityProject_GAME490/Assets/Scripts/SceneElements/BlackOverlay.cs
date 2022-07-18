using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlackOverlay : MonoBehaviour
{
    [SerializeField] [Range(0.1f, 5.0f)]
    private float gameFadeDuration = 2f; // Orignal Value = 2f
    [SerializeField] [Range(0.1f, 5.0f)]
    private float introFadeDuration = 2f; // Orignal Value = 2f
    [SerializeField] [Range(0.1f, 5.0f)]
    private float transitionFadeDuration = 1f; // Orignal Value = 1f

    private float fadeDelay = 0.1f; // Orignal Value = 0.1f
    private float fullAlpha = 1f;
    private float halfAlpha = 0.5f;
    private float zeroAlpha = 0f;

    private bool isChangingScenes = false;
    private bool isDebugging = false;
    private string mainMenu = "MainMenu";
    private string sceneName;

    private Image mainBlackOverlay;
    private Image introBlackOverlay;
    private IEnumerator lerpAlphaCoroutine;

    public static BlackOverlay instance;
    private TileMovementController playerScript;
    private LevelManager levelManagerScript;
    private EndCredits endCreditsScript;
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;

    public float GameFadeDuration
    {
        get { return gameFadeDuration + fadeDelay; }
    }

    public float IntroFadeDuration
    {
        get { return introFadeDuration + fadeDelay; }
    }

    public float TransitionFadeDuration
    {
        get { return transitionFadeDuration + fadeDelay; }
    }

    public bool IsChangingScenes
    {
        get { return isChangingScenes; }
        set { isChangingScenes = value; }
    }

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
        FadeIntoZoneCheck();
    }

    // Fades into the game
    public void GameFadeIn()
    {
        SetIntroOverlayInactiveCheck();
        isDebugging = false;

        StartLerpAlphaCoroutine(mainBlackOverlay, fullAlpha, zeroAlpha, gameFadeDuration);
    }

    // Fades out of the game
    public void GameFadeOut()
    {
        if (Time.timeScale != 1f) Time.timeScale = 1f;
        isChangingScenes = true;
        isDebugging = false;

        StartLerpAlphaCoroutine(mainBlackOverlay, zeroAlpha, fullAlpha, gameFadeDuration);
    }

    // Fades in the intro overlay
    public void IntroFadeIn() => StartLerpAlphaCoroutine(introBlackOverlay, fullAlpha, halfAlpha, introFadeDuration);

    // Fades out the intro overlay
    public void IntroFadeOut() => StartLerpAlphaCoroutine(introBlackOverlay, halfAlpha, fullAlpha, introFadeDuration);

    // Checks to fade into the zone
    private void FadeIntoZoneCheck()
    {
        if (playerScript == null || !playerScript.OnCheckpoint()) return;

        GameFadeIn();
    }

    // Checks to set the zone intro's black overlay inactive
    private void SetIntroOverlayInactiveCheck()
    {
        if (sceneName == mainMenu || introBlackOverlay == null) return;

        introBlackOverlay.gameObject.SetActive(false);
        introBlackOverlay.SetImageAlpha(0f);
    }

    // Checks which methods to call if the overlay's alpha is equal to fullAlpha (1f)
    private void FullAlphaCheck(Image overlay)
    {
        if (overlay.color.a != fullAlpha) return;

        if (!isChangingScenes || endCreditsScript.HasStartedCredits) return;

        if (sceneName == mainMenu)
            mainMenuScript.LoadNextScene();
        else
            levelManagerScript.LoadNextScene();
    }

    // Checks which methods to call if the overlay's alpha is equal to zeroAlpha (0f)
    private void ZeroAlphaCheck(Image overlay)
    {
        if (overlay.color.a != zeroAlpha) return;

        if (pauseMenuScript != null) pauseMenuScript.CanPause = true;
    }

    // Checks which methods to call BEFORE the overlay lerps to its end alpha
    // Note: pauseMenuScript.CanPause is set to false when fading
    private void StartAlphaCheck()
    {
        if (isDebugging) return;

        if (pauseMenuScript != null) pauseMenuScript.CanPause = false;
    }

    // Checks which methods to call AFTER the overlay has lerped to its end alpha
    private void EndAlphaCheck(Image overlay)
    {
        if (isDebugging) return;

        FullAlphaCheck(overlay);
        ZeroAlphaCheck(overlay);
    }

    // Starts the coroutine that fades the overlay out and then in
    public void StartTransitionFadeCoroutine()
    {
        if (lerpAlphaCoroutine != null) StopCoroutine(lerpAlphaCoroutine);

        lerpAlphaCoroutine = PlayTransitionFade();
        StartCoroutine(lerpAlphaCoroutine);
    }

    // Starts the coroutine that lerps the alpha of the overlay
    private void StartLerpAlphaCoroutine(Image overlay, float startAlpha, float endAlpha, float duration)
    {
        if (lerpAlphaCoroutine != null) StopCoroutine(lerpAlphaCoroutine);

        lerpAlphaCoroutine = LerpAlpha(overlay, startAlpha, endAlpha, duration);
        StartCoroutine(lerpAlphaCoroutine);
    }

    // Fades the overlay out and then in
    private IEnumerator PlayTransitionFade()
    {
        StartCoroutine(LerpAlpha(mainBlackOverlay, zeroAlpha, fullAlpha, transitionFadeDuration));
        yield return new WaitForSeconds(transitionFadeDuration + fadeDelay);
        StartCoroutine(LerpAlpha(mainBlackOverlay, fullAlpha, zeroAlpha, transitionFadeDuration));
    }

    // Lerps the alpha of the overlay to another over a duration (duration = seconds)
    // Note: full alpha = 1f, zero alpha = 0f
    private IEnumerator LerpAlpha(Image overlay, float startAlpha, float endAlpha, float duration)
    {      
        Color startColor = overlay.ReturnImageColor(startAlpha);
        Color endColor = overlay.ReturnImageColor(endAlpha);
        overlay.color = startColor;
        float time = 0;

        StartAlphaCheck();
        yield return new WaitForSeconds(fadeDelay); // Small delay to make fades cleaner

        while (time < duration)
        {
            overlay.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        overlay.color = endColor;
        EndAlphaCheck(overlay);
        isDebugging = false; // call this LAST!
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>() : null;
        pauseMenuScript = (sceneName != mainMenu) ? FindObjectOfType<PauseMenu>() : null;
        mainMenuScript = (sceneName == mainMenu) ? FindObjectOfType<MainMenu>() : null;

        levelManagerScript = FindObjectOfType<LevelManager>();
        endCreditsScript = FindObjectOfType<EndCredits>();
        instance = this;
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "ZI_BlackOverlay":
                    introBlackOverlay = child.GetComponent<Image>();
                    break;
                case "BlackOverlay":
                    mainBlackOverlay = child.GetComponent<Image>();
                    break;
                default:
                    break;
            }

            if (child.name == "EndCreditsHolder" || child.name == "LoadingScreenHolder") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform.parent);
    }

    // Checks to fade the game in/out - For Debugging Purposes ONLY
    public void DebuggingCheck()
    {
        if (mainBlackOverlay.color.a == fullAlpha && Input.GetKeyDown(KeyCode.Semicolon)) // Debug key is ";" (semicolon)
        {
            isDebugging = true;
            StartLerpAlphaCoroutine(mainBlackOverlay, fullAlpha, zeroAlpha, gameFadeDuration);
            Debug.Log("Debugging: game is fading in");
        }
        if (mainBlackOverlay.color.a == zeroAlpha && Input.GetKeyDown(KeyCode.Quote)) // Debug key is "'" (quote)
        {
            isDebugging = true;
            StartLerpAlphaCoroutine(mainBlackOverlay, zeroAlpha, fullAlpha, gameFadeDuration);
            Debug.Log("Debugging: game is fading out");
        }
        /*if (transitionFade.color.a == zeroAlpha && Input.GetKeyDown(KeyCode.L)) // Debug key is "L"
        {
            isDebugging = true;
            StartTransitionFadeCoroutine();
            Debug.Log("Debugging: playing transition fade");
        }*/
    }

}
