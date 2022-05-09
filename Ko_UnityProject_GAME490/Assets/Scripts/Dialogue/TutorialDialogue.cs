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

    private bool hasPlayedWelcome = false;
    private bool hasPlayedPush = false;
    private bool hasPlayedBreak = false;
    private bool hasPlayedHole = false;
    private bool hasPlayedFirestone = false;
    private bool hasPassedBridge = false;
    private bool hasPlayedInteractables = false;
    private bool hasPlayedArtifact = false;
    private bool hasplayedFinished = false;

    private TextMeshProUGUI continueButtonText;
    private GameObject continueButton;
    private Image blackOverlay;
    private Color zeroAlpha = new Color(0, 0, 0, 0);
    private Color halfAlpha = new Color(0, 0, 0, 0.5f);
    private TextMeshProUGUI tutorialDialogueText;

    private AudioSource charNoise;
    private IEnumerator fadeOverlayCorouitne;
    private IEnumerator inputCoroutine;

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
    private SkipSceneButton skipSceneButtonScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerScript.SetPlayerBoolsFalse();
        skipSceneButtonScript.SetSkipSceneButtonInactive();
    }

    // Plays the death dialogue
    public void PlayDeathDialogue()
    {
        SetDialogue(deathDialogue);
        StartDialogue();
    }

    // Checks to play the pushable block dialogue 
    public void PlayPushDialogueCheck()
    {
        if (!hasPlayedPush && playerScript.PuzzleNumber == 1)
        {
            SetDialogue(pushDialogue);
            StartDialogue();
            hasPlayedPush = true;
        }
    }

    // Checks to play the welcome dialogue
    public void PlayWelcomeDialogueCheck()
    {
        if (!hasPlayedWelcome)
        {
            SetDialogue(welcomeDialogue);
            StartDialogue();
            hasPlayedWelcome = true;
        }
    }

    // Checks to play the breakable block dialogue
    public void PlayBreakDialogueCheck()
    {
        if (!hasPlayedBreak)
        {
            SetDialogue(breakDialogue);
            StartDialogue();
            hasPlayedBreak = true;
        }
    }

    // Checks to play the hole dialogue
    public void PlayHoleDialogueCheck()
    {
        if (!hasPlayedHole)
        {
            SetDialogue(holeDialogue);
            StartDialogue();
            hasPlayedHole = true;
        }
    }

    // Checks to play the firstone dialogue
    public void PlayFirstoneDialogueCheck()
    {
        if (!hasPlayedFirestone)
        {
            SetDialogue(firestoneDialogue);
            StartDialogue();
            hasPlayedFirestone = true;
        }
    }

    // Checks to play the interactables dialogue (NPCs and Artifacts)
    public void PlayInteractablesDialogueCheck()
    {
        if (!hasPlayedInteractables)
        {
            SetDialogue(interactablesDialogue);
            StartDialogue();
            hasPlayedInteractables = true;
        }
    }

    // Checks to play the finished tutorial dialogue
    public void PlayFinishedTutorialDialogueCheck()
    {
        if (!hasplayedFinished)
        {
            SetDialogue(finishedTutorialDialogue);
            StartDialogue();
            hasplayedFinished = true;
        }
    }

    // Checks to play the bridge dialogue
    public void PlayBridgeDialogueCheck()
    {
        if (!hasPassedBridge && hasPlayedWelcome) // Note: hasPlayedWelcome was added to prevent dialogue issues if debugging
        {
            SetDialogue(bridgeDialogue);
            StartDialogue();
            hasPassedBridge = true;
        }
    }

    // Checks to play the artifact dialogue
    public bool PlayArtifactDialogueCheck()
    {
        if (!hasPlayedArtifact)
        {
            continueButton.SetActive(false);
            SetDialogue(artifcatsDialogue);
            StartDialogue();
            hasPlayedArtifact = true;
            return true;
        }

        return false;
    }

    // Starts the tutorial dialogue
    private void StartDialogue()
    {
        pauseMenuScript.enabled = false;
        skipSceneButtonScript.SetSkipSceneButtonInactive();
        originalTypingSpeed = typingSpeed;
        //typingSpeed = originalTypingSpeed;

        playerScript.SetPlayerBoolsFalse();
        tutorialDialogueText.text = string.Empty;
        sentenceIndex = 0;

        StartFadeOverlayCoroutine(halfAlpha, fadeLength);
        StartCoroutine(TypeTutorialDialogue());
        StartInputCoroutine();
    }

    // Ends the tutorial dialogue
    private void EndDialogue()
    {
        pauseMenuScript.enabled = true;
        tutorialDialogueText.text = string.Empty;
        StartFadeOverlayCoroutine(zeroAlpha, 0f);
       
        if (characterDialogueScript.hasTransitionedToArtifactView)
        {
            continueButton.SetActive(true);
            artifactScript.StartInputCoroutine();
        }
        else
        {
            skipSceneButtonScript.SetSkipSceneButtonActive();
            playerScript.SetPlayerBoolsTrue();
        }
    }

    // Sets the array of dialogue sentences
    private void SetDialogue(TextAsset textFile)
    {
        string[] textFileSentences = textFile.text.Split("\n"[0]);
        sentences = textFileSentences;
    }

    // Checks to play the next sentence in the tutorial dialogue
    private void NextSentenceCheck()
    {
        continueButton.SetActive(false);
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

    // Starts the coroutine that fades the overlay int/out
    private void StartFadeOverlayCoroutine(Color endValue, float duration)
    {
        if (fadeOverlayCorouitne != null)
            StopCoroutine(fadeOverlayCorouitne);

        fadeOverlayCorouitne = FadeOverlay(endValue, duration);
        StartCoroutine(fadeOverlayCorouitne);
    }

    // Starts the coroutine that checks for the tutorial dialogue input
    private void StartInputCoroutine()
    {
        if (inputCoroutine != null)
            StopCoroutine(inputCoroutine);

        inputCoroutine = TutorialDialogueInputCheck();
        StartCoroutine(TutorialDialogueInputCheck());
    }

    // Lerps the alpha of the overlay to another over a specific duartion (duration = seconds)
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
        continueButton.SetActive(true);
    }

    // Checks for the input that continues or speeds up the tutorial dialogue
    private IEnumerator TutorialDialogueInputCheck()
    {
        while (!pauseMenuScript.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (continueButton.activeSelf)
                    NextSentenceCheck();

                else if (!continueButton.activeSelf && typingSpeed > originalTypingSpeed / 2)
                    typingSpeed /= 2;
            }

            yield return null;
        }
        //Debug.Log("Stopped looking for tutorial dialogue inputCheck");
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
        skipSceneButtonScript = FindObjectOfType<SkipSceneButton>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName = child.name;

            if (childName == "BlackOverlay")
                blackOverlay = child.GetComponent<Image>();
            if (childName == "TutorialDialogueText")
                tutorialDialogueText = child.GetComponent<TextMeshProUGUI>();
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ContinueButton")
            {
                continueButtonText = child.GetComponent<TextMeshProUGUI>();
                continueButton = child;
            }           
        }

        charNoise = audioManagerScript.charNoiseAS;
        originalTypingSpeed = typingSpeed;
    }

}
