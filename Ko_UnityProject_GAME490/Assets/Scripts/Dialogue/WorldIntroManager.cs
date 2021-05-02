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

    public float typingDelay = 0.03f;
    private float cameraSpeed = 3f;
    private string worldNameText;
    private string currentText = string.Empty;

    public GameObject worldName;
    public GameObject blackOverlay;
    public GameObject firstBlock;
    private GameObject levelFade;
    private GameObject pixelatedCamera;

    private Vector3 originalCameraPos;
    private Vector3 newCameraPosition;
    private Vector3 cameraDestination;

    private AudioSource charNoise;
    private TextMeshProUGUI textDisplay;
    private Animator blackOverlayAnimator;

    private AudioLoops audioLoopsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private DialogueBars dialogueBarScript;
    private TorchMeterScript torchMeterScript;
    private GameHUD gameHUDScript;
    private CameraController cameraScript;
    private LevelFade levelFadeScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        dialogueBarScript = FindObjectOfType<DialogueBars>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        cameraScript = FindObjectOfType<CameraController>();
        levelFadeScript = FindObjectOfType<LevelFade>();

        pixelatedCamera = cameraScript.gameObject;
        levelFade = levelFadeScript.gameObject;     
    }

    // Start is called before the first frame update
    void Start()
    {
        levelFade.SetActive(false);
        dialogueBarScript.canMoveBars = true;
        blackOverlayAnimator = blackOverlay.GetComponent<Animator>();
        textDisplay = worldName.GetComponent<TextMeshProUGUI>();
        charNoise = GetComponent<AudioSource>();

        SetCameraVariables();        
        StartIntroCheck(); // This MUST be called in start last, NOT in awake!
    }

    void LateUpdate()
    {
        if (!cameraScript.canMoveCamera && !cameraScript.canMoveToDialogueViews && canWorldIntro)
            pixelatedCamera.transform.position = Vector3.MoveTowards(pixelatedCamera.transform.position, cameraDestination, cameraSpeed * Time.deltaTime);
    }

    private IEnumerator ShowWorldName()
    {
        dialogueBarScript.canMoveBars = false;
        cameraScript.canMoveCamera = false;
        pauseMenuScript.enabled = false;
        canWorldIntro = true;

        dialogueBarScript.SetDialogueBars();      
        playerScript.SetPlayerBoolsFalse();   
        
        worldName.SetActive(true);
        blackOverlay.SetActive(true);  
        torchMeterScript.gameObject.SetActive(false);
        gameHUDScript.notificationBubblesHolder.SetActive(false);
        //gameHUDScript.gameObject.SetActive(false);

        pixelatedCamera.transform.position = newCameraPosition;

        yield return new WaitForSeconds(2f);
        DisplayWorldName();

        for (int i = 0; i <= worldNameText.Length; i++)
        {
            currentText = worldNameText.Substring(0,i);
            textDisplay.text = currentText;

            foreach (char letter in textDisplay.text)
                charNoise.Play();

            yield return new WaitForSeconds(typingDelay);
        }

        yield return new WaitForSeconds(3f);
        blackOverlayAnimator.SetTrigger("FadeOut_WI");
        worldName.SetActive(false);

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
        torchMeterScript.gameObject.SetActive(true);
        gameHUDScript.notificationBubblesHolder.SetActive(true);
        //gameHUDScript.gameObject.SetActive(true);

        pixelatedCamera.transform.position = originalCameraPos;
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

    // Sets the camera's original position and it's destination
    private void SetCameraVariables()
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
            worldNameText = "Zone 1: Boreal Forest";
        else if (sceneName == "SecondMap")
            worldNameText = "Zone 2: Frozen Forest";
        else if (sceneName == "ThirdMap")
            worldNameText = "Zone 3: Crystal Cave";
        else if (sceneName == "FourthMap")
            worldNameText = "Zone 4: Barren Lands";
        else if (sceneName == "FifthMap")
            worldNameText = "Zone 5: Power Station";
    }

}
