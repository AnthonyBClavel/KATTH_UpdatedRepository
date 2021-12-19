using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class IntroManager : MonoBehaviour
{
    private float typingSpeed; // 3f
    private float introCameraSpeed; // 3f

    private string zoneNameText;
    private string currentText = string.Empty;
    private bool isZoneIntro = false;

    private GameObject zoneName;
    private GameObject pixelatedCamera;
    private GameObject notificationBubblesHolder;

    private Vector3 originalCameraPos;
    private Vector3 newCameraPosition;
    private Vector3 cameraDestination;

    private TextMeshProUGUI zoneTextComponent;
    private AudioSource charNoiseSFX;
    private IEnumerator cameraCoroutine;

    private TileMovementController playerScript;
    private BlackBars blackBarsScript;
    private TorchMeter torchMeterScript;
    private GameHUD gameHUDScript;
    private CameraController cameraScript;
    private AudioManager audioManagerScript;
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
        SetCameraVectors();
        StartIntroCheck(); // MUST be called LAST in start(), NOT in awake()!
    }

    // Returns or sets the introCameraSpeed
    public float IntroCameraSpeed
    {
        get
        {
            return introCameraSpeed;
        }
        set
        {
            introCameraSpeed = value;
        }
    }

    // Returns or sets the typingSpeed
    public float TypingSpeed
    {
        get
        {
            return typingSpeed;
        }
        set
        {
            typingSpeed = value;
        }
    }

    // Sets the camera's original position, new position, and it's destination
    private void SetCameraVectors()
    {
        originalCameraPos = pixelatedCamera.transform.position;
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "FirstMap")
            newCameraPosition = new Vector3(23f, 7f, -5.75f);
        else if (sceneName == "SecondMap")
            newCameraPosition = new Vector3(24f, 7f, -8f);
        else if (sceneName == "ThirdMap")
            newCameraPosition = new Vector3(10f, 7f, -2.75f);
        else if (sceneName == "FourthMap")
            newCameraPosition = new Vector3(16f, 7f, -4f);
        else if (sceneName == "FifthMap")
            newCameraPosition = new Vector3(31f, 7f, -6f);
        else
            newCameraPosition = pixelatedCamera.transform.position;

        cameraDestination = new Vector3(newCameraPosition.x + 70f, newCameraPosition.y, newCameraPosition.z);
    }

    // Determines the zone name to display
    private void DisplayZoneName()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "FirstMap")
            zoneNameText = "Zone 1: Boreal Forest";
        else if (sceneName == "SecondMap")
            zoneNameText = "Zone 2: Frozen Forest";
        else if (sceneName == "ThirdMap")
            zoneNameText = "Zone 3: Crystal Cave";
        else if (sceneName == "FourthMap")
            zoneNameText = "Zone 4: Barren Lands";
        else if (sceneName == "FifthMap")
            zoneNameText = "Zone 5: Power Station";
        else
            zoneNameText = "Zone 0: Sample Zone";
    }

    // Checks if the zone intro can start - only occurs at the begging of each zone (1st puzzle)
    private void StartIntroCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (!playerScript.checkIfOnCheckpoint() && sceneName != "TutorialMap") 
            StartCoroutine(StartZoneIntro());
        else
        {
            gameObject.SetActive(false);
            transitionFadeScript.GameFadeIn();
            audioManagerScript.FadeInBackgroundMusic();
            audioManagerScript.FadeInLoopingAmbientSFX();
        }
    }

    // Starts the coroutine that moves the camera
    private void StartCameraCoroutine()
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        cameraCoroutine = MoveCamera();
        StartCoroutine(cameraCoroutine);
    }

    // Moves the camera to the right until the intro coroutine is finished
    private IEnumerator MoveCamera()
    {
        gameManagerScript.CheckForIntroManagerDebug();

        while (isZoneIntro)
        {
            //pixelatedCamera.transform.position = Vector3.MoveTowards(pixelatedCamera.transform.position, cameraDestination, introCameraSpeed * Time.deltaTime);
            float newCameraPosX = pixelatedCamera.transform.position.x + (Time.deltaTime * introCameraSpeed);
            pixelatedCamera.transform.position = new Vector3(newCameraPosX, newCameraPosition.y, newCameraPosition.z);
            yield return null;
        }
    }

    // Sets up and starts the zone intro
    private IEnumerator StartZoneIntro()
    {
        isZoneIntro = true;
        blackBarsScript.TurnOnBlackBars();
        playerScript.SetPlayerBoolsFalse();

        zoneName.SetActive(true);
        transitionFadeScript.IntroFadeIn();
        torchMeterScript.TurnOffTorchMeter();
        notificationBubblesHolder.SetActive(false);

        pixelatedCamera.transform.position = newCameraPosition;
        StartCameraCoroutine();

        yield return new WaitForSeconds(transitionFadeScript.introFadeIn);
        DisplayZoneName();

        for (int i = 0; i <= zoneNameText.Length; i++)
        {
            currentText = zoneNameText.Substring(0, i);
            zoneTextComponent.text = currentText;

            foreach (char letter in zoneTextComponent.text)
                charNoiseSFX.Play();

            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(3f);
        transitionFadeScript.IntroFadeOut();
        zoneName.SetActive(false);

        yield return new WaitForSeconds(transitionFadeScript.introFadeOut);
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        audioManagerScript.FadeInBackgroundMusic();
        audioManagerScript.FadeInLoopingAmbientSFX();
        blackBarsScript.TurnOffBlackBars();

        transitionFadeScript.GameFadeIn();
        torchMeterScript.TurnOnTorchMeter();
        notificationBubblesHolder.SetActive(true);

        pixelatedCamera.transform.position = originalCameraPos;
        gameObject.SetActive(false);
        isZoneIntro = false;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        cameraScript = FindObjectOfType<CameraController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "ZoneName")
                zoneName = child;
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "NotificationBubblesHolder")
                notificationBubblesHolder = child;
        }

        zoneTextComponent = zoneName.GetComponent<TextMeshProUGUI>();

        pixelatedCamera = cameraScript.gameObject;
        typingSpeed = gameManagerScript.typingSpeed;
        introCameraSpeed = gameManagerScript.introCameraSpeed;
        charNoiseSFX = audioManagerScript.charNoiseAS;
    }

}
