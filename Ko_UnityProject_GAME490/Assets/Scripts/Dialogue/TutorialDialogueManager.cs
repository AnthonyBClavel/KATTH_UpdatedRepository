using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialDialogueManager : MonoBehaviour
{
    public bool inDialogue = false;
    private bool hasEnteredTutorial = false;

    public GameObject dialogueText;
    public GameObject continueButtonDM;
    public GameObject blackOverlay;
    private GameObject skipSceneButton;
    private GameObject continueButtonCD;

    private TextMeshProUGUI textDisplay;
    private AudioSource charNoise;

    private string[] sentences;
    public float typingSpeed;
    private float OGtypingSpeed;
    private int index;

    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private Artifact artifactScript;
    private CharacterDialogue characterDialogueScript;
    private NotificationBubbles notificationBubblesScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    void Start()
    {
        textDisplay = dialogueText.GetComponent<TextMeshProUGUI>();
        charNoise = GetComponent<AudioSource>();

        OGtypingSpeed = typingSpeed;;
    }

    void LateUpdate()
    {
        CheckToEnterTutorial();

        if (continueButtonDM.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                nextSentence();
        }

        else if (typingSpeed > OGtypingSpeed / 2)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
        //notificationBubblesScript.DisableNotificationsToggle();
        pauseMenuScript.enabled = false;
        inDialogue = true;

        playerScript.SetPlayerBoolsFalse(); // Disabling player movement
        typingSpeed = OGtypingSpeed;
        textDisplay.text = string.Empty;
        index = 0;

        continueButtonDM.SetActive(false);
        skipSceneButton.SetActive(false);
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
        continueButtonDM.SetActive(false);
        blackOverlay.SetActive(false);
        pauseMenuScript.enabled = true;

        if (artifactScript.IsInspectingArtifact)
        {
            artifactScript.CanRotateArtifact = true;
            continueButtonCD.SetActive(true);
        }
        else
            EndTutorialDialogueManager();
    }

    // Sets the variables that come after ending the tutorial dialogue - ONLY for when an artifact isn't being inspected in the tutorial
    public void EndTutorialDialogueManager()
    {
        skipSceneButton.SetActive(true);
        //notificationBubblesScript.EnableNotificationsToggle();
        //playerScript.hasDied = false;
        inDialogue = false;

        if (characterDialogueScript.canStartDialogue)
            playerScript.SetPlayerBoolsTrue();
    }

    // Checks to see if the player has loaded into the tutorial zone
    private void CheckToEnterTutorial()
    {
        if (!hasEnteredTutorial)
        {
            //notificationBubblesScript.DisableNotificationsToggle();
            skipSceneButton.SetActive(false);
            playerScript.SetPlayerBoolsFalse();
            playerScript.WalkIntoScene();
            playerScript.CanSetBoolsTrue = false;
            hasEnteredTutorial = true;
        }
    }

    // Shows the text dialogue in the dialogue Box
    private IEnumerator TypeDialogue()
    {
        yield return new WaitForSeconds(0.03f);
        continueButtonDM.SetActive(false); // Removes continue button until sentence is finished

        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed); // Adds delay between characters
        }

        continueButtonDM.SetActive(true); // Shows the continue button after the sentence is finished
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        artifactScript = FindObjectOfType<Artifact>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        gameHUDScript = FindObjectOfType<GameHUD>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ContinueButton")
                continueButtonCD = child;
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "SkipSceneButton")
                skipSceneButton = child;
        }
    }

}
