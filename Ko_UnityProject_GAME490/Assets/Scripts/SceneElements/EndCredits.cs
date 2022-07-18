using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndCredits : MonoBehaviour
{
    [SerializeField] [Range(10f, 90f)]
    private float endCreditsDuration = 40f;
    [SerializeField] [Range(0f, 5f)]
    private float fadeLogoDuration = 2f;
    [SerializeField] [Range(1f, 5f)]
    private float scrollSpeedMultiplier = 3f;
    private float typingSpeed = 0.03f; // Original Value = 0.03f

    private string messageToPlay = "Thanks for playing";
    private string mainMenu = "MainMenu";
    private string sceneName;

    private bool hasMovedGameLogo = false;
    private bool hasSkippedCredits = false;
    private bool hasStartedCredits = false;
    private bool isDebugging = false;

    private GameObject teamLogo;
    private GameObject endCredits;
    private GameObject gameLogoRef;
    private GameObject teamLogoRef;

    private RectTransform gameLogoRT;
    private RectTransform teamLogoRT;

    private TextMeshProUGUI endMessage;
    private Image blackOverlay;
    private Image gameLogo;

    private Vector2 gameLogoOrigPos;
    private Vector2 teamLogoOrigPos;

    private IEnumerator audioCoroutine;
    private IEnumerator gameLogoCoroutine;
    private IEnumerator endCreditsCoroutine;
    private IEnumerator endMessageCoroutine;
    private IEnumerator lerpOverlayCoroutine;

    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private LevelManager levelManagerScript;

    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;

    public bool HasStartedCredits
    {
        get { return hasStartedCredits; }
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
        SetEndCreditsInactive();
    }

    // Checks to set the end credits game object and script inactive
    private void SetEndCreditsInactive()
    {
        if (sceneName == mainMenu || sceneName == "FifthMap") return;

        endCredits.SetActive(false);
        gameObject.SetActive(false);
        enabled = false;
    }

    // Checks to start the end credits
    public void StartEndCredits()
    {
        if (hasStartedCredits) return;

        endMessage.text = string.Empty;
        endCredits.SetActive(true);
        hasStartedCredits = true;
        StartAudioCoroutine();
    }

    // Checks to skip the end credits
    private void SkipEndCredits()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (hasSkippedCredits) return;

        audioManagerScript.FadeOutEndCreditsMusic();
        hasSkippedCredits = true;
        FadeOutOfEndCredits();
    }

    // Resets the end credits
    private void ResetEndCredits()
    {
        if (endCreditsCoroutine != null) 
            StopCoroutine(endCreditsCoroutine);

        if (endMessageCoroutine != null) 
            StopCoroutine(endMessageCoroutine);

        blackOverlayScript.IsChangingScenes = false;
        blackOverlayScript.GameFadeIn();
        FadeInAudioCheck();

        gameLogo.transform.SetParent(endCredits.transform);
        blackOverlay.transform.SetAsLastSibling();

        gameLogoRT.anchoredPosition = gameLogoOrigPos;
        teamLogoRT.anchoredPosition = teamLogoOrigPos;

        blackOverlay.SetImageAlpha(0f);
        gameLogo.SetImageAlpha(0f);

        endMessage.text = string.Empty;
        endCredits.SetActive(false);
        hasMovedGameLogo = false;
        hasStartedCredits = false;
        hasSkippedCredits = false;
    }

    // Checks to speed up the end credits - returns true if so, false otherwise
    private bool CanSpeedUpCredits()
    {
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.KeypadEnter)) return false;

        return true;
    }

    // Checks to move the game logo can
    private void MoveGameLogoCheck()
    {
        if (teamLogoRef.transform.position.y < gameLogoRef.transform.position.y) return;

        if (hasMovedGameLogo) return;

        gameLogo.transform.SetParent(teamLogo.transform);
        hasMovedGameLogo = true;
    }

    // Checks to fade out the appropriate audio
    private void FadeOutAudioCheck()
    {
        audioManagerScript.FadeOutBackgroundMusic();
        if (sceneName == mainMenu) return;

        audioManagerScript.FadeOutAmbientWindSFX();
        audioManagerScript.SetTorchFireVolume(0f);
    }

    // Checks to fade in the appropriate audio
    private void FadeInAudioCheck()
    {
        audioManagerScript.FadeInBackgroundMusic();
        if (sceneName == mainMenu) return;

        audioManagerScript.FadeInAmbientWindSFX();
        audioManagerScript.ResetTorchFireVolume();
    }

    // Starts the coroutine that fades out the audio
    private void StartAudioCoroutine()
    {
        if (audioCoroutine != null) StopCoroutine(audioCoroutine);

        audioCoroutine = AudioSequence();
        StartCoroutine(audioCoroutine);
    }

    // Starts the coroutine that fades in the game logo
    private void StartGameLogoCoroutine()
    {
        if (gameLogoCoroutine != null) StopCoroutine(gameLogoCoroutine);

        gameLogoCoroutine = LerpGameLogo();
        StartCoroutine(gameLogoCoroutine);
    }

    // Starts the coroutine that starts/plays the end credits
    private void StartEndCreditsCoroutine()
    {
        if (endCreditsCoroutine != null) StopCoroutine(endCreditsCoroutine);

        endCreditsCoroutine = EndCreditsSequence();
        StartCoroutine(endCreditsCoroutine);
    }

    // Starts the coroutine that plays the end message
    private void StartEndMessageCoroutine()
    {
        if (endMessageCoroutine != null) StopCoroutine(endMessageCoroutine);

        endMessageCoroutine = EndMessageSequence();
        StartCoroutine(endMessageCoroutine);
    }

    // Starts the coroutine that fades out of the end credits
    private void FadeOutOfEndCredits()
    {
        if (lerpOverlayCoroutine != null) StopCoroutine(lerpOverlayCoroutine);

        lerpOverlayCoroutine = LerpBlackOverlay();
        StartCoroutine(lerpOverlayCoroutine);
    }

    // Fades out all applicable audio accordingly
    private IEnumerator AudioSequence()
    {
        float duration = blackOverlayScript.GameFadeDuration;

        yield return new WaitForSeconds(duration);
        FadeOutAudioCheck();

        yield return new WaitForSeconds(duration);
        StartGameLogoCoroutine();
    }

    // Lerps the alpha of the game logo to another over a duration (duration = seconds)
    private IEnumerator LerpGameLogo()
    {
        Color startColor = gameLogo.ReturnImageColor(0f);
        Color endColor = gameLogo.ReturnImageColor();
        float duration = fadeLogoDuration;
        float time = 0;

        audioManagerScript.FadeInEndCreditsMusic();
        gameLogo.color = startColor;

        while (time < duration)
        {
            gameLogo.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        gameLogo.color = endColor;
        StartEndCreditsCoroutine();
    }

    // Plays the end credits over a duration (duration = seconds)
    private IEnumerator EndCreditsSequence()
    {
        // Note: using local position here instead anchored position ignores the team logo's UI anchors, but will still respect its pivot position
        // Note: so using Vector.zero in local posiiton will always equal the center of the screen - we can avoid calculating screen sizes to center the logo
        Vector2 startPosition = new Vector2(teamLogoRT.localPosition.x, teamLogoRT.localPosition.y); 
        Vector2 endPosition = Vector2.zero;
        float duration = endCreditsDuration;
        float time = 0;

        while (time < duration)
        {
            MoveGameLogoCheck();
            SkipEndCredits();

            teamLogo.transform.localPosition = Vector2.Lerp(startPosition, endPosition, time / duration);
            time += !CanSpeedUpCredits() ? Time.deltaTime : Time.deltaTime * scrollSpeedMultiplier;
            yield return null;
        }

        teamLogo.transform.localPosition = endPosition;
        StartEndMessageCoroutine();
    }

    // Types the end message
    private IEnumerator EndMessageSequence()
    {
        yield return new WaitForSeconds(1f);

        foreach (char letter in messageToPlay)
        {
            endMessage.text += letter;
            if (!hasSkippedCredits) audioManagerScript.PlayCharNoiseSFX();
            yield return new WaitForSeconds(typingSpeed);
        }

        if (hasSkippedCredits || !hasStartedCredits) yield break;
        yield return new WaitForSeconds(4f);
        audioManagerScript.FadeOutEndCreditsMusic();
        FadeOutOfEndCredits();
    }

    // Lerps the alpha of the overlay to another over a duration (duration = seconds)
    private IEnumerator LerpBlackOverlay()
    {
        Color startColor = blackOverlay.ReturnImageColor(0f);
        Color endColor = blackOverlay.ReturnImageColor();
        float duration = blackOverlayScript.GameFadeDuration;
        float time = 0;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = endColor;
        StartCoroutine(FinishedEndCreditsCheck());
    }

    // Checks which methods to call after the credits have finished playing
    private IEnumerator FinishedEndCreditsCheck()
    {
        if (ResetEndCreditsDebug()) yield break;

        if (sceneName == mainMenu)
        {
            ResetEndCredits();

            yield return new WaitForSeconds(blackOverlayScript.GameFadeDuration * 0.2f);
            mainMenuScript.PopInMainMenu();
        }
        else
        {
            if (endCreditsCoroutine != null)
                StopCoroutine(endCreditsCoroutine);

            if (endMessageCoroutine != null)
                StopCoroutine(endMessageCoroutine);

            yield return new WaitForSeconds(1.5f);
            levelManagerScript.LoadNextScene();
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>() : null;
        pauseMenuScript = (sceneName != mainMenu) ? FindObjectOfType<PauseMenu>() : null;
        mainMenuScript = (sceneName == mainMenu) ? FindObjectOfType<MainMenu>() : null;

        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        levelManagerScript = FindObjectOfType<LevelManager>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "EndCredits":
                    endCredits = child.gameObject;
                    break;
                case "EC_BlackOverlay":
                    blackOverlay = child.GetComponent<Image>();
                    break;
                case "EC_Message":
                    endMessage = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "EC_GameLogo":
                    gameLogoRT = child.GetComponent<RectTransform>();
                    gameLogo = child.GetComponent<Image>();
                    break;
                case "GL_Ref":
                    gameLogoRef = child.gameObject;
                    break;
                case "EC_TeamLogo":
                    teamLogoRT = child.GetComponent<RectTransform>();
                    teamLogo = child.gameObject;
                    break;
                case "TL_Ref":
                    teamLogoRef = child.gameObject;
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
        gameLogoOrigPos = gameLogoRT.anchoredPosition;
        teamLogoOrigPos = teamLogoRT.anchoredPosition;
    }

    // Checks to start the end credits - For Debugging Purposes ONLY
    [ContextMenu("Start End Credits")]
    private void StartEndCreditsDebug()
    {
        if (pauseMenuScript != null) pauseMenuScript.CanPause = false;
        if (playerScript != null) playerScript.SetPlayerBoolsFalse();
        isDebugging = true;

        audioManagerScript.FadeOutGeneratorSFX();
        blackOverlayScript.GameFadeOut();
        StartEndCredits();
    }

    // Checks to reset the end credits - For Debugging Purposes ONLY
    private bool ResetEndCreditsDebug()
    {
        if (!isDebugging) return false;

        if (pauseMenuScript != null) pauseMenuScript.CanPause = true;
        if (playerScript != null) playerScript.SetPlayerBoolsTrue();
        isDebugging = false;

        ResetEndCredits();
        return true;
    }

}
