using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public bool hasStarted = false;
    public bool inDialogue = false;
    private bool hasEnteredTutorial = false;

    public GameObject dialogueText;
    public GameObject continueButton;
    public GameObject blackOverlay;
    private GameObject skipTutorialButton;
    private GameObject gameHUD;

    private TextMeshProUGUI textDisplay;
    private AudioSource charNoise;

    private string[] sentences;
    public float typingSpeed;
    private float OGtypingSpeed;
    private int index;

    private GameHUD gameHUDScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private SkipButton skipButtonScript;

    void Awake()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        skipButtonScript = FindObjectOfType<SkipButton>();

        gameHUD = gameHUDScript.gameObject;
        skipTutorialButton = skipButtonScript.gameObject;
    }

    void Start()
    {
        textDisplay = dialogueText.GetComponent<TextMeshProUGUI>();
        charNoise = GetComponent<AudioSource>();

        OGtypingSpeed = typingSpeed;

        //hasStarted = true;
    }

    void LateUpdate()
    {
        CheckToEnterTutorial();

        if (playerScript.checkIfOnCheckpoint() && !hasStarted)
            hasStarted = true;

        if (continueButton.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                nextSentence();
        }

        else if (typingSpeed > OGtypingSpeed / 2)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                typingSpeed /= 2;
        }       
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

    // Begins the dialogue - call this whenever you want to display dialogue
    public void startDialogue()
    {
        //gameHUD.SetActive(false);
        gameHUDScript.DisableNotificationsToggle();
        pauseMenuScript.enabled = false;
        inDialogue = true;

        playerScript.SetPlayerBoolsFalse(); // Disabling player movement
        typingSpeed = OGtypingSpeed;
        textDisplay.text = string.Empty;
        index = 0;

        continueButton.SetActive(false);
        skipTutorialButton.SetActive(false);
        blackOverlay.SetActive(true);
        dialogueText.SetActive(true);
        StartCoroutine("TypeDialogue");
    }

    // Displays the next sentence
    private void nextSentence()
    {
        typingSpeed = OGtypingSpeed;

        if (index < sentences.Length - 1 && sentences[index + 1] != string.Empty)
        {
            index++;
            textDisplay.text = string.Empty;
            StartCoroutine("TypeDialogue");
        }
        else endDialogue();
    }
    
    // Ends the dialogue - call this when the dialogue is finished
    private void endDialogue()
    {
        dialogueText.SetActive(false);
        continueButton.SetActive(false);
        blackOverlay.SetActive(false);
        skipTutorialButton.SetActive(true);
        //gameHUD.SetActive(true);

        gameHUDScript.EnableNotificationsToggle();
        playerScript.SetPlayerBoolsTrue();

        playerScript.hasDied = false;
        pauseMenuScript.enabled = true;
        inDialogue = false;
    }

    // Checks to see if the player has loaded into the tutorial zone
    private void CheckToEnterTutorial()
    {
        if (!hasEnteredTutorial)
        {
            //gameHUD.SetActive(false);
            gameHUDScript.DisableNotificationsToggle();
            skipTutorialButton.SetActive(false);
            playerScript.SetPlayerBoolsFalse();
            playerScript.WalkIntoScene();
            playerScript.canSetBoolsTrue = false;
            hasEnteredTutorial = true;
        }
    }

    // Shows the text dialogue in the dialogue Box
    private IEnumerator TypeDialogue()
    {
        yield return new WaitForSeconds(0.03f);
        continueButton.SetActive(false); // Removes continue button until sentence is finished

        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed); // Adds delay between characters
        }

        continueButton.SetActive(true); // Shows the continue button after the sentence is finished
    }

}
