using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFade : MonoBehaviour
{
    [Header("Fade Durations (in seconds)")]
    [Range(0.1f, 5.0f)]
    public float gameFadeIn = 2f;
    [Range(0.1f, 5.0f)]
    public float gameFadeOut = 2f;
    [Range(0.1f, 5.0f)]
    public float introFadeIn = 2f;
    [Range(0.1f, 5.0f)]
    public float introFadeOut = 2f;
    [Range(0.1f, 10.0f)]
    public float fadeOutAndIn = 2f;

    private Image transitionFade;
    private Image zoneIntroBlackOverlay;

    private IEnumerator transitionFadeCoroutine;

    private Color32 zeroAlpha = new Color(0, 0, 0, 0);
    private Color32 halfAlpha = new Color(0, 0, 0, 0.5f);
    private Color32 fullAlpha = new Color(0, 0, 0, 1);

    private PauseMenu pauseMenuScript;
    private IntroManager introManagerScript;
    private TileMovementController playerScript;
    private GameHUD gameHUDScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        introManagerScript = FindObjectOfType<IntroManager>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        gameManagerScript = FindObjectOfType<GameManager>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        pauseMenuScript.canPause = false;

        if (playerScript.checkIfOnCheckpoint())
            GameFadeIn();
    }

    // Fades into the game
    public void GameFadeIn()
    {
        GameObject blackOverlay = zoneIntroBlackOverlay.gameObject;

        if (zoneIntroBlackOverlay.color != zeroAlpha)
            zoneIntroBlackOverlay.color = zeroAlpha;

        if (blackOverlay.activeSelf)
            blackOverlay.SetActive(false);

        transitionFade.color = fullAlpha;
        StartCoroutine(LerpGameFade(zeroAlpha, gameFadeIn));
    }

    // Fades out of the game
    public void GameFadeOut()
    {
        pauseMenuScript.isChangingScenes = true;
        //playerScript.canSetBoolsTrue = false;
        playerScript.SetPlayerBoolsFalse();
        DisableMenuInputs();

        if (Time.timeScale != 1f)
            Time.timeScale = 1f;

        transitionFade.color = zeroAlpha;
        StartCoroutine(LerpGameFade(fullAlpha, gameFadeOut));
    }

    // Fades into the zone intro - for intro manager script ONLY
    public void IntroFadeIn()
    {
        zoneIntroBlackOverlay.color = fullAlpha;
        StartCoroutine(LerpZoneIntroFade(halfAlpha, introFadeIn));
    }

    // Fades out of the zone intro - for intro manager script ONLY
    public void IntroFadeOut()
    {
        zoneIntroBlackOverlay.color = halfAlpha;
        StartCoroutine(LerpZoneIntroFade(fullAlpha, introFadeOut));
    }
    
    // Fades out and then in - for artifact script ONLY (transition to the artifact view)
    public void PlayTransitionFade()
    {
        transitionFade.color = zeroAlpha;

        if (transitionFadeCoroutine != null)
            StopCoroutine(transitionFadeCoroutine);

        transitionFadeCoroutine = LerpTransitionFade(fadeOutAndIn);
        StartCoroutine(transitionFadeCoroutine);
    }

    // Lerps the color of the image to another, over a specific duration (endValue = color to lerp to, duration = seconds)
    private IEnumerator LerpGameFade(Color endValue, float duration)
    {
        yield return new WaitForSeconds(0.1f); // Just a small delay to make transition between fades cleaner
        float time = 0;
        Color startValue = transitionFade.color;

        while (time < duration)
        {
            transitionFade.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transitionFade.color = endValue;

        // Players can pause after the game fades in
        if (!pauseMenuScript.canPause && !pauseMenuScript.isChangingScenes)
            pauseMenuScript.canPause = true;
        // Loads the next scene if applicable
        if (pauseMenuScript.isChangingScenes)
            gameManagerScript.LoadNextSceneCheck();
    }

    // Lerps the color of the image to another, over a specific duration - for the zone intro ONLY
    private IEnumerator LerpZoneIntroFade(Color endValue, float duration)
    {
        float time = 0;
        Color startValue = zoneIntroBlackOverlay.color;

        while (time < duration)
        {
            zoneIntroBlackOverlay.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        zoneIntroBlackOverlay.color = endValue;
    }

    // Lerps the color of the image to another, over a specific duration - for the zone intro ONLY
    private IEnumerator LerpTransitionFade(float duration)
    {
        pauseMenuScript.canPause = false;
        float time01 = 0;
        while (time01 < (duration / 2))
        {
            transitionFade.color = Color.Lerp(zeroAlpha, fullAlpha, time01 / (duration / 2));
            time01 += Time.deltaTime;
            yield return null;
        }

        transitionFade.color = fullAlpha;

        yield return new WaitForSeconds(0.1f); // Just a small delay to make transition between fades cleaner
        float time02 = 0;
        while (time02 < (duration / 2))
        {
            transitionFade.color = Color.Lerp(fullAlpha, zeroAlpha, time02 / (duration / 2));
            time02 += Time.deltaTime;
            yield return null;
        }

        transitionFade.color = zeroAlpha;
        pauseMenuScript.canPause = true;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "TransitionFade")
                transitionFade = child.GetComponent<Image>();
        }

        for (int i = 0; i < introManagerScript.transform.childCount; i++)
        {
            GameObject child = introManagerScript.transform.GetChild(i).gameObject;

            if (child.name == "BlackOverlay")
                zoneIntroBlackOverlay = child.GetComponent<Image>();
        }
    }

    // Fades the game in or out - For Debugging Purposes Only!
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Semicolon) && transitionFade.color == zeroAlpha) // ;
            {
                Debug.Log("Debugging: Game Is Fading Out");
                StartCoroutine(LerpGameFade(fullAlpha, gameFadeOut));
            }
            if (Input.GetKeyDown(KeyCode.Quote) && transitionFade.color == fullAlpha) // '
            {
                Debug.Log("Debugging: Game Is Fading In");
                StartCoroutine(LerpGameFade(zeroAlpha, gameFadeIn));
            }
        }
    }  

    // Enables all UI inputs
    private void EnableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
    }

    // Disables all UI inputs
    private void DisableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
    }

}
