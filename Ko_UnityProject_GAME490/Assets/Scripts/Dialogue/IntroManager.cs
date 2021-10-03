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

    private GameObject zoneName;
    private GameObject pixelatedCamera;
    private GameObject notificationBubblesHolder;

    private TextMeshProUGUI zoneTextComponent;
    private AudioSource charNoiseSFX;

    private Vector3 originalCameraPos;
    private Vector3 newCameraPosition;
    private Vector3 cameraDestination;

    private TileMovementController playerScript;
    private BlackBars blackBarsScript;
    private TorchMeterScript torchMeterScript;
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

    // Update is called once per frame
    void Update()
    {
        DebuggingCheck();
    }

    // Sets the camera's original position, new position, and it's destination
    private void SetCameraVectors()
    {
        originalCameraPos = pixelatedCamera.transform.position;
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "FirstMap")
            newCameraPosition = new Vector3(23f, 7f, -5.75f);
        if (sceneName == "SecondMap")
            newCameraPosition = new Vector3(24f, 7f, -9f);
        if (sceneName == "ThirdMap")
            newCameraPosition = new Vector3(10f, 7f, -2.75f);
        if (sceneName == "FourthMap")
            newCameraPosition = new Vector3(16f, 7f, -4f);
        if (sceneName == "FifthMap")
            newCameraPosition = new Vector3(31f, 7f, -6f);

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
    }

    // Checks if the zone intro can start - only occurs at the begging of each zone (1st puzzle)
    private void StartIntroCheck()
    {
        if (!playerScript.checkIfOnCheckpoint())
        {           
            StartCoroutine("StartZoneIntro");
        }
        else
        {
            //Debug.Log("Did not start intro");
            gameObject.SetActive(false);
        }
    }

    // Moves the position of the camera towards a new position (endPosition = position to move towards)
    private IEnumerator MoveCamera(Vector3 endPosition)
    {
        while (pixelatedCamera.transform.position != endPosition)
        {
            pixelatedCamera.transform.position = Vector3.MoveTowards(pixelatedCamera.transform.position, endPosition, introCameraSpeed * Time.deltaTime);
            yield return null;
        }

        pixelatedCamera.transform.position = endPosition;
    }

    // Sets up and starts the zone intro
    private IEnumerator StartZoneIntro()
    {
        blackBarsScript.TurnOnBlackBars();
        playerScript.SetPlayerBoolsFalse();

        zoneName.SetActive(true);
        transitionFadeScript.IntroFadeIn();
        torchMeterScript.TurnOffTorchMeter();
        notificationBubblesHolder.SetActive(false);

        pixelatedCamera.transform.position = newCameraPosition;
        StartCoroutine(MoveCamera(cameraDestination));

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
        audioManagerScript.FadeInBackgroundMusic();
        audioManagerScript.FadeInLoopingAmbientSFX();
        blackBarsScript.TurnOffBlackBars();

        transitionFadeScript.GameFadeIn();
        torchMeterScript.TurnOnTorchMeter();
        notificationBubblesHolder.SetActive(true);

        StopAllCoroutines();
        pixelatedCamera.transform.position = originalCameraPos;
        gameObject.SetActive(false);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
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
        charNoiseSFX = audioManagerScript.charNoiseSFX;
        typingSpeed = gameManagerScript.typingSpeed;
        introCameraSpeed = gameManagerScript.introCameraSpeed;
    }

    // Updates the typing delay and intro camera speed - For Debuging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (typingSpeed != gameManagerScript.typingSpeed)
                typingSpeed = gameManagerScript.typingSpeed;

            if (introCameraSpeed != gameManagerScript.introCameraSpeed)
                introCameraSpeed = gameManagerScript.introCameraSpeed;
        }
    }

}
