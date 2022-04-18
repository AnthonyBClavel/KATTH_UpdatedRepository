using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    [Range(0, 3f)]
    public float fadeLength = 1f;
    [Range(0.005f, 0.1f)]
    public float typingSpeed = 0.03f;

    private float originalTypingSpeed;
    private string[] sentences;
    private int sentenceIndex;
    private bool inDialogue = false;

    private bool hasPlayedWelcome = false;
    private bool hasPlayedPush = false;
    private bool hasPlayedBreak = false;
    private bool hasPlayedHole = false;
    private bool hasPlayedFirestone = false;
    private bool hasPassedBridge = false;
    private bool hasPlayedInteractables = false;
    private bool hasPlayedArtifacts = false;
    private bool hasplayedFinished = false;
    private bool canPlayDeathDialogue = false;

    private GameObject skipSceneButton;
    private GameObject continueButtonTD;
    private GameObject continueButtonCD;

    private Image blackOverlay;
    private TextMeshProUGUI tutorialDialogueText;
    private Color zeroAlpha = new Color(0, 0, 0, 0);
    private Color halfAlpha = new Color(0, 0, 0, 0.5f);

    private AudioSource charNoise;
    private IEnumerator fadeOverlayCorouitne;

    public TextAsset welcomeDialogue;
    public TextAsset pushDialogue;
    public TextAsset breakDialogue;
    public TextAsset holeDialogue;
    public TextAsset firestoneDialogue;
    public TextAsset interactablesDialogue;
    public TextAsset artifcatsDialogue;
    public TextAsset finishedTutorialDialogue;
    public TextAsset bridgeDialogue;
    public TextAsset deathDialogue;

    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private Artifact artifactScript;
    private CharacterDialogue characterDialogueScript;
    private GameHUD gameHUDScript;
    private AudioManager audioManagerScript;
    private TorchMeter torchMeterScript;
    private TransitionFade transitionFadeScript;

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

    void Update()
    {
        TutorialDialogueInputCheck();
        TutorialDialogueCheck();
    }

    // Returns or sets the value of the bool canPlayDeathDialogue
    public bool CanPlayDeathDialogue
    {
        get { return canPlayDeathDialogue; }
        set { canPlayDeathDialogue = value; }
    }

    // Sets the array of dialogue sentences
    private void SetDialogue(TextAsset textFile)
    {
        string[] textFileSentences = textFile.text.Split("\n"[0]);
        sentences = textFileSentences;
    }

    // Begins the tutorial dialogue
    public void StartDialogue()
    {
        pauseMenuScript.enabled = false;
        inDialogue = true;

        continueButtonTD.SetActive(false);
        skipSceneButton.SetActive(false);
        originalTypingSpeed = typingSpeed;
        //typingSpeed = originalTypingSpeed;

        playerScript.CanSetBoolsTrue = false;
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

    // Sets the variables that come after ending the tutorial dialogue - ONLY for when an artifact isn't being inspected in the tutorial
    public void EndTutorialDialogue()
    {
        skipSceneButton.SetActive(true);
        inDialogue = false;

        if (characterDialogueScript.canStartDialogue)
        {
            if (canPlayDeathDialogue)
                canPlayDeathDialogue = false;

            playerScript.CanSetBoolsTrue = true;
            playerScript.SetPlayerBoolsTrue();
        }      
    }

    // Checks to play the next sentence in the tutorial dialogue
    private void NextSentenceCheck()
    {
        continueButtonTD.SetActive(false);
        originalTypingSpeed = typingSpeed;
        //typingSpeed = originalTypingSpeed;

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
        if (!pauseMenuScript.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (continueButtonTD.activeSelf)
                    NextSentenceCheck();

                else if (!continueButtonTD.activeSelf && typingSpeed > originalTypingSpeed / 2)
                    typingSpeed /= 2;
            }
        }
    }

    // Checks when and which dialogue should play throughout the tutorial zone
    private void TutorialDialogueCheck()
    {
        if (playerScript.CanMove && !transitionFadeScript.IsChangingScenes)
        {
            Collider collider = playerScript.GetCollider();
            string currentPuzzle = playerScript.CurrentPuzzle.name;

            if (collider != null)
                switch (collider.tag)
                {
                    case ("PushableBlock"):
                        if (!hasPlayedPush)
                        {
                            SetDialogue(pushDialogue);
                            StartDialogue();
                            hasPlayedPush = true;
                        }
                        break;
                    default:
                        break;
                }

            if (currentPuzzle != null)
                switch (currentPuzzle)
                {
                    case ("Puzzle01"):
                        if (!hasPlayedWelcome)
                        {
                            SetDialogue(welcomeDialogue);
                            StartDialogue();
                            hasPlayedWelcome = true;
                        }
                        break;
                    case ("Puzzle02"):
                        if (!hasPlayedBreak)
                        {
                            SetDialogue(breakDialogue);
                            StartDialogue();
                            hasPlayedBreak = true;
                        }
                        break;
                    case ("Puzzle03"):
                        if (!hasPlayedHole)
                        {
                            SetDialogue(holeDialogue);
                            StartDialogue();
                            hasPlayedHole = true;
                        }
                        break;
                    case ("Puzzle04"):
                        if (!hasPlayedFirestone)
                        {
                            SetDialogue(firestoneDialogue);
                            StartDialogue();
                            hasPlayedFirestone = true;
                        }
                        break;
                    case ("Puzzle05"):
                        if (!hasPlayedInteractables)
                        {
                            SetDialogue(interactablesDialogue);
                            StartDialogue();
                            hasPlayedInteractables = true;
                        }
                        break;
                    case ("Puzzle06"):
                        if (!hasplayedFinished)
                        {
                            SetDialogue(finishedTutorialDialogue);
                            StartDialogue();
                            hasplayedFinished = true;
                        }
                        break;
                    default:
                        break;
                }

        }

        else if (playerScript.OnLastTileBlock() && !hasPassedBridge)
        {
            SetDialogue(bridgeDialogue);
            StartDialogue();
            hasPassedBridge = true;
        }

        if (artifactScript.IsInspectingArtifact && !hasPlayedArtifacts)
        {
            SetDialogue(artifcatsDialogue);
            StartDialogue();

            continueButtonCD.SetActive(false);
            artifactScript.CanRotateArtifact = false;
            hasPlayedArtifacts = true;
        }

        if (torchMeterScript.CurrentVal <= 0 && !playerScript.CanRestartPuzzle && !inDialogue && !canPlayDeathDialogue)
            canPlayDeathDialogue = true;
    }

    // Plays the death dialogue
    public void PlayDeathDialogue()
    {
        SetDialogue(deathDialogue);
        StartDialogue();
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

        typingSpeed = originalTypingSpeed;
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
        torchMeterScript = FindObjectOfType<TorchMeter>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
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
