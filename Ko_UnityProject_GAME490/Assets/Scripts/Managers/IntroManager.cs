using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [Header("Camera Variables")]
    [SerializeField] [Range(0f, 100f)]
    private float distanceToTravel = 23f; // Original Value = 23f

    [Header("Zone Name Variables")]
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    [SerializeField] [Range(0f, 5f)]
    private float displayNameDuration = 3f; // Original Value = 3f

    private float typeNameDuration;
    private string zoneName;

    private GameObject zoneIntro;
    private GameObject pixelatedCamera;
    private GameObject notificationBubbles;

    private TextMeshProUGUI zoneText;
    private AudioSource charNoiseSFX;

    private Vector3 cameraOriginalPos;
    private Vector3 cameraStartPos;
    private Vector3 cameraEndPos;

    private IEnumerator cameraCoroutine;

    private TileMovementController playerScript;
    private BlackBars blackBarsScript;
    private TorchMeter torchMeterScript;
    private GameHUD gameHUDScript;
    private CameraController cameraScript;
    private AudioManager audioManagerScript;
    private TransitionFade transitionFadeScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartZoneIntroCheck();
    }

    // Sets the camera vectors and zone name
    private void SetVectorsAndZoneName()
    {
        cameraOriginalPos = pixelatedCamera.transform.position;
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "FirstMap":
                zoneName = "Zone 1: Boreal Forest";
                cameraStartPos = new Vector3(23f, 7f, -5.75f);
                break;
            case "SecondMap":
                zoneName = "Zone 2: Frozen Forest";
                cameraStartPos = new Vector3(24f, 7f, -8f);
                break;
            case "ThirdMap":
                zoneName = "Zone 3: Crystal Cave";
                cameraStartPos = new Vector3(10f, 7f, -2.75f);
                break;
            case "FourthMap":
                zoneName = "Zone 4: Barren Lands";
                cameraStartPos = new Vector3(16f, 7f, -4f);
                break;
            case "FifthMap":
                zoneName = "Zone 5: Power Station";
                cameraStartPos = new Vector3(31f, 7f, -6f);
                break;
            default:
                //Debug.Log("Unrecognizable scene name");
                zoneName = "Zone 0: Sample Zone";
                cameraStartPos = pixelatedCamera.transform.position;
                break;
        }

        cameraEndPos = cameraOriginalPos + new Vector3(distanceToTravel, 0, 0);
    }

    // Checks to start the zone intro
    private void StartZoneIntroCheck()
    {
        // Note01: The intro is currenlty not played during the tutorial
        // Note02: the intro will ONLY play at the beginning of each zone (while on the first puzzle)
        if (SceneManager.GetActiveScene().name != "TutorialMap" && !playerScript.OnCheckpoint())
            StartCoroutine(StartZoneIntro());
        else
        {          
            playerScript.MovePlayerCheck(); // This method will move the player across a bridge if applicable
            transitionFadeScript.GameFadeIn();
            audioManagerScript.FadeInBackgroundMusic();
            audioManagerScript.FadeInLoopingAmbientSFX();

            SetZoneIntroInactve();
        }
    }

    // Returns the time it takes to type the zone name
    private float TypeZoneNameDuration()
    {
        typeNameDuration = 0f;

        foreach (char letter in zoneName)
            typeNameDuration += typingSpeed;

        return typeNameDuration;
    }

    // Sets the zone intro game object and script inactive
    private void SetZoneIntroInactve()
    {
        zoneIntro.SetActive(false);
        gameObject.SetActive(false);
        enabled = false;
    }

    // Starts the coroutine that lerps the camera's position
    private void StartCameraCoroutine()
    {
        StopCameraCoroutine();

        cameraCoroutine = LerpCamera();
        StartCoroutine(cameraCoroutine);
    }

    // Stops the coroutine that lerps the camera's position
    private void StopCameraCoroutine()
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);
    }

    // Lerps the position of the camera to another over a duration (duration = length of zone intro)
    private IEnumerator LerpCamera()
    {
        float time = 0f;
        float duration = transitionFadeScript.introFadeIn + TypeZoneNameDuration() + displayNameDuration + transitionFadeScript.introFadeOut;

        while (time < duration)
        {
            pixelatedCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        pixelatedCamera.transform.position = cameraEndPos;
    }

    // Sets up and starts the zone intro
    private IEnumerator StartZoneIntro()
    {
        pixelatedCamera.transform.position = cameraStartPos;
        notificationBubbles.SetActive(false);

        blackBarsScript.TurnOnBlackBars();
        transitionFadeScript.IntroFadeIn();
        torchMeterScript.TurnOffTorchMeter();
        playerScript.SetPlayerBoolsFalse();
        playerScript.TorchMeterAnimCheck();
        StartCameraCoroutine();

        yield return new WaitForSeconds(transitionFadeScript.introFadeIn);

        foreach (char letter in zoneName)
        {
            zoneText.text += letter;
            charNoiseSFX.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayNameDuration);
        transitionFadeScript.IntroFadeOut();
        zoneText.text = string.Empty;

        yield return new WaitForSeconds(transitionFadeScript.introFadeOut);
        audioManagerScript.FadeInBackgroundMusic();
        audioManagerScript.FadeInLoopingAmbientSFX();

        blackBarsScript.TurnOffBlackBars();
        transitionFadeScript.GameFadeIn();
        torchMeterScript.TurnOnTorchMeter();
        playerScript.MovePlayerCheck();
        StopCameraCoroutine();

        pixelatedCamera.transform.position = cameraOriginalPos;
        notificationBubbles.SetActive(true);
        SetZoneIntroInactve();
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
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ZoneIntro")
            {
                zoneIntro = child;

                for (int j = 0; j < zoneIntro.transform.childCount; j++)
                {
                    GameObject child02 = zoneIntro.transform.GetChild(j).gameObject;

                    if (child02.name == "Text")
                        zoneText = child02.GetComponent<TextMeshProUGUI>();
                }
            }          
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "NotificationBubbles")
                notificationBubbles = child;
        }


        pixelatedCamera = cameraScript.gameObject;
        charNoiseSFX = audioManagerScript.charNoiseAS;
        SetVectorsAndZoneName();
    }

}
