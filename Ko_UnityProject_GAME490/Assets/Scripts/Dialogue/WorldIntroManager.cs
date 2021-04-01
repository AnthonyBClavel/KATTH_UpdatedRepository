using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorldIntroManager : MonoBehaviour
{
    public GameObject worldName;
    public GameObject blackOverlay;
    public GameObject levelFade;
    public GameObject firstBlock;
    public GameObject blackBars;
    private GameObject pixelatedCamera;

    private Vector3 originalCameraPos;
    private Vector3 newCameraPosition;
    private Vector3 cameraDestination;

    public TextMeshProUGUI textDisplay;
    public AudioSource charNoise;

    public bool hasPlayedIntro;
    public float typingDelay = 0.03f;
    private float cameraSpeed = 3f;
    private string worldNameText;
    private string currentText = "";

    private AudioLoops audioLoopsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private DialogueBars dialogueBarScript;
    private TorchMeterScript torchMeterScript;
    private GameHUD gameHUDScript;
    private CameraController cameraScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        dialogueBarScript = FindObjectOfType<DialogueBars>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        cameraScript = FindObjectOfType<CameraController>();
        pixelatedCamera = cameraScript.gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetCameraVariables();
        StartIntroCheck(); //MUST be called in start, not awake!
    }

    void LateUpdate()
    {
        if(!cameraScript.canMoveCamera)
            pixelatedCamera.transform.position = Vector3.MoveTowards(pixelatedCamera.transform.position, cameraDestination, cameraSpeed * Time.deltaTime);
    }

    private IEnumerator ShowWorldName()
    {   
        cameraScript.canMoveCamera = false;
        pixelatedCamera.transform.position = newCameraPosition;
        dialogueBarScript.canMoveBars = false;
        dialogueBarScript.SetDialogueBars();      
        playerScript.SetPlayerBoolsFalse(); // Disable player movement      
        pauseMenuScript.enabled = false;  
        blackOverlay.SetActive(true);
        worldName.SetActive(true);     
        torchMeterScript.gameObject.SetActive(false);
        gameHUDScript.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);      
        displayWorldName();

        for (int i = 0; i <= worldNameText.Length; i++)
        {
            currentText = worldNameText.Substring(0,i);
            textDisplay.text = currentText;       
            TypeWorldName();
            yield return new WaitForSeconds(typingDelay);
        }

        yield return new WaitForSeconds(3f);
        worldName.SetActive(false);
        blackOverlay.GetComponent<Animator>().SetTrigger("FadeOut_WI");

        yield return new WaitForSeconds(2f);
        cameraScript.canMoveCamera = true;
        pixelatedCamera.transform.position = originalCameraPos;     
        dialogueBarScript.ResetDialogueBars();
        dialogueBarScript.canMoveBars = true;
        playerScript.WalkIntoScene();
        //player.GetComponent<TileMovementController>().SetPlayerBoolsTrue(); // Enable player movement - this is now enabled when the player hits a checkpoint
        pauseMenuScript.enabled = true;
        blackOverlay.SetActive(false); 
        levelFade.SetActive(true);
        audioLoopsScript.FadeInAudioLoops();
        torchMeterScript.gameObject.SetActive(true);
        //torchMeterScript.SetFirstPuzzleValue();
        gameHUDScript.gameObject.SetActive(true);
    }
    
    // Plays an SFX for every character in the world name
    private void TypeWorldName()
    {
        foreach (char letter in textDisplay.text)
        {
            charNoise.Play();
        }
    }

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

    // Sets the world name to a string
    private void displayWorldName()
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
