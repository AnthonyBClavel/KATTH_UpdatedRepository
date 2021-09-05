using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorldIntroManager : MonoBehaviour
{
    public bool hasPlayedIntro;
    private bool canWorldIntro = false;

    private float typingDelay;
    private float cameraSpeed = 3f;

    private string zoneNameText;
    private string currentText = string.Empty;

    private GameObject zoneName;
    private GameObject blackOverlay;
    private GameObject firstBlock;
    private GameObject levelFade;
    private GameObject pixelatedCamera;

    private TextMeshProUGUI zoneTextComponent;
    private AudioSource charNoiseSFX;
    private Animator blackOverlayAnimator;

    private Vector3 originalCameraPos;
    private Vector3 newCameraPosition;
    private Vector3 cameraDestination;

    private AudioLoops audioLoopsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private DialogueBars dialogueBarScript;
    private TorchMeterScript torchMeterScript;
    private GameHUD gameHUDScript;
    private CameraController cameraScript;
    private LevelFade levelFadeScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        levelFade.SetActive(false);
        dialogueBarScript.canMoveBars = true;

        SetCameraVectors(); 
        StartIntroCheck(); // MUST be called last in start, NOT in awake!
    }

    void LateUpdate()
    {
        if (!cameraScript.canMoveCamera && !cameraScript.canMoveToDialogueViews && canWorldIntro)
            pixelatedCamera.transform.position = Vector3.MoveTowards(pixelatedCamera.transform.position, cameraDestination, cameraSpeed * Time.deltaTime);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        dialogueBarScript = FindObjectOfType<DialogueBars>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        cameraScript = FindObjectOfType<CameraController>();
        levelFadeScript = FindObjectOfType<LevelFade>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "ZoneName")
                zoneName = child.gameObject;
            if (child.name == "BlackOverlay")
                blackOverlay = child.gameObject;
        }

        blackOverlayAnimator = blackOverlay.GetComponent<Animator>();
        zoneTextComponent = zoneName.GetComponent<TextMeshProUGUI>();

        pixelatedCamera = cameraScript.gameObject;
        levelFade = levelFadeScript.gameObject;
        charNoiseSFX = audioManagerScript.charNoiseSFX;
        firstBlock = gameManagerScript.firstBlock;
        typingDelay = gameManagerScript.typingDelay;
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

    // Determines the world name to display
    private void DisplayWorldName()
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

    // Checks to see if the world intro can be started
    private void StartIntroCheck()
    {
        if (playerScript.gameObject.transform.position == firstBlock.transform.position)
        {
            StartCoroutine("ShowWorldName");
            audioLoopsScript.SetAudioLoopsToZero();
        }
        else
        {
            levelFade.SetActive(true);
            audioLoopsScript.SetAudioLoopsToDefault();
        }
    }

    private IEnumerator ShowWorldName()
    {
        dialogueBarScript.canMoveBars = false;
        cameraScript.canMoveCamera = false;
        pauseMenuScript.enabled = false;
        canWorldIntro = true;

        dialogueBarScript.SetDialogueBars();
        playerScript.SetPlayerBoolsFalse();

        zoneName.SetActive(true);
        blackOverlay.SetActive(true);
        torchMeterScript.TurnOffTorchMeter();
        gameHUDScript.notificationBubblesHolder.SetActive(false);
        //gameHUDScript.gameObject.SetActive(false);

        pixelatedCamera.transform.position = newCameraPosition;

        yield return new WaitForSeconds(2f);
        DisplayWorldName();

        for (int i = 0; i <= zoneNameText.Length; i++)
        {
            currentText = zoneNameText.Substring(0, i);
            zoneTextComponent.text = currentText;

            foreach (char letter in zoneTextComponent.text)
                charNoiseSFX.Play();

            yield return new WaitForSeconds(typingDelay);
        }

        yield return new WaitForSeconds(3f);
        blackOverlayAnimator.SetTrigger("FadeOut_WI");
        zoneName.SetActive(false);

        yield return new WaitForSeconds(2f);
        playerScript.WalkIntoScene();
        audioLoopsScript.FadeInAudioLoops();
        dialogueBarScript.ResetDialogueBars();

        dialogueBarScript.canMoveBars = true;
        cameraScript.canMoveCamera = true;
        pauseMenuScript.enabled = true;
        canWorldIntro = false;

        //player.GetComponent<TileMovementController>().SetPlayerBoolsTrue(); // Enable player movement - this is now enabled when the player hits a checkpoint
        //torchMeterScript.SetFirstPuzzleValue();

        blackOverlay.SetActive(false);
        levelFade.SetActive(true);
        torchMeterScript.TurnOnTorchMeter();
        gameHUDScript.notificationBubblesHolder.SetActive(true);
        //gameHUDScript.gameObject.SetActive(true);

        pixelatedCamera.transform.position = originalCameraPos;
    }

}
