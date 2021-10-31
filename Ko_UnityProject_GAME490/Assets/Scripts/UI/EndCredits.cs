using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndCredits : MonoBehaviour
{
    private bool hasMovedGameLogo = false;
    private bool hasSkippedCredits = false;
    private bool hasStartedCredits = false;

    [Range(10f, 90f)]
    public float endCreditsLength = 40f;
    [Range(0f, 5f)]
    public float fadeLogoLength = 2f;
    [Range(1f, 5f)]
    public float scrollSpeedMutiplier = 3f;
    private float typingSpeed; // 0.03f
    private float originalVolumeLFSFX; // LFSFX = looping fire sfx;

    [Header("UI Elements")]
    private Image blackOverlay;
    private Image gameLogo;
    private GameObject teamLogo;
    private GameObject endCredits;
    private GameObject gameLogoRef;
    private GameObject teamLogoRef;
    private TextMeshProUGUI endMessage;

    private AudioSource charNoiseSFX;
    private AudioSource loopingFireSFX;

    private Vector2 gameLogoOrigPos;
    private Vector2 teamLogoOrigPos;

    private Color zeroAlphaGL = new Color (1, 1, 1, 0); // GL = game logo
    private Color fullAlphaGL = new Color(1, 1, 1, 1); // GL = game logo
    private Color zeroAlphaBO = new Color(0, 0, 0, 0); // BO = black overlay
    private Color fullAlphaBO = new Color(0, 0, 0, 1); // BO = black overlay

    private IEnumerator fadeOutAudioCoroutine;
    private IEnumerator gameLogoCoroutine;
    private IEnumerator endCreditsCoroutine;
    private IEnumerator endMessageCorouitne;
    private IEnumerator blackOverlayCoroutine;

    private AudioManager audioManagerScript;
    private MainMenuMusicScript mainMenuMusicScript;
    private PauseMenu pauseMenuScript;
    private GameManager gameManagerScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameLogoOrigPos = gameLogo.GetComponent<RectTransform>().anchoredPosition;
        teamLogoOrigPos = teamLogo.GetComponent<RectTransform>().anchoredPosition;
    }

    // Resets the end credits elements to defaults - called at the end of the secondFade's animation via animation event
    /*public void ResetEndCredits()
    {
        StopCoroutine("StartEndCredits");
        StopCoroutine("PlayCreditsMessage");
        StopCoroutine("FadeOutCharNoise");
        SetCreditsBoolsFalse();
        firstFade.SetActive(false);
        gameLogo.SetActive(false);
        endCreditsText.SetActive(false);
        endCreditsMusic.SetActive(false);
        //charNoise.volume = 1f;
        logoAlpha = 0f;
        messageText.text = string.Empty;
        gameLogo.transform.localPosition = gameLogoFirstPosition;
        endCreditsText.transform.localPosition = endCreditsFirstPosition;
        mainMenuMusicScript.FadeInMusicVolume();
        secondFade.GetComponent<Animator>().SetTrigger("FadeIn");
    }*/

    // Starts the end credits sequence
    /*private IEnumerator StartEndCredits()
    {
        firstFade.SetActive(true);
        CheckToFadeMainMenuAudio();
        charNoiseSFX.volume = 1f;

        yield return new WaitForSeconds(2f);
        CheckToFadeLoopAudio();

        yield return new WaitForSeconds(2f);
        gameLogo.SetActive(true);
        StartCoroutine("IncreaseGameLogoAlpha");
        SetCreditsMusicActive();

        yield return new WaitForSeconds(2f);
        endCreditsText.SetActive(true);
        canMoveCredits = true;
        canSkipCredits = true;
        canSpeedUpCredits = true;

        //yield return new WaitForSeconds(3f);
        //canMoveGameLogo = true;
        //canSpeedUpCredits = true;
    }*/

    // Checks if the main menu music is present and can fade
    /*private void CheckToFadeMainMenuAudio()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            mainMenuMusicScript.FadeOutMusicVolume();
    }*/

    // Returns the value of the bool hasStartedCredits
    public bool HasStartedEndCredits
    {
        get
        {
            return hasStartedCredits;
        }
    }

    // Checks to start the end credits
    public void StartEndCredits()
    {
        if (!hasStartedCredits)
        { 
            //Debug.Log("Has Started End Credits");
            //transitionFadeScript.GameFadeOut();
            endCredits.SetActive(true);
            FadeOutAudio();
            hasStartedCredits = true;
        }
    }

    // Checks to skip the end credits
    private void SkipEndCreditsCheck()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !hasSkippedCredits)
        {
            FadeOutOfEndCredits();
            audioManagerScript.FadeOutEndCreditsMusic();
            audioManagerScript.FadeOutCharNoiseSFX();
            hasSkippedCredits = true;
        }
    }

    // Resets all elements so that the end credits can be played again
    private void ResetEndCredits()
    {
        if (endCreditsCoroutine != null)
            StopCoroutine(endCreditsCoroutine);

        if (endMessageCorouitne != null)
            StopCoroutine(endMessageCorouitne);

        transitionFadeScript.GameFadeIn();
        audioManagerScript.FadeInBackgroundMusic();
        audioManagerScript.FadeInLoopingAmbientSFX();
        loopingFireSFX.volume = originalVolumeLFSFX;

        gameLogo.transform.SetParent(endCredits.transform);
        gameLogo.GetComponent<RectTransform>().anchoredPosition = gameLogoOrigPos;
        teamLogo.GetComponent<RectTransform>().anchoredPosition = teamLogoOrigPos;

        blackOverlay.transform.SetAsLastSibling();
        endMessage.text = string.Empty;
        blackOverlay.color = zeroAlphaBO;
        gameLogo.color = zeroAlphaGL;
        endCredits.SetActive(false);
        hasMovedGameLogo = false;
        hasStartedCredits = false;
        hasSkippedCredits = false;
    }

    // Checks when the game logo can move with the text
    private void MoveGameLogoCheck()
    {
        if (teamLogoRef.transform.position.y > gameLogoRef.transform.position.y && !hasMovedGameLogo)
        {
            gameLogo.transform.SetParent(teamLogo.transform);
            hasMovedGameLogo = true;
        }
    }

    // Checks to fade out the zone audio (audio loops and music)
    private void FadeOutZoneAudioCheck()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "MainMenu")
        {
            audioManagerScript.FadeOutBackgroundMusic();
            audioManagerScript.FadeOutLoopingAmbientSFX();
            originalVolumeLFSFX = loopingFireSFX.volume;
            loopingFireSFX.volume = 0;
        }
    }

    // Starts the coroutine that fades out the audio
    private void FadeOutAudio()
    {
        if (fadeOutAudioCoroutine != null)
            StopCoroutine(fadeOutAudioCoroutine);

        fadeOutAudioCoroutine = FadeOutAudioSequence();
        StartCoroutine(fadeOutAudioCoroutine);
    }

    // Starts the coroutine that fades in the game logo
    private void FadeInGameLogo()
    {
        if (gameLogoCoroutine != null)
            StopCoroutine(gameLogoCoroutine);

        gameLogoCoroutine = LerpGameLogo(fadeLogoLength);
        StartCoroutine(gameLogoCoroutine);
    }

    // Starts the coroutine that plays the end credits
    private void PlayEndCredits()
    {
        if (endCreditsCoroutine != null)
            StopCoroutine(endCreditsCoroutine);

        endCreditsCoroutine = EndCreditsSequence();
        StartCoroutine(endCreditsCoroutine);
    }

    // Starts the coroutine that plays a message at the end of credits
    private void PlayEndMessage()
    {
        if (endMessageCorouitne != null)
            StopCoroutine(endMessageCorouitne);

        endMessageCorouitne = EndMessageSequence();
        StartCoroutine(endMessageCorouitne);
    }

    // Starts the coroutine that fades out the black overlay
    private void FadeOutOfEndCredits()
    {
        float fadeOutLength = transitionFadeScript.gameFadeOut;

        if (blackOverlayCoroutine != null)
            StopCoroutine(blackOverlayCoroutine);

        blackOverlayCoroutine = LerpBlackOverlay(fadeOutLength);
        StartCoroutine(blackOverlayCoroutine);
    }

    // Fades out all applicable audio accordingly
    private IEnumerator FadeOutAudioSequence()
    {
        float fadeAudioLength = transitionFadeScript.gameFadeOut;
        // Fade out main menu audio if applicable here

        yield return new WaitForSeconds(fadeAudioLength);
        FadeOutZoneAudioCheck(); // Fade out zone audio if applicable

        yield return new WaitForSeconds(fadeAudioLength);
        FadeInGameLogo();
    }

    // Lerps the alpha of the game logo over a specific duration (duration = seconds) - fades in game logo
    private IEnumerator LerpGameLogo(float duration)
    {
        float time = 0;
        audioManagerScript.FadeInEndCreditsMusic();

        while (time < duration)
        {
            gameLogo.color = Color.Lerp(zeroAlphaGL, fullAlphaGL, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        gameLogo.color = fullAlphaGL;
        PlayEndCredits();
    }

    // Plays the end credits (duration = seconds)
    // Note: normally we'd use anchorPosition rather than localPosition, but we want the center of teamLogo to equal the center of any screen (ignores its anchors but respects its pivots)
    private IEnumerator EndCreditsSequence()
    {
        float time = 0;
        float duration = endCreditsLength;

        Vector2 startPos = new Vector2(teamLogo.transform.localPosition.x, teamLogo.transform.localPosition.y); // Center of teamLogo (ignores UI anchors, but respects pivot position)
        Vector2 endPos = Vector2.zero; // Center of screen

        while (time < duration)
        {
            SkipEndCreditsCheck();
            MoveGameLogoCheck();
            teamLogo.transform.localPosition = Vector2.Lerp(startPos, endPos, time / duration);

            // Checks to speed up credits
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
                time += Time.deltaTime * scrollSpeedMutiplier;
            else
                time += Time.deltaTime;

            yield return null;
        }

        teamLogo.transform.localPosition = endPos;
        PlayEndMessage();
    }

    // Types a message that plays at the end of the credits - when it stops moving
    private IEnumerator EndMessageSequence()
    {
        yield return new WaitForSeconds(1f);
        string endCreditsMessage = "Thanks for playing";

        for (int i = 0; i <= endCreditsMessage.Length; i++)
        {
            endMessage.text = endCreditsMessage.Substring(0, i);

            foreach (char letter in endMessage.text)
            {
                charNoiseSFX.Play();
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        if (!hasSkippedCredits && hasStartedCredits)
        {
            yield return new WaitForSeconds(4f);
            audioManagerScript.FadeOutEndCreditsMusic();
            FadeOutOfEndCredits();
        }
    }

    // Lerps the alpha of the black overlay over a specific duration (duration = seconds) - fades out black overlay
    private IEnumerator LerpBlackOverlay(float duration)
    {
        float time = 0;
        string currentScene = SceneManager.GetActiveScene().name;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(zeroAlphaBO, fullAlphaBO, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = fullAlphaBO;

        if (currentScene == "MainMenu")
        {
            ResetEndCredits();
            // Need to set and load main menu canvas here
        }
        else
        {
            if (endCreditsCoroutine != null)
                StopCoroutine(endCreditsCoroutine);
            if (endMessageCorouitne != null)
                StopCoroutine(endMessageCorouitne);

            yield return new WaitForSeconds(1.5f);
            gameManagerScript.LoadMainMenu();
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            mainMenuMusicScript = FindObjectOfType<MainMenuMusicScript>();

        audioManagerScript = FindObjectOfType<AudioManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        gameManagerScript = FindObjectOfType<GameManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < pauseMenuScript.transform.childCount; i++)
        {
            GameObject child = pauseMenuScript.transform.GetChild(i).gameObject;

            if (child.name == "EndCredits")
            {
                endCredits = child;

                for (int j = 0; j < endCredits.transform.childCount; j++)
                {
                    GameObject child02 = endCredits.transform.GetChild(j).gameObject;

                    if (child02.name == "GameLogo") // set the team logo as the game logos paret if you want it to move it along with the end credits
                    {
                        gameLogo = child02.GetComponent<Image>();

                        for (int k = 0; k < gameLogo.transform.childCount; k++)
                        {
                            GameObject child03 = gameLogo.transform.GetChild(k).gameObject;

                            if (child03.name == "RefGL")
                                gameLogoRef = child03;
                        }
                    }
                    if (child02.name == "TeamLogo")
                    {
                        teamLogo = child02; //lerp the team logo to zero

                        for (int l = 0; l < teamLogo.transform.childCount; l++)
                        {
                            GameObject child04 = teamLogo.transform.GetChild(l).gameObject;

                            if (child04.name == "RefTL")
                                teamLogoRef = child04;
                            if (child04.name == "EndMessage")
                                endMessage = child04.GetComponent<TextMeshProUGUI>();
                        }
                    }
                    if (child02.name == "BlackOverlay")
                        blackOverlay = child02.GetComponent<Image>(); // call the function to fade out via transition fade?
                }
            }
        }

        charNoiseSFX = audioManagerScript.charNoiseAS;
        loopingFireSFX = audioManagerScript.loopingFireAS;
        typingSpeed = gameManagerScript.typingSpeed;
    }

    // Plays the end credits (duration = seconds) - OLD VERSION
    /*private IEnumerator EndCreditsSequence()
    {
        float time = 0;
        float duration = endCreditsLength;
        Vector2 startPos = teamLogo.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = Vector2.zero;

        while (time < duration)
        {
            // Checks to move the game logo along with the credits
            if (teamLogo.GetComponent<RectTransform>().anchoredPosition.y > moveGameLogoAtLength && !hasMovedGameLogo)
            {
                gameLogo.transform.SetParent(teamLogo.transform);
                hasMovedGameLogo = true;
            }
            teamLogo.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        teamLogo.GetComponent<RectTransform>().anchoredPosition = endPos;
        PlayEndMessage();
    }*/

}
