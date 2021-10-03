using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndCredits : MonoBehaviour
{
    [Header("Bools")]
    public bool hasEndedCredits;
    public bool hasStartedCredits;
    private bool canMoveGameLogo;
    private bool canMoveCredits;
    private bool hasPlayedEndMessage;
    private bool canSpeedUpCredits;
    private bool canSkipCredits;

    [Header("Floats")]
    public float typingDelay = 0.03f;
    public float scrollSpeed;
    private float creditsBGM = 0.4f;
    private float charNoiseAudio = 1f;
    private float logoAlpha = 0f;
    private string endCreditsMessage;
    private string currentText = string.Empty;

    [Header("UI Elements")]
    public TextMeshProUGUI messageText;
    private Image gameLogoImage;

    [Header("GameObjects")]
    public GameObject firstFade;
    public GameObject secondFade;
    public GameObject endCredits;
    public GameObject gameLogo;

    [Header("Destinations")]
    public Transform endCreditsDestination;
    public Transform gameLogoDestination;
    private Vector3 gameLogoFirstPosition;
    private Vector3 endCreditsFirstPosition;

    [Header("Audio")]
    public GameObject endCreditsMusic;
    private AudioSource charNoiseSFX;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private MainMenuMusicScript mainMenuMusicScript;

    void Awake()
    {
        SetScriptsCheck();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameLogoFirstPosition = gameLogo.transform.localPosition;
        endCreditsFirstPosition = endCredits.transform.localPosition;
        gameLogoImage = gameLogo.GetComponent<Image>();
        endCreditsMusic.GetComponent<AudioSource>().volume = creditsBGM;
        hasEndedCredits = false;
        hasStartedCredits = false;
        canMoveCredits = false;
        canMoveGameLogo = false;
        hasPlayedEndMessage = false;
        canSpeedUpCredits = false;
        canSkipCredits = false;
    }

    void Update()
    {        
        StartEndCreditsCheck();
        StopMovingCreditsCheck();
        SpeedUpCreditsCheck();
        CanSkipCreditsCheck();
        //canMoveCreditsCheck();
    }

    void LateUpdate()
    {
        canMoveCreditsCheck();
    }

    // Calls the function that starts the credits - for the credits button in the main menu
    public void StartEndCreditsManually()
    {
        if (!hasStartedCredits)
        {
            StartCoroutine("StartEndCredits");
            hasStartedCredits = true;
        }
    }

    // Resets the end credits elements to defaults - called at the end of the secondFade's animation via animation event
    public void ResetEndCredits()
    {
        StopCoroutine("StartEndCredits");
        StopCoroutine("PlayCreditsMessage");
        StopCoroutine("FadeOutCharNoise");
        SetCreditsBoolsFalse();
        firstFade.SetActive(false);
        gameLogo.SetActive(false);
        endCredits.SetActive(false);
        endCreditsMusic.SetActive(false);
        //charNoise.volume = 1f;
        logoAlpha = 0f;
        messageText.text = string.Empty;
        gameLogo.transform.localPosition = gameLogoFirstPosition;
        endCredits.transform.localPosition = endCreditsFirstPosition;
        mainMenuMusicScript.FadeInMusicVolume();
        secondFade.GetComponent<Animator>().SetTrigger("FadeIn");
    }

    // Checks to see which scripts can be found
    private void SetScriptsCheck()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            playerScript = FindObjectOfType<TileMovementController>();
            audioManagerScript = FindObjectOfType<AudioManager>();
        }
        else
            mainMenuMusicScript = FindObjectOfType<MainMenuMusicScript>();
    }

    // Checks to see if the credits can be played
    private void StartEndCreditsCheck()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            if (playerScript.bridge.name == "EndBridge" && !hasStartedCredits)
            {
                StartCoroutine("StartEndCredits");
                hasStartedCredits = true;
            }
        }
        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.P) && !hasStartedCredits)
        {
            StartCoroutine("StartEndCredits");
            hasStartedCredits = true;
        }
        /*** End Debugging ***/
    }

    // Checks if the logo and the credits can start moving
    private void canMoveCreditsCheck()
    {
        if (canMoveGameLogo)
            gameLogo.transform.localPosition = Vector3.MoveTowards(gameLogo.transform.localPosition, gameLogoDestination.localPosition, scrollSpeed * Time.deltaTime);
        if (canMoveCredits)
            endCredits.transform.localPosition = Vector3.MoveTowards(endCredits.transform.localPosition, endCreditsDestination.localPosition, scrollSpeed * Time.deltaTime);
    }

    // Checks when the logo and credits should stop moving - when the team logo is centered on the screen
    private void StopMovingCreditsCheck()
    {
        if (endCredits.transform.localPosition == endCreditsDestination.localPosition && !hasPlayedEndMessage)
        {
            StartCoroutine("PlayCreditsMessage");
            canMoveCredits = false;
            canMoveGameLogo = false;
            hasPlayedEndMessage = true;
        }
    }

    // Starts the end credits sequence
    private IEnumerator StartEndCredits()
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
        endCredits.SetActive(true);
        canMoveCredits = true;
        canSkipCredits = true;
        canSpeedUpCredits = true;

        //yield return new WaitForSeconds(3f);
        //canMoveGameLogo = true;
        //canSpeedUpCredits = true;
    }

    // Types a message that plays at the end of the credits - when it stops moving
    private IEnumerator PlayCreditsMessage()
    {
        canSpeedUpCredits = false;

        yield return new WaitForSeconds(1f);
        endCreditsMessage = "Thanks for playing";

        for (int i = 0; i <= endCreditsMessage.Length; i++)
        {
            currentText = endCreditsMessage.Substring(0, i);
            messageText.text = currentText;

            foreach (char letter in messageText.text)
            {
                charNoiseSFX.Play();
            }
            yield return new WaitForSeconds(typingDelay);
        }

        yield return new WaitForSeconds(4f);
        canSkipCredits = false;
        hasEndedCredits = true;
        hasStartedCredits = false;
        secondFade.SetActive(true); // Next scene is loaded or canvas is re-activated at the end of the fade animation via animation event 
        
        yield return new WaitForSeconds(1f);
        StartCoroutine("FadeOutMusicEC");
    }

    // Stops playing the credits - used only while in the main menu
    private IEnumerator StopPlayingCredits()
    {
        hasEndedCredits = true;
        hasStartedCredits = false;
        secondFade.SetActive(true);
        charNoiseAudio = 1f;
        StartCoroutine("FadeOutCharNoise");

        yield return new WaitForSeconds(1f);      
        StartCoroutine("FadeOutMusicEC");     
    }

    // Increases the alpha of the game logo over time until it reaches its max value
    private IEnumerator IncreaseGameLogoAlpha()
    {
        for (float i = 0f; i <= 1; i += 0.015f)
        {
            i = logoAlpha;
            logoAlpha += 0.015f;
            gameLogoImage.color = new Color(1, 1, 1, logoAlpha);
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Decreases the creditsBGM volume until it reaches its min value
    private IEnumerator FadeOutMusicEC()
    {
        for (float i = 0.4f; i >= 0f; i -= 0.01f)
        {
            i = creditsBGM;
            creditsBGM -= 0.01f;
            endCreditsMusic.GetComponent<AudioSource>().volume = creditsBGM;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Decreases the charNoise volume until it reaches its min value - ONLY used if you skip the credits
    private IEnumerator FadeOutCharNoise()
    {
        for (float j = 1f; j >= 0; j -= 0.02f)
        {
            j = charNoiseAudio;
            charNoiseAudio -= 0.02f;
            charNoiseSFX.volume = charNoiseAudio;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Sets the volume and volume float to the correct values before starting coroutine
    private void SetCreditsMusicActive()
    {
        endCreditsMusic.GetComponent<AudioSource>().volume = 0.4f;
        creditsBGM = 0.4f;
        endCreditsMusic.SetActive(true);
    }

    // Checks if the audio loops are present and can fade
    private void CheckToFadeLoopAudio()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            //audioLoopsScript.FadeOutAudioLoops();
            audioManagerScript.FadeOutBackgroundMusic();
            audioManagerScript.FadeOutLoopingAmbientSFX();
            audioManagerScript.loopingFireSFX.volume = 0f;
        }
    }

    // Checks if the main menu music is present and can fade
    private void CheckToFadeMainMenuAudio()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            mainMenuMusicScript.FadeOutMusicVolume();
    }

    // Checks to see when you can speed up credits
    private void SpeedUpCreditsCheck()
    {
        if (endCredits.transform.localPosition.y >= 280f && !canMoveGameLogo)
            canMoveGameLogo = true;

        if (canSpeedUpCredits)
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.KeypadEnter))
                scrollSpeed = 270f;
            else
                scrollSpeed = 90f;
        }
    }

    // Checks to see if the credits can be skipped - can only skip in the main menu
    private void CanSkipCreditsCheck()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" && Input.GetKeyDown(KeyCode.Escape) && canSkipCredits)
        {
            Debug.Log("Credits Have Been Skipped");
            StartCoroutine("StopPlayingCredits");
            canSkipCredits = false;
        }
    }

    // Sets all of the bools to false
    private void SetCreditsBoolsFalse()
    {
        canSpeedUpCredits = false;
        canMoveCredits = false;
        canMoveGameLogo = false;

        hasEndedCredits = false;
        hasPlayedEndMessage = false;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        charNoiseSFX = audioManagerScript.charNoiseSFX;
    }

}
