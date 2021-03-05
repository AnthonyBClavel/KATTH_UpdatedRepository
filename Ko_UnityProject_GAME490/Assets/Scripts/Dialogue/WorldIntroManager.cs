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

    public TextMeshProUGUI textDisplay;
    public AudioSource charNoise;

    public float typingDelay = 0.03f;
    private string worldNameText;
    private string currentText = "";

    private AudioLoops audioLoopsScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        StartCoroutine(ShowWorldName());
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
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = false;
        player.GetComponent<TileMovementController>().SetPlayerBoolsFalse(); // Disable player movement
        blackOverlay.SetActive(true);
        worldName.SetActive(true);

        yield return new WaitForSeconds(0.5f);
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
        //yield return new WaitForSeconds(0.5f);
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = true;
        player.GetComponent<TileMovementController>().SetPlayerBoolsTrue(); // Enable player movement
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

    // Sets the world name to a string
    private void displayWorldName()
    {
        if (SceneManager.GetActiveScene().name == "FirstMap")
            worldNameText = "World 1: Boreal Forest";
        else if (SceneManager.GetActiveScene().name == "SecondMap")
            worldNameText = "World 2: Frozen Forest";
        else if (SceneManager.GetActiveScene().name == "ThirdMap")
            worldNameText = "World 3: Crystal Cave";
        else if (SceneManager.GetActiveScene().name == "FourthMap")
            worldNameText = "World 4: Ember City";
        else if (SceneManager.GetActiveScene().name == "FifthMap")
            worldNameText = "World 5: Power Station";
    }

}
