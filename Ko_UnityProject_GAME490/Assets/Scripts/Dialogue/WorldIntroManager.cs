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

    public TextMeshProUGUI textDisplay;
    public AudioSource charNoise;

    public bool hasPlayedIntro;

    public float typingDelay = 0.03f;
    private string worldNameText;
    private string currentText = "";

    private AudioLoops audioLoopsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();    
    }

    // Start is called before the first frame update
    void Start()
    {
        StartIntroCheck(); //MUST be called in start, not awake!
    }

    void Update()
    {

    }

    private IEnumerator ShowWorldName()
    {
        playerScript.SetPlayerBoolsFalse(); // Disable player movement
        pauseMenuScript.enabled = false;  
        blackOverlay.SetActive(true);
        worldName.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        playerScript.PopOutTorchMeterCheck();
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
        playerScript.WalkIntoScene();
        //player.GetComponent<TileMovementController>().SetPlayerBoolsTrue(); // Enable player movement - this is now enabled when the player hits a checkpoint
        pauseMenuScript.enabled = true;
        blackOverlay.SetActive(false);       
        levelFade.SetActive(true);
        audioLoopsScript.FadeInAudioLoops();
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
