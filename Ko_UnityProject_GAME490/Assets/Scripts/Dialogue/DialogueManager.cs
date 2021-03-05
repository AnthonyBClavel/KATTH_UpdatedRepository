using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject player;
    public GameObject dialogue;
    public GameObject continueTrigger;
    public GameObject blackOverlay;
    public GameObject skipTutorialButton;
    public Canvas pauseMenuCanvas;
    private GameHUD gameHUDScript;

    public TextMeshProUGUI textDisplay;
    private string[] sentences;
    private int index;

    public float typingSpeed;
    private float OGtypingSpeed;
    public AudioSource charNoise;

    public bool hasStarted = false;
    public bool inDialogue = false;

    void Awake()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
    }

    void Start()
    {
        OGtypingSpeed = typingSpeed;
        hasStarted = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && continueTrigger.activeSelf == true)
            nextSentence();
        else if (Input.GetKeyDown(KeyCode.Return) && typingSpeed > OGtypingSpeed/2)
            typingSpeed /= 2;
    }

    
    // Begins the dialogue - call this whenever you want to display dialogue
    public void startDialogue()
    {
        gameHUDScript.TurnOffHUD();
        inDialogue = true;
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = false;
        player.GetComponent<TileMovementController>().SetPlayerBoolsFalse(); // Disabling player movement
        typingSpeed = OGtypingSpeed;
        textDisplay.text = "";
        index = 0;
        continueTrigger.SetActive(false);
        skipTutorialButton.SetActive(false);     
        blackOverlay.SetActive(true);
        dialogue.SetActive(true);
        StartCoroutine(Type());
    }

    
    // Shows the text dialogue in the Dialogue Box
    IEnumerator Type()
    {
        yield return new WaitForSeconds(0.03f);

        continueTrigger.SetActive(false); // Removes continue button until sentence is finished
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed); // Adds delay between characters
        }
        continueTrigger.SetActive(true); // Shows the continue button after the sentence is finished
    }
    
    // Displays the next sentence
    public void nextSentence()
    {
        typingSpeed = OGtypingSpeed;
        if (index < sentences.Length - 1 && sentences[index+1] != "")
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(Type());
        }
        else endDialogue();
    }
    
    // Ends the dialogue - call this when the dialogue is finished
    public void endDialogue()
    {
        gameHUDScript.TurnOnHUD();
        dialogue.SetActive(false);
        continueTrigger.SetActive(false);
        blackOverlay.SetActive(false);     
        skipTutorialButton.SetActive(true);
        pauseMenuCanvas.GetComponent<PauseMenu>().enabled = true;
        player.GetComponent<TileMovementController>().SetPlayerBoolsTrue();
        player.GetComponent<TileMovementController>().hasDied = false;
        inDialogue = false;
    }

    // Sets the dialogue
    public void setDialogue(string[] dialogue)
    {
        sentences = dialogue;
    }

    public string[] readTextFile(TextAsset textFile)
    {
        return textFile.text.Split("\n"[0]);
    }

}
