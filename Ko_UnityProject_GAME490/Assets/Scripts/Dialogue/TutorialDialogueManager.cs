using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogueManager : MonoBehaviour
{
    [Range(0, 3f)]
    public float fadeLength = 1f;
    [Range(0.005f, 0.1f)]
    public float typingSpeed = 0.03f;

    private float originalTypingSpeed;
    private string[] sentences;
    private int sentenceIndex;
    private bool inDialogue = false;

    private GameObject skipSceneButton;
    private GameObject continueButtonTD;
    private GameObject continueButtonCD;

    private Image blackOverlay;
    private TextMeshProUGUI tutorialDialogueText;
    private Color zeroAlpha = new Color(0, 0, 0, 0);
    private Color halfAlpha = new Color(0, 0, 0, 0.5f);

    private AudioSource charNoise;
    private IEnumerator fadeOverlayCorouitne;

    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private Artifact artifactScript;
    private CharacterDialogue characterDialogueScript;
    private GameHUD gameHUDScript;
    private AudioManager audioManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    void Start()
    {
        playerScript.SetPlayerBoolsFalse();
        playerScript.CanSetBoolsTrue = false;
        skipSceneButton.SetActive(false);
    }

    void LateUpdate()
    {
        TutorialDialogueInputCheck();
    }

    // Returns or sets the value of the bool inDialogue
    public bool InDialogue
    {
        get
        {
            return inDialogue;
        }
        set
        {
            inDialogue = value;
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

    // Sets the variables that come after ending the tutorial dialogue - ONLY for when an artifact isn't being inspected in the tutorial
    public void EndTutorialDialogue()
    {
        skipSceneButton.SetActive(true);
        inDialogue = false;

        if (characterDialogueScript.canStartDialogue)
            playerScript.SetPlayerBoolsTrue();
    }

    // Begins the tutorial dialogue
    public void StartDialogue()
    {
        pauseMenuScript.enabled = false;
        inDialogue = true;

        continueButtonTD.SetActive(false);
        skipSceneButton.SetActive(false);

        playerScript.SetPlayerBoolsFalse();
        tutorialDialogueText.text = string.Empty;
        sentenceIndex = 0;

        FadeBlackOverlay(halfAlpha, fadeLength);
        StartCoroutine(TypeTutorialDialogue());
    }

    // Ends the tutorial dialogue
    private void EndDialogue()
    {
        pauseMenuScript.enabled = true;
        tutorialDialogueText.text = string.Empty;
        FadeBlackOverlay(zeroAlpha, 0f);

        if (artifactScript.IsInspectingArtifact)
        {
            artifactScript.CanRotateArtifact = true;
            continueButtonCD.SetActive(true);
        }
        else
            EndTutorialDialogue();
    }

    // Checks to play the next sentence in the tutorial dialogue
    private void NextSentenceCheck()
    {
        continueButtonTD.SetActive(false);
        typingSpeed = originalTypingSpeed;

        if (sentenceIndex < sentences.Length - 1 && sentences[sentenceIndex + 1] != string.Empty)
        {
            sentenceIndex++;
            tutorialDialogueText.text = string.Empty;
            StartCoroutine(TypeTutorialDialogue());
        }
        else 
            EndDialogue();
    }

    // Checks for the input that continues or speeds up the tutorial dialogue
    private void TutorialDialogueInputCheck()
    {
        if (continueButtonTD.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                NextSentenceCheck();
        }
        else if (typingSpeed > originalTypingSpeed / 2)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                typingSpeed /= 2;
        }
    }

    // Starts the coroutine that fades the black overlay
    private void FadeBlackOverlay(Color endValue, float duration)
    {
        if (fadeOverlayCorouitne != null)
            StopCoroutine(fadeOverlayCorouitne);

        fadeOverlayCorouitne = FadeOverlay(endValue, duration);
        StartCoroutine(fadeOverlayCorouitne);
    }

    // Fades the alpha of the overlay to another over a specific duartion (duration = seconds)
    private IEnumerator FadeOverlay(Color endValue, float duration)
    {
        float time = 0;
        Color startValue = blackOverlay.color;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = endValue;
    }

    // Types out the tutorial dialogue text
    private IEnumerator TypeTutorialDialogue()
    {
        yield return new WaitForSeconds(0.03f);

        foreach (char letter in sentences[sentenceIndex].ToCharArray())
        {
            tutorialDialogueText.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        continueButtonTD.SetActive(true);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        artifactScript = FindObjectOfType<Artifact>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName = child.name;

            if (childName == "BlackOverlay")
                blackOverlay = child.GetComponent<Image>();
            if (childName == "ContinueButtonTD")
                continueButtonTD = child;
            if (childName == "TutorialDialogueText")
                tutorialDialogueText = child.GetComponent<TextMeshProUGUI>();
        }

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

        charNoise = audioManagerScript.charNoiseAS;
        originalTypingSpeed = typingSpeed;
    }

}
