using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorldIntroManager : MonoBehaviour
{
    public Canvas pauseMenuCanvas;
    public GameObject player;
    public GameObject worldName;
    public GameObject blackOverlay;
    public GameObject levelFade;
    public GameObject firstBlock;

    public TextMeshProUGUI textDisplay;
    public AudioSource charNoise;

    public bool hasPlayedIntro;

    public float typingDelay = 0.03f;
    private string worldNameText;
    private string currentText = "";

    private AudioLoops audioLoopsScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();

        StartIntroCheck();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    void Update()
    {

    }

    private IEnumerator ShowWorldName()
    {
        player.GetComponent<TileMovementController>().SetPlayerBoolsFalse(); // Disable player movement
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = false;  
        blackOverlay.SetActive(true);
        worldName.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        player.GetComponent<TileMovementController>().PopOutTorchMeterCheck();
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
        player.GetComponent<TileMovementController>().WalkIntoScene();
        //player.GetComponent<TileMovementController>().SetPlayerBoolsTrue(); // Enable player movement - this is now enabled when the player hits a checkpoint
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = true;
        blackOverlay.SetActive(false);       
        levelFade.SetActive(true);
        audioLoopsScript.SetAudioLoopsActive();
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
        if (player.transform.position == firstBlock.transform.position)
        {
            StartCoroutine(ShowWorldName());
        }
        else
        {
            levelFade.SetActive(true);
            audioLoopsScript.SetAudioLoopsToDefault();
        }
    }

    // Sets the world name to a string
    private void displayWorldName()
    {
        if (SceneManager.GetActiveScene().name == "FirstMap")
            worldNameText = "Zone 1: Boreal Forest";
        else if (SceneManager.GetActiveScene().name == "SecondMap")
            worldNameText = "Zone 2: Frozen Forest";
        else if (SceneManager.GetActiveScene().name == "ThirdMap")
            worldNameText = "Zone 3: Crystal Cave";
        else if (SceneManager.GetActiveScene().name == "FourthMap")
            worldNameText = "Zone 4: Barren Lands";
        else if (SceneManager.GetActiveScene().name == "FifthMap")
            worldNameText = "Zone 5: Power Station";
    }

}
