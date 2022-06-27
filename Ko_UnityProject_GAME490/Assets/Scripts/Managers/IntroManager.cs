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
    private GameObject mainCamera;
    private GameObject notificationBubbles;

    private TextMeshProUGUI zoneText;
    private AudioSource charNoiseSFX;

    private Vector3 cameraOriginalPos;
    private Vector3 cameraStartPos;
    private Vector3 cameraEndPos;
    private Vector3 cameraOriginalRot;
    private Vector3 cameraDefaultRot = new Vector3(54, 0, 0);

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

    // Checks to start the zone intro
    private void StartZoneIntroCheck()
    {
        // Note: the intro will ONLY play at the beginning of each zone (on the first puzzle)
        if (SceneManager.GetActiveScene().name != "TutorialMap" && !playerScript.OnCheckpoint())
            StartCoroutine(StartZoneIntro());
        else
        {
            audioManagerScript.FadeInLoopingAmbientSFX();
            audioManagerScript.FadeInBackgroundMusic();
            transitionFadeScript.GameFadeIn();
            playerScript.EnablePlayerInputCheck();

            SetZoneIntroInactive();
        }
    }

    // Sets the zone intro game object and script inactive
    private void SetZoneIntroInactive()
    {
        zoneIntro.SetActive(false);
        gameObject.SetActive(false);
        enabled = false;
    }

    // Returns the time it takes to type the zone name
    private float TypeZoneNameDuration()
    {
        typeNameDuration = 0f;

        foreach (char letter in zoneName)
            typeNameDuration += typingSpeed;

        return typeNameDuration;
    }

    // Starts the coroutine that lerps the camera's position
    private void StartCameraCoroutine()
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        cameraCoroutine = LerpCameraPosition();
        StartCoroutine(cameraCoroutine);
    }

    // Lerps the position of the camera to another over a duration (duration = seconds)
    private IEnumerator LerpCameraPosition()
    {
        // Note: the duration is total length of the zone intro
        float duration = transitionFadeScript.introFadeIn + TypeZoneNameDuration() + displayNameDuration + transitionFadeScript.introFadeOut;
        float time = 0f;

        while (time < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = cameraEndPos;
    }

    // Sets up and starts the zone intro
    private IEnumerator StartZoneIntro()
    {
        mainCamera.transform.position = cameraStartPos;
        mainCamera.transform.eulerAngles = cameraDefaultRot;
        notificationBubbles.SetActive(false);

        blackBarsScript.TurnOnBars();
        transitionFadeScript.IntroFadeIn();
        torchMeterScript.TurnOffTorchMeter();
        playerScript.SetPlayerBoolsFalse();
        playerScript.TorchMeterAnimationCheck();
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

        blackBarsScript.TurnOffBars();
        transitionFadeScript.GameFadeIn();
        torchMeterScript.TurnOnTorchMeter();
        playerScript.EnablePlayerInputCheck();

        if (cameraCoroutine != null) StopCoroutine(cameraCoroutine);
        mainCamera.transform.position = cameraOriginalPos;
        mainCamera.transform.eulerAngles = cameraOriginalRot;
        notificationBubbles.SetActive(true);
        SetZoneIntroInactive();
    }

    // Sets the camera vectors and zone name
    private void SetVectorsAndZoneName()
    {
        cameraOriginalPos = mainCamera.transform.position;
        cameraOriginalRot = mainCamera.transform.eulerAngles;
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
                cameraStartPos = mainCamera.transform.position;
                break;
        }

        cameraEndPos = cameraStartPos + new Vector3(distanceToTravel, 0, 0);
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


        mainCamera = cameraScript.gameObject;
        charNoiseSFX = audioManagerScript.charNoiseAS;
        SetVectorsAndZoneName();
    }

}
