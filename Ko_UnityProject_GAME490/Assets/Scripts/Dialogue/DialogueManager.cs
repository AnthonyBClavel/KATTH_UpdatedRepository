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
    public Canvas pauseMenuCanvas;

    public TextMeshProUGUI textDisplay;
    private string[] sentences;
    private int index;

    public float typingSpeed;
    private float OGtypingSpeed;
    public AudioSource charNoise;

    public bool hasStarted = false;
    public bool inDialogue = false;

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

    /***
     * Begins the dialogue
     * Call this whenever you want to display dialogue
     ***/
    public void startDialogue()
    {
        inDialogue = true;
        pauseMenuCanvas.GetComponent<PauseMenu01>().enabled = false;
        player.GetComponent<TileMovementV2>().enabled = false; // Disabling player movement script
        typingSpeed = OGtypingSpeed;
        textDisplay.text = "";
        index = 0;
        continueTrigger.SetActive(false);
        blackOverlay.SetActive(true);
        dialogue.SetActive(true);
        StartCoroutine(Type());
    }

    /***
     * Shows the text dialogue in the Dialogue Box
     ***/
    IEnumerator Type()
    {
        continueTrigger.SetActive(false); // Removes continue button until sentence is finished
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed); // Adds delay between characters
        }
        continueTrigger.SetActive(true); // Sentence is finished, show continue button
    }

    /***
     * Displays the next sentence
     ***/
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

    /***
     * Ends the dialogue
     * Call this when the dialogue is finished
     ***/
    public void endDialogue()
    {
        dialogue.SetActive(false);
        continueTrigger.SetActive(false);
        blackOverlay.SetActive(false);
        pauseMenuCanvas.GetComponent<PauseMenu01>().enabled = true;
        player.GetComponent<TileMovementV2>().enabled = true;
        player.GetComponent<TileMovementV2>().hasDied = false;
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
