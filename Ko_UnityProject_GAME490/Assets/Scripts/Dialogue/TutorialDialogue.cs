using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    private int sentenceIndex;
    private string[] sentences;

    [Header("Tutorial Dialogue Variables")]
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    [SerializeField] [Range(0, 3f)]
    private float fadeDuration = 1f; // Original Value = 1f
    private float originalTypingSpeed;

    private bool hasPlayedWelcome = false;
    private bool hasPlayedPush = false;
    private bool hasPassedBridge = false;

    private GameObject tutorialDialogueHolder;
    private GameObject artifactButtons;
    private GameObject continueButton;

    private TextMeshProUGUI tutorialDialogueText;
    private Image blackOverlay;
    private AudioSource charNoise;
    private TextAsset deathDialogue;
    public List<TextAsset> tutorialDialogue = new List<TextAsset>();

    private IEnumerator fadeOverlayCorouitne;
    private IEnumerator dialogueInputCoroutine;

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

    // Starts the tutorial dialogue
    private void StartDialogue()
    {
        skipSceneButtonScript.SetSkipSceneButtonInactive();
        playerScript.SetPlayerBoolsFalse();

        tutorialDialogueHolder.SetActive(true);
        tutorialDialogueText.text = string.Empty;

        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        pauseMenuScript.enabled = false;
        sentenceIndex = 0;

        StartFadeOverlayCoroutine(0.5f, fadeDuration);
        StartCoroutine(TypeTutorialDialogue());
        StartDialogueInputCoroutine();
    }

    // Ends the tutorial dialogue
    private void EndDialogue()
    {
        if (characterDialogueScript.IsInteractingWithArtifact)
        {
            artifactButtons.SetActive(true);
            artifactScript.StartInputCoroutine();
        }
        else
        {
            skipSceneButtonScript.SetSkipSceneButtonActive();
            playerScript.SetPlayerBoolsTrue();
        }

        tutorialDialogueHolder.SetActive(false);
        tutorialDialogueText.text = string.Empty;

        pauseMenuScript.enabled = true;
        blackOverlay.SetImageAlpha(0f);
    }

    // Sets the array of dialogue sentences
    private void SetDialogue(TextAsset textFile)
    {
        string[] textFileSentences = textFile.text.Split("\n"[0]);
        sentences = textFileSentences;
    }

    // Plays the death dialogue
    public void PlayDeathDialogue()
    {
        SetDialogue(deathDialogue);
        StartDialogue();
    }

    // Checks to play the welcome dialogue
    public void PlayWelcomeDialogueCheck()
    {
        if (hasPlayedWelcome) return;

        PlayDialogueCheck("Welcome");
        hasPlayedWelcome = true;
    }

    // Checks to play the pushable block dialogue 
    public void PlayPushDialogueCheck(Collider collider)
    {
        if (collider == null || !collider.CompareTag("PushableBlock") || hasPlayedPush) return;

        PlayDialogueCheck("Push");
        hasPlayedPush = true;
    }

    // Checks to play the bridge dialogue
    public void PlayBridgeDialogueCheck(bool onLastBridgeTile)
    {
        if (!onLastBridgeTile || !hasPlayedWelcome || hasPassedBridge) return;

        PlayDialogueCheck("Bridge");
        hasPassedBridge = true;
    }

    // Checks if the dialogue can be played - returns true and plays the dialogue if so, false otherwise
    public bool PlayDialogueCheck(string nameOfTextAsset)
    {
        if (tutorialDialogue.Count == 0) return false;

        foreach (TextAsset textFile in tutorialDialogue)
        {
            if (textFile.name != nameOfTextAsset) continue;

            SetDialogue(textFile);
            StartDialogue();
            tutorialDialogue.Remove(textFile);
            return true;
        }
        return false;
    }

    // Checks to play the next sentence in the tutorial dialogue
    private void NextSentenceCheck()
    {
        continueButton.SetActive(false);
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        tutorialDialogueText.text = string.Empty;

        if (sentenceIndex < sentences.Length - 1 && sentences[sentenceIndex + 1] != string.Empty)
        {
            sentenceIndex++;
            StartCoroutine(TypeTutorialDialogue());
        }
        else
            EndDialogue();
    }

    // Checks for the input to continue or speed-up the tutorial dialogue
    private void ContinueInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        if (continueButton.activeSelf)
            NextSentenceCheck();

        else if (!continueButton.activeSelf && typingSpeed > originalTypingSpeed / 2)
            typingSpeed /= 2;
    }

    // Starts the coroutine that fades the overlay
    private void StartFadeOverlayCoroutine(float endAlpha, float duration)
    {
        if (fadeOverlayCorouitne != null) StopCoroutine(fadeOverlayCorouitne);

        fadeOverlayCorouitne = FadeOverlay(endAlpha, duration);
        StartCoroutine(fadeOverlayCorouitne);
    }

    // Starts the coroutine that checks for the tutorial dialogue input
    private void StartDialogueInputCoroutine()
    {
        if (dialogueInputCoroutine != null) StopCoroutine(dialogueInputCoroutine);

        dialogueInputCoroutine = TutorialDialogueInputCheck();
        StartCoroutine(TutorialDialogueInputCheck());
    }

    // Lerps the alpha of the overlay to another over a specific duartion (duration = seconds)
    private IEnumerator FadeOverlay(float endAlpha, float duration)
    {
        Color startColor = blackOverlay.color;
        Color endColor = blackOverlay.ReturnImageColor(endAlpha);
        float time = 0;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = endColor;
    }

    // Types out the text for the tutorial dialogue
    private IEnumerator TypeTutorialDialogue()
    {
        yield return new WaitForSeconds(0.03f);

        foreach (char letter in sentences[sentenceIndex])
        {
            tutorialDialogueText.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        typingSpeed = originalTypingSpeed;
        continueButton.SetActive(true);
    }

    // Checks for the tutorial dialogue input
    private IEnumerator TutorialDialogueInputCheck()
    {
        while (!pauseMenuScript.enabled)
        {
            if (Time.deltaTime > 0) ContinueInputCheck();
            yield return null;
        }
        //Debug.Log("Tutorial dialogue input check has ENDED");
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
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "KeybindButtons")
            {
                GameObject keybindButtons = child;

                for (int j = 0; j < keybindButtons.transform.childCount; j++)
                {
                    GameObject child02 = keybindButtons.transform.GetChild(j).gameObject;

                    if (child02.name == "ContinueButton")
                        continueButton = child02;
                    if (child02.name == "ArtifactButtons")
                        artifactButtons = child02;
                }
            }

            if (child.name == "TutorialDialogue")
            {
                tutorialDialogueHolder = child;

                for (int j = 0; j < tutorialDialogueHolder.transform.childCount; j++)
                {
                    GameObject child02 = tutorialDialogueHolder.transform.GetChild(j).gameObject;

                    if (child02.name == "BlackOverlay")
                        blackOverlay = child02.GetComponent<Image>();
                    if (child02.name == "Text")
                        tutorialDialogueText = child02.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // Sets the death dialogue and removes it from the list
        foreach (TextAsset textFile in tutorialDialogue)
        {
            if (textFile.name == "Death")
            {
                deathDialogue = textFile;
                tutorialDialogue.Remove(textFile);
                break;
            }
        }

        charNoise = audioManagerScript.charNoiseAS;
        originalTypingSpeed = typingSpeed;
    }

}
