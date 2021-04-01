using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndCredits : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    private Image image;

    [Header("GameObjects")]
    public GameObject firstFade;
    public GameObject secondFade;
    public GameObject endCredits;
    public GameObject gameLogo;

    [Header("Destinations")]
    public Transform endCreditsDestination;
    public Transform gameLogoDestination;
    Vector3 gameLogoFirstPosition;
    Vector3 endCreditsFirstPosition;

    [Header("Audio")]
    public GameObject endCreditsMusic;
    public AudioSource charNoise;

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
    //public float endCreditsPositionY;
    private string endCreditsMessage;
    private string currentText = "";

    private AudioLoops audioLoopsScript;
    private TileMovementController playerScript;
    private TorchMeterScript torchMeterScript;
    private MainMenuMusicScript mainMenuMusicScript;

    void Awake()
    {
        SetScriptsCheck();
    }

    // Start is called before the first frame update
    void Start()
    {      
        gameLogoFirstPosition = gameLogo.transform.localPosition;
        endCreditsFirstPosition = endCredits.transform.localPosition;
        image = gameLogo.GetComponent<Image>();
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
        image.color = new Color(1, 1, 1, logoAlpha);

        StartEndCreditsCheck();
        //CheckIfCanMove();
        CheckToStopMove();
        SpeedUpCreditsCheck();
        CanSkipCreditsCheck();
    }

    void LateUpdate()
    {
        CheckIfCanMove();
    }

    // Calls the function that starts the credits - for the credits button in the main menu
    public void StartEndCreditsManually()
    {
        if(!hasStartedCredits)
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
        messageText.text = "";
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
            audioLoopsScript = FindObjectOfType<AudioLoops>();
            playerScript = FindObjectOfType<TileMovementController>();
            torchMeterScript = FindObjectOfType<TorchMeterScript>();
        }
        else
            mainMenuMusicScript = FindObjectOfType<MainMenuMusicScript>();
    }

    // Checks to see if the credits can be played
    private void StartEndCreditsCheck()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
        {
            if (playerScript.checkIfCompletedLevel() && !hasStartedCredits)
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
    private void CheckIfCanMove()
    {
        if (canMoveGameLogo)
            gameLogo.transform.localPosition = Vector3.MoveTowards(gameLogo.transform.localPosition, gameLogoDestination.localPosition, scrollSpeed * Time.deltaTime);
        if (canMoveCredits)
            endCredits.transform.localPosition = Vector3.MoveTowards(endCredits.transform.localPosition, endCreditsDestination.localPosition, scrollSpeed * Time.deltaTime);
    }

    // Checks when the logo and credits should stop moving - when the team logo is centered on the screen
    private void CheckToStopMove()
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
        charNoise.volume = 1f;

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
            TypeEndCreditsText();
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

    // Plays an SFX for every character in the end credits message
    private void TypeEndCreditsText()
    {
        foreach (char letter in messageText.text)
        {
            charNoise.Play();
        }
    }

    // Increases the alpha of the game logo over time until it reaches its max value
    private IEnumerator IncreaseGameLogoAlpha()
    {
        for (float i = 0f; i <= 1; i += 0.015f)
        {
            i = logoAlpha;
            logoAlpha += 0.015f;
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
            charNoise.volume = charNoiseAudio;
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
            audioLoopsScript.FadeOutAudioLoops();
            torchMeterScript.audioSource.volume = 0f;
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

        if (Input.GetKey(KeyCode.Return) && canSpeedUpCredits)
            scrollSpeed = 270f;
        else
            scrollSpeed = 90f;
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

}
